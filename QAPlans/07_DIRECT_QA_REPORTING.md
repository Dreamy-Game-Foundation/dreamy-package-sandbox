# 07 — Direct QA reporting

Status: IN PROGRESS. Scope: Android Development/controlled QA direct Discord or Telegram; existing relay and outbox retained. No cloud relay/hosting changes.

## Architecture and tasks

- [x] Read reporting diagnostic, transport/pump/outbox, runtime composition, build staging, Editor settings and runtime Report form.
- [x] Decision: separate Development-only direct adapter assembly; shared IBundleTransport/OutboxPump. Device credentials are session-only and never configuration fields.
- [x] Implement direct text, JSON/JPEG, bounded responses, provider acknowledgements and status/retry classification.
- [x] Durable per-destination step receipts. Ambiguous sends require explicit duplicate-risk confirmation; preserve acknowledged Telegram steps across restart/retry.
- [x] Local Only / Direct QA / Relay config with backward-compatible existing Relay+UploadsEnabled behavior.
- [x] Locked QA session device provisioning UI, text connection test, image/JSON test, forget and uncertain retry controls (device UX validation pending).
- [ ] Production/ReleaseDiagnostics exclusion at build and runtime; credential/APK scan.
- [ ] C#/Python regression, Android ARM64 IL2CPP Development build and installation.
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
