# Options workflow — 0.3.0-preview.3

2026-11-05, Unity 6000.4.12f1 Linux. Authorized follow-up to the SR Options review. No commit/tag/push/publication or live destination delivery.

## Changes

1. `runtime.RegisterCheats(object container, string prefix)` owns a disposable attributed registration, notification subscription and shutdown cleanup. Engine owns the convenience API; registry stays independent of Unity/Core/Editor. `registry.RegisterContainer` is available for standalone integrations with explicit caller disposal/tick.
2. `INotifyPropertyChanged` queues a coalesced refresh, including events raised by worker threads. Runtime drains on its main-thread tick. Reflection still happens at registration; bounded polling remains fallback. Notifications currently refresh the binding set, not just a single property.
3. Plain reflected actions run immediately after registry authorization. `[CheatConfirmation]` opts into the dialog. Resource/ownership card grants always require confirmation; typed Button confirmation remains unchanged. Default sample reset/grant explicitly opt in.
4. Structural registry changes reconcile on the next owner tick. Bridge keeps surviving definition identity and publishes batched dynamic deltas. UI keeps surviving controls/cards/category state, input/caret and pins; it only recomposes a card when its role membership changes. Stale bindings reject removed/replaced owners, including reused IDs.

Host/sample/README/authoring/migration docs updated. Existing IDs, class/assembly ownership and original serialized GUIDs retained. No UI prefab authoring needed for these changes.

## Verification

- **85/85 source EditMode +41/41 source PlayMode** in isolated package consumer.
- **85/85 archive EditMode +41/41 archive PlayMode** in a fresh consumer of the exported tgz.
- **10/10 project EditMode** in a copy of the sandbox Assets/ProjectSettings with local dependencies resolved to their verified source paths. Main Editor is not used by the test harness.
- Consumer Linux development/release and Android ARM64 IL2CPP build receipts/hashes are in verification.json and builds/.
- Read [verification.json](verification.json) for exact XML/hash/source/archive checks and artifact location. Earlier preview.2 receipts are historical.

Regression cases cover coalesced worker notifications, lease disposal and runtime shutdown unsubscribe, duplicate rollback, default/explicit/card confirmation, same-ID owner identity, nested structural batches, input focus/caret retention, card/definition/pin retention, card completion/fallback and a 20-registration burst producing one structural notification per tick. Existing authorization, stale confirmation, binding recovery, PIN and large-catalog tests still pass.

## Scope and limits

The target sandbox MCP instance was unavailable. A read/refresh initially routed to the other connected project before this was detected; no scene/prefab/game-data mutation or tests were performed there. Further verification used isolated consumers. Direct batchmode against the main project correctly refused because its Editor was open; it was not forced closed. Main live Editor state is therefore not certified by MCP in this receipt.

The first full project clone import failed with package filesystem quota error -122. Only owned disposable Library caches from completed older consumers were removed, then clone import/tests passed. A wrong-working-directory npm pack attempt produced no archive; the final pack uses an explicit package path.

Source C#/Markdown/JSON diff checks and documentation relative links/fences pass. Whole serialized YAML whitespace remains Unity-owned, as recorded in earlier UI receipts. No game scene/config/prefab edit is part of this task. No real wallet save grant or Telegram/email delivery was performed by verification.

Consumer builds certify package/sample compile and linking, not on-device behavior or the specific game container's IL2CPP runtime. Real mobile input/DPI/rotation/IME, GPU capture/native faults, p95/GC/thermal and iOS remain open. No claim of lag-free behavior or complete SR feature parity is made.
