# Shop reward holder deferral — 2026-10-28

Before this change, the real Shop prefab regression failed: reward text was `1.1K`
instead of `100` during flight. Shop holders had auto refresh enabled but the consumer
only ran presentation after the wallet mutation and never acquired a defer scope.

Shop 0.3.0 adds optional, engine-independent `IShopPurchasePresentation`. The presenter
acquires the view's scope before calling the purchase service, retains it through
presentation and disposes on completion/failure/cancellation/disposal. Failed purchases
release before shortage recovery. Existing IShopView implementations remain compatible.

The sample ShopPanel defers only matching reward holders via Economy 0.8.0. It cancels
waits, stops its specific flight and releases holds on pre-hide, disable, destroy or
presenter disposal. Pre-hide cancels the presenter lifetime before waiting for a tween.
Source and imported ShopPanel scripts are identical; serialized fields/GUIDs are unchanged.

Unity MCP validation on 6000.4.12f1:

- Compile succeeded; final Console contains zero errors.
- 3/3 real-flight PlayMode tests passed: complete, pre-hide with active GameObject,
  presenter disposal. Each asserts reward text waits, cost text updates immediately
  and an independent Home holder keeps auto refreshing.
- 21/21 Shop Runtime/Recovery EditMode tests passed. New presenter tests establish
  before-service scope acquisition, pending-service close/reopen isolation, failed
  purchase release before recovery, cancellation and exception cleanup/reporting.
- Diff whitespace, serialized-field equality, sample script equality and Runtime/Editor
  dependency checks passed. Bootstrap start-scene preference was disabled only for
  isolated tests, then restored; original BootstrapScene remains open outside PlayMode.

ResourceUIHolder stays in `Runtime.UI` / `Dreamy.Economy.UI.Runtime`, a player-side
presentation assembly. Engine-independent wallet/transactions remain in
`Dreamy.Economy.Runtime`; the inspector remains Editor-only. Shop Runtime references
only the optional presentation contract and introduces no Economy UI or Feedback dependency.

No player/mobile build, device run, full-project game smoke or Daily/Missions migration
was performed. No publish/push occurred. See `results.json` for test jobs and source hashes.
