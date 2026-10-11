using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamy.Feedback.Samples.Editor
{
    /// <summary>Scoped sample maintenance. Exactly seven shared effect assets; four existing reward icons are reused.</summary>
    public static class FeedbackStarterAuthoring
    {
        private static readonly Dictionary<FeedbackDefinition, FeedbackStepBinding[]> migratedBindings = new Dictionary<FeedbackDefinition, FeedbackStepBinding[]>();
        private static string FindPath<T>(string root, string name) where T : UnityEngine.Object
        { return AssetDatabase.FindAssets(name + " t:" + typeof(T).Name, new[] { root }).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == name); }
        public static FeedbackLibrary PrepareAssets(string generatedFolder)
        {
            FeedbackSampleBuilder.RequireEditMode();
            string folder = generatedFolder + "/Effects";
            Directory.CreateDirectory(folder); AssetDatabase.Refresh();
            var vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindPath<GameObject>(generatedFolder, "RewardParticles"));
            var textPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindPath<GameObject>(generatedFolder, "RewardText"));
            if (!vfxPrefab || !textPrefab || !textPrefab.GetComponent<FloatingTextInstance>())
                throw new InvalidOperationException("Import valid starter VFX/text prefabs first.");
            var icons = IconFlyOptions.Create(AssetDatabase.LoadAssetAtPath<Sprite>(FindPath<Sprite>(generatedFolder, "Coin")), Vector3.zero, Vector3.zero);
            icons.Count = 10;
            var textSettings = FloatingTextFeedbackSettings.Default; textSettings.Prefab = textPrefab;
            textSettings.Color = new Color(1, .85f, .2f); textSettings.PrewarmCount = 2;
            var flash = ScreenFlashOptions.WhiteFlash(); flash.MaxAlpha = .18f;
            var effects = new Dictionary<FeedbackNodeType, FeedbackEffectAsset>();
            Add<VfxFeedbackAsset>("RewardVfx", a => a.Configure("effect.reward-vfx", new VfxFeedbackSettings { Prefab = vfxPrefab, MaximumDuration = 1.5f, Scale = 1, PrewarmCount = 2 }));
            Add<FloatingTextFeedbackAsset>("RewardText", a => a.Configure("effect.reward-text", textSettings));
            Add<IconFlyFeedbackAsset>("RewardIcons", a => a.Configure("effect.reward-icons", icons));
            Add<UiPunchFeedbackAsset>("HudPunch", a => a.Configure("effect.hud-punch", UiPunchOptions.Default));
            Add<CameraShakeFeedbackAsset>("HitShake", a => a.Configure("effect.hit-shake", CameraShakeOptions.Small));
            Add<ScreenFlashFeedbackAsset>("ScreenFlash", a => a.Configure("effect.screen-flash", flash));
            Add<HapticFeedbackAsset>("LightHaptic", a => a.Configure("effect.light-haptic", HapticType.Light));
            var definitions = AssetDatabase.FindAssets("t:FeedbackDefinition", new[] { generatedFolder })
                .Select(g => AssetDatabase.LoadAssetAtPath<FeedbackDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(d => !AssetDatabase.GetAssetPath(d).Contains("/Groups/")).ToArray();
            foreach (var definition in definitions)
            {
                Undo.RecordObject(definition, "Use shared feedback effect assets");
                if (definition.IsLegacy)
                {
                    Convert(definition.Root);
                    migratedBindings[definition] = Dreamy.Feedback.Editor.FeedbackListMigration.Convert(definition, true);
                }
                EditorUtility.SetDirty(definition); AssetDatabase.SaveAssetIfDirty(definition);
            }
            string libraryPath = FindPath<FeedbackLibrary>(generatedFolder, "FeedbackLibrary") ?? generatedFolder + "/FeedbackLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<FeedbackLibrary>(libraryPath);
            if (!library) { library = ScriptableObject.CreateInstance<FeedbackLibrary>(); AssetDatabase.CreateAsset(library, libraryPath); }
            library.Configure(effects.Values, definitions);
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssetIfDirty(library);
            var errors = library.Validate(); if (errors.Count != 0) throw new InvalidOperationException(string.Join("\n", errors));
            Organize(generatedFolder);
            var vfxDb = AssetDatabase.LoadAssetAtPath<VfxDatabase>(FindPath<VfxDatabase>(generatedFolder, "VfxDatabase"));
            if (vfxDb) { vfxDb.Configure(effects.Values.OfType<VfxFeedbackAsset>()); EditorUtility.SetDirty(vfxDb); AssetDatabase.SaveAssetIfDirty(vfxDb); }
            var textDb = AssetDatabase.LoadAssetAtPath<FloatingTextDatabase>(FindPath<FloatingTextDatabase>(generatedFolder, "FloatingTextDatabase"));
            if (textDb) { textDb.Configure(effects.Values.OfType<FloatingTextFeedbackAsset>()); EditorUtility.SetDirty(textDb); AssetDatabase.SaveAssetIfDirty(textDb); }
            var shakeDb = AssetDatabase.LoadAssetAtPath<CameraShakeDatabase>(FindPath<CameraShakeDatabase>(generatedFolder, "CameraShakeDatabase"));
            if (shakeDb) { shakeDb.Configure(effects.Values.OfType<CameraShakeFeedbackAsset>()); EditorUtility.SetDirty(shakeDb); AssetDatabase.SaveAssetIfDirty(shakeDb); }
            return library;

            void Add<T>(string name, Action<T> configure) where T : FeedbackEffectAsset
            {
                string path = FindPath<T>(generatedFolder, name) ?? folder + "/" + name + ".asset";
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (!asset) { asset = ScriptableObject.CreateInstance<T>(); configure(asset); AssetDatabase.CreateAsset(asset, path); }
                EditorUtility.SetDirty(asset); AssetDatabase.SaveAssetIfDirty(asset);
                effects.Add(asset.Type, asset);
            }
            void Convert(FeedbackNode node)
            {
                if (node.Effect) return;
                if (node.Type == FeedbackNodeType.Sequence || node.Type == FeedbackNodeType.Parallel)
                { foreach (var child in node.Children) Convert(child); return; }
                if (node.Type == FeedbackNodeType.Delay) return;
                node.Effect = effects[node.Type];
                var changes = new FeedbackEffectOverrides();
                switch (node.Type)
                {
                    case FeedbackNodeType.Vfx: changes.Fields = FeedbackOverrideFields.Duration; changes.Duration = node.Duration; break;
                    case FeedbackNodeType.FloatingText: changes.Fields = FeedbackOverrideFields.Text; changes.Text = node.Text; break;
                    case FeedbackNodeType.IconFly: changes.Fields = FeedbackOverrideFields.IconOptions; changes.IconOptions = node.IconFly; break;
                    case FeedbackNodeType.UiPunch:
                        changes.Fields = FeedbackOverrideFields.PunchOptions; changes.PunchOptions = node.Punch; break;
                    case FeedbackNodeType.CameraShake:
                        changes.Fields = FeedbackOverrideFields.ShakeOptions; changes.ShakeOptions = node.Shake; break;
                    case FeedbackNodeType.ScreenFlash: changes.Fields = FeedbackOverrideFields.FlashOptions; changes.FlashOptions = node.Flash; break;
                    case FeedbackNodeType.Haptic: changes.Fields = FeedbackOverrideFields.Haptic; changes.Haptic = node.Haptic; break;
                }
                node.Overrides = changes;
            }
        }

        public static FeedbackDemoEntry[] Entries(FeedbackLibrary library)
        {
            var effects = library.Effects.ToDictionary(e => e.Type);
            var result = new List<FeedbackDemoEntry>();
            Add("VFX", FeedbackNodeType.Vfx);
            Add("Haptic", FeedbackNodeType.Haptic);
            Add("Floating Text", FeedbackNodeType.FloatingText);
            AddIcons("Scatter Magnet", IconFlyStyle.ScatterMagnet);
            Add("Screen Flash", FeedbackNodeType.ScreenFlash);
            Add("Camera Shake", FeedbackNodeType.CameraShake);
            result.Add(new FeedbackDemoEntry { Label = "Selected Reward", SelectedReward = true });
            AddIcons("Straight", IconFlyStyle.Straight);
            AddIcons("Arc", IconFlyStyle.Arc);
            AddIcons("Fountain", IconFlyStyle.Fountain);
            AddIcons("Spiral", IconFlyStyle.Spiral);
            Add("UI Punch", FeedbackNodeType.UiPunch);
            result.Add(new FeedbackDemoEntry { Label = "Hit", Definition = library.Definitions.First(d => d.name == "Hit"), Intensity = 1.5f });
            result.Add(new FeedbackDemoEntry { Label = "Button Click", Definition = library.Definitions.First(d => d.name == "ButtonClick") });
            return result.ToArray();
            void Add(string label, FeedbackNodeType type) => result.Add(new FeedbackDemoEntry { Label = label, Effect = effects[type] });
            void AddIcons(string label, IconFlyStyle style)
            {
                var options = effects[FeedbackNodeType.IconFly].Resolve().IconFly;
                options.Style = style; options.Ease = style == IconFlyStyle.Straight ? FeedbackEase.InCubic : FeedbackEase.InOutSine;
                result.Add(new FeedbackDemoEntry { Label = label, Effect = effects[FeedbackNodeType.IconFly],
                    Overrides = new FeedbackEffectOverrides { Fields = FeedbackOverrideFields.IconOptions, IconOptions = options } });
            }
        }

        private static void Organize(string root)
        {
            var paths = AssetDatabase.FindAssets("", new[] { root }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            foreach (var path in paths)
            {
                if (AssetDatabase.IsValidFolder(path)) continue;
                var asset = AssetDatabase.LoadMainAssetAtPath(path); string folder = null;
                if (asset is FeedbackEffectAsset effect) folder = "Effects/" + effect.Type;
                else if (asset is FeedbackDefinition definition)
                {
                    if (path.Contains("/Groups/")) folder = "Definitions/Groups";
                    string category = definition.name.EndsWith("Reward") ? "Rewards" : definition.name.StartsWith("Button") ? "UI" : definition.name == "SoftShake" || definition.name == "SuccessFlash" ? "Screen" : "Gameplay";
                    if (folder == null) folder = "Definitions/" + category;
                }
                else if (asset is FeedbackLibrary) folder = "Libraries";
                else if (asset is ScriptableObject && asset.GetType().Name.EndsWith("Database")) folder = "Databases";
                else if (asset is SceneAsset) folder = "Scenes";
                else if (asset is GameObject && (asset.name == "GameFeedbackRig" || asset.name == "FeedbackRig")) folder = "Rigs";
                if (folder == null) continue;
                string destination = root + "/" + folder + "/" + Path.GetFileName(path);
                if (destination == path) continue;
                Directory.CreateDirectory(root + "/" + folder); AssetDatabase.Refresh();
                string error = AssetDatabase.MoveAsset(path, destination); if (error.Length > 0) throw new InvalidOperationException(error);
            }
        }

        /// <summary>Only call for a saved fixture/copy; never opens or saves the user's dirty scene.</summary>
        public static void ConfigureScene(Scene scene, FeedbackLibrary library)
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var controls in root.GetComponentsInChildren<FeedbackDemoControls>(true))
                { controls.ConfigureEntries(Entries(library)); foreach (var pair in migratedBindings) if (library.Definitions.Contains(pair.Key)) controls.ConfigureBindings(pair.Key, pair.Value); EditorUtility.SetDirty(controls); }
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
