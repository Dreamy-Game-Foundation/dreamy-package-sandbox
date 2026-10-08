using System;
using Dreamy.Economy;

namespace Dreamy.Feedback.Samples.Economy
{
    /// <summary>Observes committed balance changes. Never grants resources from an animation callback.</summary>
    public sealed class FeedbackRewardPresenter : IDisposable
    {
        private readonly IResourceBalanceSource source;
        private readonly ResourceId resource;
        private readonly Action<long, long> changed;
        private long previous;
        public FeedbackRewardPresenter(IResourceBalanceSource source, ResourceId resource, Action<long, long> changed)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.resource = resource; this.changed = changed ?? throw new ArgumentNullException(nameof(changed));
            previous = source.GetBalance(resource); source.BalanceChanged += BalanceChanged;
        }
        private void BalanceChanged(ResourceBalanceChanged state)
        {
            if (!state.ResourceId.Equals(resource)) return;
            long delta = state.Balance - previous; previous = state.Balance;
            changed(state.Balance, delta);
        }
        public void Dispose() => source.BalanceChanged -= BalanceChanged;
    }
}
