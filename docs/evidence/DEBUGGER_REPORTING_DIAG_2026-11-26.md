# Dreamy Debugger reporting diagnostic — 2026-11-26

## Finding

The APK does not contain a built-in Direct Discord/Telegram sender. Editor tests and Android runtime use `RelayTransport`; Python on the relay performs provider API calls. The current QA build contains `http://127.0.0.1:8766/reports`. `DebuggerRuntime` refuses mobile loopback before creating the transport/pump. Editor success therefore cannot establish Android connectivity or direct-provider support. The observed failure is build configuration and unsupported mode, not an observed provider HTTP, attachment, rate-limit or IL2CPP failure.

Confirmed source path:

1. `Editor/DebuggerWindow.cs`: selected URL becomes `/reports/discord-test` or `/reports/telegram-test`, sent by RelayTransport. A managed local relay can be selected for a destination; this is still relay delivery.
2. `Editor/TelegramRelaySettings.cs` and `Tools~/relay/telegram_setup.py`: prompts and system credential store, keyed by the host project. Python loads the provider secret into its own relay process environment. Editor preferences/Library metadata are local, not Android configuration.
3. `Editor/DebuggerStartupBuild.cs` clones the configuration selected by `Editor/DebuggerProjectSettings.cs` into a build-scoped Resources payload. `DebuggerBootstrap` uses the Editor provider in Editor and Resources in a player. Runtime report config contains the relay URL, limited ingest key and upload policy, with no bot-token/webhook fields.
4. `Runtime/Engine/DebuggerRuntime.cs`: provider-credential rejection, mobile loopback rejection, then RelayTransport and OutboxPump. Local save is supported without a pump. The imported InternalBugReporterHandler is unusable by default; runtime installs its OutboxReportHandler rather than a direct provider sender. No project custom sender override was found.
5. `Tools~/relay/server.py`: provider adapters are server-side. Telegram sends report.json and optional original JPEG using sendDocument; Discord sends payload_json plus files as multipart with wait=true and an identifying User-Agent. Provider-specific payloads differ from the client's schema1 JSON.

## Protocol and diagnosis

- Client POST: application/json, schema1 metadata, optional jpegBase64, stable Idempotency-Key and optional X-Dreamy-Key. HTTP timeout15 seconds, redirects disabled, HTTPS required outside local desktop loopback. JPEG cap512KiB, context256KiB; relay request cap3000000 bytes. The APK guard prevents entering this path under the observed localhost configuration.
- Relay ingest: 200/202 accepted by client, 401 ingest authorization rejection, 400 validation/idempotency rejection, 413 size rejection, 429 throttling, 503 storage unavailability. A202 follows SQLite persistence; it is not provider delivery.
- Provider requests:15-second timeout, bounded retries with Retry-After, Telegram ok acknowledgement validation, Discord wait=true. Durable destination receipts carry provider HTTP errors without secret URLs/body. Default relay ingest rate30/minute and bounded storage can independently limit delivery.
- Before this change, C# discarded exact HTTP status and OutboxPump collapsed exceptions to generic Retry. Immediate manual status could say pending even for a permanent rejection. This diagnostic loss is an observed implementation defect; it was repaired without changing relay wire format or adding Direct Mode.
- New safe result: HttpStatusCode and RelayFailure categories Authentication, Payload, RateLimit, Timeout, Network, Server, Endpoint and Unexpected. HttpStatusCode0 means no response. Pump/runtime expose latest completed result; Editor and immediate manual failures show the safe summary. No credentials, response bodies, headers or exception messages are emitted. Authentication here is relay ingest authorization, not a verdict on provider credentials.

## Fresh evidence

| Check | Result | Evidence / limits |
|---|---|---|
| Direct vs relay source tracing | PASS | No built-in direct runtime sender; relay-only configuration and construction |
| Plain text provider test from Windows | PASS | Discord200 with message acknowledgement; Telegram200 with ok=true; IDc571d8ac15f3484cacc3e8738fb026f3 |
| Subsequent image report from Windows through relay | PASS | ID52f166c75fad477a8671932e42c0ed95, ingest202; both destinations delivered on attempt1 |
| Existing Android APK reproduction | FAIL for sending; PASS for local save | LDPlayer Android9; entered QA_ANDROID_TEXT_ONLY_20261126 without screenshot, got Saved locally and Save report button |
| INTERNET permission | PASS | aapt APK permissions and installed dumpsys both show INTERNET granted |
| APK architecture/signature | PASS for inspected binary | IL2CPP ARM64, minSDK25/target36, APK v2; SHA256dc3e545dc38bf242b6898e6b6d5ea71182814045301c74f37093e9b5e1fca827 |
| Known provider credentials in APK entries | PASS | Exact saved token/webhook absent after decompression; not exhaustive auditing |
| C# focused regression suite | PASS | 68 passed,0 failed,15 new diagnostic tests; fresh XML end2026-11-25T17:27:12Z |
| Python adapter/relay/setup suite | PASS |44 passed |
| Root Editor refresh/Console | BLOCKED |3 CoreCLR/GC heap initialization errors; independent consumer compilation/tests passed |
| APK HTTPS/provider delivery and patched APK | BLOCKED | No reachable configured HTTPS relay; direct adapter does not exist. Existing binary only reproduces the previous configuration; new C# diagnostic changes are not device-validated |

External evidence directory: `D:/Game/DreamyDebuggerValidation/LDPlayerQA`.

- `direct_text_diagnostic.py`, `direct-text-diagnostic.json`: repeatable explicit provider text probe; credentials read from Windows vault, never stored by the script. Running it sends one message to each destination.
- `delivery-fixture.json`: subsequent synthetic image report ID; use server.py --delivery-status with relay-live.sqlite for durable per-provider status.
- `scan_apk_provider_credentials.py`, `apk-provider-credential-scan.json`: only booleans are printed; no matched contents or credentials exported.
- `transport-diagnostic-editmode.xml` and matching log: C# suite.
- `android-report-current.png`, `android-report-text-saved.png`: inspected real screenshots of the existing APK before/after text-only local save.

## Smallest proposed QA-only Direct Mode

This is a proposal, not an implementation in this change:

1. Keep IBundleTransport/OutboxPump and the default relay unchanged. Allow a host-supplied QA transport factory at the runtime composition root; dispose the chosen transport through its owner instead of hard-coding a RelayTransport field.
2. Add isolated DiscordDirectTransport and TelegramDirectTransport adapters behind an explicit QA opt-in and Development build compilation gate. DebuggerMode.Qa alone is not a security boundary. Non-Development player builds must reject direct selection and omit the adapters/credential provisioning UI.
3. Supply credentials after installation into memory from explicit QA input or a QA provisioning source. Do not serialize credentials into DebuggerConfiguration, Resources, prefabs, PlayerPrefs or production builds. Desktop OS credentials cannot magically be used by Android; Android needs its own explicit QA provisioning. A QA device can still expose runtime secrets, so use restricted disposable destinations.
4. Probe text first (Discord content JSON with wait=true; Telegram sendMessage with chat_id/text). Only after acknowledged success send provider-specific multipart JSON/JPEG attachments. Reuse cancellation, bounds, safe error classification and retry policy; do not send the relay schema to a provider URL.
5. Regression gates: release compile/build excludes adapters and credential assets; missing QA credentials fail without network; text, JSON/JPEG,401/403/404,413,429/Retry-After,timeout,cancel,redaction tests; IL2CPP Development APK text and image receipts on device.

For the current implementation, the operational fix is still to configure and test a reachable HTTPS relay, then build with the same URL/key. Changing INTERNET permission, allowing cleartext, disabling TLS validation or embedding provider credentials would not fix the observed missing-direct/localhost path.

Official payload references: [Discord Execute Webhook](https://github.com/discord/discord-api-docs/blob/main/developers/resources/webhook.mdx), [Telegram Bot API](https://core.telegram.org/bots/api#sendmessage).
