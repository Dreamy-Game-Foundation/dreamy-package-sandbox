# Automatic startup and persistent runtime preferences

Completed implementation and desktop/build acceptance on 2026-11-06, Unity 6000.4.12f1 Linux. Candidate **0.3.0-preview.4**, package commit **001b859**, not published. Approved plan: [phases and contracts](../../../plans/DREAMY_DEBUGGER_AUTOSTART_AND_PREFERENCES_2026-11-06.md). Package baseline d8711e7, sandbox baseline 8038fd2, plan commit 298ad16.

## Accepted phases

| Phase | Result and evidence |
|---|---|
| 0 — startup/build spike | Editor provider resolves catalog without runtime UnityEditor references. Empty-scene development player starts automatically; default release has zero debugger assets and no runtime. Temporary Resources payload is removed after successful and intentionally failed builds. |
| 1 — automatic/manual ownership | Automatic default needs no host/config/sample. Explicit Initialize is idempotent. Tests cover inactive/Off authored hosts, Manual loading, late manual handover without deleting user roots, disable/re-enable and two Play cycles with Domain/Scene Reload disabled. |
| 2 — HUD preferences | Async bounded file store, backup, per-path sequencer, 0.5 s debounce and local state merge. First-use Off; saved Compact restored in native player and actual sandbox Editor. Normalized free position, scale/memory/audio/drag and Reset are supported. Capture hide does not persist Off. |
| 3 — other presentation settings | FPS thresholds/colors/warnings, console severity/collapse, category collapse and pinned stable IDs. Late registered pins restore only after unlock. Actions/cheat values/security/report policy are excluded. Tests cover corruption, oversize, unknown schema, concurrent writers, delayed load and reset. |
| 4 — onboarding/migration | Settings window supports project-owned Automatic/Manual config. README/API/sample/migration/changelog updated. Existing meta identities, SR originals and shared game fonts unchanged. |
| 5 — consumer/build acceptance | Source and exact archive: 92 EditMode + 51 PlayMode passed each. Project integration: 10 EditMode passed in isolated project copy. Linux dev/release and Android ARM64 IL2CPP dev builds succeeded. Physical-device/iOS/production gates below remain open. |

See [verification.json](verification.json), XML receipts and per-target build receipts. `source-files.json` fingerprints every exported file. Archive SHA256: `c79f61595a236e6458f7c23f2b3eab7d6bf56f4471fb0d646d1d83ff8dea3ef4`. Archive was byte-compared with package source; all existing meta GUID files match the earlier baseline. All 659 protected SR/font files match.

## Reproduction and limits

Scripts here preserve the actual consumer paths/commands used. Source consumer references the local package; archive consumer starts from the exported tgz plus Unity Test Framework, with editable sample code imported for its own tests. Player build scene is empty, with **no sample or debugger host in the scene**. `StartupPlayerProbe.cs` is a consumer-only fixture, not shipped package code: first run starts Off, unlocks/opens panel and saves Compact; second executable launch restores Compact; release has no runtime. `StartupFailureProbe.cs` intentionally throws after staging, then verifies automatic recovery on subsequent Editor updates. ReleaseDiagnostics config-only payload is covered by Editor tests; its native player behavior has not been separately run.

Main MCP target was unavailable initially, then reconnected as `dreamy-package-sandbox@5f78127b991a2b92`. [MCP smoke](mcp-smoke.json) records the actual project: one SandboxDebugger owner, default PIN unlock, 50 options, Compact restored across Play, access locked again. HUD was restored to its original Off and Editor left stopped in clean BootstrapScene. No report-send API was invoked. A transport loss after Stop was recovered by restarting the cached local MCP server; no project/package connection configuration was changed.

Two concurrent batch PlayMode attempts were interrupted and ended with native Unity resource/mutex errors without XML receipts. They were not counted as passes. Fresh sequential reruns with `-job-worker-count 2` produced the passing source/archive XML above. An early candidate also exposed a manifest/runtime version mismatch; Version.cs was fixed and the archive rebuilt before the accepted runs. A build-config clone explicitly clears HideAndDontSave before becoming a build subasset.

Build figures are consumer build sizes, not an incremental package overhead measurement. Async storage and drag-end saves are mechanism checks, not proof of no lag on phones. No physical device p95/p99/GC/thermal/battery/capture benchmark, Android runtime install/touch/rotation/kill test, or iOS build was performed. Production screenshot disclosure/consent/redaction and deployed HTTPS relay remain separate gates. Gmail stays pending. Existing Audio/Editor Tools/lock/ProjectSettings changes were excluded from this work's commits.
