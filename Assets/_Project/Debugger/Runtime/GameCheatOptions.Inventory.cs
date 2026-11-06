using System.ComponentModel;
using Dreamy.Debugger.Options;
using Dreamy.Debugger.Presentation;
using Dreamy.Economy;

namespace Dreamy.Template.Debugging
{
    public sealed partial class GameCheatOptions
    {
        private const string KnightCardsCheatId = "sandbox.card.knight";

        [Category("02 Inventory"), ResourceCheat(KnightCardsCheatId, "Knight cards", CheatCardRole.Balance)]
        public long KnightCardsBalance => Balance("card.knight");

        [Category("02 Inventory"), ResourceCheat(KnightCardsCheatId, "Knight cards", CheatCardRole.Amount)]
        [NumberRange(1, 1000000), Increment(1)]
        public long KnightCardsGrantAmount { get; set; } = 10;

        [Category("02 Inventory"), ResourceCheat(KnightCardsCheatId, "Knight cards", CheatCardRole.Action)]
        public void AddKnightCards() => Grant(new ResourceId("card.knight"), KnightCardsGrantAmount);

        private const string SilverChestsCheatId = "sandbox.chest.silver";

        [Category("02 Inventory"), ResourceCheat(SilverChestsCheatId, "Silver chests", CheatCardRole.Balance)]
        public long SilverChestsBalance => Balance("chest.silver");

        [Category("02 Inventory"), ResourceCheat(SilverChestsCheatId, "Silver chests", CheatCardRole.Amount)]
        [NumberRange(1, 1000000), Increment(1)]
        public long SilverChestsGrantAmount { get; set; } = 1;

        [Category("02 Inventory"), ResourceCheat(SilverChestsCheatId, "Silver chests", CheatCardRole.Action)]
        public void AddSilverChests() => Grant(new ResourceId("chest.silver"), SilverChestsGrantAmount);

        private const string DailyRewardCardsCheatId = "sandbox.item.demo-card";

        [Category("02 Inventory"), ResourceCheat(DailyRewardCardsCheatId, "Daily reward cards", CheatCardRole.Balance)]
        public long DailyRewardCardsBalance => Balance("item.demo-card");

        [Category("02 Inventory"), ResourceCheat(DailyRewardCardsCheatId, "Daily reward cards", CheatCardRole.Amount)]
        [NumberRange(1, 1000000), Increment(1)]
        public long DailyRewardCardsGrantAmount { get; set; } = 10;

        [Category("02 Inventory"), ResourceCheat(DailyRewardCardsCheatId, "Daily reward cards", CheatCardRole.Action)]
        public void AddDailyRewardCards() => Grant(new ResourceId("item.demo-card"), DailyRewardCardsGrantAmount);

        private const string RedSkinCheatId = "sandbox.skin.hero.red";

        [Category("02 Inventory"), OwnershipCheat(RedSkinCheatId, "Red hero skin", CheatCardRole.Owned)]
        public bool OwnsRedHeroSkin => IsOwned(new ResourceId("skin.hero.red"));

        [Category("02 Inventory"), OwnershipCheat(RedSkinCheatId, "Red hero skin", CheatCardRole.Action)]
        public void UnlockRedHeroSkin() => GrantOwnership(new ResourceId("skin.hero.red"));
    }
}
