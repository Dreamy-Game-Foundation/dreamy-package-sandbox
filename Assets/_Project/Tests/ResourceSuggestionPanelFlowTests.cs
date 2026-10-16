using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Economy;
using Dreamy.Economy.UI;
using Dreamy.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Dreamy.Template.Tests
{
    public sealed class ResourceSuggestionPanelFlowTests
    {
        private static readonly ResourceId Coin = new("currency.coin"), Gem = new("currency.gem");
        private GameObject root;
        private PanelManager manager;
        private Gateway gateway;
        private IInsufficientResourcesPresentation coin, gem;
        private static ResourceAmount[] Costs(ResourceId resource) => new[] { new ResourceAmount(resource, 100) };

        private sealed class Balances : IResourceBalanceSource
        {
            public event Action<ResourceBalanceChanged> BalanceChanged;
            public long GetBalance(ResourceId id) => 0;
        }
        private sealed class Gateway : IResourceAcquisitionGateway
        {
            public int Calls;
            public ResourceAcquisitionRequest Request;
            public Func<bool> Closed;
            public UniTaskCompletionSource<ResourceAcquisitionResult> Pending;
            public ResourceAcquisitionAvailability GetAvailability(ResourceId resource) => ResourceAcquisitionAvailability.Available;
            public async UniTask<ResourceAcquisitionResult> OpenAsync(ResourceAcquisitionRequest request, CancellationToken token)
            {
                Assert.That(Closed(), Is.True, "Popup must leave the stack before acquisition.");
                Calls++; Request = request;
                return Pending == null ? ResourceAcquisitionResult.Returned : await Pending.Task.AttachExternalCancellation(token);
            }
        }

        [UnitySetUp]
        public IEnumerator SetUp() => Initialize().ToCoroutine();
        private async UniTask Initialize()
        {
            if (!Application.isPlaying) Assert.Ignore("Addressable popup flow is a PlayMode check.");
            Assert.That(PanelManager.HasInstance, Is.False, "Run in an isolated test scene.");
            root = new GameObject("Suggestion flow test", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            manager = root.AddComponent<PanelManager>();
            // Complete the manager's startup child scan before loading panels.
            await UniTask.NextFrame();
            var balances = new Balances();
            gateway = new Gateway { Closed = () => manager.LastPanel == null };
            var acquisition = new ResourceAcquisitionCoordinator(gateway);
            var factory = new PanelPresenterFactory();
            coin = ResourceSuggestionInstaller.Install<CoinSuggestionPanel>(factory, Coin, Address.CoinSuggestionPanel, balances, acquisition);
            gem = ResourceSuggestionInstaller.Install<GemSuggestionPanel>(factory, Gem, Address.GemSuggestionPanel, balances, acquisition);
            manager.PresenterFactory = factory;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root);
            yield return null;
        }

        private async UniTask<TPanel> Open<TPanel>() where TPanel : ResourceSuggestionPanel
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await UniTask.WaitUntil(() => manager.LastPanel is TPanel, cancellationToken: timeout.Token);
            return manager.Get<TPanel>();
        }
        private static Button Button(ResourceSuggestionPanel panel, string field) =>
            (Button)typeof(ResourceSuggestionPanel).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel);

        [UnityTest]
        public IEnumerator CoinGemCoin_LoadsCorrectAddressAndFreshPresenter() => Reopen().ToCoroutine();
        private async UniTask Reopen()
        {
            for (int i = 0; i < 3; i++)
            {
                bool isCoin = i != 1;
                var task = (isCoin ? coin : gem).ShowAsync(Costs(isCoin ? Coin : Gem), "reopen", default);
                ResourceSuggestionPanel panel = isCoin ? await Open<CoinSuggestionPanel>() : await Open<GemSuggestionPanel>();
                Assert.That(panel.ResourceId, Is.EqualTo(isCoin ? Coin : Gem));
                Assert.That(panel.Request.Required, Is.EqualTo(100));
                Button(panel, "laterButton").onClick.Invoke();
                Assert.That(await task, Is.EqualTo(ResourceRecoveryStatus.Cancelled));
                await UniTask.NextFrame();
                Assert.That(panel == null, Is.True);
            }
            Assert.That(gateway.Calls, Is.Zero);
        }

        [UnityTest]
        public IEnumerator Acquire_WaitsForClose_AndSurvivesPresenterDisposal() => Acquire().ToCoroutine();
        private async UniTask Acquire()
        {
            var task = coin.ShowAsync(new[] { new ResourceAmount(Coin, 60), new ResourceAmount(Gem, 10), new ResourceAmount(Coin, 40) }, "upgrade", default);
            var panel = await Open<CoinSuggestionPanel>();
            Button(panel, "acquireButton").onClick.Invoke();
            Button(panel, "acquireButton").onClick.Invoke();
            Assert.That(await task, Is.EqualTo(ResourceRecoveryStatus.Returned));
            Assert.That(gateway.Calls, Is.EqualTo(1));
            Assert.That(gateway.Request.TargetResourceId, Is.EqualTo(Coin));
            Assert.That(gateway.Request.MissingAmount, Is.EqualTo(100));
            Assert.That(gateway.Request.Deficits.Count, Is.EqualTo(2));
            Assert.That(gateway.Request.SourceContextId, Is.EqualTo("upgrade"));
        }

        [UnityTest]
        public IEnumerator BackDuringShow_ClosesWithoutNavigation() => Back().ToCoroutine();
        private async UniTask Back()
        {
            var task = coin.ShowAsync(Costs(Coin), "back", default);
            var panel = await Open<CoinSuggestionPanel>();
            await panel.Hide();
            Assert.That(await task, Is.EqualTo(ResourceRecoveryStatus.Cancelled));
            Assert.That(gateway.Calls, Is.Zero);
            Assert.That(manager.LastPanel, Is.Null);
        }

        [UnityTest]
        public IEnumerator NewResourceRequest_ReplacesOldPopupThroughRouter() => Replace().ToCoroutine();
        private async UniTask Replace()
        {
            var router = new ResourceRecoveryPresentationRouter(new Balances(),
                new Dictionary<ResourceId, IInsufficientResourcesPresentation> { [Coin] = coin, [Gem] = gem });
            var first = router.ShowAsync(Costs(Coin), "first", default);
            await Open<CoinSuggestionPanel>();
            var second = router.ShowAsync(Costs(Gem), "second", default);
            var panel = await Open<GemSuggestionPanel>();
            Assert.That(await first, Is.EqualTo(ResourceRecoveryStatus.Cancelled));
            Button(panel, "laterButton").onClick.Invoke();
            Assert.That(await second, Is.EqualTo(ResourceRecoveryStatus.Cancelled));
            Assert.That(gateway.Calls, Is.Zero);
        }

        [UnityTest]
        public IEnumerator OwnerCancellationDuringAcquisition_DoesNotReopenPopup() => CancelNavigation().ToCoroutine();
        private async UniTask CancelNavigation()
        {
            gateway.Pending = new UniTaskCompletionSource<ResourceAcquisitionResult>();
            using var owner = new CancellationTokenSource();
            var task = coin.ShowAsync(Costs(Coin), "cancel", owner.Token);
            var panel = await Open<CoinSuggestionPanel>();
            Button(panel, "acquireButton").onClick.Invoke();
            await UniTask.WaitUntil(() => gateway.Calls == 1);
            owner.Cancel();
            Assert.That(await task, Is.EqualTo(ResourceRecoveryStatus.Cancelled));
            gateway.Pending.TrySetResult(ResourceAcquisitionResult.Returned);
            await UniTask.NextFrame();
            Assert.That(manager.LastPanel, Is.Null);
        }
    }
}
