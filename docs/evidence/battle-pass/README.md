# Battle Pass candidate validation — 2026-10-20

Unity6000.4.12f1; checkpoint parent e774311. Candidate source is tracked as a new local package; no remote Git URL/tag is invented. Dependency state is observed from the local source manifests/lock, not certified by a missing toolkit registry.

- **69/69 EditMode tests**: Battle Pass23 cases plus Economy reward/recovery, Daily/Missions presentation adapters and Shop. File-backed failure/reconstruction, UTC/grace, fixed window, config snapshot, duplicate XP, cap, premium, batch and presenter lifecycle are covered. See results.json.
- **100 real panel open/close cycles** through UIPanel/presenter factory:10rows,1view listener when open,0closed; model returns to its root-listener baseline1.
- **Live UI**: read-only preview, double Claim once, Shop premium purchase-once, silent claim without panel, Claim All and durable reload. See live-smoke.json and screenshots.
- **Isolated consumer**: imported Samples~ source with preserved GUIDs; compile, catalog validation, all panel binding fields, no missing prefab/scene scripts. Package source and imported sample bytes match exactly.
- **Linux development player build**: succeeded,0errors,9warnings,169994981bytes. Warnings include existing uGUI/InputSystem/TMP/Addressables/UI tween diagnostics; see build-summary.log. Player artifact remains under /tmp, not in Git. Build success does not claim native player runtime or mobile validation.

Reproduce from repository root with a licensed Editor and resolved local package cache:

```sh
python3 docs/evidence/battle-pass/validate-consumer.py --unity /path/to/Unity --build-player
```

The script creates a unique /tmp consumer, copies only TMP Essentials and the shipped Battle Pass sample, resolves local Dreamy dependencies, then validates/builds there. It never changes gameplay saves or the sandbox build settings. Package sources are referenced from the checkout; source-sha256.json records the exact candidate bytes used for this checkpoint.

Android/iOS/device, production IAP, full Missions/Progression gameplay fan-out, optional resource-acquisition navigation and multi-season persistence benchmarks remain outside the passed evidence. The standalone sample demonstrates the durable Battle Pass completion outbox; production embeds completion + all consumer cursors into the game's gameplay save.


## Prefab inheritance correction

Latest asset checkpoint uses **7 connected variants**:3panels from BaseFeaturePanel,4items/entry from BaseFeatureItem. All7GUIDs are unchanged. See prefab-variants.json and variant-migration.json. The shipped builder now instantiates the required bases, binds inherited components, saves connected variants and runs BattlePassPrefabValidator; missing/incompatible bases cause explicit failure.

After migration: consumer `/tmp/dreamy-battlepass-consumer-17v2tejw` imported the shipped sample, compiled and validated all7variant source links and bindings with exit0. Real UI passed100cycles again with61inherited tweens, stable10rows/listeners; no invisible CanvasGroups after show. Manager Back restores the pass from Premium and closes RewardPanel. Confirm kept coins150until Claim; double Claim increased them exactly once to300. Console0errors. New screenshots are variant-panel.png and variant-confirm.png; earlier screenshots belong to the original asset checkpoint.

The Linux player build recorded above preceded this prefab migration and has **not** been repeated for the latest assets. The old source hash manifest is retained as source-before-variants-sha256.json; source-sha256.json records the current package and base prefab sources. Android/iOS/device checks remain unrun.
