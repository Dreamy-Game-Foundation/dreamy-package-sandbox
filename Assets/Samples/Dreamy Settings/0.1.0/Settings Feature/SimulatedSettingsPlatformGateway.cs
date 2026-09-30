using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Settings;

namespace Dreamy.Feature.Settings.Integration
{
    public sealed class SimulatedSettingsPlatformGateway : ISettingsPlatformGateway
    {
        public bool CanShowGdprConsent => true;
        public bool CanRestorePurchases => true;
        public bool CanOpenStore => true;
        public bool CanRequestReview => true;

        public UniTask<SettingsOperationResult> ShowGdprConsentAsync(CancellationToken cancellationToken = default) =>
            UniTask.FromResult(cancellationToken.IsCancellationRequested
                ? SettingsOperationResult.Unavailable("Consent cancelled")
                : SettingsOperationResult.Succeeded("Consent updated"));

        public UniTask<SettingsOperationResult> RestorePurchasesAsync(CancellationToken cancellationToken = default) =>
            UniTask.FromResult(cancellationToken.IsCancellationRequested
                ? SettingsOperationResult.Unavailable("Restore cancelled")
                : SettingsOperationResult.Succeeded("Purchases restored"));

        public UniTask<SettingsOperationResult> OpenStoreAsync(CancellationToken cancellationToken = default) =>
            UniTask.FromResult(cancellationToken.IsCancellationRequested
                ? SettingsOperationResult.Unavailable("Store unavailable")
                : SettingsOperationResult.Succeeded("Store opened"));

        public UniTask<SettingsOperationResult> RequestReviewAsync(CancellationToken cancellationToken = default) =>
            UniTask.FromResult(cancellationToken.IsCancellationRequested
                ? SettingsOperationResult.Unavailable("Review request cancelled")
                : SettingsOperationResult.Succeeded("Thanks for the rating!"));
    }
}
