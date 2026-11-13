# Debugger UI fixes — 2026-11-13

Status: complete.

Scope: default three taps, uniform PIN indicators, Console search clipping in landscape, indefinite manual UI hiding alongside 15/30/60-second timers.

Environment: Unity 6000.4.12f1, local com.dreamy.debugger 0.1.6, uGUI 2.0.0, Input System 1.19.0. Package manifest and consumer lock inspected. Nine source changes existed before this task; these were retained and completed. No package dependency or assembly changes.

Observed failures and corrections:
- PIN prefab had 15x15 fills in indicators 2 and 4 while the other fills were 8x8. Normalize authored and runtime layout to 8x8.
- Search layout investigation measured only ~2.27px of placeholder height at 852x393. Set the search row minimum height to 44px, in both authored prefab and runtime initialization.
- The first PlayMode run passed 28/29; the new search test read an unpopulated cached TextGenerator. Generate text explicitly from the measured rectangle before checking line count.
- A source cleanup temporarily replaced the wrong matching line in DebuggerMaterialAssets.cs, causing CS1525/CS1002 and a subsequent test startup timeout. Restored the unrelated styling block exactly from HEAD, retaining only the intended PIN authoring change; stopped the abandoned test Play Mode before re-running.
- docs/agents/dreamy-workflow.md and the referenced installed Dreamy CLI/lesson workflow were unavailable in this workspace. No lessons service was exposed. This local receipt records observed failures; it is not a substitute for unavailable toolkit delivery.

Verification: EditMode AccessTests 21/21 passed (job 2baa9f12826e4ec8865edeca1fb998a2). Final PlayMode 29/29 passed (job 4a11546fa6304088805ec37275f865ba); see test-receipts.json. Device touch input and Android/iOS builds have not been tested.

Additional environment finding: the open Device Simulator reported 1284x2778 with safe area (0,102,1284,2535), while tests rendered Game View. This produced two missing feedback pixel assertions and the existing screen-center PIN assertion. With Simulator temporarily closed, the same 29-test selection passed without code/test assertion changes. Simulator was reopened afterward. Device Simulator-specific rendering is not claimed verified.
