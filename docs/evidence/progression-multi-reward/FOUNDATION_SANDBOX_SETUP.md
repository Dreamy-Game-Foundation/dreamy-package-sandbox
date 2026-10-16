# Foundation demo integration

Date: 2026-10-16. Installed in the existing Foundation demo flow.

## How to test

Play `Assets/_Project/Scenes/BootstrapScene.unity`, then Foundation → Progression.
The existing ProgressionDemoPanel prefab now includes reward controls under its SafeArea.
Bootstrap installs FoundationProgressionRewards, registers IProgressionService and
IProgressionRewardService, and binds the existing HUD/presenter to the same model.

- Existing button: +100 XP.
- +300 XP: cross multiple levels; Retry last XP replays the same ID and returns AlreadyProcessed.
- Fail Card: fail one Card delivery, then grant XP. Earlier entries remain acknowledged;
  subsequent independent milestones may complete. Retry rewards resumes missing entries.
- Fail Ack Save: destination persists but journal acknowledgement fails. XP remains committed;
  use Reload Save + Recover, or restart Play Mode.
- Fail Final Save: all entries can be acknowledged while milestone remains Pending;
  Reload Save + Recover completes the durable milestone without duplicate effects.
- Scroll the reward area for each milestone and entry status.

Catalog: existing Basic Progression Resources/DataConfig/progressionCatalog.json now defines
30 Experience levels with concrete Coin/Gem/Card/Chest/Cosmetic bundles. Every fourth level
has no reward. The package source sample remains unchanged; this is sandbox integration data.

Progression save key: `foundation-progression-multi-reward-v2` (preserves the older progression key).
Coin/Gem use the shared bootstrap wallet; these demo grants intentionally affect Shop/Economy
balances too. Card/Chest/Cosmetic have sandbox-owned persistent destinations and keys prefixed
`foundation-progression-v2-`. Receipt/effect persistence stays owned by those destinations.
Game-specific installers, fault injection and panel controls are in Assets/_Project.

## Verification

- Unity compile and Console: 0 error entries after integration.
- PlayMode Bootstrap → MainScene: GameInstaller Ready; progression service installed,
  model initially level 1 / XP 0, Foundation panel ready.
- Invoked the Foundation Progression button: opened the existing panel, all 9 serialized
  Button bindings and both added text bindings present; SafeArea retained.
- RewardScroll has positive height (724 at inspected Game View size), scrollable entry text.
- Controller smoke in isolated /tmp save: Card failure → grant 300 → retry → duplicate XP →
  acknowledgement-save failure → grant 300 → recovery → final-save failure → grant 300 → recovery.
  Result: XP 900, Coin 700, seven reward milestones Delivered, no duplicate Coin effects.
- The smoke did not mutate the bootstrap shared wallet. Live test panel was left at XP 0.

Editor Play start scene is BootstrapScene. To rebuild controls on a clean original prefab,
use Tools/Dreamy/Sandbox/Install Progression Reward Controls; it refuses duplicate installation.
The pre-existing broad Foundation demo builder recreates the original basic Progression prefab;
if that builder is run, reinstall these reward controls afterwards.


## Follow-up: shared Economy inventory

The separate Card/Chest/Cosmetic destinations described in the original setup were replaced
by Economy 0.5.0 adapters to the SAME bootstrap DatasaveResourceWallet. Foundation Daily item's
`item.demo-card` destination also uses that owner. `economy.json` now contains every active
Foundation resource balance/ownership and receipt. Old split files are read-only migration sources.
See `SHARED_ECONOMY_VALIDATION.md` for migration and regression evidence.
