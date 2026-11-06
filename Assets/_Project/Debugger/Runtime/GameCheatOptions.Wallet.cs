using System;
using Dreamy.Economy;

namespace Dreamy.Template.Debugging
{
    public sealed partial class GameCheatOptions
    {
        private long Balance(string resourceId) => balances.GetBalance(new ResourceId(resourceId));
        private bool IsOwned(ResourceId id) => balances.GetBalance(id) > 0;

        private void GrantOwnership(ResourceId id)
        {
            if (!IsOwned(id)) Grant(id, 1);
        }

        private void Grant(ResourceId id, long amount)
        {
            // A deliberate click is a new grant; persistence/rejection/events stay owned by the wallet.
            var request = new ResourceGrantRequest("debugger/sandbox/" + id.Value + "/" + Guid.NewGuid().ToString("N"),
                new ResourceAmount(id, amount));
            try
            {
                LastResult = wallet.TryGrant(request)
                    ? "Saved: +" + amount + " " + id.Value
                    : "Not granted: wallet rejected " + id.Value + ". Check save recovery/limits.";
            }
            catch (Exception exception)
            {
                LastResult = "Not confirmed: " + id.Value + " (" + exception.GetType().Name + "). Check save recovery before retrying.";
            }
        }
    }
}
