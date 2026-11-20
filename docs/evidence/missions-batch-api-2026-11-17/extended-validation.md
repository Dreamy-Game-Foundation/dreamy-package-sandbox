# Extended Missions batch validation

This follows the initial 82-test source candidate report. The verified source was subsequently released as [v0.4.0](https://github.com/Dreamy-Game-Foundation/com.dreamy.feature.missions/tree/v0.4.0), commit `9d146b0c955242ed816137f63c267aa1d1d7c7e1`; main and the annotated tag were pushed atomically and verified remotely. The actual consumer pin was not changed. User confirmed that no Android device is available; device smoke/profiling is explicitly not run.

## Concrete persistence failures

Added four Runtime cases covering Permanent/Daily and failures before/after a real write. Before-write injection throws from the actual Datasave codec before AtomicWrite. After-write injection delegates to a concrete DatasaveService and throws an acknowledgement failure after its Save completes. Tests discard the old service, construct a new concrete service from the actual filesystem envelope, inspect persisted counters/distinct values, reconstruct the model and replay the original IDs twice. They verify stale-instance mutation rejection, no premature publication/notification, backup existence and no leftover temporary file.

The expanded original package run passed 86/86 via MCP, job `0e571b255c154880a7891d819aefec11`; [receipt](unity-editmode-extended.json). Four additional deterministic randomized cases compare public sequential/batch statuses and entire saved state over 20 variable-sized chunks with reload every third chunk. They run in the minimum-dependency Unity host below. The subsequent MCP package job was interrupted by a disconnected plugin/restart; it is not counted as passed evidence. After confirming that no Editor was open on the sandbox, a final batchmode run of both package assemblies passed **90/90**, zero failed/skipped: [NUnit XML](package-final.xml). This receipt includes all four randomized cases with the current source.

## Minimum dependency validation

Inspected and exported actual dependency source revisions into an isolated validation directory, without modifying dependency worktrees. [Exact commits and versions](minimum-dependencies.json): DataConfig 0.2.0, Datasave 0.2.0, Feature 0.2.0, Economy 0.7.0 source commit `1373c1d0a68e3f1310c11e0212f505fcfb1c21e8`. Economy v0.7.0 has no remote tag, so this is evidence for that source commit/version, not an installable tag declaration.

The engine-independent model [build passed](minimum-dependency-build.log) and completed 144 allocation/transaction benchmark samples using those sources: [raw output](minimum-dependency-runtime.jsonl). Existing warning CS0649 is the JSON-populated private catalogId field.

A separate Unity 6000.4.12f1 host compiles the actual model/config/contracts/persistence and relevant minimum-dependency source files. No model stub is used. Its assembly names differ from package asmdefs, so it proves source/API/runtime compatibility, not the full minimum-version UPM dependency graph. All **30/30 batch tests passed**, including the four randomized cases and real filesystem fault cases: [NUnit XML](minimum-unity.xml), [source hashes](minimum-unity-inputs.json).

## Android ARM64 IL2CPP build

The same minimum-source host builds a dedicated probe scene referencing the batch model and concrete Datasave. BuildPipeline completed **Succeeded, zero errors, zero warnings**. The APK contains `lib/arm64-v8a/libil2cpp.so`; APK signature verification passed v2. See [build result and SHA-256](android-build-result.json) and [signature check](android-apk-signature.txt).

APK: `/run/media/tutrinh/NewData/.missions-batch-validation/artifacts/missions-batch-minimum-arm64.apk`. This is a development validation artifact, not a consumer/store release. The included runtime probe reports 30 events, reopens a new DatasaveService/model, replays IDs and expects count 30. Its runtime result is **not observed** because no device is available. A successful IL2CPP build does not prove Android execution or allocation behavior.

## Isolated migrated consumer

Created a separate filesystem copy of the current Planet Bunker consumer at `/run/media/tutrinh/NewData/.missions-batch-validation/consumer`; existing uncommitted source was included without editing or reverting the original worktree. The original consumer stays on Missions v0.2.0. The validation copy uses a source snapshot of the new Missions API through `ValidationPackages` and translates QuestProgressEvent to MissionProgressEvent in the Quest/XP owners. It removes pre-batch Refresh and unconditional successful invalidation; model access reconstructs instances whose RequiresReload flag is set. Forwarding duplicate Quest events to XP is retained.

The existing public-DTO adapter remains available for its own tests; Quest/XP production paths in the validation copy call the upstream batch API directly. [Experimental migration patch](consumer-migration-candidate.patch) and [consumer source/test hashes](consumer-candidate-inputs.json) document this exact candidate.

Baseline on the original pinned consumer passed **103/103** related tests via MCP: [receipt](consumer-baseline.json). The selected groups are BattleQuestProgressTests, AccountProgressionIntegrationTests, QuestServiceStrictTests and MissionProgressBatchTests. They include Daily/Weekly partial-save retry, original captured windows after rollover and XP save failures before/after write. Migrated results are recorded separately once available.

## Restart and harness failures

Orca restart interrupted the first consumer import and Android build before a final result; processes were checked before restarting only those incomplete operations. Their logs remain suffixed `restart-interrupted`. The completed minimum-dependency tests were not rerun needlessly. Two validation-host setup compile errors (BuildReport count types and omitted SaveEnvelope source) were corrected; successful receipts above correspond to the corrected host. No production runtime fix was hidden in those setup corrections.

The original toolkit static project-inspect BOM limitation and lesson-observation pruning failure remain as recorded in the initial report. They are not relabeled as passes.

## Final migrated results and discovered recovery issue

The first migrated run passed 97/103 and exposed six claim-recovery failures: [original failure XML](consumer-migrated-first-failure.xml). Automatically reconstructing a model while rendering the result of a failed claim could immediately resume delivery again, and recovery during construction could fail before a new owner existed. The migration now renders an existing read-only snapshot while reload is required, and handles construction-time recovery failure as an unconfirmed claim rather than retrying again in that response. Mutable operations still reconstruct before retrying.

The source API now throws `MissionReloadRequiredException`, an `InvalidOperationException` subtype, when a tainted instance is used or construction-time automatic recovery cannot confirm persistence. Original Save exceptions still propagate unchanged where they did previously. Hosts can catch the specific recovery condition without suppressing unrelated configuration failures. Tests assert this dedicated exception for stale-instance mutations. This is included in the refreshed upstream patch and documentation.

After the fix, **103/103 migrated-consumer tests passed**, zero skipped/failed: [final XML](consumer-migrated.xml). Consumer assertions were not weakened or changed; the same 103 selected tests passed on the pinned baseline. The final full source package rerun passed **90/90**, and the minimum-dependency host rerun passed **30/30** after the exception change. Refer to each XML's timestamps and source-hash records for the matching run. The incremental Android rebuild after that source change also succeeded with zero errors/warnings; its final result is stored in [android-build-result.json](android-build-result.json). The final APK signature check passed. Source parity checks confirmed that the consumer snapshot and minimum host contain the exact current Missions runtime source. [Final machine-readable summary](final-summary.json).

Completed validation does not imply that the actual consumer has been repinned. Only the isolated migration candidate changed. Upstream version/changelog/tag were published after these checks; real consumer adoption remains pending, with physical-device smoke/profiling not run because the user has no device available.
