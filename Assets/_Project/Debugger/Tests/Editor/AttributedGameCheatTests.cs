using System;
using System.Linq;
using Dreamy.Debugger.Options;
using Dreamy.Economy;
using NUnit.Framework;
using UnityEngine;

namespace Dreamy.Template.Debugging.Tests
{
    public sealed class AttributedGameCheatTests
    {
        private static GameCheatOptions CreateOptions()
        {
            var wallet = new EmptyWallet();
            return new GameCheatOptions(wallet, wallet);
        }

        private sealed class EmptyWallet : IResourceWallet, IResourceBalanceProvider
        {
            public long GetBalance(ResourceId id) => 0;
            public bool TryGrant(ResourceGrantRequest request) => false;
            public bool TryExchange(ResourceExchangeRequest request) => false;
        }

        private CheatRegistry registry;
        private IDisposable registration;
        private bool unlocked;
        private float originalTimeScale;
        private int originalFrameRate;

        [SetUp] public void SetUp()
        {
            originalTimeScale = Time.timeScale;
            originalFrameRate = Application.targetFrameRate;
            unlocked = true;
            registry = new CheatRegistry(() => unlocked);
            registration = registry.Register(CreateOptions().CreateOptions());
        }

        [TearDown] public void TearDown()
        {
            registration.Dispose();
            Time.timeScale = originalTimeScale;
            Application.targetFrameRate = originalFrameRate;
        }

        [Test] public void PartialFilesContributeTypedOptionsAndReadOnlyProperties()
        {
            var options = registry.Snapshot();
            Assert.That(options.Count, Is.EqualTo(32));
            var time = options.Single(o => o.Id == "sandbox.game.TimeScale");
            Assert.That(time.Category, Is.EqualTo("05 Gameplay"));
            Assert.That(time.DisplayName, Is.EqualTo("Time scale"));
            Assert.That(time.Minimum, Is.EqualTo(0.1));
            Assert.That(time.Maximum, Is.EqualTo(3));
            Assert.That(time.Increment, Is.EqualTo(0.1));
            Assert.That(options.Single(o => o.Id == "sandbox.game.ActiveScene").IsReadOnly, Is.True);
            Assert.That(registry.Read("sandbox.game.ActiveScene"), Is.TypeOf<string>());
            registry.Set("sandbox.game.TargetFrameRate", FrameRatePreset.Fps60);
            Assert.That(Application.targetFrameRate, Is.EqualTo(60));
            Assert.Throws<ArgumentException>(() => registry.Set("sandbox.game.TargetFrameRate", (FrameRatePreset)999));
        }

        [Test] public void AttributesStillEnforceRangesAccessAndActionConfirmation()
        {
            registry.Set("sandbox.game.TimeScale", 0.5f);
            Assert.That(Time.timeScale, Is.EqualTo(0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => registry.Set("sandbox.game.TimeScale", 0f));
            registry.Execute("sandbox.game.ResetTimeScale");
            Assert.That(Time.timeScale, Is.EqualTo(1));
            unlocked = false;
            Assert.Throws<UnauthorizedAccessException>(() => registry.Set("sandbox.game.TimeScale", 2f));
            Assert.Throws<UnauthorizedAccessException>(() => registry.Execute("sandbox.game.ResetTimeScale", true));
        }

        [Test] public void TypedCardsAndPartialContainerCoexistAndDisposeTogether()
        {
            registration.Dispose();
            var options = CreateOptions().CreateOptions().Concat(new[]
            {
                CheatOption.Value("example.balance", "Example", "Balance", () => 100L)
                    .WithCard("example", "Coins", CheatCardKind.Resource, CheatCardRole.Balance)
            });
            using (registry.Register(options)) Assert.That(registry.Snapshot().Count, Is.EqualTo(33));
            Assert.That(registry.Snapshot(), Is.Empty);
        }
    }
}
