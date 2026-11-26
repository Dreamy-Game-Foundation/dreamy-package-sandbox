# 07 — Direct QA reporting

**Current checkpoint (2026-11-26)** · Editor verification passed for saved Discord and Telegram setup, three delivery-mode checklists, formatted Direct reports, and screenshot orientation. Android build is deferred at the user's request. Physical-device delivery and offline/retry acceptance remain open.

## Architecture and tasks

- [x] Read reporting diagnostic, transport/pump/outbox, runtime composition, build staging, Editor settings and runtime Report form.
- [x] Implement a Development-only direct adapter assembly using the shared transport and outbox. Editor credentials come from the local credential store or session input. Android Development builds can include those credentials through temporary staging. Project assets do not store provider credentials.
- [x] Implement direct text, JSON/JPEG, bounded responses, provider acknowledgements and status/retry classification.
- [x] Durable per-destination step receipts. Ambiguous sends require explicit duplicate-risk confirmation; preserve acknowledged Telegram steps across restart/retry.
- [x] Local Only / Direct QA / Relay config with backward-compatible existing Relay+UploadsEnabled behavior.
- [x] Editor credential provisioning and destination tests. The QA runtime exposes destination selection, report tests, and explicit retry for uncertain deliveries. Device UX validation is pending.
- [x] Production/ReleaseDiagnostics configuration and runtime guards.
- [x] Static APK evidence for ReleaseDiagnostics exclusion.
- [x] C#/Python regression and Android ARM64 IL2CPP compilation on the previous machine.
- [ ] Install and run the current Development APK on a physical Android device (tester-owned).
- [ ] Actual direct provider delivery with local relay stopped; manual/auto, attachments, offline/retry and UI evidence.

## Evidence

## Progress checkpoint — 2026-11-26, requested push

- C# focused regression: **PASS,88 tests**,0 failed. Fresh XML ends2026-11-25T19:07:33Z: `D:/Game/DreamyDebuggerValidation/LDPlayerQA/direct-qa-editmode.xml`. Includes direct adapters, partial Telegram429 retry, duplicate suppression after reopen, destination separation, cancellation, invalid credentials, relay regression, build rejection and secret-free diagnostics.
- Python regression rerun before push: **PASS,44 tests**,16.024 seconds. Staged changes in both repositories scanned against the two saved provider credentials:51 files,0 matches; no credentials printed. This is an exact-known-secret check, not an exhaustive security audit.
- Android Development build: **PASS for compilation/package generation only**, Unity6000.4.12f1, IL2CPP ARM64,0 errors,45 warnings,183.04 seconds. New artifact: `D:/Game/DreamyDebuggerValidation/LDPlayerQA/DreamyDebuggerQA.apk`; log `direct-qa-android-build.log`, receipt `build-receipt.json` beside it.
- Local live relay was stopped after checking ownership for the upcoming direct device test. No tunnel/cloud service was created.
- Source implements Development compilation gates plus runtime mode/unlock checks. Production rejection tests pass, but a new non-Development APK exclusion scan is **NOT RUN**.
- Device delivery, screenshot/JSON acknowledgement, auto-report, offline/retry and responsive UI on the new APK are **NOT RUN / PENDING**. Do not mark Android acceptance PASS from build/tests alone.
- External owned build snapshot includes a Development-only validation harness with no embedded credentials. Its post-install private input is deleted before network use; it is not distributed in the package. Build/test helpers and local artifacts remain outside Git. A physical PC-off check cannot be established using PC-hosted LDPlayer; physical Android validation remains separate.
- User requested pushing all current workspace changes. This checkpoint is IN PROGRESS, not a release-ready claim. Existing sandbox render/font/build-setting changes are included under that explicit request; no reset/discard performed.

## Linux continuation — 2026-11-26

- Pulled sandbox `c79e150` and package `1be1d04`. The diagnostic above describes the earlier relay-only APK. Direct QA is present in the current source through `IBundleTransport` and the existing disk outbox.
- Discord uses the documented DiscordBot User-Agent. HTTP 413/429 keep their classification even if the error response body is oversized. A manual retry respects offline state. The duplicate-risk confirmation is bound to the selected report ID and resets when the session/destination changes.
- On Android Development in Direct QA, the native keyboard can take focus while editing a Report field without relocking the debugger. App pause still locks the debugger. Closing/hiding the panel clears input that has not been applied.

### Fresh Linux evidence

| Check | Result | Evidence |
|---|---|---|
| Unity 6000.4.12f1 focused C# regressions | PASS | 81 EditMode and 6 PlayMode, no failures. Includes adapters, provider errors, outbox/idempotency, credential masking and cleanup on close, and Relay/build guards. |
| Python relay/setup tests | PASS | 44 tests. |
| C# RelayTransport → local Python fake relay | PASS | Same ID received Accepted twice and SQLite stored one report. |
| Android Development / QA APK generation | PASS | Unity 6000.4.12f1, IL2CPP ARM64, 0 build errors. Actual archive 44,927,987 bytes. Application ID `com.dreamy.debugger.directqa`, Android API 25+, target API 36, INTERNET present, debuggable, v2 signature verified. |
| Android ReleaseDiagnostics / Local Only APK generation and static check | PASS | Non-Development, non-debuggable, IL2CPP ARM64, 0 build errors. The direct senders and provisioning markers are absent from executable IL2CPP metadata/code. Serialized MonoScript names can still appear in Unity data. Both APKs matched zero provider credential patterns outside a confirmed stock Unity native-library byte match. |

APK and receipts are at `~/Downloads/DreamyDebuggerQA-2026-11-26/` on the Linux machine. The QA APK uses an isolated demo scene with screenshot targets, the package's normal Report UI, and a separate application ID. It carries no credential provisioning harness. The ReleaseCheck APK and logs/XML/static report are under `verification/`. These are QA artifacts.

- QA APK SHA-256 `29b48df8c75cafdc2adda55db2748fcd006eebddd6838737fa1ba9cee3da97c6`
- ReleaseCheck APK SHA-256 `1bbddac136266b8379ea9b8d31df7eb3c1dc92d5b7ada35999e4a206a6668148`

### Physical device acceptance

| Check | Status | Evidence to capture |
|---|---|---|
| Discord, phone on Internet with PC and relay off | PENDING | Text acknowledgement, report ID, `report.json` and JPEG in the QA channel |
| Telegram, phone on Internet with PC and relay off | PENDING | Text acknowledgement, report ID on both documents, original JPEG opens |
| Manual and automatic reports | PENDING | Send a manual draft, then use Auto report and **Test exception**; confirm both report IDs arrive |
| Offline and resume across app restart | PENDING | Save with airplane mode on, relaunch, reapply session credentials, restore network and confirm one delivery |
| Ambiguous delivery | PENDING | Interrupt an active request, confirm no silent duplicate, then test the explicit retry confirmation if needed |
| Native Android input and layout | PENDING | Credential typing/paste, keyboard dismissal, pause/return, portrait/landscape and screenshot attachment on the device |

## Embedded destination for internal Android QA

- Implemented an opt-in destination preset under Editor Reports. Discord or Telegram credentials stay in the current Editor session, keyed to the selected configuration asset. A temporary build resource is created only for Android Development with Direct QA and uploads enabled. Store mode Off skips debugger staging even when that Editor session has a QA preset.
- Android QA loads the preset at startup and can send reports immediately. Opening the Report tab or reselecting the same provider keeps the configured transport. Manual on-device setup remains available.
- Built the internal Discord APK using the project-owned local Discord relay's destination, selected as the default provider. The extracted credential was handed to the build through a private temporary input and was omitted from Git and the receipts. Discord and Telegram staging were both covered by Editor fixtures.
- Device installation and live delivery stay pending for the tester. The APK contains the selected credential and is restricted to internal QA distribution.
- Fresh checks passed: 83 EditMode, 6 PlayMode and 44 Python. Both ARM64 IL2CPP APKs built with zero errors and both signatures passed v2 verification. The exact Discord credential and executable Direct QA markers were present in Development and absent in Store Off. Temporary input and staging were removed and the exact credential was absent from pending Git source files.
- Artifacts and static receipts: `~/Downloads/DreamyDebuggerQA-Embedded-2026-11-26/`. Development SHA-256 `52e7be60f50599f550550fa7719eb91aba426be402d00acd31f3c23519ed94f7`. Store Off SHA-256 `6f295410f50720011017560686e29244c4414537787542c54f8c2752df7cb2ba`.

## Dual-destination QA wizard

- Editor checklist covers QA policy, Discord/Telegram credentials, per-provider test acknowledgement, and Android build readiness. The configuration asset contains no provider credentials. The Editor session owns both presets and the APK receives them only for Android Development with Direct QA enabled.
- Android Report exposes destination selection and marks the active provider. It has no provider credential inputs. The built-in default is restored on restart. Pending delivery follows the current selection. Switching is blocked while an upload is in progress.
- Local relay setup has an **Install Python** action. Optional HTTPS setup has an **Install cloudflared** action. Windows uses winget, macOS uses Homebrew or opens the official installer page, and Linux uses a supported package manager for Python or project-local cloudflared. Installation requires an explicit click. Installer flows have not been executed on Windows or macOS.
- User confirmed Direct Discord delivery before these changes. The dual-profile APK has been built and inspected but installation and new live Discord/Telegram delivery are still tester-owned. APKs and receipts are under `~/Downloads/DreamyDebuggerQA-Wizard-2026-11-26/`. Direct QA APK SHA-256 `a3dcbd8d8f409d27ad33b05c3a0616cd048422d0f70ecedce0c63789ff547ed9`. Store Off omits both selected credentials and Direct QA executable markers.
- Current regression passed 83 EditMode, 6 PlayMode and 44 Python tests. Unity 6000.4.12f1 built both ARM64 IL2CPP APKs with zero errors. Both v2 signatures verified. The Linux Unity window opened with test data; the native screen-read returned a black image, so visual capture is unverified.

## Direct message formatting and screenshot orientation

- Discord uses a color-coded embed with device, scene, performance, a stack preview, Report ID, and the attached JPEG preview. Telegram uses a formatted report caption and a separate JPEG document carrying the same Report ID. The attached JSON retains the full captured stack and console excerpt.
- A two-color frame reproduced the vertical flip in the old Vulkan readback path. The shared capture code now corrects top-origin readback during the existing downscale. The actual JPEG output from `CaptureCoordinator` preserved top and bottom on both Vulkan and OpenGL Core in Unity 6000.4.12f1 on Linux.
- Editor and relay checks passed with 85 EditMode, 6 PlayMode, and 44 Python tests. Provider payload tests used mock HTTP. The Android build was stopped at the user's request. No APK containing the orientation fix was built, and physical Android verification remains pending.

## Reports setup checklists and saved destinations

- Reports now shows a dedicated checklist for Local Only, Direct QA, and Relay. Local relay tools, mobile HTTPS setup, capture options, and developer tests are collapsible.
- Direct QA reads the current project's saved Discord and Telegram profiles through the OS credential store. It fills missing session entries and masks provider credentials in the Editor. Relay can fill the chosen local URL and its saved limited key. The active configuration already has a limited key, while neither local launcher has a separate saved ingest-key file.
- The helper restored both real saved profiles on Linux without sending a message. Unity screenshots used fixture credentials and covered the three modes at normal width and Direct QA at 440 px. The push checkpoint reran the regression checks with 85 EditMode, 6 PlayMode, and 46 Python tests passed. APK build and device testing remain deferred.
