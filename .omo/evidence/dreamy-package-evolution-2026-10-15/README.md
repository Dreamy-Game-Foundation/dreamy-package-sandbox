# Reward evolution execution — 2026-10-15

Target: sandbox, Unity 6000.4.12f1, Linux Editor via Unity MCP.
Base sandbox SHA: `06693e03e9c7b2a9a9805f51c931ea5b71cbf549`.
Base package SHAs and manifest/lock hashes: `baseline.json`.
Final runs include uncommitted Daily source changes; identify those inputs with
`verified-input-hashes.json` and the working-tree diff, not base SHA alone.

## Results

| Run | Suite | Passed / discovered | Failed | Skipped |
| --- | --- | ---: | ---: | ---: |
| Baseline EditMode | Economy | 41 / 41 | 0 | 0 |
| Baseline EditMode | Missions | 52 / 52 | 0 | 0 |
| Baseline EditMode | Shop | 8 / 8 | 0 | 0 |
| Baseline PlayMode | Daily | 38 / 38 | 0 | 0 |
| Baseline PlayMode | Progression | 21 / 21 | 0 | 0 |
| Final PlayMode | Daily | 42 / 42 | 0 | 0 |
| Final PlayMode | Progression | 21 / 21 | 0 | 0 |

Raw MCP results and test details are stored in `baseline-editmode.json`,
`baseline-playmode.json`, `final-playmode.json`. Per-suite counts above are grouped
from leaf results. These are separate runs, not one combined final-suite result.
Economy/Missions/Shop source did not change after their successful run.

The red run `daily-regressions-before.json` recorded 42 completed tests and four
new failing cases. MCP returned no final summary for that failed run, so its
progress/failure records are the evidence; no complete summary is reconstructed.

## Changes proved by regression

- Resolved Daily recovery no longer reconstructs the duplicate PendingClaim
  payload before using its authoritative journal. Invalid duplicate payload
  does not cause an exception or call the resolver again.
- Intent recovery rejects an invalid payload with InvalidJournal and a changed
  fingerprint with PayloadMismatch before resolver invocation or destination grant.
- Completed StopAfterLastDay schedules expose IsCompleted and mark all rows
  Claimed after model/store recreation. Existing constructor callers can omit
  the new optional view-state argument; no save schema field/version changed.
- Imported Daily panel displays completed/busy state. Source/imported panel and
  both sample READMEs match byte-for-byte. Missing imported Bundle README was
  added; Unity generated its `.meta`. Existing script/prefab GUIDs were preserved.
- README demonstrates composing a host-resolved Daily model with the existing
  feature installer overload that accepts a service; no new resolver framework.

## Actual sample smoke

`daily-sample-smoke.cs.txt` is the exact code executed through MCP in PlayMode.
`daily-sample-smoke.json` is the successful returned result.

An isolated unsaved empty scene loaded the actual imported Daily panel prefab.
The fixture used actual DatasaveService files in a unique /tmp directory,
DatasaveResourceWallet, DemoItemDestination, gateway, Daily model and presenter host.
The concrete two-entry StopAfterLastDay schedule was supplied in memory, without
changing the active sample Resources schedule or player saves.

- Actual claim button invocation produced Pending/DestinationUnavailable after
  committing 100 coins while the item destination was unavailable.
- Retry delivered one card and finalized the claim without a second currency grant.
- 100 presenter-host Show/Dispose + panel enable/disable cycles kept one bound
  claim listener while open and none after disposal, with a stable one-row count.
- Completed label was `All rewards claimed`; currency remained 100, cards 1.
- The temporary runtime objects and /tmp save directory were removed.
- Original clean Feedback scene and Play-from-Bootstrap preference were restored.
- Final Console query returned 0 error entries; Editor was idle in EditMode.

This does not exercise PanelManager Show/Hide transitions, Addressables, Home
navigation, device safe area, screenshot visual QA, native allocations or handles.
Do not close the broader 100-cycle lifetime gate based on this narrower smoke.

## MCP runner incidents

The first PlayMode attempt (`85654e798be64af0b92ba7136d5f7de8`) did not reach
test callbacks while the Bootstrap start-scene override loaded the game scenes.
The run was canceled/cleared; it is not a passing run. The Bootstrap preference
was temporarily disabled for tests and later restored.

Stopping that attempt left an orphaned MCP completion. Unity's own active runner
was inspected and canceled using its API. Only after confirming no active Unity
runner, its orphaned MCP completion was canceled and a domain reload released the
test operation. No package-cache source was edited.

The filtered red attempt (`36a493f9a01c4927a795fe01c0b92d8d`) timed out before
initialization. A full asset refresh/import was required after source edits;
scripts-only compile requests were insufficient to establish imported test changes
in this session. The subsequent whole-Daily run discovered all four new cases and
recorded their failures, then the final whole-Daily/Progression run passed.

## Remaining work

P0 crash/restart coverage is substantially exercised by existing suites, including
real filesystem codec/temp-write/backup tests in Economy. Post-commit notification
faults, broader corrupt context/journal cases and full cross-feature filesystem
crash matrices remain to audit. Snapshot compaction/schema changes are deferred.

Template and PB integration require their correct consumer checkpoints; the local
PB checkout observed earlier is main and no matching local migration ref was found.
No consumer main, unrelated repo, released tag, player save or stash was changed.
No production SDK/device/build, calibrated benchmark or rollout gate was run.
