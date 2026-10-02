using System;

namespace Dreamy.LuckyWheel
{
    public sealed class LuckyWheelPresenter : IDisposable
    {
        private readonly ILuckyWheelService service;
        private readonly ILuckyWheelView view;
        private bool isBound;
        private string activeRevealTransactionId;

        public LuckyWheelPresenter(ILuckyWheelService service, ILuckyWheelView view)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Show()
        {
            Bind();
            Refresh();
        }

        public void Refresh()
        {
            LuckyWheelViewState state = service.GetState();
            view.Render(state);
            view.SetSpinInteractable(state.CanSpin);
            if (state.PendingResult.HasValue)
            {
                PlayOutcomeOnce(state.PendingResult.Value);
            }
        }

        public void Dispose()
        {
            if (!isBound)
            {
                return;
            }

            view.SpinRequested -= Spin;
            view.RetryRequested -= Retry;
            view.Activated -= Refresh;
            view.RevealInterrupted -= InterruptReveal;
            view.RevealCompleted -= CompleteReveal;
            view.CloseRequested -= Close;
            isBound = false;
        }

        private void Bind()
        {
            if (isBound)
            {
                return;
            }

            view.SpinRequested += Spin;
            view.RetryRequested += Retry;
            view.Activated += Refresh;
            view.RevealInterrupted += InterruptReveal;
            view.RevealCompleted += CompleteReveal;
            view.CloseRequested += Close;
            isBound = true;
        }

        private void Spin()
        {
            view.SetSpinInteractable(false);
            HandleSpinResult(service.TrySpin());
        }

        private void Retry()
        {
            view.SetSpinInteractable(false);
            HandleSpinResult(service.RetryPendingGrant());
        }

        private void HandleSpinResult(LuckyWheelSpinResult result)
        {
            if (result.HasOutcome)
            {
                PlayOutcomeOnce(result);
                return;
            }

            view.ShowError(result.Status);
            Refresh();
        }

        private void PlayOutcomeOnce(LuckyWheelSpinResult result)
        {
            if (string.Equals(activeRevealTransactionId, result.TransactionId, StringComparison.Ordinal))
            {
                return;
            }

            activeRevealTransactionId = result.TransactionId;
            view.SetSpinInteractable(false);
            view.PlaySpin(result);
        }

        private void CompleteReveal(string transactionId)
        {
            if (!string.Equals(activeRevealTransactionId, transactionId, StringComparison.Ordinal))
            {
                view.ShowError(LuckyWheelSpinStatus.InvalidReveal);
                return;
            }

            if (!service.AcknowledgeReveal(transactionId))
            {
                view.ShowError(LuckyWheelSpinStatus.InvalidReveal);
                return;
            }

            activeRevealTransactionId = null;
            Refresh();
        }

        private void InterruptReveal(string transactionId)
        {
            if (string.Equals(activeRevealTransactionId, transactionId, StringComparison.Ordinal))
            {
                activeRevealTransactionId = null;
            }
        }

        private void Close()
        {
            Dispose();
            view.Close();
        }
    }
}
