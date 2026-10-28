using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Dreamy.UI;
namespace Dreamy.BattlePass.Sample.Editor
{
    public static class BattlePassPrefabValidator
    {
        public static string Validate()
        {
            string root=BattlePassDemoBuilder.Root;
            var rows=new List<object>();
            foreach(string name in new[]{"BattlePassPanel","BattlePassPremiumPopup","RewardPanel","BattlePassRewardItem","BattlePassTierItem","RewardItem","BattlePassEntry"})
            {
                string path=root+"/Prefabs/"+name+".prefab";
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(asset==null || PrefabUtility.GetPrefabAssetType(asset)!=PrefabAssetType.Variant)throw new InvalidOperationException("Expected prefab variant: "+path);
                bool panel=asset.GetComponent<UIPanel>()!=null;
                string expected="Packages/com.dreamy.feature/Runtime/Prefabs/"+(panel?"BaseFeaturePanel":"BaseFeatureItem")+".prefab";
                var parent=PrefabUtility.GetCorrespondingObjectFromSource(asset);
                string source=AssetDatabase.GetAssetPath(parent);
                if(source!=expected)throw new InvalidOperationException("Wrong prefab base: "+path+" -> "+source);
                foreach(var child in asset.GetComponentsInChildren<Transform>(true))
                    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject)>0)throw new InvalidOperationException("Missing script: "+path);
                if(panel && (asset.GetComponentsInChildren<SafeArea>(true).Length!=1 || asset.GetComponent<UITweenPlayer>()==null || asset.transform.Find("SafeArea/Content")==null))
                    throw new InvalidOperationException("Invalid inherited panel ownership: "+path);
                foreach(var component in asset.GetComponents<MonoBehaviour>())
                {
                    if(component==null || component.GetType().Namespace==null || !component.GetType().Namespace.StartsWith("Dreamy.BattlePass.Sample",StringComparison.Ordinal))continue;
                    var serialized=new SerializedObject(component);var property=serialized.GetIterator();
                    while(property.NextVisible(true))
                        if(property.propertyType==SerializedPropertyType.ObjectReference && property.name!="m_Script" && property.objectReferenceValue==null)
                            throw new InvalidOperationException("Missing binding: "+path+":"+property.name);
                }
                rows.Add(new {prefab=name,guid=AssetDatabase.AssetPathToGUID(path),source=source,variant=true});
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(rows,Newtonsoft.Json.Formatting.Indented);
        }
    }
}
