# Missions batch API: source candidate verification

Extended validation is recorded in [the follow-up report](extended-validation.md): final package tests **90/90**, minimum-dependency Unity tests **30/30**, real persistence fault injection and a successful Android ARM64 IL2CPP probe APK. The migrated consumer passed **103/103** on the isolated candidate. Physical-device execution remains unavailable.

Date: 2026-11-17. Implementation is in source submodule `LocalPackages/com.dreamy.feature.missions`, based on HEAD `2474d139cd2c9ca3d1667296be1cd30d579fb7bd` (v0.3.1). No release version/tag, consumer pin, immutable package or cache edit was made.

The complete source change is also exported as [an upstream patch](../../patches/missions-batch-api-source-v0.3.1.patch). `git apply --check --cached` passed against an isolated temporary index loaded from that base commit. [Source and dependency hashes](source-inputs.json) bind the patch to the tested inputs; no user Git index was changed.

## Implemented

- Immutable MissionProgressEvent and optional IMissionBatchProgress; IMissionService and existing single-event signatures remain unchanged.
- Input-ordered results, one clock/period evaluation for structurally valid batches, one lazy detached candidate, at most one Save/StateChanged including rollover.
- Ordinal receipt HashSet checks persisted IDs and all earlier accepted IDs in the batch, including receipts evicted by Permanent historyLimit during that call. Cross-call bounded retention remains explicit.
- Save before publication. Any Save exception sets RequiresReload and prevents mutation of the stale instance. Read-only snapshots remain available; reconstruct with authoritative persistence before replaying original IDs. Subscriber failure after commit does not mark persistence unknown.
- Audited schema-v2 copies detach all mutable progress, policies, containers and claim journals. Immutable definitions/bundles may be shared. No JSON round-trip in hot progress candidate/snapshot copies; persistence serialization and cold configuration copies remain.
- Documentation: `LocalPackages/com.dreamy.feature.missions/Documentation~/BatchProgress.md` includes failure/retry and consumer migration contracts.

## Verification

Unity MCP targeted `dreamy-package-sandbox@5f78127b991a2b92`, Unity 6000.4.12f1, Linux Editor, active build target Android. It did not alter scene/prefab state. Baseline before implementation passed 59/59 tests in Dreamy.Missions.Runtime.Tests and Dreamy.Missions.RewardPresentation.Tests.

Final job `6caa37f56a9d4479a1d17eabc8444bd6` passed **82/82 EditMode tests**, zero skipped/failed, 5.830 seconds test duration. See [full result and benchmark outputs](unity-editmode.json). Console had zero error entries after the final compile/test run. Runtime asmdef direct Editor-reference check [passed](runtime-asmdef.json); asmdefs and dependency manifests were unchanged. Diff whitespace checks passed.

Coverage includes sequential/batch equivalence, mixed ordered statuses, namespace/raw-ID encoding, duplicate IDs in/across batches and reload, Permanent eviction, max-long caps, completed/claimed receipts, distinct values, window rollover/rollback, one clock read, empty/all-invalid/no-op cases, save-before/save-after-write exceptions, reload failure, stale-instance rejection, save/subscriber reentry, subscriber exceptions after publication, copy isolation and pending strict reward recovery. Existing claim/reward-presentation regressions remained in the run.

Four existing failure-retry tests now reconstruct after Save errors to match the explicit RequiresReload contract. They still assert unchanged published state and no consumed event/claim/reroll on before-write failure. Two additional stale-instance retries were discovered by the first expanded run (76/78 passed); they were corrected to reload and passed in subsequent runs. No production logic was weakened to satisfy them.

## Allocation and commit measurements

[Raw .NET samples](allocations-net10.jsonl) compile the actual model and dependency source with .NET SDK 10.0.115, runtime .NET 10.0.15, X64, Ubuntu 26.04.1. Newtonsoft.Json is the consumer's DLL, assembly version 13.0.0.0. Matrix: 4,096/50,000 existing receipts; 30/60/100 shots; one/three events per shot; additive plus distinct objective; eight completed claim journals; reference storage and JSON serialization; three samples/case. Fixture/constructor/load/input setup is outside the measured region. Reproduction lives in `LocalPackages/com.dreamy.feature.missions/Benchmarks~`.

Selected medians with 50,000 receipts (decimal MB, not MiB):

| Workload | Store | Sequential allocated MB | Batch allocated MB | Sequential / batch commits |
| --- | --- | ---: | ---: | ---: |
| 30 shots × 1 event | reference | 49.73 | 2.72 | 30 / 1 |
| 60 shots × 1 event | reference | 99.52 | 2.74 | 60 / 1 |
| 100 shots × 1 event | reference | 165.99 | 2.76 | 100 / 1 |
| 100 shots × 3 events | reference | 499.87 | 2.88 | 300 / 1 |
| 100 shots × 3 events | JSON serialization | 2,121.90 | 8.38 | 300 / 1 |

These are .NET managed allocation measurements, not Unity Mono/IL2CPP/device allocations. Unity Mono returned zero for GC.GetAllocatedBytesForCurrentThread despite known allocations; those measurements were rejected and the Unity tests report allocation unsupported. A probe of GC.Alloc ProfilerRecorder reported nanoseconds rather than bytes and was also not used for allocation claims.

Unity tests separately exercised **real Datasave serialization, envelope, backup and filesystem writes** in unique disposable temporary directories. Recorded single-run Editor times for 50,000 existing receipts:

| Events | Sequential ms | Batch ms | Sequential / batch commits |
| --- | ---: | ---: | ---: |
| 30 | 496.286 | 16.946 | 30 / 1 |
| 100 | 1,702.290 | 52.104 | 100 / 1 |

These are local Editor observations, not latency guarantees. The real-store fixture includes additive/distinct objectives; the larger completed-claim history is covered by the .NET allocation fixture. Batch progress with a pending claim is separately covered by the strict gateway regression.

## Other checks and recorded limitations

- Toolkit `task verify --command validate` failed on an unchanged third-party file: `Assets/Packages/StompyRobot/SRF/StompyRobot.SRF.asmdef` contains a BOM rejected by the toolkit JSON parser. This is not a successful static project receipt and was not changed as part of this task.
- Toolkit `package-check` given the source package directory expected a Unity project manifest/lock/ProjectVersion and failed. [Its raw output](package-check.json) is retained; it is not a package compatibility pass.
- Lesson recall returned no matching items. Explicit lesson recording failed inside existing toolkit observation pruning with `Cannot read properties of undefined (reading 'localeCompare')`; failure inputs remain in `.dreamy/inputs/`. Existing knowledge was not edited or deleted, and no lesson was marked verified.
- Initial script-only refresh compiled the modified model before discovering the new interface file (CS0246); an asset refresh imported the new scripts, after which the type was verified in the Editor and compile/test checks passed. No Editor package was patched.
- Compatibility catalog has no Missions record. Dependency sources were inspected and source-bound hashes are captured in `source-inputs.json`; a passing source test suite is not an upstream release compatibility declaration.

## Release and consumer gates

The original draft was found at `/run/media/tutrinh/NewData/planet-bunker-tu-code/docs/patches/missions-batch-api.patch`, based on v0.2.0 commit `1e1b4b57b738a83d7325ee229a80852c397c739e`. It was reviewed, not applied. It calls committing AdvanceWindow before batch commit, removes evicted IDs from the temporary set and retains JSON round-trip copies; this candidate addresses those gaps without replacing the newer reward-presentation API.

The consumer at `/run/media/tutrinh/NewData/planet-bunker-tu-code` is still pinned to Missions v0.2.0. It was inspected read-only. This source candidate is based on v0.3.1, so release selection and dependency compatibility must be reviewed before adoption. Version/changelog/tag/publication and consumer migration remain pending. Required consumer regressions include Quest Daily/Weekly partial saves, original-window retry after rollover, and XP partial saves; they were not run during the initial stage; the later isolated migration run passed **103/103**, as recorded in [extended validation](extended-validation.md). The existing public-DTO adapter and host invalidation remain intact until that gate passes.

Source implementation and local verification are complete. No published API or completed consumer migration is claimed.
