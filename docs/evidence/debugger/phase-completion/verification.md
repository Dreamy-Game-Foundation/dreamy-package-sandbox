# Dreamy Debugger phase implementation / acceptance

Date: 2026-11-04. Unity: 6000.4.12f1, Linux Editor. Package preview: 0.2.0-preview.1.

The StompyRobot import was moved into the debugger submodule through AssetDatabase, preserving script/prefab GUIDs. Runtime code/resources/assemblies were renamed, legacy Editor/vendor endpoints replaced, and references/animation bindings updated through Unity APIs. A temporary compatibility type restored a deleted upstream script GUID so its obsolete prefab component could be removed. Final package validation: 28 prefabs, no missing scripts.

New native Control Center pages and original responsive runtime panel are implemented. Runtime smoke used the actual numeric keypad and default 0000, then displayed 11 grouped options (3 built-in + 8 editable project sample options). HUD is independent and hides while the full panel is visible to avoid covering its controls.

## Fresh matching receipts

The final receipt files and input hash manifest in this folder are authoritative; earlier/intermediate files are explicitly historical.

- Clean consumer EditMode covers access, typed options, bounds/permissions, report persistence/recovery/quota, retry/HTTP status, native provider import, assets/catalog, project sample customization and generic type names.
- Clean consumer PlayMode covers host lifecycle/unscaled access, real numeric/passphrase panel, all tabs, HUD single-sampler/toggles, local synthetic reports and release-off startup.
- Relay tests cover auth/schema/body caps, durable/duplicate acceptance, rate/storage limits, fake delivery, destination retry-after and dead state.
- C# relay smoke sent an actual HTTP report twice to the localhost fake relay: both acknowledgements Accepted; SQLite contained one report and one fake delivery attempt.
- Linux development/release builds and Android development IL2CPP ARM64 builds were run in a clean consumer containing no other Dreamy packages, Graphy or Input System. Release packed-assets receipt lists zero debugger assets. This is not a binary-code-size or mobile-runtime guarantee.
- Android APK inspection found INTERNET permission and `lib/arm64-v8a/libil2cpp.so`.
- Actual GPU capture produced a JPEG through async readback/worker encoding. `capture-receipt.json` and `warm-capture/capture-receipt.json` record cold/warm dispatch smoke timings; these are not percentile benchmarks.
- Retained runtime screenshots show the actual HUD and options. Native Editor desktop capture returned black; rejected image was removed and this limit is recorded separately.

## Observed failures and corrections

- Initial package registration previously produced a nominal Passed with zero tests; a targeted import and nonzero matching runs replaced it. Zero tests are not acceptance.
- Read-only package prefabs did not save in place. The authoring builder now moves owned assets into temporary Assets staging, edits with Prefab/Animation APIs and moves them back with the same GUIDs.
- A synchronous NUnit async assertion wrapper stalled an Editor test run. The wrapper was corrected and the sandbox Editor was restarted after confirming its scene was saved; acceptance tests then ran in an isolated consumer.
- Editor-only PlayMode assembly configuration misclassified UI tests; corrected to a PlayMode assembly before the successful run.
- UI teardown rebuilt option controls after revoking access. Destruction now hides/destroys the panel and detaches option views before relocking; subsequent PlayMode tests pass.
- A clean consumer exposed unused Physics/Physics2D helper references. Those unused helpers were removed rather than imposing game physics dependencies. Required animation/audio/JNI modules are declared.

## Unavailable acceptance

ADB reported no devices. No weak-device touch/GC/thermal/battery benchmark or native crash/ANR/OOM test was performed. No Telegram/Gmail/SMTP account credentials were supplied, so live destination delivery/deployment was not performed. iOS/Xcode/HDRP/WebGL are not certified. A previous abnormal-session marker is not a confirmed crash; native process death may leave no new screenshot.

No fake test deliberately crashes the user's scene or sends external messages. Reference relay runs in fake mode only for localhost smoke and is stopped afterward. Temporary MCP reconnect helper is removed after verification; no agent hook configuration is installed.

## Final run summary

- **64/64 EditMode**, **10/10 PlayMode**, zero failed/skipped in clean consumer. Includes the actual configured five-tap UI recognizer and invalid-configuration fail-closed regression.
- **13/13 Python relay tests**, plus actual C# HTTP idempotency smoke.
- Linux development, Linux release and Android IL2CPP ARM64: **Succeeded** with fresh matching inputs. Release packed debugger assets: **0**.
- `final-inputs-sha256.json` fingerprints implementation/test/asset inputs; `final-results.json` and final XML/build receipts carry exact results.

## Delivery checkpoint

Package commit `1f9a5e7`, annotated tag `v0.2.0-preview.1`, pushed to the private package remote. Final 263 source/test/asset fingerprints match this commit. The final extra capture attempt was interrupted by an unresponsive MCP/Editor session; it is not counted as new capture evidence. Earlier successful cold/warm GPU captures remain explicitly scoped smoke evidence.

The owned Editor session required a restart after the transport stopped answering. Scene recovery was preserved at `/tmp/dreamy-debugger-recovery-delivery` before temporary recovery assets were removed through AssetDatabase. Final MCP state: not playing; `Assets/_Project/Scenes/BootstrapScene.unity`, not dirty; Dreamy Debugger window focused; zero Console error entries. Temporary reconnect source was removed through AssetDatabase. No unrelated localization font modifications were included in the debugger commit.
