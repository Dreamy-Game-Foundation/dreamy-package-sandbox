# Gameplay, Missions and Progression integration

A puzzle run gets a durable unique RunId, even when replaying the same level. Commit its completion result and a Battle Pass XP outbox entry together in the gameplay save. After commit, send occurrence+RunId+XP. Acknowledge on Success or AlreadyApplied. Keep the original payload on retries; same ID with different XP is PayloadMismatch. Unknown/expired occurrences require an explicit host dead-letter policy, never remap old run XP into the current season.

Missions progress, player Progression and Battle Pass season XP are independent consumers. Fan out from committed gameplay completion; maintain separate consumer acknowledgements. Mission UI state changes and reward-panel animations must not produce XP. Optional bonus XP for a mission claim needs a committed mission-claim outbox event with its own stable ID; it is not enabled by this package.

The demo's BattlePassCompletionOutbox is an executable single-consumer example. For a full game, embed that consumer record alongside the game's own durable run completion and other consumer cursors. Do not create a separate save write after winning and claim it is atomic with gameplay.

Reward display policy belongs to host: no panel → call Claim/RewardClaimFlow; preview → Confirm through shared queue; already granted notice → Notice(snapshot). Single-slot preview verifies immutable reference/fingerprint at execution. Claim All is a separate explicit user action with a frozen manifest, not a queue of independently re-queried current offers. If a game wants a batch preview, add a view for that exact batch admission contract before granting.
