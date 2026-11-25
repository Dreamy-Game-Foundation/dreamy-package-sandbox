# Debugger mobile setup checkpoint — 2026-11-25

## Reporting root-cause audit — 2026-11-26

- New scope: inspect Direct-vs-Relay differences, credentials, Android networking, a text diagnostic, regression tests and actual APK behavior. No built-in Direct Sender was found; both Editor and runtime construct RelayTransport. Python relay owns provider adapters and OS-stored credentials.
- Existing APK reproduced on LDPlayer: text `QA_ANDROID_TEXT_ONLY_20261126` saved with no image, UI shows Save report / Saved locally. Android INTERNET permission granted. Same APK remains the 2026-11-25 binary; new diagnostic fixes have not been built into it. Loopback runtime guard prevents request construction, so there is no failing HTTP response in this reproduction.
- Explicit Windows direct text probe: both provider APIs returned HTTP200 with acknowledgement; test ID `c571d8ac15f3484cacc3e8738fb026f3`. Subsequent image fixture `52f166c75fad477a8671932e42c0ed95` received HTTP202 and both relay destination receipts delivered on attempt1. These are Windows/server evidence, not APK direct delivery.
- Exact saved provider credentials were absent in decompressed entries of the existing APK; scan prints booleans only. This is not an exhaustive secret scan.
- Fixed lost transport diagnostics: RelayResult retains HTTP status and sanitized failure category; network versus timeout remain distinct and caller cancellation propagates. Pump exposes latest result, runtime exposes LastUploadResult and immediate manual failure summary; Editor tests show the same safe classification. No direct runtime adapter or credential packaging added.
- Fresh matching checks: 68 C# EditMode tests passed (including15 new diagnostic cases), 44 Python tests passed. XML `LDPlayerQA/transport-diagnostic-editmode.xml`, end2026-11-25T17:27:12Z. Root Unity Console returned3 CoreCLR/GC initialization errors after refresh; independent consumer compile/tests passed. Do not claim root Editor zero-error verification.
- Complete findings/proposal: `docs/evidence/DEBUGGER_REPORTING_DIAG_2026-11-26.md`. External scripts, JSON receipts and screenshots are under `D:/Game/DreamyDebuggerValidation/LDPlayerQA`. No new tunnel attempt, production secret embedding, push or branch change.

## Latest delivery preparation — 23:38 local

- User explicitly requested checking/preparing again so the next APK can deliver. User then asked the agent to find available HTTPS configuration independently. Read-only search found no HTTPS relay candidate in project/QA configuration, logs or relevant environment; no active cloudflared/ngrok/caddy process or local HTTPS listener was found. Only live local relay port8766 exists.
- A narrowly scoped attempt to start cloudflared was again rejected with `blocked by policy`. A separately scoped verified-owner local relay restart was also rejected. Neither command executed. Do not retry through another execution mechanism. Existing relay PID24420 remains untouched and on older scripts.
- Fresh synthetic Windows fixture `b08de2a2be894fe28131a8d6083baac5` received202; local durable receipts confirm Discord and Telegram both delivered, one attempt each. This is local HTTP/Windows evidence, not HTTPS or APK delivery.
- Limited ingestion key (not provider credentials) is prepared under Library for both managed launchers. Root and snapshot report URL/key/upload/screenshot fields match. Current root/snapshot still use localhost; no fabricated URL has been applied.
- Added target-aware build validation: Android/iOS uploads enabled with loopback/non-HTTPS/malformed URLs fail. Startup preprocess passes the real build target and validates before creating staged assets. Scene-owned configurations are checked using the report platform. Offline uploads-disabled and desktop localhost builds remain supported.
- Fresh isolated consumer EditMode receipt `LDPlayerQA/mobile-prebuild-editmode.xml`: 28 passed, 0 failed, end2026-11-25T16:36:38Z. Includes actual startup staging rejection and existing startup regressions. Root MCP refreshed; error Console read returned0. Package whitespace check passed.
- Current readiness receipt `D:/Game/DreamyDebuggerValidation/LDPlayerQA/mobile-readiness.json` explicitly marks delivery readiness/build-for-delivery false. No new APK was built during this preparation. Next Android build intentionally fails on current uploads-enabled localhost until a verified HTTPS relay is configured.
- Optional async question is pending for an existing HTTPS relay URL if the user has one; do not ask for tokens/webhooks. Without a reachable endpoint or a permitted way to provision it, mobile delivery remains blocked. Existing OS credentials need not be sent again.

## Subsequent build retry — 23:22 local

- User requested retry. Unity batch build in the existing snapshot succeeded in 189.7 seconds, zero errors, eight warnings; exit 0. Log: `LDPlayerQA/android-rebuild.log`. Short Gradle home `D:/dqg-1125` was retained. Package C# source comes from the existing LocalPackages junction; project configuration is the previously captured QA snapshot.
- Refreshed APK: `D:/Game/DreamyDebuggerValidation/LDPlayerQA/DreamyDebuggerQA.apk`, 104107944 bytes, SHA256 `dc3e545dc38bf242b6898e6b6d5ea71182814045301c74f37093e9b5e1fca827`, APK v2 signature verified, ARM64, min SDK25/target36. Unity summary size includes build outputs and is not the APK file size; receipt separates both.
- LDPlayer streamed install failed without detailed reason; retry with `adb install --no-streaming -r` succeeded. App launched with PID4011, activity resumed; fresh Unity/AndroidRuntime error-level query returned no errors. This is a startup smoke check, not full runtime acceptance.
- Downloaded official cloudflared Windows executable (2026.11.2), Authenticode valid, under external QA folder. No tunnel was started: both attempted relay restart/tunnel launch and alternate new relay/tunnel launch were rejected by automatic approval review. Do not route around that rejection. No additional reason was returned. User was informed; existing relay was left running.
- APK retains localhost and therefore cannot verify mobile report delivery. HTTPS configuration and actual APK Discord/Telegram delivery remain incomplete. No push, branch change or provider credential output was performed.

Latest user priority: provide the full user setup workflow before the next Android build. Setup implementation is complete; live HTTPS tunnel and APK destination delivery remain pending. Do not confuse previous Windows fixtures with APK evidence.

## Checkout and changes

- Original project: `D:/Game/dreamy-package-sandbox`, branch `main`, baseline `4f20c5006decfcb68cc8cbc4bbbceed265bae3ac`.
- Debugger submodule baseline `ca1bef9ab67c33742c91c97ae11f63f3607e43b2`; setup edits are uncommitted. Preserve unrelated project asset/build-setting changes.
- Added Editor-only `ReportHttpsSetup`: limited ingest-key preparation, live/protected local and HTTPS health checks, cloudflared install guidance and executable selection, window-owned tunnel start/stop, verified URL application with uploads initially disabled, and direct setup checklist access.
- Managed launchers load the limited ingest key from Library. Saved-relay stop checks both project/destination identity and saved PID, retaining OS credentials. Health optionally validates supplied ingest headers. Editor managed-relay tests retain the configured ingest key.
- Corrected Reports tab mapping and moved local setup immediately below its entry button.
- User guide: `LocalPackages/com.dreamy.debugger/Documentation~/MOBILE_REPORT_SETUP.md`; linked from reporting and relay docs.

## Matching evidence

- Python relay suite: 44 tests passed on 2026-11-25.
- Isolated consumer EditMode setup suite: 14 passed, zero failed; XML `D:/Game/DreamyDebuggerValidation/LDPlayerQA/setup-editmode.xml`, start `2026-11-25T16:04:11Z`.
- Native MCP selected `dreamy-package-sandbox@4c906ebe8d7b45ef`; confirmed original Assets path. Refreshed compilation; subsequent Console error query returned zero entries.
- Actual Reports renders inspected at 700 and 440 logical pixels, 1.5 DPI, dark skin. Light skin, live tunnel execution and device delivery have not been verified for these changes.
- Package diff whitespace check passed. Root whitespace check reports pre-existing whitespace in `Assets/DebuggerConfiguration.asset`; do not rewrite unrelated content.
- Installed workflow points to `docs/agents/dreamy-workflow.md`, absent in this checkout; installed CLI did not supply the requested lessons command. No toolkit receipt is claimed.

## Continue after setup

- Existing APK: `D:/Game/DreamyDebuggerValidation/LDPlayerQA/DreamyDebuggerQA.apk`; installed on LDPlayer. It uses localhost, rejected by mobile runtime, so does not prove delivery.
- Build snapshot: `D:/Game/DreamyDebuggerValidation/SandboxAndroidQA-20261125`; Gradle recovery home `D:/dqg-1125` avoids Windows long-path native build failure. Keep original Editor open; build in the snapshot.
- A local live relay previously ran on port 8766 using saved OS credentials, launched by external `LDPlayerQA/run_saved_live_relay.py`. Recheck live PID/health before restart; its custom profile is not a managed relay and the new saved-stop button should refuse to stop it.
- Provider credentials are in the Windows credential store. Do not print them or put them in source, command arguments, build artifacts or handoffs. No credentials are included here.
- No cloudflared install/tunnel has been executed yet. Use a separate terminal-owned tunnel for the APK build so Unity domain reload cannot terminate its URL. Keep the relay and tunnel alive, verify HTTPS, update root and snapshot configurations, rebuild/install, submit an actual runtime report and correlate its ID and receipts for both destinations.
- A window-owned tunnel intentionally stops when the window closes, scripts reload or Unity quits; restart changes the URL and requires rebuilding clients. Documented in the product checklist.
