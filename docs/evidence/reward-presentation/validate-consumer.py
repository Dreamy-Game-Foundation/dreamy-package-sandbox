#!/usr/bin/env python3
"""Compile source packages and the shipped reward sample in an isolated consumer.
Requires the same licensed Unity editor and locally resolved dependency cache.
Creates only a unique /tmp fixture; this does not run or modify a player's saves.
"""
import argparse
import json
from pathlib import Path
import shutil
import subprocess
import tempfile

args_parser=argparse.ArgumentParser()
args_parser.add_argument('--unity',required=True,type=Path)
args=args_parser.parse_args()
root=Path.cwd()
fixture=Path(tempfile.mkdtemp(prefix='dreamy-reward-consumer-'))
for name in ('Assets/Editor','Packages','ProjectSettings'):
    (fixture/name).mkdir(parents=True,exist_ok=True)
manifest=json.loads((root/'Packages/manifest.json').read_text())
cache=root/'Library/PackageCache'
deps={}
for name,version in manifest['dependencies'].items():
    if name.startswith('com.dreamy.'):
        deps[name]='file:'+str(root/'LocalPackages'/name)
    elif name in ('com.cysharp.unitask','com.demigiant.dotween','com.unity.addressables','com.unity.ugui','com.unity.nuget.newtonsoft-json','com.unity.test-framework','com.unity.inputsystem'):
        resolved=next(cache.glob(name+'@*'),None)
        deps[name]='file:'+str(resolved) if resolved else version
    elif name.startswith('com.unity.modules.'):
        deps[name]=version
(fixture/'Packages/manifest.json').write_text(json.dumps({'dependencies':deps},indent=2)+'\n')
shutil.copyfile(root/'ProjectSettings/ProjectVersion.txt',fixture/'ProjectSettings/ProjectVersion.txt')
shutil.copytree(root/'Assets/TextMesh Pro',fixture/'Assets/TextMesh Pro')
shutil.copytree(root/'LocalPackages/com.dreamy.feature.economy/Samples~/Reward Presentation',fixture/'Assets/Samples/Reward Presentation')
(fixture/'Assets/Editor/RewardConsumerCheck.cs').write_text('''using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class RewardConsumerCheck
{
    public static void Run()
    {
        foreach(var name in new[]{"Dreamy.Economy.UI.Integration","Dreamy.Economy.RewardPresentation.Sample","Dreamy.DailyReward.RewardPresentation","Dreamy.Missions.RewardPresentation"})
            if(!AppDomain.CurrentDomain.GetAssemblies().Any(a=>a.GetName().Name==name))throw new Exception("Missing assembly: "+name);
        string folder="Assets/Samples/Reward Presentation";
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/Prefabs/RewardPanel.prefab");
        var panel=prefab.GetComponent<Dreamy.Economy.RewardSample.RewardPanel>();
        if(!panel.CanCache)throw new Exception("Scene host requires cached panel");
        var serialized=new SerializedObject(panel);
        foreach(var field in new[]{"titleText","statusText","claimButton","claimLabel","closeButton","itemContainer","itemPrefab","displayCatalog","canvasGroup"})
            if(serialized.FindProperty(field).objectReferenceValue==null)throw new Exception("Missing binding: "+field);
        foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{folder}))
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            foreach(var transform in asset.GetComponentsInChildren<Transform>(true))
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)!=0)throw new Exception("Missing prefab script");
        }
        var scene=EditorSceneManager.OpenScene(folder+"/Scenes/RewardPresentationDemo.unity");
        foreach(var root in scene.GetRootGameObjects())foreach(var transform in root.GetComponentsInChildren<Transform>(true))
            if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)!=0)throw new Exception("Missing scene script");
        Debug.Log("PASS_REWARD_CLEAN_CONSUMER: compile, isolated sample, adapters, prefab fields and scene scripts");
    }
}''')
print('Fixture: '+str(fixture),flush=True)
log=fixture/'validation.log'
result=subprocess.run([str(args.unity),'-batchmode','-nographics','-projectPath',str(fixture),'-executeMethod','RewardConsumerCheck.Run','-quit','-logFile',str(log)])
output=log.read_text(errors='replace') if log.exists() else ''
summary={'fixture':str(fixture),'exitCode':result.returncode,'passed':result.returncode==0 and 'PASS_REWARD_CLEAN_CONSUMER:' in output,'log':str(log)}
(root/'docs/evidence/reward-presentation/clean-consumer.json').write_text(json.dumps(summary,indent=2)+'\n')
print(json.dumps(summary),flush=True)
if not summary['passed']:
    print('\n'.join(output.splitlines()[-70:]))
    raise SystemExit(result.returncode or 1)
print('PASS isolated reward consumer',flush=True)
