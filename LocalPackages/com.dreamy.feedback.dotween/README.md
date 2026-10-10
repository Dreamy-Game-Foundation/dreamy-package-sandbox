# Dreamy Feedback DOTween

Optional UPM adapter. Install DOTween first, then add this folder as a local package (or publish the adapter separately). The base Feedback package never references this adapter.

Add `DotweenFeedbackProvider` on your rig and assign it to `FeedbackHost.animationProvider`. The portable starter rig uses the Unity backend by default. Assign the provider explicitly; no rebuild is needed. Remove the provider/assignment before removing the adapter.

This adapter targets the sandbox DOTween DLL distribution (`DOTween.dll`, package `com.demigiant.dotween` 0.0.3). Other DOTween distributions must provide the same DLL or adjust this adapter's asmdef. Gameplay sees FeedbackHandle only. Tween kill and complete callbacks converge on one playback state.

See the Casual Icon Fly sample for tuning. UI Punch, camera shake, floating text and screen flash also use this backend through the host.

The tested DOTween 0.0.3 distribution includes Audio, Physics and Physics2D modules. Enable `com.unity.modules.audio`, `com.unity.modules.physics`, and `com.unity.modules.physics2d` in a minimal consumer, or configure those DOTween modules out in your distribution. These are DOTween distribution requirements; the Feedback base package does not depend on them.
