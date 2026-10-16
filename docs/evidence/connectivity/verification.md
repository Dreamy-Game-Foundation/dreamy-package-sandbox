# Connectivity Diagnostics verification — 2026-10-16

Unity 6000.4.12f1, Linux Editor, dreamy-package-sandbox.

## Change

Scene-owned inactive Retry prefab; direct model/presenter/recovery gateway, no PanelManager or Addressables navigation. Production feature navigation remains separate. Imported Diagnostics and distributable package sample match byte-for-byte. Diagnostics prefab is independent so its button row can be reparented without changing the production prefab.

## Observed checks

- Compile: successful.
- Play Mode: PanelManager.HasInstance == false while Retry is visible.
- Offline + Run + Retry: zero dispatched operations while offline.
- Recover + Retry: Succeeded; attempts 1; total calls 1.
- Panel Cancel: Cancelled; attempts 0; dispatch count unchanged.
- Timeout: Retry panel displays timeout message after the check deadline; panel Cancel works.
- Server Error: Failed / simulated_503; no recovery popup.
- Unknown Outcome: OutcomeUnknown / reconcile_transaction_before_retry; no automatic retry.
- Owner disable: pending recovery cancels and view hides; re-enable restores Run interactability.
- Scene prefab binding: three decision buttons remain under DecisionButtons after save/reload; row height 66.
- Geometry checks: controls/dialog do not overlap and active buttons stay within their regions at virtual Canvas sizes 1080x1920, 1920x1080, and 1663x1247 (4:3 after CanvasScaler scaling).
- Editor/Device Simulator: observed safe-area anchors > 1 when simulated safe-area pixels and Game View dimensions differ. Editor-only SafeArea fallback keeps anchors within [0, 1]. Device build branch unchanged.
- OS Settings: hidden in Editor by the existing platform gateway; native device behavior not tested.
- Screen capture through MCP was unreliable with Device Simulator / Overlay Canvas, so no screenshot is presented as layout evidence.

## Automated checks

- Connectivity Runtime + UI EditMode tests: 35/35 passed (job 7e56e8810c7d4a5fadc3a628e9de562d).
- Expanded UI + Connectivity run: 59 tests completed, 5 failures, all in PanelPresenterTests (job aaf28bfeadb0465fa69d4e4873f48c88).
- Repeated PanelPresenterTests with the original SafeArea source: same 5 failures out of 6 tests (job 71670e48942445d59fde267b334b0057). These failures also occur without this SafeArea change. No presenter lifecycle source was changed.
- validate-connectivity.py --allow-imported-customizations: passed package boundaries, sample metadata, source/import comparison; existing host production Retry prefab customization retained.
- Targeted source/README diff whitespace checks: passed.
