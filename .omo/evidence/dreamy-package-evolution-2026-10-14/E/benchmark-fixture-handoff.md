# Task 22 Editor benchmark fixture

Authored `Assets/_Project/Editor/RewardEvolutionBenchmark.cs` and its GUID-preserving sidecar for this new script. This document is a fixture handoff, not a measured baseline. No Unity action, benchmark execution, network request, commit, or runtime/package edit was performed by the author.

## Exact entry points and root execution

```csharp
Dreamy.Template.Editor.RewardEvolutionBenchmark.Start(
    ".omo/evidence/dreamy-package-evolution-2026-10-14/E/baseline-editor-domain.json",
    "<caller-supplied source/environment identity>");

Dreamy.Template.Editor.RewardEvolutionBenchmark.Cancel();
```

`Start(string outputPath, string sourceIdentity)` returns after registering the callback. The path must be a fresh `.json` inside this project's `.omo`; existing result files are rejected. Relative paths resolve from the Unity project root. Supply package SHAs and the current dirty-source diff/content hashes, fixture identity, and caller-controlled profiling/environment conditions. HEAD alone is not the identity of dirty source. The fixture records this string verbatim, plus Unity/runtime versions, OS, CPU, processor count, active build target, and start/end UTC timestamps. The business clock is separately fixed at `2026-10-14T12:00:00Z`.

1. Ensure the containing `Dreamy.Template.Editor` asmdef has **direct** references to `Dreamy.Missions.Runtime`, `Dreamy.Datasave.Runtime`, and `Dreamy.Economy.Runtime`. The inspected asmdef references integration assemblies, not these runtime assemblies. This author was prohibited from editing asmdefs. Newtonsoft.Json is already installed; the containing asmdef currently has `overrideReferences` disabled. The script otherwise uses only framework, UnityEditor, and UnityEngine APIs. No new dependency or runtime helper is needed.
2. Import/compile through the real Unity MCP and inspect Console errors. The author could not obtain C# LSP diagnostics because the LSP daemon did not become reachable; compilation is unverified.
3. Create the output parent directory, then arm a **file-create monitor before calling Start**. Use a fresh path for each run. Finish publication writes a sibling `.tmp` then moves it to the selected final JSON path, so the monitor sees the complete document.
4. Call `Start` once in Edit Mode through real Unity MCP. Keep Unity available for Editor updates, without source edits/reloads during measurement. Do not wrap Start in a blocking poll or giant benchmark MCP command.
5. On the file event, inspect `Success`, `Failure`, all three `Sizes[].Correct`, diagnostics, executed/warmup counters, category metrics, and growth snapshots. Success requires 30000 completed measured events and 60 separate warmup events. Errors/cancellation publish `Success=false` and log an Editor error. Result-publication failure logs and throws after callback cleanup; absence of a result is not success.
6. If needed, call `Cancel()`. It unsubscribes update, before-reload, and quit handlers and drops the static session before publishing a failure result. Reload and Editor quit automatically take this path.

Expected successful scheduling is 3768 Editor update callbacks: per size, one setup, three warmup chunks, one fresh-model reset, 1250 measured chunks, and one final audit. Each business chunk contains at most eight operations. Setup, growth snapshot, and final audit phases are separate updates; individual synchronous domain operations are not interruptible. Wall runtime has not been measured and is not promised: it depends on Editor scheduling and the growing serialized state, potentially minutes. No elapsed-time success condition, delay, sleep, or external polling loop exists.

## Deterministic content and independent oracle

Catalogs contain 16, 100, and 1000 missions; half additive and half distinct-value, with one matching mission per event key. All targets are 100000, preventing saturation. Daily fixed UTC windows retain the full periodic replay ledger. No selection, reroll, claim, or reward delivery occurs. Both clock and RNG are injected; the model RNG must consume zero calls on this path.

The event generator uses seed `220014`, unsigned LCG `1664525*x+1013904223`, and its high 24 bits modulo each bound. Each of 1000 groups contains exactly these ten slots:

| Slot | Content | Expected status | Save delta |
| --- | --- | --- | --- |
| 0 | Unique additive ID, amount 1..5 | Updated | 1 |
| 1 | Immediate replay of slot 0 | AlreadyProcessed | 0 |
| 2 | Distinct objective, value drawn from 32 values | Updated | 1 |
| 3 | Same distinct value as slot 2, different ID | Updated | 1 |
| 4 | Replay of slot 2 | AlreadyProcessed | 0 |
| 5 | Unknown key | NoMatchingMission | 0 |
| 6 | Zero amount | InvalidEvent | 0 |
| 7 | Wrong window | WrongWindow | 0 |
| 8 | Second unique additive ID, independently drawn mission, same amount as slot 0 | Updated | 1 |
| 9 | Replay of the first group's slot 0, including late in the stream | AlreadyProcessed | 0 |

Current `MissionModel.ReportProgress` persists a new matching event ID even when a distinct value is already present. Slot 3 is therefore a **ledger-only mutation**, not a true no-op. Slot 2 may also revisit a value from a prior group. Each category executes exactly 1000 operations per size: 4000 saves/notifications, 3000 replay rejections, and 3000 true no-op rejections.

The oracle uses arithmetic arrays and per-mission `HashSet<string>` values constructed from event generation, not results returned by the model or its state/identity helpers. It manually length-prefixes expected persisted IDs, checks the exact ledger set **and order**, final per-mission progress/distinct membership, no claimed flags, legacy save mirrors, every operation's status/save/notification delta, and reconstruction from the serialized save. Restored `GetState` must match expected progress without another save. Any mismatch remains in JSON diagnostics and prevents success.

A separate JavaScript unsigned-LCG/set calculation, not a Unity run, yielded these expected totals:

| Catalog size | Additive total | Distinct total | Total progress | Saves / ledger IDs |
| --- | --- | --- | --- | --- |
| 16 | 6138 | 250 | 6388 | 4000 / 4000 |
| 100 | 6138 | 753 | 6891 | 4000 / 4000 |
| 1000 | 6138 | 967 | 7105 | 4000 / 4000 |

The result records event-content and catalog SHA256 hashes for each size. Check these remain identical for before/after comparisons.

## Measurement boundaries

Each measured `ReportProgress` call is bracketed by `Stopwatch.GetTimestamp()` and `GC.GetAllocatedBytesForCurrentThread()`. Raw ticks, frequency, allocated bytes, operation count, and save deltas are reported per category and per size. Tick intervals measure main-thread managed-operation elapsed duration, not exclusive process CPU time, and retain timer-sampling overhead. Editor scheduling gaps are excluded; runtime GC during an operation is included. Category rows separately expose replay and each true-no-op cost.

`SerializedStore` implements the inspected `IDatasaveService` contract with detached JSON serialization and UTF8 byte counting **in memory**. Save serialization is inside the operation measurements. Cumulative serialized bytes, peak serialized save size, retained save UTF8 size, and retained UTF16 string code units are reported. UTF8 size is a representation size, not an actual managed-heap retained-memory measurement. It is **not** real filesystem DataSave IO, encryption, compression, backup, disk throughput, or production durability.

Snapshots at 0 and every 1000 events record ledger/distinct/mission entry growth and retained save size. Final public-state JSON size is measured outside the operation interval. Setup, 20 warmup events on a discarded same-size model, fresh-model reset, oracle construction/checks, growth snapshots, restoration, state serialization, and final result-file IO are excluded from operation metrics.

This fixture does not supply UI rebuild, UI lifetime/open-close, Profiler captures, GPU, mobile/device, or physical filesystem measurements. Those remain root-owned task 22/task 24 evidence tracks. No budget or improvement percentage has been invented.

## Author checks

The required Editor tooling, profiling, and testing skills and current MissionModel, catalog/policy/state/save, IDatasaveService, SaveData, wallet, and reward identity contracts were read. The full fixture was reviewed statically; the new meta GUID has only one match in Assets. LSP diagnostics failed because its daemon was unreachable. Unity compile and benchmark execution are explicitly pending with the root; there are no measured performance results in this document.
