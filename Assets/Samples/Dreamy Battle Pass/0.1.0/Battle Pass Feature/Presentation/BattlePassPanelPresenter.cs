using Dreamy.UI;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassPanelPresenter : IPanelPresenter
    {
        private readonly BattlePassPresenter presenter;
        public BattlePassPanelPresenter(BattlePassPresenter presenter){this.presenter=presenter;}
        public void Show()=>presenter.Show();public void Dispose()=>presenter.Dispose();
    }
}
