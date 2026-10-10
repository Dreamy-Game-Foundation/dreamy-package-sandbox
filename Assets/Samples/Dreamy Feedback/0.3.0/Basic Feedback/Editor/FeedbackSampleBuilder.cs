using System;
using System.IO;
using System.Linq;
using Dreamy.Feedback.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Dreamy.Feedback.Samples.Editor
{
    /// <summary>Creates only a new sample folder; never replaces an existing user scene/prefab.</summary>
    public static class FeedbackSampleBuilder
    {
        [MenuItem("Dreamy/Feedback/Create Preview Copy")]
        public static void Build()
        {
            RequireEditMode();
            BuildAt(AssetDatabase.GenerateUniqueAssetPath("Assets/FeedbackPreview"));
        }
        public static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Exit Play Mode and close Prefab Stage before preparing feedback assets.");
        }
        public static void BuildAt(string folder)
        {
            RequireEditMode();
            if (Directory.Exists(folder)) throw new IOException("Output already exists: " + folder);
            var guid = AssetDatabase.FindAssets("FeedbackDemo t:Scene").FirstOrDefault(id => AssetDatabase.GUIDToAssetPath(id).Contains("Basic Feedback/Generated/"));
            if (guid == null) throw new InvalidOperationException("Import the Basic Feedback starter sample first.");
            var source = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guid)).Replace('\\','/');
            if (!AssetDatabase.CopyAsset(source, folder)) throw new IOException("Could not duplicate feedback starter assets.");
            AssetDatabase.Refresh();
            Debug.Log("Feedback preview copied to " + folder + ". Open FeedbackDemo.unity to preview; use GameFeedbackRig.prefab in your game.");
        }
    }
}
