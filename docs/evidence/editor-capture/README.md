# Game View Screenshot — implementation evidence

2026-11-05. Implemented in `LocalPackages/com.dreamy.editor-tools/Editor/Capture/` (Unreleased, package version unchanged).

## Observed results

- Clean consumer Unity 6000.4.12f1: package compiled; 9/9 EditMode tests passed. `EditMode.xml` is the fresh test receipt.
- Ubuntu 26.04.1 Wayland session, Unity GUI through XWayland/OpenGL: Game View capture with green Screen Space Overlay marker and magenta camera background passed pixel assertions.
- Copy, Save, Copy + Save; Copy used an invalid output path without creating a file. Copy + Save reused the same captured frame.
- Realtime countdown at timeScale=0, duplicate rejection, cancellation and frame timeout after pausing passed.
- GUI smoke passed with default domain reload enabled, then reran with domain reload disabled against final source. `gui-ubuntu-no-domain-reload.json` records the final run.
- 50 sequential captures passed; the harness verified previous preview textures were destroyed when replaced. This is ownership verification, not a measured peak-memory claim.
- After Unity quit, `wl-paste --type image/png` returned a decodable PNG matching a saved frame byte-for-byte. `game-view-clipboard.png` is that fixture image, not a user desktop capture.
- Screenshot window opened; fixture scene remained clean. No game source/scenes in the sandbox were mutated by tests.

## Environment and dependencies

The connected Unity MCP instance targeted another project, so no mutation was sent through that bridge. Verification used an isolated consumer at `/tmp/dreamy-capture-consumer` and the installed Unity binary directly.

For clipboard testing only, Ubuntu packages `wl-clipboard` 2.2.1-2build1 and `xclip` 0.13-4build1 were downloaded/extracted under `/tmp/dreamy-clipboard-tools`, without installing them system-wide. Only wl-copy was exercised. The consumer PATH included `/tmp/dreamy-clipboard-tools/root/usr/bin`.

The real machine does not currently expose wl-copy/xclip on its normal PATH. Users must install the helper corresponding to their session before selecting Copy; Save remains usable without it.

## Reproduction

1. Create an isolated Unity 6000.4 consumer with `consumer-manifest.json` (adjust the local package path). The additional uGUI and Animation dependencies belong to the test fixture.
2. Place `CaptureSmoke.cs` under the consumer's `Assets/Editor/`. This harness creates its own fixture scene only in that disposable consumer, enters Play Mode, runs the checks, and exits Unity.
3. Run EditMode tests with `-batchmode -nographics -projectPath /tmp/dreamy-capture-consumer -runTests -testPlatform EditMode -testFilter Dreamy.EditorTools.Tests -testResults <result.xml> -logFile <log>`.
4. Run GUI with `-projectPath /tmp/dreamy-capture-consumer -dreamyCaptureSmoke -dreamyNoReload -logFile <log>` and the helper on PATH. Omit `-dreamyNoReload` to test normal domain reload. Do not pass `-batchmode` or `-nographics` to GUI smoke.
5. Read back clipboard PNG using wl-paste and compare its SHA-256 to the capture from Copy + Save. GUI harness outputs are under `/tmp/dreamy-capture-smoke-output`.

`verification.json` includes hashes of implementation/test sources and package manifest. Sanitized log summaries retain test/compiler observations; raw Unity licensing logs were not copied into this repository.

## Observed failures and fixes

Initial fixture omitted the built-in Animation module; opening the screenshot window triggered a Unity default-asset warning about a disabled component. The disposable consumer manifest now declares the fixture module; the final GUI log has no such warning. Initial Unity QuickSearch indexing also raised an internal indexing exception on first import; it did not reproduce in the final GUI run. Neither issue was hidden by clearing the Console or editing package behavior.

Native Dreamy recall/checkpoint/harness could not run because the workflow file/CLI was not available in this workspace, as recorded in the plan. This evidence is a manual checkpoint, not a native harness receipt.

## Remaining verification

Windows native clipboard/paste is implemented but has not run on a Windows host. The DIB byte format has a passing regression; Linux compile does not establish Windows interop success. Ubuntu 24.04, X11-only sessions, URP/portrait fixtures, paste into individual third-party apps and peak-memory measurements remain unverified. No release/tag was created.

## 2026-11-06 — Clipboard setup UI

Added a requirements panel and selectable/copyable install command, detected from the current session (Wayland: wl-clipboard; X11: xclip). Windows shows that no additional install is needed. Missing-helper instructions appear before Play Mode too. The button copies text only and never runs the command. Clean-consumer compile passed after the final UI refinement; nine existing tests passed before that foldout-only refinement. New GUI interaction was not rerun; see `clipboard-setup-2026-11-06.json`. The earlier full GUI receipt remains historical for its recorded source hashes.
