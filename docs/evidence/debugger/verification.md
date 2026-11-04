# Dreamy Debugger — foundation verification, 2026-11-04

Target: `dreamy-package-sandbox@5f78127b991a2b92`, Unity 6000.4.12f1, Editor on Linux, sandbox target Android. Scope: standalone domain foundation + editable sample, not vendor UI migration or player/platform support.

## Receipts

- `vendor-baseline-sha256.json`: observed original import inventory, retained without edits.
- `sandbox-editmode.json`: 44/44 passed through Unity MCP.
- `sandbox-playmode.json`: 3/3 passed through Unity MCP.
- `clean-consumer-editmode.xml`: 44/44 passed, batchmode exit 0.
- `clean-consumer-playmode.xml`: 3/3 passed, batchmode exit 0.
- `clean-consumer-manifest.json`, `clean-consumer-packages-lock.json`: standalone dependency evidence.
- `implementation-sha256.json`: matching implementation/test input hashes.

The clean consumer is `/tmp/dreamy-debugger-consumer-20261104`; Editor executable `/home/tutrinh/Unity/Hub/Editor/6000.4.12f1/Editor/Unity`.

Commands: `Unity -batchmode -nographics -projectPath /tmp/dreamy-debugger-consumer-20261104 -runTests -testPlatform EditMode -testFilter Dreamy.Debugger -testResults <editmode.xml> -logFile <editor.log>`; repeat with `-testPlatform PlayMode -testFilter Dreamy.Debugger.Sample.Tests.HostLifecycleTests` for lifecycle tests.

The immediate post-test Console query returned zero errors and `EditorUtility.scriptCompilationFailed == false`. BootstrapScene path remained unchanged and dirty=false. Test Runner changed enter-play-mode options on disk; restored only that task-generated change to its original value, with Editor memory also set to None.

## Observed failure and correction

The initial newly registered file package was resolved by UPM but its assemblies were not imported yet. First MCP test request (`a2bde9bd75de47f1861351714b525642`) returned a nominal Passed with **zero tests**. This is **not acceptance**. A targeted `AssetDatabase.ImportAsset("Packages/com.dreamy.debugger", ImportRecursive | ForceUpdate)` imported the package; assembly presence and nonzero test counts were then checked, and matching successful runs were recorded above. Avoid interpreting a successful test command as proof without expected tests.

During EditorSettings cleanup, `InternalEditorUtility.SaveToSerializedFileAndForget` rejected persistent settings objects and emitted `You may not pass in objects that are already persistent`. No code compile failure was observed. The attempted save was not used as a successful receipt; the original task-generated settings diff was restored with Git, while the Editor API value was restored to None. The later Console therefore contains this maintenance error; it is not a zero-error final Console claim.

Submodule implementation commits: `e6dc521` (foundation) and `0e27e74` (empty-folder metadata cleanup), pushed to the private remote.

## Not run / not accepted

No player build, IL2CPP/stripping, weak-device touch/performance measurement, screenshot capture, persistent outbox/recovery, real relay/destination, native-crash/ANR/OOM verification or full SR UI migration. The Editor playground was compiled/opened; that is not visual/manual acceptance. Per-phase acceptance matrix is `LocalPackages/com.dreamy.debugger/Documentation~/ACCEPTANCE.md`.
