# Foundation feature demos

## Goal

Extend FoundationDemoPanel so all installed Dreamy feature packages can be demonstrated from BootstrapScene. This document is a plan only; implementation has not started.

## Verified baseline

- Panel: Assets/_Project/Scripts/Demo/FoundationDemoPanel.cs and Assets/_Project/Prefabs/Panel/FoundationDemoPanel.prefab.
- Host: Assets/_Project/Scripts/Demo/FoundationDemoRoot.cs; host prefab: Assets/_Project/Prefabs/Demo/FoundationDemo.prefab.
- Bootstrap: Assets/_Project/Scripts/Bootstrap/GameInstaller.cs.
- Shop, Daily Reward, Settings and Rate Us already have panel buttons.
- Tutorial is connected through FoundationTutorialAdapter.cs and starts automatically; there is no manual demo button.
- Missions, Progression and Lucky Wheel are installed in manifest/lock but are not wired into Foundation.
- Imported samples include MissionPanel, ProgressionHud and LuckyWheelPanel. ProgressionHud is a MonoBehaviour rather than a UIPanel.
- LuckyWheelSampleController creates private in-memory save/wallet services; it should not be the controller for the integrated demo.
- GameInstaller already owns shared DataConfig, Datasave and a DatasaveResourceWallet, with starting currency.coin balance 100.
- Progression sample is Sequential and rewards currency.gold; mission and wheel catalogs use currency.coin plus other resources.
- Unity MCP targets this project; Editor is idle in BootstrapScene. Baseline Console query returned no errors or warnings.
- toolkit.json and compatibility catalog were not found in the workspace file search. Implementation must use checked-out source and record this missing toolkit metadata; no maturity/support claims are implied.
- Existing user changes include imported Lucky Wheel/Settings assets and a modified Tutorial sample prefab. Preserve these changes.

## Buttons and demo behavior

| Button | Behavior |
| --- | --- |
| Shop / Daily Reward / Settings / Rate Us | Preserve existing demos and verify open/close navigation |
| Missions | Open mission list, simulate stage wins and coin collection, claim rewards |
| Progression | Open a project-owned panel containing ProgressionHud and an Advance Stage action for the current Sequential catalog |
| Lucky Wheel | Open wheel, spin and reveal reward, update the shared wallet |
| Tutorial Demo | Start a repeatable manual walkthrough using the existing target registry/overlay |
| Economy | Show wallet balances and allow a demo grant through the shared wallet; other feature rewards must appear here |

Economy is also demonstrated by Shop/Daily Reward/Missions/Progression/Wheel, but an explicit wallet panel makes its behavior directly accessible. The base com.dreamy.feature package is represented by these panels, not by an empty standalone demo.

## Implementation order

1. Capture Editor preflight and inspect prefab ownership, dirty scene state, actual sample asmdefs, catalog resources and close behavior. Check for duplicate Settings assemblies and Resources keys before changing anything. Read the exact package contracts used by the implementation.
2. Extend GameInstaller to register the Missions, Progression and Lucky Wheel configs before its existing DataConfig initialization. Install their services after the shared save/wallet/config services are ready; unregister/dispose owned services on teardown. Keep tutorial ownership in its existing adapter.
3. Use one Resources document per config key. Reuse existing imported catalogs initially. If customization is needed, move the catalog and its meta into project ownership instead of copying it to a second Resources path. Align the progression reward to currency.coin for the foundation demo, or show currency.gold explicitly in the wallet UI; preferred choice is currency.coin.
4. Add the runtime/sample assembly references needed by direct type usage to Assets/_Project/Scripts/Dreamy.Template.Runtime.asmdef. Keep every Editor reference in Editor-only assemblies.
5. Add project-owned feature demo host/adapters under Assets/_Project/Scripts/Demo. FoundationDemoPanel emits intents; the root/host owns panel creation, binding and cleanup. Use MissionPanel.Configure with the shared mission service; create a LuckyWheelPresenter against the shared wheel service; do not attach LuckyWheelSampleController.
6. Create project-owned ProgressionDemoPanel and EconomyDemoPanel scripts/prefabs. Progression wraps the existing HUD, calls the current mode's service action with a unique operation ID and displays the result/max-level state. Economy reads balances and grants a small coin amount through IResourceWallet, never by editing save data directly.
7. Add mission simulation controls in a project-owned wrapper/variant. Report stage.win and coin.collect using unique event IDs. Display progress and claim results; retry of the same logical event retains its ID.
8. Add manual tutorial entry to FoundationTutorialAdapter and connect it to the panel event. ITutorialService has no reset method: use a separate demo save key and a demo-owned in-memory save adapter for repeatable manual runs. Dispose the previous controller/overlay subscriptions before recreating the manual session; retain existing persistent automatic tutorial behavior. Prevent simultaneous automatic/manual walkthroughs.
9. Add Missions, Progression, Lucky Wheel, Tutorial Demo and Economy buttons to FoundationDemoPanel through Unity Editor APIs. Use a scrollable feature section or a grid that fits the current panel style. Preserve current Add Score/Open Shop objects used as tutorial targets.
10. Wire events in FoundationDemoRoot, prefab references in FoundationDemo.prefab and any necessary BootstrapScene overrides. Use one active feature demo at a time with consistent Close/Back return to Foundation. Prevent duplicate instances and repeated clicks during transitions. Unbind listeners and dispose presenters on close/destruction, including interrupted initialization and wheel reveal.
11. Save only intended assets, verify serialized references, inspect Addressables entry for the existing Foundation panel, and update README with button behavior and demo persistence/reset scope.

## Validation and acceptance

- Compile through Unity; compare Console against the baseline and require no new errors/warnings caused by this change.
- Validate all new serialized button/prefab references and runtime assembly references; run git diff --check on changed files.
- From BootstrapScene, open each demo, exercise its action, close/back to Foundation and reopen. Repeat clicks while opening: only one instance/action should result.
- Missions: simulate three stage wins, observe progress, claim once, verify wallet increases once; repeated claim must not increase it again.
- Progression: advance from level 1 to max, see HUD/reward changes, then verify max-level action does not grant another reward.
- Wheel: spin, observe the selected outcome/reveal, verify shared wallet grant once; exercise interruption and reopen/retry without duplicate reward.
- Economy: inspect starting/current balances, grant demo coins, verify Shop and reward demos share those balances. Show all resource IDs used by demo catalogs, not only coins.
- Tutorial: automatic flow remains usable; manual run can be repeated after completion and highlights Add Score/Open Shop correctly. Close/reopen and disable/destroy leave no orphan overlay or duplicate listeners.
- Existing Shop/Daily Reward/Settings/Rate Us demos remain operable. Demo platform actions stay simulated.
- Play Mode restart preserves normal shared feature state; manual tutorial repeatability does not delete user save data. Do not invoke DeleteAll.
- Inspect UI at the current Game View aspect and a narrow mobile aspect: every button is reachable, labels readable, scroll and Back work.
- Add focused automated lifecycle/idempotency checks only where new adapter logic warrants them; reuse package tests for unchanged domain behavior.

## Expected changed files

- Existing: FoundationDemoPanel.cs, FoundationDemoRoot.cs, FoundationTutorialAdapter.cs, GameInstaller.cs, Dreamy.Template.Runtime.asmdef, FoundationDemoPanel.prefab, FoundationDemo.prefab, README.md.
- New: project demo host/adapters, ProgressionDemoPanel, EconomyDemoPanel, mission wrapper/variant and project-owned demo prefabs under Assets/_Project.
- Conditional: project-owned catalogs, BootstrapScene overrides, Addressables metadata only if the chosen loading route requires them.
- Per implementation direction, reusable missing UI and demo controls live in package Samples~ and imported samples; project adapters own service wiring/navigation.

## Implementation outcome

Completed: five buttons, package sample panels/controls, bootstrap services, shared wallet wiring, repeatable tutorial, scrollable Foundation layout and Editor prefab builder. A Missions catalog JSON collision required a runtime serialization fix with a regression test. Missions suite passed 17/17 and all feature buttons were exercised in Editor Play Mode. See `.omo/evidence/foundation-feature-demos.md` for checks and limits.
