# Conditional gaps: tasks 19-21

Observed: 2026-10-14. Deliverable: local source/history investigation only.
No runtime, package, consumer, Unity, branch, commit, tag, push, or network actions were performed. This report does not close Q-D1/Q-D2/Q-D3 or certify implementation, compilation, tests, billing, or recovery.

## Decision

- **19:** Released Progression already has Experience and Sequential modes, threshold lookup, multi-level advancement, optional milestone rewards, and long overflow rejection. No missing XP calculation capability is established for PB's launch rules.
- **20:** **No package change recommended for current PB XP behavior.** Keep `PlayerProgressSave` as the player-level/XP authority. Consumer adoption remains conditional on the pending user decision. Legacy XP-floor migration and int saturation are host adapter requirements if adoption is chosen, not reasons to add another progression save.
- **21:** **Keep the package virtual purchase as one `TryExchange`.** Confirmed IAP gaps are durable pending/redelivery and immutable purchase reward snapshots. Current PB has a separate, direct callback-to-currency path with no IAP transaction identity. Do not silently restore the old integration or route virtual exchange through per-entry grants.

The minimum action now is this report and a consumer integration decision. If integration is selected, re-baseline current PB and prove the migration/SDK contracts below before implementing only the confirmed gaps. Mixed-owner paid purchases remain disabled pending a separate partial-fulfillment or unified-owner decision.

## Source identity and drift

Prefixes in references resolve to these exact roots:

- `P/` = `D:/Game/dreamy-package-sandbox/LocalPackages/`.
- `PB/` = `D:/Game/Planet Bunker/`.
- `T/` = `D:/Game/dreamy-base-project/DreamyTemplateProject/`.
- `PB@20e1264/` references the historical Git blob, **not a current file**.

| Source | Observed identity/state |
|---|---|
| Progression released `v0.1.1` | Annotated tag object `b12470884c7e91b53907df79faf775c64b1e2d69`; peeled commit `d56ef9bd145b413f23c5eed43c8d2e0b6571bf37`. Use the peeled commit for API attribution. |
| Progression HEAD | `3f591a8998deba7b3757cef8f970b68e79ed8da1`; package.json still says `0.1.1`. Only the test asmdef was worktree-dirty when inspected; domain/config/persistence had no worktree changes. |
| Shop HEAD/released `v0.1.2` | Both `3fee66cfd839d2d626e5c15878dcf0a440c413f9`; worktree was clean when inspected. |
| Economy HEAD | `97ff875f73735c4169062545949aeee7b87de1fa`; wallet/save/docs and new Rewards/tests were worktree changes. Current wallet observations below are **unreleased worktree source**, not v0.2.1 guarantees. |
| PB current HEAD | `2330bf8a610a77c678986f71728c66d38708aa91`, rather than plan baseline `20e1264e12ad971d9bc02ca33fc5458df55c8b33`. |
| PB worktree | `ResourceSave.cs` and `UnitUpgradeSystem.cs` modified; currency/card destinations, receipt helper, and destination tests untracked. These concurrent artifacts were only read, not changed or validated. |
| Template HEAD | `f6486d0e8fa7e877360d7b775c96ad419474fe0f`; billing wiring inspected as a demo, not production evidence. |

`PB/Packages/manifest.json:7-13` currently lists seven Dreamy foundation packages: Assets v1.0.0, Audio v1.0.2, Core v1.1.1, DataConfig v1.0.0, DataSave v1.0.0, Editor Tools v1.0.0, UI v1.0.2. Economy, Shop, Progression, Daily, and Missions have no entries in the current manifest or lock. The Dreamy records in `PB/Packages/packages-lock.json:33-119` were read; the plan's twelve-package baseline is not current.

`git diff --name-status 20e1264..HEAD -- Assets/_Project/Scripts Packages` confirms deletion of:

- `PB/Assets/_Project/Scripts/Gameplay/Progression/PlayerResourceService.Economy.cs`.
- `PB/Assets/_Project/Scripts/Shop/DreamySdkShopGateway.cs` and `PlanetBunkerShopCatalog.cs`.
- `PB/Assets/_Project/Scripts/Bootstrap/GameInstaller.Features.cs` and the other split installer files.

Current `PB/Assets/_Project/Scripts/Gameplay/Progression/PlayerResourceService.cs:19-26` is a non-partial game service, not `IResourceWallet`. Current `PB/Assets/_Project/Scripts/Bootstrap/GameInstaller.cs:145-147` registers the host `DreamyShopPurchaseGateway`, not package `ShopModel`. `PB/Assets/_Project/Scripts/UI/Presenters/ShopPresenter.cs:15-19` defines that host purchase interface; it is not `Dreamy.Shop.IShopPurchaseGateway`.

The untracked `CurrencyRewardDestination.cs:8-21,31-42,48-100` exposes a strict reward destination and detached currency candidate, but no legacy `TryExchange` facade. Its existence does not establish installation, bootstrap registration, compilation, or Shop cutover.

## 19: Released Progression versus PB XP

Read-only Git comparison returned exit 0 and empty output for:

```text
git -C P/com.dreamy.feature.progression diff --exit-code v0.1.1 HEAD -- Runtime/Domain Runtime/Config Runtime/Persistence Runtime/Contracts/IProgressionService.cs
git -C P/com.dreamy.feature.progression diff --exit-code HEAD -- Runtime/Domain Runtime/Config Runtime/Persistence
```

`P/` above is the expanded root defined earlier, not a shell variable. Thus the following domain/config/persistence references apply to both the released tag and HEAD. The full tag-to-HEAD diff has 21 changed files, including presentation, samples, asmdefs, package metadata, and test fixes. HEAD's `Runtime/Presentation/ProgressionPresenter.cs` must not be advertised as released v0.1.1 functionality. The released test source also differs: the reward-failure test's anonymous-object initializer was corrected to `new()` and a DataConfig using was added at HEAD; no released-tag compile/test pass is inferred.

| Capability | Released source | Actual PB requirement | Decision |
|---|---|---|---|
| Modes/thresholds | `P/com.dreamy.feature.progression/Runtime/Config/ProgressionCatalogConfig.cs:29-75,79-83,91-114,137-159`: consecutive levels, first XP threshold zero, strictly increasing cumulative thresholds, Sequential/Experience. `Runtime/Domain/ProgressionModel.cs:166-180` resolves the highest reached threshold. | `PB/Assets/_Project/Scripts/Gameplay/Progression/PlayerLevelRules.cs:25-37`; `PB/Assets/Resources/DataConfig/playerLevelConfigs.json:4-10`: cap 100, base 100, increment 20. | Explicit catalog thresholds can represent the launch formula. No new formula engine is needed. |
| Multi-level advancement | `ProgressionModel.cs:104-118` resolves target once and walks every crossed level. | PB computes level directly from cumulative XP (`PlayerProgressSave.cs:49-54`). | Already present. |
| Maximum level | Catalog's final entry bounds level lookup; Experience grants can still accumulate XP at cap (`ProgressionModel.cs:104-121,218-242`). Sequential rejects further advancement (`:65-68`). | PB caps level at 100 but continues cumulative XP up to int.MaxValue. | Experience matches level-cap semantics; do not confuse Sequential's MaxLevel result with Experience behavior. |
| Milestones | Optional resource rewards, no initial-level reward (`ProgressionCatalogConfig.cs:61-64,111-114,161-174`); deterministic per-level wallet ID `progression:{catalogId}:level:{levelId}` (`ProgressionModel.cs:182-190`). | Actual XP writer has no player-level milestone grant (`PlayerProgressSave.cs:49-54`). Campaign first-win currency is separate (`BattleFlowService.cs:137-157`). | Do not add milestones or move campaign rewards into XP merely to use the package. |
| Integer bounds | Positive long grants with overflow check before XP addition (`ProgressionModel.cs:94-104`). | PB int XP saturates using a long intermediate (`PlayerProgressSave.cs:53`); launch thresholds fit int. | Different policy, not missing arithmetic. An adapter would cap the granted delta to remaining int capacity and skip zero grants. Do not change PB saturation to long overflow failure. |
| Match eligibility | Package accepts generic positive XP grants; it does not define match outcomes. | `PlayerLevelRules.cs:40-44`: completed/non-forfeit win 50, draw 45, loss 40, otherwise zero. `BattleFlowService.cs:131-135` obtains launch config and calls the save writer. | Host responsibility. Ineligible battles must not call a positive-only package grant with zero. |
| Legacy XP floor | Package's own save has catalog/index/long XP/processed IDs (`Runtime/Persistence/ProgressionSaveData.cs:12-28`); constructor loads it and may reset mismatched catalog (`ProgressionModel.cs:38-40,124-151`). | `PlayerProgressSave.cs:77-86` floors legacy XP at the old level's threshold once, sets XpVersion=1, clamps nonnegative, derives level. | Confirmed integration mismatch: loading an unrelated package save cannot preserve PB's floor or authority. Minimum is a host adapter/migration under the existing owner if adoption is approved. |

### Comparison cases derived from source, not executed fixtures

For launch rules, cumulative threshold at level L is `100*(L-1) + 20*(L-1)*(L-2)/2`. A package catalog generated with these thresholds and no milestone rewards yields:

| Case | Expected PB/package level or policy |
|---|---|
| XP 0, 99, 100, 101 | Levels 1, 1, 2, 2. |
| XP 219, 220, 221 | Levels 2, 3, 3. |
| Grant 360 from XP 0 | Level 4; three crossed levels (thresholds 100, 220, 360). |
| XP 106919, 106920, 106921 | Levels 99, 100, 100. |
| XP int.MaxValue | Level 100 in either launch mapping. |
| PB XP int.MaxValue-10 plus eligible win 50 | Saturates at int.MaxValue; unadapted package would add the whole 50 as long. |
| Package XP long.MaxValue-1 plus 2 | InvalidGrant before mutation; PB cannot represent that initial XP. |
| Legacy PB PlayerLevel=25, XP=0, XpVersion=0 | Floor 7920, level 25, version 1; next win gives 7970. Package's default new save would start at XP 0 instead. |

Existing source coverage: `PB/Assets/_Project/Tests/EditMode/UnitRosterSyncTests.cs:289-317` checks launch match XP, cap, and level-25 legacy floor. `P/com.dreamy.feature.progression/Tests/Runtime/ProgressionModelTests.cs:12-111` checks Sequential advancement/retry, multi-level milestone grants, reward failure, mode mismatch, Sequential max, and invalid thresholds. These tests were read, **not run**. The table is not a substitute for Q-D1 boundary, overflow, migration, or save round-trip execution.

## 20: Conditional Progression action

**No-change branch:** PB's current XP calculation does not justify extending Progression. Leave player level, XP, migration version, campaign stars/unlocks, and unit upgrades under their existing owners. `PlayerProgressSave.cs:11-18,36-54` contains both campaign and player-level fields, and `BattleFlowService.cs:135-158` writes that authority. A second `ProgressionSaveData` authority would introduce a reconciliation problem.

**If adoption or recoverable milestones is selected:** the released model has real persistence gaps, not absent XP capabilities:

1. It holds the loaded save (`ProgressionModel.cs:38`), modifies XP/index/processed IDs before persistence (`:114-118,193-215`), and has no detached candidate or busy guard. A save exception can leave in-memory progress and dedup state ahead of disk. Initialization similarly mutates the loaded document before saving.
2. Multi-level rewards are delivered before XP commit (`:106-118`), with no durable resolved snapshot or embedded pending journal. A later reward failure or restart depends on the wallet's durable per-level dedup; catalog reward changes can change the replayed payload.
3. Processed XP grant IDs are bounded by the constructor limit, default 4096 (`:10,25,199-215`). Older XP grants can be replayed after eviction; this is not lifetime idempotency.

These are source-confirmed limitations to any stronger future durability claim. They do not authorize a package rewrite for today's PB XP path. Minimum conditional work: preserve PB save ownership and floor/int policy in the host; keep milestones off unless explicitly requested. If milestones are enabled, require durable reward identities/snapshots and a recoverable owner commit with restart/fault tests before claiming Q-D2. Do not invent historical grant fingerprints or claim retroactive recovery of torn legacy grants.

## 21: Shop fulfillment, identity, and no-change decisions

### Preserve the working virtual exchange boundary

`P/com.dreamy.feature.shop/Runtime/Domain/ShopModel.cs:116-130` constructs one `ResourceExchangeRequest` containing cost plus all rewards and calls `wallet.TryExchange` once. Do not replace this with cost save followed by bundle entry delivery. This is an atomic delegation contract; the wallet determines persistence guarantees.

Current **unreleased** reference wallet source (`P/com.dreamy.feature.economy/Runtime/DatasaveResourceWallet.cs:91-118,207-219`) validates entries, computes detached debit/all-reward balances, saves one candidate before replacing caches, and uses checked arithmetic. This source supports the intended single-owner boundary but was not executed here and is not release certification. The historical PB facade had the corresponding detached single-save exchange (`PB@20e1264/Assets/_Project/Scripts/Gameplay/Progression/PlayerResourceService.Economy.cs:43-61`); it does not exist at current PB HEAD.

Package purchase-once behavior also already has a safety rule: `P/com.dreamy.feature.shop/Runtime/Config/ShopCatalogConfig.cs:145-153` requires the ownership reward exactly once **as the last reward**. `ShopModel.cs:54-55,113-114` checks ownership and per-model in-flight state. Do not report a first-entry ownership bug or add duplicate entitlement machinery; existing tests at `Tests/Runtime/ShopModelTests.cs:65-112` cover ownership after model recreation and retry after cancellation, but not process-death recovery.

PB's current gem chest path is different: `PB/Assets/_Project/Scripts/Shop/DreamyShopPurchaseGateway.cs:90-120` spends gems, calls the separate chest service, and refunds only on a reported failure. This is not the package's atomic virtual exchange and does not prove crash-safe mixed-owner fulfillment. Leave it outside a currency-only Shop cutover rather than silently weakening the package boundary.

### Confirmed package IAP gaps

| Gap | Evidence | Minimum conditional action |
|---|---|---|
| Pending purchase state is volatile and not resumable by receipt | `ShopModel.cs:15,79-83,94-109`: per-model HashSet, grant loop, removal in finally; no persisted pending document or receipt resume/query API in the inspected model. | If durable IAP fulfillment is adopted, persist original receipt/store product/offer and resolved ordered reward snapshot in a Shop-owned document, then resume delivery without dispatching another purchase. |
| Partial rewards depend on wallet dedup and stable live config | `ShopModel.cs:94-101` independently grants each current `offer.Rewards[index]`; later false leaves earlier delivered entries. | Preserve fixed original payload and entry mapping across retry/config reorder/revision. Same receipt plus changed payload must fail/reconcile, not grant a new bundle. |
| Legacy bool wallet route is not strict lifetime delivery | Shop still calls `IResourceWallet.TryGrant` (`:99`). Current reference wallet's `HasTransaction` uses retained legacy IDs/strict receipts (`DatasaveResourceWallet.cs:179`); legacy IDs are pruned (`:199-204`). Strict tombstones do not make legacy TryGrant a lifetime dedup route. | Do not label an arbitrary legacy wallet strict. Preserve old IDs as migration tombstones/aliases and use an explicitly durable owner route only where the integration contract is proven. |
| Persistence dependency not yet declared for Shop-owned recovery | `P/com.dreamy.feature.shop/package.json:12-19` and `Runtime/Dreamy.Shop.Runtime.asmdef:4-9` have Economy but no direct DataSave dependency. | Add the explicit DataSave dependency only if Shop-owned persistence is actually implemented; no independent Economy journal save. |

Old released IAP sub-transaction identity is exactly:

```text
{catalogId}:iap:{offerId}:{storeTransactionId}:{zeroBasedRewardIndex}
```

Source: `P/com.dreamy.feature.shop/Runtime/Domain/ShopModel.cs:94-98`. Example asserted by existing tests: `main-shop:iap:iap-starter-pack:store-tx:0` (`Tests/Runtime/ShopModelTests.cs:37-49`).

Any shared gateway adoption must retain this complete identity or an explicit durable alias/tombstone mapping to it. Preserve catalog identity, offer ID, actual store receipt/transaction ID, and the original ordered index-to-reward snapshot. Do not swap to Guid request IDs, a new namespace, current reordered entry indices, or fabricated fingerprints for already-delivered receipts. A strict path encountering an old ID-only receipt must refuse unverifiable payload collisions without granting again.

### Confirmed current PB gaps, separate from package gaps

1. **No IAP identity/dedup in the active callback path.** `PB/Assets/_Project/Scripts/Shop/DreamyShopPurchaseGateway.cs:52-65` passes a parameterless success callback into the SDK and calls AddGems/AddCoins. It neither reads store transaction ID nor checks a durable receipt. Repeated success invocation would repeat the add; restart has no receipt-linked pending state. `ShopPurchaseSave.cs:11-17,31-42` stores chests and ad claims, not IAP receipts.
2. **Current add path is not a detached durable transaction.** `PlayerResourceService.cs:93-104` mutates loaded ResourceSave before save. `ResourceSave.cs:52-59` adds in int without a checked bound; overflow can wrap before SetAmount clamps negative values. Existing untracked strict destinations do not change the IAP callback's Add path.
3. **Old receipt and entitlement storage is not preserved by current schema.** Historical `PB@20e1264/Assets/_Project/Scripts/Gameplay/Progression/ResourceSave.cs:18-28` declares WalletTransactionIds and WalletBalances. Current HEAD schema has neither; worktree `ResourceSave.cs:18-25` adds RewardReceipts only. `CurrencyRewardDestination.cs:54-57,84-97` copies/checks new receipts, with no import of old wallet IDs. This is a migration evidence gap and replay risk if old receipts become reachable again, not proof that any actual player save has already lost data. Before returning to package Shop, preserve old ID-only receipts and non-currency balances explicitly; do not reset them or fabricate fingerprints.

Minimum conditional fix is at the selected host transaction/save boundary, not a new economy.save: retain ResourceSave, pass real store identity through the application-owned fulfillment flow, keep mutation and receipt in the same detached owner candidate, and preserve legacy aliases/tombstones. Do not edit the SDK bridge to simulate these guarantees.

### SDK and template evidence limits

The current local bridge is a fixture: `PB/Assets/DreamySDK/Bridge/Scripts/DreamySDK.cs:607,617-620` returns ready and immediately invokes success; `:668-670` returns null product details; `:677-679` reports restore success with an empty list. No pending receipt query/recovery implementation was established in this inspected bridge. The injected production SDK's pending-query, acknowledgement timing, receipt validation, transaction ID timing, restore semantics, and process-death redelivery remain **unverified production blockers**, not proven absent capabilities of the real SDK.

Historical `PB@20e1264/Assets/_Project/Scripts/Shop/DreamySdkShopGateway.cs:27-33,36-75` read Product.transactionID, guarded duplicate callbacks, failed missing IDs outside the Editor fallback, and kept dispatched purchases application-owned despite UI cancellation. It is deleted now; those protections must not be credited to the current callback gateway.

`PB/docs/plans/dreamy-migration-progress.md` is absent locally. Its historical blob at baseline was inspected: P5 `:51-62` explicitly distinguishes Editor evidence from unrun real receipt, process-death, restore, cancellation, and SDK transaction timing gates. Reported historical test counts are not current results.

`T/Assets/_Project/Scripts/Bootstrap/GameInstaller.Features.cs:64-74` still generates `simulated-iap-{Guid}` and returns success. `T/Assets/_Project/Tests/Editor/ShopBillingTests.cs:26-47` checks simulated distinct IDs and pre-cancel, not real billing. Neither fixture can pass Q-D3.

## Missing evidence and minimum verification after a decision

- **Consumer direction:** pending user decision on restoring/adopting packages versus retaining current PB architecture. Do not infer that decision from untracked host adapter files.
- **19/20:** no executed tag or HEAD fixture, old-save serialization round-trip, boundary/overflow comparison run, or failure/restart milestone run. Existing tests were read only. Run these against the exact selected source and preserved PB owner if adoption proceeds.
- **21 package:** no executed partial-bundle, save-failure, receipt replay after model/wallet/process recreation, changed-config snapshot, legacy alias, callback duplication, cancellation, or purchase-once recovery matrix. Existing `ShopModelTests.cs:12-129` is narrower: delegation, success IDs, ownership, consumables, in-memory pending/cancel, and validation.
- **21 current PB:** existing `ShopPresenterTests.cs:10-52` tests rendering/tab/intent forwarding, not fulfillment. Historical P5 tests and removed facade do not prove current behavior. No active old-player save or receipt history was inspected.
- **SDK/device:** real injected SDK and receipt recovery/query/ack contract, Android/device restart and restore evidence are unavailable in this source-only scope. No real purchase was made.
- **Dependencies/build:** no Unity resolve, C# diagnostics, compile, package tests, consumer tests, or player build was run. In particular, untracked PB files reference Economy Rewards although the current manifest lacks Economy; they cannot be treated as integrated or validated.

Inspected guidance: sandbox plan tasks 19-21 and package design rules; local toolkit progression, system-shop, and system-iap skills, transaction-flow/idempotency/restore references, rules index, and compatibility catalog. That catalog has no Progression/Shop record; direct local source and peeled tag evidence above are the authority.

Read-only evidence probes completed: Progression tag/HEAD domain equality (exit 0, empty diff), Progression domain/worktree equality (exit 0, empty diff), Shop tag resolution (exit 0, expected SHA), and Shop model/worktree equality (exit 0, empty diff). File/path absence and checkout drift were recorded rather than repaired. This report is the only authored artifact.

Report formatting check: `git diff --no-index --check -- /dev/null <report-path>` returned exit 1 for the new-file comparison, no whitespace findings on stdout, and only the repository's LF-to-CRLF notice on stderr. This was a document check, not a build or test run.
