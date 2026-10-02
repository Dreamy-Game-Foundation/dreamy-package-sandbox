namespace Dreamy.LuckyWheel
{
    public interface ILuckyWheelService
    {
        LuckyWheelViewState GetState();
        LuckyWheelSpinResult TrySpin();
        LuckyWheelSpinResult RetryPendingGrant();
        bool AcknowledgeReveal(string transactionId);
    }
}
