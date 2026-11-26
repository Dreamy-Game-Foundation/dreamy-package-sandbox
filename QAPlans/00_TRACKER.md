# QA tracker

| Plan | Status | Evidence |
|---|---|---|
| [07 Direct QA reporting](07_DIRECT_QA_REPORTING.md) | EDITOR PASS · Android build deferred | Latest checks cover saved destinations, per-mode checklists, formatted Direct messages, and screenshot orientation. Earlier APK inspection remains recorded below. The latest changes are awaiting a new APK and physical Android acceptance. |
| [07 Embedded QA destination](07_DIRECT_QA_REPORTING.md#embedded-destination-for-internal-android-qa) | BUILD/STATIC PASS · device pending | Internal Android Development APK with Discord preset and Store Off APK verified. 83 EditMode, 6 PlayMode and 44 Python passed. |
| [07 Dual-destination QA wizard](07_DIRECT_QA_REPORTING.md#dual-destination-qa-wizard) | BUILD/STATIC PASS · device pending | Build embeds Discord and Telegram. PlayMode exercises destination selection without credential fields. Store Off omits both credentials and executable Direct QA. |
| [07 Direct message formatting and screenshot orientation](07_DIRECT_QA_REPORTING.md#direct-message-formatting-and-screenshot-orientation) | EDITOR PASS · Android pending | Mocked provider payloads and shared JPEG capture on Vulkan and OpenGL Core passed. Android build paused at user request. |
| [07 Reports setup checklists](07_DIRECT_QA_REPORTING.md#reports-setup-checklists-and-saved-destinations) | EDITOR PASS · Android deferred | Both saved profiles loaded locally. Three mode checklists rendered in Unity. 85 EditMode, 6 PlayMode, and 46 Python tests passed. |
