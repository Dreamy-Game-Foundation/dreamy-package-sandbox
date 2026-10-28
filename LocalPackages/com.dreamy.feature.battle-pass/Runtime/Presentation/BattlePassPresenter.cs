using System;
using Dreamy.Feature;
namespace Dreamy.BattlePass
{
    public sealed class BattlePassPresenter : FeaturePresenter
    {
        private readonly IBattlePassService service;
        private readonly IBattlePassView view;
        private readonly Action premiumNavigation;
        private readonly Action<string,string> claimNavigation;
        public BattlePassPresenter(IBattlePassService service,IBattlePassView view,Action premiumNavigation=null,Action<string,string> claimNavigation=null)
        { this.service=service ?? throw new ArgumentNullException(nameof(service));this.view=view ?? throw new ArgumentNullException(nameof(view));this.premiumNavigation=premiumNavigation;this.claimNavigation=claimNavigation; }
        protected override void Bind()
        { service.StateChanged+=Refresh;view.ClaimRequested+=Claim;view.ClaimAllRequested+=ClaimAll;view.RecoverRequested+=Recover;view.PremiumRequested+=Premium; }
        protected override void Unbind()
        { service.StateChanged-=Refresh;view.ClaimRequested-=Claim;view.ClaimAllRequested-=ClaimAll;view.RecoverRequested-=Recover;view.PremiumRequested-=Premium; }
        public override void Refresh() => view.Render(service.GetState());
        private void Claim(string slot)
        { var occurrence=service.GetState().OccurrenceId;if(claimNavigation != null)claimNavigation(occurrence,slot);else view.ShowResult(service.Claim(occurrence,slot));Refresh(); }
        private void ClaimAll() { view.ShowResult(service.ClaimAll());Refresh(); }
        private void Recover() { view.ShowResult(service.RecoverPending());Refresh(); }
        private void Premium() { premiumNavigation?.Invoke();Refresh(); }
    }
}
