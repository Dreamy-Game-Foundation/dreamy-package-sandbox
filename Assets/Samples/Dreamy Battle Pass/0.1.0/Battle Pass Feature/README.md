# Battle Pass Feature sample

Independent persistent demo for Unity6000, Dreamy UI1.0.6, Shop0.2.0, Economy0.7.0 candidate, DataConfig/Datasave, UniTask, uGUI/TMP Essentials and InputSystem. Install these first, import this sample, disable a project-specific bootstrap play override, and open `Scenes/BattlePassDemo.unity`.

Enroll → win runs → open pass → claim Free → buy Premium for200gems → claim previous Premium tiers → Claim All. Toggle confirmation to compare preview-before-grant versus no panel. Replay Last Callback is idempotent. Advance Day persists demo time; Reload recreates feature/wallet/config owners and resumes pending work. Demo state persists under DreamyBattlePassDemo; remove that directory only when intentionally resetting this isolated demo.

`BattlePassFeatureInstaller` is the composition example. `BattlePassPanel` only renders/intents; the presenter handles feature requests. Source prefabs are authored by Unity Editor APIs; use Dreamy/Battle Pass/Samples/Build Demo after closing this demo scene to regenerate. Builder resolves its imported root dynamically and preserves the active scene. Scene owns PanelManager/EventSystem; load it standalone.

The sample includes its own shared-reward UI host built on Economy contracts, without requiring another sample import. RewardPanel supports both Confirm and Notice; this demo toggles single-slot Confirm versus direct claim. Claim All executes the explicit batch action directly. Item labels use demo English text; host replaces art/localization and resource catalog. No production IAP or Missions/Progression bootstrap is installed.


All seven shipped UI prefabs are Unity Prefab Variants. BattlePassPanel, BattlePassPremiumPopup and RewardPanel inherit `Packages/com.dreamy.feature/Runtime/Prefabs/BaseFeaturePanel.prefab`; BattlePassRewardItem, BattlePassTierItem, RewardItem and BattlePassEntry inherit `BaseFeatureItem.prefab`. The sample keeps the Feature package installed as the source of these assets. Decorative item variants disable their inherited root Button/raycast, while child reward buttons remain interactive.

The builder owns the generated sample layout: regenerate only when intentionally replacing that demo layout. Customize a further game-owned variant for game art. Builder fails clearly if either base is missing/incompatible, retains existing prefab GUIDs, saves connected variants and validates the source/bindings. `BattlePassPrefabValidator.Validate()` can be run separately after import.
