using UnityEditor;
namespace ThunderPunch.Debugger.Sample.Editor {
    // The old sample menu shares the production tool styling. Fake transports and commands remain editable sample code.
    public sealed class DebuggerAcceptanceWindow : EditorWindow {
        [MenuItem("Window/Dreamy Debugger/Acceptance Playground")]
        public static void Open()=>ThunderPunch.Debugger.Editor.DebuggerWindow.Open();
    }
}
