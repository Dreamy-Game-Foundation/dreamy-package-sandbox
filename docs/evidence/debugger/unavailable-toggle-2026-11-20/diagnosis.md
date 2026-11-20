# Unavailable God Mode toggle — 2026-11-20

Status: complete (source fix and Editor verification; not an Android build).

## Cause

GameInstaller.Cheats registers battle.god-mode.on with an availability predicate requiring a BattleController with CanCheatCompleteMatch: playing, active, not destroyed, match started, a definition present, not practice mode, and not GameOver. The game command registry also returns the same unavailable result for an unknown command, an executing registry or a disposed registry. The supplied stack alone does not identify which rejection happened at the original timestamp.

PlanetCheatOptions.Run converted that normal unsuccessful CheatResult into InvalidOperationException. Reflection wrapped it, the debugger registry propagated it as designed, and DataBoundControl.UpdateValue invoked the setter without a catch. The exception escaped the Toggle UnityEvent. The old UI also cached the requested value instead of reading the actual backend value, so a silently rejected write could leave an incorrect ON indication. Repeated stack frames alone do not establish multiple physical taps or recursion.

## Changes

The game adapter uses TryRun for God Mode and No Ads setters, retaining execution policy and LastResult diagnostics. Existing action confirmation/exception behavior is unchanged. The package contains unexpected setter errors at the input boundary, unwraps reflection wrappers for a warning and Unavailable badge, re-reads authoritative state on success/failure, and blocks write reentry. Bool state rendering uses SetIsOnWithoutNotify. Registry.Set/Execute are not made to swallow programmatic failures. No command gate, gameplay save, prefab reference or dependency was changed. Maintained sandbox package and embedded consumer copies match for all changed C# package files.

## Verification

Before fix: 4/4 new consumer cases failed with Command unavailable escaping through reflection, including battle.god-mode.on matching the report. After fix: 16/16 EditMode tests passed (consumer adapter + existing package registry checks), and 4/4 PlayMode tests passed (throwing setter recovery/rollback, rejected-write readback/no synthetic input, existing getter recovery and live values). See test-receipts.json and source-fingerprint.json. Console reported no errors after compilation/tests.

PlayMode startup initially ran the real game and timed out before any test callbacks. The project toolbar re-applied playModeStartScene=BootstrapScene; temporarily disabling its project-specific Play From Bootstrap preference allowed the isolated tests to run. The original preference/start scene and BootstrapScene were restored; the temporary scene was removed. These setup timeouts are not reported as test passes. An execute_code helper initially referenced an internal MCP type directly and failed in-memory compilation; recovery used the inspected reflection boundary without changing project source. The code tool also blocked asset deletion; the scoped manage_asset delete operation removed only the newly created temporary scene.

Dreamy lesson recall returned Invalid or oversized data file. The only local lesson was inspected as fallback and did not match this bug. The observed failure was recorded without promotion. Task validate passed as static inspection only, separately from Unity tests. Toolkit lessons record also pruned expired tracked observations; those tool-induced deletions were restored from HEAD because the initial working tree had no such deletions.

## Deployment limits

The package remains an uncommitted hotfix on top of 0.1.7; no version bump, commit, tag or push was requested for this fix. A new Android build from the fixed consumer is required. Physical touch, Crashlytics ingestion and original release APK behavior were not tested. God Mode still requires an active non-practice match to enable; there is no new pre-arm-at-Home behavior.

## User scope correction

After verification, the user requested that Planet Packages/ remain untouched and will update the package separately. Removed only the four task-owned embedded package edits (runtime controls, PlayMode tests and changelog), restoring their original tracked contents. The maintained sandbox package hotfix and game-owned PlanetCheatOptions/adapter tests remain. The successful PlayMode receipt above is historical proof of the tested hotfix, not certification of the consumer currently running the old package. Authoritative UI rollback and the generic setter boundary require the later package update.

## Package publication

The user subsequently requested release and publication of the maintained package/sandbox. The debugger hotfix was released as v0.1.8 at 1516f9fff4f62e7b3acb5f38b566509f5e47291b; main and annotated tag were pushed. Planet Packages/ remains unchanged as explicitly requested; that consumer update/rebuilt APK is still owned by the user. See ../release-v0.1.8/ for distinct fresh version/archive checks.
