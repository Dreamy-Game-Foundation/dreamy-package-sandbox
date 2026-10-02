using System;

namespace Dreamy.LuckyWheel
{
    public interface ILuckyWheelView
    {
        event Action SpinRequested;
        event Action RetryRequested;
        event Action Activated;
        event Action<string> RevealInterrupted;
        event Action<string> RevealCompleted;
        event Action CloseRequested;

        void Render(LuckyWheelViewState state);
        void SetSpinInteractable(bool interactable);
        void PlaySpin(LuckyWheelSpinResult result);
        void ShowError(LuckyWheelSpinStatus status);
        void Close();
    }
}
