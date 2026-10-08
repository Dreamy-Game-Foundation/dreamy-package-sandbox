# Basic Feedback

Import this sample, open `Generated/FeedbackDemo.unity`, and press Play. The scene has complete databases, rig/particle/text prefabs, camera, UI, original coin sprite, font and sample shaders.

Try all seven channels, Play All and Stop/Clear. Toggle haptics; actual vibration requires a mobile player and has one generic intensity. Controls support safe area and scrolling. FeedbackHost owns and shuts down the services. There is no wallet/save/audio dependency.

`Dreamy/Feedback/Build Basic Demo` creates a new output folder after saving your scene and exiting Play Mode/Prefab Stage. Rebuilding requires TMP Essential Resources for font generation; the supplied scene runs directly. Tests cover wiring, all channels, Clear and button sizing. Liberation Sans license: `Generated/FontLicense.txt`.

The shipped EventSystem uses StandaloneInputModule. Set Active Input Handling to Input Manager (Old) or Both. For new-input-only projects, replace that component with InputSystemUIInputModule after importing the Input System package.

Before running either sample in a fresh project, import **Window > TextMeshPro > Import TMP Essential Resources** once. UGUI 2.0 TMP still needs its project-wide TMP Settings/default style resources in players. The sample uses its own static font and shaders, but does not ship a second `Resources/TMP Settings` asset that could conflict with your project.
