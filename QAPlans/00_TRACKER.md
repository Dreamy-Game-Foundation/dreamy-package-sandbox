# QA tracker

| Plan | Status | Evidence |
|---|---|---|
| [07 Direct QA reporting](07_DIRECT_QA_REPORTING.md) | EDITOR PASS · Android APK works, user-confirmed | The user confirmed Direct QA reporting works on their tested APK. APK revision, provider coverage, and individual acceptance cases remain unspecified. |
| [07 Embedded QA destination](07_DIRECT_QA_REPORTING.md#embedded-destination-for-internal-android-qa) | BUILD/STATIC PASS · device pending | Internal Android Development APK with Discord preset and Store Off APK verified. 83 EditMode, 6 PlayMode and 44 Python passed. |
| [07 Dual-destination QA wizard](07_DIRECT_QA_REPORTING.md#dual-destination-qa-wizard) | BUILD/STATIC PASS · device pending | Build embeds Discord and Telegram. PlayMode exercises destination selection without credential fields. Store Off omits both credentials and executable Direct QA. |
| [07 Direct message formatting and screenshot orientation](07_DIRECT_QA_REPORTING.md#direct-message-formatting-and-screenshot-orientation) | EDITOR PASS · Android pending | Mocked provider payloads and shared JPEG capture on Vulkan and OpenGL Core passed. Android build paused at user request. |
| [07 Reports setup checklists](07_DIRECT_QA_REPORTING.md#reports-setup-checklists-and-saved-destinations) | EDITOR PASS · Android deferred | Both saved profiles loaded locally. Three mode checklists rendered in Unity. 85 EditMode, 6 PlayMode, and 46 Python tests passed. |
| [07 Post-audit hardening](07_DIRECT_QA_REPORTING.md#post-audit-hardening) | EDITOR/ARCHIVE PASS; Android and distribution BLOCKED | 178 EditMode, 115 PlayMode, 48 Python. Clean UPM install and archive parity passed. |
| [07 Dreamy outbox sync](07_DIRECT_QA_REPORTING.md#dreamy-outbox-sync-checkpoint) | EditMode PASS | 180 tests passed, including transient read and locked cleanup. |
| [ThunderPunch independent QA](../docs/plans/THUNDERPUNCH_DEBUGGER_ASSET_STORE_2026-11-27.md#preview-độc-lập-không-có-relay) | Editor/Windows/Android build PASS · device pending | No Relay. 17/17 in each Editor configuration. Android ARM64 Development APK has INTERNET and a verified v2 signature. |
