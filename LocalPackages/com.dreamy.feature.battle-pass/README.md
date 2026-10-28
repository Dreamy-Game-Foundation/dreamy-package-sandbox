# Dreamy Battle Pass

`com.dreamy.feature.battle-pass` **0.1.0 candidate**, Unity 6000.0+. Offline Free/Premium season progression for casual/puzzle games, separate from player level and wallet balances.

- Explicit enrollment, 14-day duration or authored fixed UTC window, 48-hour claim grace.
- Cumulative `long` XP, occurrence-scoped durable event receipts, bounded XP without overflow.
- Frozen enrollment config; retained archived seasons and receipts.
- Single-slot claims and durable frozen Claim All, resumed through Economy journals.
- Premium authority is an injected entitlement reader; no duplicated premium flag.
- Pure C# MVP presenter, optional Economy Reward Presentation adapter, independent persistent uGUI sample.

## Install

This is a local package candidate tracked in the sandbox. No remote repository or release tag is published. Copy the package folder with `.meta` files and add a local dependency:

```json
"com.dreamy.feature.battle-pass": "file:../LocalPackages/com.dreamy.feature.battle-pass"
```

Install its declared dependencies from your Dreamy sources first: Core 1.1.2, Feature 0.2.0, DataConfig/Datasave 0.2.0, Economy **0.7.0 candidate**, Newtonsoft 3.2.1 and UniTask 2.5.10. Economy currently pulls Dreamy UI transitively. These are source requirements, not toolkit-certified compatibility. Use this sandbox's package commits until release pins exist.

For the **Battle Pass Feature** sample also install Dreamy Shop 0.2.0, Dreamy UI 1.0.6, uGUI/TMP, Input System, and import TMP Essential Resources. The prebuilt demo scene uses `InputSystemUIInputModule`. Import the sample using Package Manager, open `Scenes/BattlePassDemo.unity`, and Play with your project's bootstrap override disabled. The standalone scene owns its own PanelManager; do not load it beside another UI root.

Demo flow: Enroll → Complete Run (+20 XP) → Open Battle Pass → Claim/Claim All. Toggle **Show Claim Confirmation Panel** off for immediate single-slot claim; on for preview with grant only after pressing Claim. Claim All is the explicit batch-confirm action and executes directly. Premium costs 200 demo gems through Shop. Reload reconstructs all owners; Advance Day tests expiry. Saves use `DreamyBattlePassDemo`, separate from gameplay. Seed 500 gems is demo-only, protected by a receipt.

## Production composition

1. Register `BattlePassInstaller.RegisterConfig(configService)` **before** `InitializeAsync`. Provide exactly one `Resources/DataConfig/battlePassCatalog.json` or your own config source.
2. Construct one shared Economy wallet/inventory and gateway. Declare all item and unique entitlement targets before constructing the wallet. Keep the existing wallet used by Shop/Missions/Progression.
3. Create one `BattlePassModel` per profile/save key with explicit config, save, gateway, clock and premium reader. `BattlePassInstaller.Install(...)` optionally registers that same instance in ServiceLocator. The sample uses direct composition and does not register globals.
4. Call `RecoverPending()` before enrollment/rollover or new feature work. If a persistence result is uncertain, reconstruct feature **and affected destination owners** from durable storage before retrying. Reconstructing the panel does not clear this guard.
5. Host explicitly decides unlock/enrollment and supplies preauthored unique occurrence IDs. Opening a panel never enrolls.
6. Persist gameplay completion + consumer outbox in the same gameplay document, then report `BattlePassProgressRequest(occurrenceId, stableRunId, xp)`. Acknowledge only Success/AlreadyApplied; retry with identical payload after restart.
7. Read `GetState()`, show with `BattlePassPresenter`, dispose subscriptions when the panel hides. Refresh domain on resume/time boundaries to persist UTC high-water; render countdown without writing save every frame.

```csharp
var model = new BattlePassModel(catalog, save, sharedGateway,
    new SystemBattlePassClock(), new EconomyBattlePassPremiumAccess(sharedWallet), "profile-1.battle-pass");
model.RecoverPending();
model.Enroll("season-2026-01"); // host-controlled
model.ReportProgress(new BattlePassProgressRequest("season-2026-01", persistedRunId, 20));
```

Optional panel integration: `BattlePassClaimSource` in `Dreamy.BattlePass.RewardPresentation`. Pass its `Reference(occurrence, slot)` to `RewardPresentationQueue.OpenAsync(RewardPresentationRequest.Confirm(...), source)` or `RewardClaimFlow.ClaimAsync(...)`. Query is read-only; claim checks the expected fingerprint in the domain. Notice may display a completed snapshot and never grants again.

See [architecture](Documentation~/architecture.md), [configuration](Documentation~/configuration.md), [recovery](Documentation~/save-and-recovery.md), [offline time](Documentation~/offline-time-and-seasons.md), [premium](Documentation~/premium-and-shop.md), and [integration](Documentation~/integration.md). Validation status is recorded in the sandbox's `docs/evidence/battle-pass/`; candidate status does not imply a device or store certification.
