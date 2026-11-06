# Dreamy Package Manager verification — 2026-11-06

Working-tree implementation in `com.dreamy.editor-tools` on Unity 6000.4.12f1; version remains 1.1.0, no release/tag published. `verification.json` records scope, gates and limitations; `source-fingerprints.json` identifies final C#/catalog/assembly inputs.

- `EditMode.xml`: final clean-consumer run, 46 passed, 0 failed/skipped. This includes 37 package-manager cases and 9 existing screenshot cases. Final subsequent UI wording/unresolved-manifest display changes were compiled and checked for Console errors in the sandbox; model/provider/planner test inputs were unchanged.
- `sandbox-EditMode.json`: real sandbox bridge run after reconnect, 45 passed, 1 screenshot batch-only case skipped, 0 failed.
- `consumer-smoke.json`: live GitHub core and configured Unity registry metadata; Git install 1.0.0 → update 1.1.0 → restore 1.0.0 → remove using the production operation coordinator. All pinned versions/hashes verified.
- Self-update changed the isolated consumer's editor-tools from local source to a Git fixture at 1.1.1. UPM journal completed and verified before reload; the test-assembly driver stopped ticking when its own assembly source changed. Restarting the consumer and running `PackageSmokeResume.Run` verified version/source/commit and completed the receipt. This is not a claim that every historical editor-tools version supports reconciliation.
- `PackageManagerConsumerSmoke.cs` and `PackageSmokeResume.cs` are consumer-only test harness sources, outside Assets/Packages and not shipped in the package. Copy the first into the isolated package's Tests/Editor (friend assembly); copy the second into consumer Assets/Editor. Fixture refs are in `fixture-revisions.json`; repositories were temporary local Git repositories. Unity supports local Git dependencies via `file://` URLs: https://docs.unity.com/en-us/engine/6000.0/manual/packages-list/upm-git.
- `consumer-manifest.json` / `consumer-packages-lock.json`: final clean consumer dependencies, using a copied package to keep mutation tests isolated from the sandbox.
- `consumer-package-check.json`: pass. `sandbox-asmdef.json`: pass. `sandbox-package-check.json`: failed on an existing BOM in StompyRobot.SRF.asmdef; left untouched.

A first bridge run stalled the sandbox Editor while using a synchronous cancellation assertion. The assertion was replaced with an awaited async test; the consumer and later sandbox runs passed. The user authorized restarting Unity and reconnecting MCP. The final Editor opened the Dreamy Packages window (840×552), MCP responded for the correct project, Bootstrap scene was not dirty, and Console had no errors. The temporary reconnect script was removed. Pixel-level UI validation remains unavailable: the desktop provider reports no screenshot capability and AT-SPI observation timed out.

Reproduce the focused clean-consumer run (using the local Unity executable):

```sh
Unity -batchmode -nographics -projectPath /tmp/dreamy-package-manager-consumer-20261106 -runTests -testPlatform EditMode -assemblyNames Dreamy.EditorTools.Tests.Editor -testResults EditMode.xml -logFile test.log
```

The smoke tests do not upgrade any sandbox dependency. The only sandbox manifest change enables `com.dreamy.editor-tools` in `testables`.
