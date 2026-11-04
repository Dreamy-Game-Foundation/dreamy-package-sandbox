# Project cheat setup — complete implementation checkpoint

Date: 2026-11-04. Unity 6000.4.12f1. Dreamy Debugger 0.2.0-preview.4.

## Ownership and behavior

- Reusable delegate-bound resource and ownership templates live in debugger Samples~/QuickStart/BoundCheatExamples.cs and the editable imported Quick Start.
- Game catalog, wallet mutations, registration host, config and prefab variant live under Assets/_Project/Debugger. Runtime package has no Core/Economy/Shop/game dependency.
- BootstrapScene now uses one SandboxDebugger variant. Standalone DebuggerDemo remains on its in-memory sample host. Scene installation was repeated without duplicates, with no missing scripts. Existing project configuration values were cloned into project-owned configuration.
- The game registers 27 options after GameInstaller Ready; with 3 debugger built-ins the live panel has 30. Scope disable removes the game group (3 remain), re-enable registers it once (30).
- Resources: Coin, Gem, Knight cards, Silver chests, Daily reward cards and Red hero skin. Ownership: No Ads, Lifetime VIP, Starter Fund, Season 1 Premium Pass.
- Grants use wallet transactions, persistence and BalanceChanged. Numeric grants have limits/confirmation. Ownership is grant-only and skips already owned entitlements. No store receipts are fabricated.

## Fresh verification

- 6/6 project EditMode tests passed with isolated temporary on-disk saves: resource persistence/events, unique transaction IDs, inventory, entitlement persistence/effect once, locked/unconfirmed/out-of-range rejection, disposal and rejected wallet feedback.
- 9/9 package UI PlayMode tests passed in the clean consumer. New test checks actual Int64 controls, range metadata, exact text/arrows beyond 2^53, long.MaxValue, invalid input rollback, and disposal while Options remains open.
- Live main game: unlocked through PIN keypad; Add Coins UI button opened confirmation without mutating wallet; Confirm changed 9700 → 9825 (+125). Restarted Play session retained 9825. This deliberate grant remains in the local game save.
- Final numeric UI smoke: 10 Int64 controls, Coin grant amount input/readback 125, range 1–1000000, Coin balance display 9825; no fake ProjectCheatHost in the game scene. Live Console read returned zero errors after corrected controls.
- No Ads live entitlement was left unchanged (false). Its persistent grant and existing EntitlementEffectBinding were tested on an isolated wallet. GameInstaller.ApplyNoAdsDemo currently logs a demo effect; no ad SDK exists in this project.
- No new platform/device builds or live ad/store checks were run for this scoped change. Earlier preview builds are historical receipts, not fresh acceptance of preview.4.

## Observed failures corrected

- Original numeric control did not bind Int64 values; control/type ranges and 20-character input were added. Exact Int64 uses text/arrows, disabling the legacy double-based horizontal drag path.
- Sequential per-option removal rebuilt stale registry getters when a scope was disposed while UI was open. The bridge now swaps one owned container at a time.
- Invalid numeric input reverted to an old cached display. Successful edits now synchronize the display with the actual getter before the next edit.
- Initial targeted UI runs exposed those failures; only the final nonzero 9/9 receipt is accepted. A locked-session tool read was rejected as expected during focus changes; the final read followed a fresh PIN unlock.

Installed docs/agents/dreamy-workflow.md, compatibility catalog and dreamy CLI were unavailable. Verified installed package/API facts are recorded in compatibility.json; source fingerprints and exact test results are kept beside this checkpoint. No broad generated-font changes are included in this task.
