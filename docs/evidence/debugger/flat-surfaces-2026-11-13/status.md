# Debugger flat surfaces and rounded geometry

Status: complete.

Observed before: rounded-fill.png was 32x32 with alpha bounding box (1,1)-(31,31); its entire outermost row/column was transparent. ScrollBackground in Options/System was Sliced, using rounded-fill with alpha 0 and raycastTarget enabled. ApplySkin assigned the rounded Panel sprite to any BG/Background.

Changes: code-author the same 32x32 rounded-fill sprite without empty perimeter, keeping its GUID and 8px slicing borders; uncompressed RGBA32 mobile overrides for this tiny surface. Use flat Images for layout backgrounds, keeping authored alpha and raycasts. Preserve rounded control/card BGs, including controls with a sibling Selectable. Set StyleComponent.IgnoreImage for flat surfaces so style refresh cannot restore a rounded sprite. Apply the policy during runtime skinning and maintainer authoring, and update affected prefabs without replacing components or changing GUIDs.

Static pixel check: alpha bounding box now (0,0)-(32,32); midpoints of all four edges alpha 255; corner alpha 0. Existing panel/card dimensions and safe-area ownership are unchanged.

Environment: Unity 6000.4.12f1; local com.dreamy.debugger 0.1.6; uGUI 2.0.0. Dreamy workflow file remains unavailable as recorded in ../ui-fixes-2026-11-13/status.md. Earlier task changes are retained.

Fresh Unity PlayMode receipt: 4/4 passed, job e76286dbde9f4c3a826ffb0167a035d7 (test-receipt.json), covering edge alpha, flat/rounded skin policy, PIN popup and Console landscape search. MCP progress callbacks duplicated the completed count (8), while the runner summary correctly reports four tests. Android/iOS device rendering has not been verified.

Observed verification issues: the first test startup stalled across Play Mode/domain reload and was retried after stopping Play Mode. The PIN popup regression used NUnit SameAs on Sprite managed wrappers after reimport. Diagnostics proved actual and expected InstanceID both 65076, native equality true, and identical asset paths. Changed that assertion to Unity native object equality, retaining the same asset-identity requirement.
