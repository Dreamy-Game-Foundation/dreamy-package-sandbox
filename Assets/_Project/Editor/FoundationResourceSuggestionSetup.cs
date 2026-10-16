using System;
using Dreamy.Economy.UI;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Dreamy.Template.Editor
{
    public static class FoundationResourceSuggestionSetup
    {
        [MenuItem("Tools/Dreamy/Sandbox/Validate Resource Suggestion Panels")]
        public static void Validate()
        {
            Check<CoinSuggestionPanel>("Assets/_Project/Prefabs/UI/ResourceSuggestions/CoinSuggestionPanel.prefab", Address.CoinSuggestionPanel);
            Check<GemSuggestionPanel>("Assets/_Project/Prefabs/UI/ResourceSuggestions/GemSuggestionPanel.prefab", Address.GemSuggestionPanel);
            Debug.Log("PASS_FOUNDATION_RESOURCE_SUGGESTION_SETUP: typed Coin/Gem panels and addresses");
        }

        private static void Check<TPanel>(string path, string address) where TPanel : ResourceSuggestionPanel
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null || prefab.GetComponent<TPanel>() == null)
                throw new InvalidOperationException("Missing typed suggestion panel: " + path);
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings == null ? null : settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));
            if (entry == null || entry.address != address)
                throw new InvalidOperationException("Missing suggestion address: " + address);
        }
    }
}
