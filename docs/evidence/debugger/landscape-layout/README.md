# Landscape layout — 2026-11-06

Status: complete. Dreamy Debugger 0.1.2, final package commit de6b75ffdb66b3b1c8fdd8bfb6a67f419d5672d6 pushed to origin/main. No tag created. Asset/prefab/meta GUIDs and assembly definitions are preserved; runtime adapts existing prefab instances.

## Result

- HUD ignores its old SafeArea component and stretches its parent across the full viewport. Bottom-left/corner/free anchors and saved normalized dragging remain supported. Default footprint is bounded by viewport size with no DPI amplification; explicit HudScale still applies. The small visible grip has a separate transparent minimum-48px touch region. Other HUD graphics pass through.
- PIN is centered inside the safe viewport. Landscape uses prompt/dots beside the full numeric keypad; portrait keeps the vertical form. Both fit the available dimensions with padding and a maximum 84% footprint on constrained viewports. Passphrase input, Cancel and Submit fit short landscape. HUD is hidden while the keypad shows, and restored on dismissal.
- Tap entry has no sprites or visible art, including error notifications, while its transparent raycast target remains functional. ShowCornerTrigger remains the serialized field; Editor label now says Enable hidden tap trigger.
- Sidebar navigation uses compact horizontal icon/title rows and reacts to height changes so System is reachable on short landscape. Its nested error-count layout remains active. Safe-area sizers update in Editor/Simulator instead of being reset to zero each frame. Panel/PIN keep safe area; only HUD opts out.

## Fresh verification

An isolated consumer resolves the final exact HTTPS Git commit from GitHub; Quick Start is imported. Installed-source parity checks 885 files with no missing/different files. Unity 6000.4.12f1/Linux, Test Framework 1.6.0: **92 EditMode and 69 PlayMode passed**. PlayMode uses OpenGL Core so the captured viewport fixtures are actual Unity renders. The archive guard also passes the final tgz; its hash/size and contents are attached. Binary: /tmp/dreamy-debugger-release-v0.1.2/com.dreamy.debugger-0.1.2.tgz.

Bounds/interaction coverage: 852×393, 640×300, 393×852, portrait/landscape rotation, every number key, Cancel/Submit, 32-digit dots, passphrase unlock, HUD corner/footprint/drag hit area, keypad hiding/restoration, invisible trigger raycasts and panel search/close/navigation with drawer open/closed. Screenshots are in screenshots/. Reviewed numeric and passphrase panels, HUD and full sidebar; final tests preserve access/cheat/report behavior.

## Observations and limits

Initial iterations exposed a Unity fake-null CanvasGroup creation error, optional auto-sample fixture contamination, a stale access-prompt flag after cancel and an undersized drag target after HUD reduction. All were corrected; only final matching successful receipts above count as acceptance. XML, exact consumer manifest/lock and screenshot hashes are attached.

These are synthetic isolated render fixtures, not captures of the user's game or proof of physical-phone touch, native keyboard, notch/rotation on every device or frame-time/thermal budgets. The user's other connected project and unrelated package-manager work were not modified. No gameplay resources or real reports were sent. Referenced docs/agents/dreamy-workflow.md remains absent; this is the manual completion/failure-observation checkpoint.
