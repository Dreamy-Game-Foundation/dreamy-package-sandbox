using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Economy;
using Dreamy.Economy.UI;

namespace Dreamy.Template.Demo
{
    public sealed class CustomShopResourceAcquisitionGateway : IResourceAcquisitionGateway
    {
        private readonly Func<CancellationToken, UniTask<UIShopPanel>> openReadyPanel;
        private readonly CancellationToken ownerToken;
        private bool busy;
        public CustomShopResourceAcquisitionGateway(Func<CancellationToken, UniTask<UIShopPanel>> openReadyPanel,
            CancellationToken ownerToken = default)
        {
            this.openReadyPanel = openReadyPanel ?? throw new ArgumentNullException(nameof(openReadyPanel));
            this.ownerToken = ownerToken;
        }

        public ResourceAcquisitionAvailability GetAvailability(ResourceId resource) =>
            resource == new ResourceId("currency.coin") || resource == new ResourceId("currency.gem")
                ? ResourceAcquisitionAvailability.Available : ResourceAcquisitionAvailability.Unavailable;

        public async UniTask<ResourceAcquisitionResult> OpenAsync(ResourceAcquisitionRequest request, CancellationToken token)
        {
            using var owner = CancellationTokenSource.CreateLinkedTokenSource(token, ownerToken);
            token = owner.Token;
            if (token.IsCancellationRequested) return ResourceAcquisitionResult.Cancelled;
            if (!request.IsValid) return ResourceAcquisitionResult.Failed;
            if (busy) return ResourceAcquisitionResult.Busy;
            if (GetAvailability(request.TargetResourceId) == ResourceAcquisitionAvailability.Unavailable)
                return ResourceAcquisitionResult.NoMatchingDestination;
            busy = true;
            try
            {
                var panel = await openReadyPanel(token);
                token.ThrowIfCancellationRequested();
                if (panel == null) return ResourceAcquisitionResult.Failed;
                using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token, panel.GetCancellationTokenOnDestroy());
                var returned = new UniTaskCompletionSource();
                void Closed() => returned.TrySetResult();
                panel.OnPostHide += Closed;
                panel.Destroyed += Closed;
                try
                {
                    if (!await panel.FocusResourceAsync(request.TargetResourceId, lifetime.Token)) return ResourceAcquisitionResult.NoMatchingDestination;
                    await returned.Task.AttachExternalCancellation(lifetime.Token);
                    return ResourceAcquisitionResult.Returned;
                }
                finally { panel.OnPostHide -= Closed; panel.Destroyed -= Closed; }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { return ResourceAcquisitionResult.Cancelled; }
            catch (Exception) { return ResourceAcquisitionResult.Failed; }
            finally { busy = false; }
        }
    }
}
