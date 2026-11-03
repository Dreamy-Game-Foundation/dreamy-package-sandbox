# Latest extension status

Sandbox production setup, full Foundation labels/dynamic text and Country Flags: **41/41 EditMode + 17/17 PlayMode passed**, main game Ready with persisted canvas scaling, native-name/flag dropdown and 12×50 keys. See [latest sandbox evidence](sandbox-flags/VALIDATION.md).

Production Starter and optional Settings integration: Localization source 0.2.0 / Settings source 0.3.0; fresh 41/41 EditMode, 15/15 PlayMode and an independent consumer without Settings passed. See [current packaging/integration evidence](production-starter/VALIDATION.md). No tag/publish or player/device certification is implied.

12 locales and dropdown layout: **41/41 EditMode and 15/15 PlayMode passed** on 2026-11-03. See [latest evidence](multilanguage/VALIDATION.md). Component API and manager-free test scene previously passed 41/13; see [component evidence](component-api/VALIDATION.md). The pilot section below records historical baselines; use the dated extension receipts for current coverage.

# Localization pilot validation — 2026-11-02

## Implemented

- One JSON per locale, 18 shared keys, en source/fallback and vi translations.
- Editor JSON parser, SmartFormat-aware selector validation, read-only preview, stale-plan guard, owned-output checks, table import and file backups/recovery path.
- UniTask service with initialization sharing/retry, cancellation for init/switch/assets, disposal, string fallback, string table preload and deduplicated commit notifications.
- Optional Settings language capability/gateway with busy/error state, panel selector, localized labels and injected host adapter.
- Standalone LocalizationSettingsTest scene, real SettingsModel/Presenter/Panel, localized text outside the panel and dynamic reward arguments.
- GameInstaller integration and localized Foundation title/Open Settings label on the actual loaded prefab.

## Fresh evidence

- Baseline EditMode: 15 passed, 0 failed — phase-00/baseline-tests.json.
- Final EditMode: 29 passed, 0 failed — phase-07/editmode.xml (native TestRunner); phase-07/editmode.json retains the earlier bridge receipt. Includes duplicate/invalid JSON, exact key coverage, numeric/nested Smart String selectors, read-only preview, stale plan and byte-identical repeat import with stable table GUID/shared entry IDs.
- PlayMode: 9 passed, 0 failed — phase-07/playmode.xml, native Unity TestRunner API. Tests cover dropdown live apply, close/reopen, external locale updates, unsupported/same locale, cancelled request, scene reload/preference, arguments, init/dispose, no gateway and gateway failure recovery.
- Main-game smoke: BootstrapState.Ready, real Settings dropdown switches en/vi and both Foundation labels update — phase-06/main-en-state.json, main-vi-state.json and screenshots.
- Audio smoke: music volume changed 1→0→1 through ISettingsService and was restored; no new console errors in the smoke. Haptic hardware and platform/review SDK flows were not exercised.
- Teardown smoke: no ISettingsLanguageGateway registration remained after stopping main game.

## Failures observed and corrections

- Initial scene builder compile failed because Editor asmdef lacked direct Audio/Core references. Added explicit references and recompiled successfully.
- Start Scene toolbar forced BootstrapScene during standalone/test startup. Scene-specific startup routing was added using public Editor scene APIs. TestRunner attempts affected by this override were cancelled and are not passing evidence.
- Bridge PlayMode retries could retain an orphaned job or stalled runner with reload optimizations. Used Unity's native TestRunner API with a result callback instead. Native result XML is the source for the PlayMode count; no zero-test job is counted as passing.
- Initial PlayMode setup exposed a real warm-cache race: Smart String binding enabled before the amount argument existed. LocalizationTestRoot now disables content in early Awake and assigns arguments before enabling. Initial failure retained in phase-07/playmode-initial-failure.json; matching regression passes in the final XML.
- Updating Start Scene during the Play transition was unreliable. The dedicated Play Settings Test menu configures a clean launch before entering Play and restores settings afterward.
- A cold backend settings reload deserialized default SmartFormatter extension slots as null. The pilot profile explicitly rebuilds Unity defaults after backend initialization; custom formatter profiles are unchanged unless they opt in. The cold-start assertion is part of PlayMode setup.

## Scope and remaining gates

- These results apply to the dirty working copies identified by source-identity.json on Unity 6000.4.12f1 with resolved Unity Localization 1.5.8, uGUI 2.0.0 and TMP 5.0.0 builtin. Baseline package HEADs are not released implementation commits.
- Clean external consumer/6000.0 fixture, mobile/player build, device/landscape layout and forced mid-apply rollback/crash recovery have not been verified.
- Runtime supports OnDemand and Preload for the string pilot. Retained/ReleaseWhenUnused reject explicitly; consumer leases remain unimplemented.
- Global variable registry is not connected to Unity persistent variables; the sample uses SetArguments. Complete asset-table JSON import, scanner, CSV conversion and a portable package-only scene remain future work.
- Existing warnings about missing Core/Feedback package folders remain. Unity Localization 1.5.8 can emit a SmartFormatTag base Serializable warning under this Editor; no upstream package was patched to hide it.
- No commit, push, tag or release was performed. No unrelated staged changes were reverted.
