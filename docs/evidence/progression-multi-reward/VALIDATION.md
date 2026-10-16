# Progression multi-reward validation

Date: 2026-10-16. Package working tree: Progression 0.2.0, Economy local 0.4.0.
Unity instance: dreamy-package-sandbox@5f78127b991a2b92, Unity 6000.4.12f1, Linux Editor.
No commit/tag/publish performed. This is implementation evidence for the local working tree.

## Results

| Gate | Result | Scope |
| --- | --- | --- |
| Managed runtime build | Passed, 0 warnings/errors | All Progression Runtime source; references installed Unity dependency assemblies |
| Managed sample build | Passed, 0 warnings/errors | Progression Rewards source and sample destinations |
| Managed tests build/run | 59 passed, 0 failed, 1 Unity-only fixture excluded | Current domain, config, migration, persistence failure, retry and presentation tests |
| Direct NUnit invocation in Unity Editor | 60 passed, 0 failed | Same Progression test methods plus actual Datasave disk-envelope migration fixture |
| Economy direct NUnit regression | 30 passed, 0 failed | RewardContractTests and RewardJournalTests; shared protocol unchanged |
| Imported sample compile/smoke | Passed | Temporary import; preview scene, sample components, isolated /tmp save directory |
| Unity Console | 0 error entries at final check | Compile after current Progression sources; other workspace work may change subsequently |
| Runtime boundary/diff | Passed | noEngineReferences retained; no UnityEngine/UnityEditor imports in Runtime; git diff --check |

Command to repeat the managed gate from sandbox root:

```sh
python3 LocalPackages/com.dreamy.feature.progression/Tests~/validate-progression.py
```

The managed harness compiles current Progression source, rather than running the previous
Progression DLL. Dependencies are generated Library/ScriptAssemblies and Unity plugin DLLs.
Direct Unity verification invoked NUnit Test/TestCase methods inside the selected Editor,
using fresh fixture instances and recording exceptions. It is not a Test Runner XML result
and does not exercise PlayMode scheduling or Unity coroutine lifecycle.

## Behaviors verified

- One grant crosses multiple levels; XP/level, grant ID and every milestone intent exist on disk
  before the first destination effect. Coin/Gem/Card/Chest/Cosmetic all delivered once.
- Card failure leaves earlier currency entries acknowledged. Restart recreates all owners,
  retries missing entries and leaves existing balances unchanged.
- Every coordinator write boundary tested with failures before write and after durable write:
  resolve, partial acknowledgements, all acknowledged, and Completed.
- Progression XP write and currency destination write tested before/after durable persistence.
  Model stops further mutation until reload; reward recovery never grants XP again.
- Duplicate XP IDs survive restart and more than the old 4096-entry retention limit.
- Independent milestones continue after non-persistence delivery errors; unknown kind blocked.
- Frozen pending payload survives reward config changes. Threshold changes gate advancement
  while allowing delivery from the saved snapshot.
- V1 preserves reached-level baseline and IDs. Future legacy effects require explicit host
  reconciliation: NotDelivered, AlreadyDelivered, Unverifiable all tested across restart.
- Disk fixture loads a v1 Datasave envelope, migrates to v2, saves a pending journal and reloads.
- Invalid/modified journals and payloads are blocked without duplicate destination mutation.
- Legacy no-reward defaults (`rewardAmount:0`, empty/null resource ID) remain accepted.
- Listeners throwing and recursive advance/delivery calls do not invalidate committed effects.
- Reward presenter binds, queries state, unbinds and binds again.

## Sample smoke

Imported complete `Samples~/Progression Rewards` into temporary
`Assets/_ProgressionRewardsValidation`, compiled it, and used a preview scene without saving
user scenes. Set sample saveDirectory to `/tmp/dreamy-progression-sample-validation`.

Sequence: Card failure → grant 300 XP → retry → arm ack-save failure → grant 100 XP → reload
all owners → arm completion-save failure → grant 100 XP → reload all owners.

Final assertions: XP=500, Coin=400, all four reward milestones Delivered. Level 4 has no reward.
No duplicate Coin mutation after acknowledgement/completion failures. The temporary asset
import was removed through AssetDatabase tools; temporary save files were cleaned up.
The final change exposing a serialized saveDirectory field compiled and was included in smoke.

## Editor/Test Runner limitations

PlayMode Test Runner attempts did not initialize: initially Bootstrap playModeStartScene caused
gameplay scene startup; removing that setting temporarily still resulted in timeout. No
PlayMode suite pass is claimed. The temporary start-scene change was restored to
`Assets/_Project/Scenes/BootstrapScene.unity`. Editor exited PlayMode and the original
ConnectivityDiagnostics scene was reopened. No production save was touched.

During concurrent Connectivity edits, one refresh observed an unrelated missing
ConnectivityFeatureInstaller compile error. It subsequently cleared; no Connectivity source
was changed by this Progression work.

No device/player build, cloud merge, cross-generation multi-file restore, production legacy
receipt audit or UI interaction/raycast pass was run. The sample UI is diagnostic IMGUI with
Screen.safeArea, not a production uGUI prefab. Strict delivery requires host destinations to
persist effects and fingerprint receipts atomically within their owner document. V1 truncated
history cannot be reconstructed; host reconciliation remains intentionally required.

Source hashes for runtime/tests/sample/config are recorded in source-sha256.json.
