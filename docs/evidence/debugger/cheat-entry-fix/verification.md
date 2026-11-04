# Cheat panel entry fix — 2026-11-04

## Cause and change

MCP inspection found the sandbox in Edit Mode in BootstrapScene with no DebuggerRuntime. The Control Center previously exposed its live button only while a runtime existed, leaving no direct scene setup action on its Cheat options page.

Added Window → Dreamy Debugger → Cheat Panel, explicit runtime/Game view/PIN guidance and an undoable scene bootstrap installer that selects existing hosts instead of duplicating them. Project-owned bootstrap configuration/prefab creation is separate from demo scene creation, so attaching to an untitled scene does not replace that scene. BootstrapScene now contains one project-owned debugger prefab instance; existing scene objects/references are unchanged.

## Verification

- Isolated package EditorIntegration regression: 5 tests, includes scene ownership, configuration/catalog references and repeat installation without duplicate host. See final XML for matching result.
- Actual sandbox: play BootstrapScene; game transitions to MainScene; runtime Ready with 11 registered options; direct menu opens numeric prompt; four actual keypad PushNumber(0) calls unlock the Options tab. `cheat-panel-main-scene.png` shows actual runtime UI. Console read after that smoke returned 0 errors.
- Before smoke, transient GameObjectInspector missing-selection errors appeared after closing the additive setup scene; no such errors occurred in the subsequent Play session. Test fixture initially could not add a scene beside an unsaved untitled scene; corrected fixture to replace only its clean blank scene, then reran successfully.
- Preview.3 also fixes a confirmed runtime PIN lifecycle issue: scene/focus/pause relock now cancels the old PIN view/callback before clearing access, so a new Options prompt can open. Eight UI PlayMode regressions passed, including an actual scene change during PIN entry and subsequent Options unlock. Prior preview.1 platform build receipts remain historical and are not claimed as fresh builds of preview.3. No device/live-service checks were added.

Existing unrelated localization font changes were excluded. Final Editor is left in Play Mode with the cheat panel open for use.

During live startup the original pending PIN view stayed visible after access relocked on MainScene activation; reopening was ignored as a duplicate prompt. This reproduced the lifecycle issue that preview.3 fixes.

Final preview.3 live state after the UI initialization frame: MainScene, Access unlocked, panel visible, ActiveTab Options, 11 registered options. Immediate inspection of ActiveTab within the same opening call preceded UI initialization and threw in the tool query; the next-frame read succeeded. The too-early blank capture was rejected and removed. Final targeted results: 5/5 EditorIntegration and 8/8 UI PlayMode tests.
