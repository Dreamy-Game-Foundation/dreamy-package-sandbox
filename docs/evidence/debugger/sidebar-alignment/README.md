# Sidebar alignment fix — 2026-11-09

Status: complete for targeted source/UI regression scope. User screenshot showed tab backgrounds and separators crossing the sidebar/header boundary on a notched landscape viewport.

Reproduced with the current package and its existing local prefab edits: adding 24 logical units of left safe-area spacing makes a tab row exceed the rail by exactly 24 units. The width-measurement row reserved the full body width plus the inset, but MobileMenuController still allocated only the body width to the rail. Existing no-inset fixtures did not catch this.

MobileMenuController now includes the left inset in the allocated rail width (clamped by the available viewport), sets the measurement spacer to the remaining body width, and positions tab icons/titles clear of that inset. The existing content offset follows the actual rail edge. Sidebar logo can shrink within the same header when its close control is visible. Insets are reread when they change; no additional scaler or safe-area owner is introduced.

Verification: Unity 6000.4.12f1/Linux OpenGL, isolated consumer using current local package. **34/34 WorkspaceUiTests passed**, including the new regression across 852×393, 393×852 and 1280×720; left insets 0/24/48; canvas scales 1 and 1.5; open/close and rotation. Assertions check tab bounds, logo bounds, icon clearance and rail/content edge alignment. Screenshots are actual synthetic Unity renders. Physical-notch behavior on a phone is not claimed from the synthetic spacer overrides.

Pre-existing uncommitted changes in DebugPanel.prefab, ConfirmationPrompt.prefab and PerformanceHud.prefab were retained. This fix changes MobileMenuController.cs and its regression test, not those prefab files. The isolated tests use those existing edited prefabs. No game scene was saved, gameplay run, resource granted or report sent. The referenced docs/agents/dreamy-workflow.md remains missing; this is the manual observed-failure/completion checkpoint.
