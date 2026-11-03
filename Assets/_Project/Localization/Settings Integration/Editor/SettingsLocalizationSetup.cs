using System;
using System.Collections.Generic;
using System.Linq;
using Dreamy.Localization.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Localization.SettingsIntegration.Editor
{
    /// <summary>Opt-in authoring tool. Only imported when the game uses both packages.</summary>
    public sealed class SettingsLocalizationSetup : EditorWindow
    {
        [SerializeField] private GameObject settingsPrefab;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private string table = "Game";
        [SerializeField] private ScriptableObject languageIcons;

        [MenuItem("Dreamy/Localization/Settings Integration")]
        private static void Open() => GetWindow<SettingsLocalizationSetup>("Settings Localization");

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Choose your imported Settings prefab and production font. This adds localized bindings to recognized labels; the gateway controls dropdown availability.", MessageType.Info);
            settingsPrefab = (GameObject)EditorGUILayout.ObjectField("Settings prefab", settingsPrefab, typeof(GameObject), false);
            font = (TMP_FontAsset)EditorGUILayout.ObjectField("Font (optional)", font, typeof(TMP_FontAsset), false);
            table = EditorGUILayout.TextField("String table", table);
            languageIcons = (ScriptableObject)EditorGUILayout.ObjectField("Language icons (optional)", languageIcons, typeof(ScriptableObject), false);
            using (new EditorGUI.DisabledScope(settingsPrefab == null || string.IsNullOrWhiteSpace(table)))
                if (GUILayout.Button("Apply to prefab")) Apply(settingsPrefab, table, font, languageIcons);
        }

        public static void Apply(GameObject prefab, string table, TMP_FontAsset font, ScriptableObject languageIcons = null)
        {
            var path = AssetDatabase.GetAssetPath(prefab);
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !path.EndsWith(".prefab", StringComparison.Ordinal))
                throw new ArgumentException("Choose a project-owned prefab under Assets.");
            if (string.IsNullOrWhiteSpace(table)) throw new ArgumentException("String table is required.");
            if (languageIcons != null && languageIcons.GetType().Name != "SettingsLanguageIcons")
                throw new ArgumentException("Choose a SettingsLanguageIcons asset.");
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (!root.GetComponents<MonoBehaviour>().Any(c => c != null && (c.GetType().Name == "SettingsPanel" || c.GetType().Name == "StandaloneSettingsView")))
                    throw new ArgumentException("The prefab must contain a Dreamy Settings view.");
                if (languageIcons != null)
                {
                    var view=root.GetComponents<MonoBehaviour>().First(c=>c!=null && (c.GetType().Name=="SettingsPanel" || c.GetType().Name=="StandaloneSettingsView"));
                    var data=new SerializedObject(view);
                    data.FindProperty("languageIcons").objectReferenceValue=languageIcons;
                    data.ApplyModifiedPropertiesWithoutUndo();
                    ConfigureDropdownIcons(root.GetComponentInChildren<TMP_Dropdown>(true));
                }
                var keys = new Dictionary<string, string>
                {
                    ["Settings"]="settings.title", ["Music"]="settings.music", ["SFX"]="settings.sfx",
                    ["Sound effects"]="settings.sfx", ["Haptics"]="settings.haptics", ["Privacy & GDPR"]="settings.privacy",
                    ["Restore Purchases"]="settings.restore", ["Rate Us"]="settings.rate", ["Close"]="settings.close",
                    ["Language"]="settings.language"
                };
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (font != null) text.font = font;
                    var binding = text.GetComponent<LocalizedTmpText>();
                    string key;
                    if (binding != null && !binding.StringReference.IsEmpty) key = binding.StringReference.TableEntryReference.Key;
                    else if (text.name == "Language Error") key = "settings.language_failed";
                    else if (!keys.TryGetValue(text.text, out key)) continue;
                    if (string.IsNullOrEmpty(key)) continue; // Preserve stable-ID-only references without guessing.
                    if (binding == null) binding = text.gameObject.AddComponent<LocalizedTmpText>();
                    binding.SetTarget(text, captureFallback:false);
                    binding.SetReference(table, key);
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        public static void ConfigureDropdownIcons(TMP_Dropdown dropdown)
        {
            if(dropdown==null) throw new ArgumentException("The Settings view needs a language dropdown.");
            dropdown.captionImage=GetImage(dropdown.transform,"Caption Flag",new Vector2(30,0));
            dropdown.captionText.rectTransform.offsetMin=new Vector2(62,6);
            dropdown.captionText.rectTransform.offsetMax=new Vector2(-34,-6);
            var item=dropdown.itemText.transform.parent;
            dropdown.itemImage=GetImage(item,"Item Flag",new Vector2(54,0));
            dropdown.itemText.rectTransform.offsetMin=new Vector2(88,6);
            dropdown.itemText.rectTransform.offsetMax=new Vector2(-12,-6);
        }
        private static Image GetImage(Transform parent,string name,Vector2 position)
        {
            var child=parent.Find(name);
            if(child==null)
            {
                child=new GameObject(name,typeof(RectTransform),typeof(Image)).transform;
                child.SetParent(parent,false);
            }
            var image=child.GetComponent<Image>();image.raycastTarget=false;image.preserveAspect=true;
            var rect=(RectTransform)child;rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);
            rect.anchoredPosition=position;rect.sizeDelta=new Vector2(44,32);
            return image;
        }
    }
}
