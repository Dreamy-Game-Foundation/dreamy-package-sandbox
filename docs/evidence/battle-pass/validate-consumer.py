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
args_parser.add_argument('--build-player',action='store_true')
args=args_parser.parse_args()
root=Path.cwd()
fixture=Path(tempfile.mkdtemp(prefix='dreamy-battlepass-consumer-'))
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
shutil.copytree(root/'LocalPackages/com.dreamy.feature.battle-pass/Samples~/Battle Pass Feature',fixture/'Assets/Samples/Battle Pass Feature')
check_code='''using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class BattlePassConsumerCheck
{
    public static void Run()
    {
        foreach(var name in new[]{"Dreamy.BattlePass.Runtime","Dreamy.BattlePass.Sample","Dreamy.BattlePass.RewardPresentation","Dreamy.Shop.Runtime"})
            if(!AppDomain.CurrentDomain.GetAssemblies().Any(a=>a.GetName().Name==name))throw new Exception("Missing assembly: "+name);
        Dreamy.BattlePass.Sample.Editor.BattlePassPrefabValidator.Validate();
        string folder="Assets/Samples/Battle Pass Feature";
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/Prefabs/BattlePassPanel.prefab");
        var panel=prefab.GetComponent<Dreamy.BattlePass.Sample.BattlePassPanel>();
        if(!panel.CanCache)throw new Exception("Scene host requires cached panel");
        var serialized=new SerializedObject(panel);
        foreach(var field in new[]{"titleText","xpText","timeText","statusText","progressFill","closeButton","claimAllButton","premiumButton","recoverButton","content","tierPrefab","canvasGroup"})
            if(serialized.FindProperty(field).objectReferenceValue==null)throw new Exception("Missing binding: "+field);
        var catalog=Newtonsoft.Json.JsonConvert.DeserializeObject<Dreamy.BattlePass.BattlePassCatalogConfig>(System.IO.File.ReadAllText(folder+"/Resources/DataConfig/battlePassCatalog.json"));
        catalog.Initialize("battlePassCatalog");
        foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{folder}))
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            foreach(var transform in asset.GetComponentsInChildren<Transform>(true))
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)!=0)throw new Exception("Missing prefab script");
        }
        var scene=EditorSceneManager.OpenScene(folder+"/Scenes/BattlePassDemo.unity");
        foreach(var root in scene.GetRootGameObjects())foreach(var transform in root.GetComponentsInChildren<Transform>(true))
            if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)!=0)throw new Exception("Missing scene script");
        Debug.Log("PASS_BATTLEPASS_CLEAN_CONSUMER: compile, isolated Battle Pass sample, config, adapters, prefab fields and scene scripts");
    }
}'''
if args.build_player:
    build_code='''var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{folder+"/Scenes/BattlePassDemo.unity"}, locationPathName="PLAYER_PATH", target=BuildTarget.StandaloneLinux64, options=BuildOptions.Development });
        if(report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Battle Pass player build failed: "+report.summary.totalErrors);
        Debug.Log("PASS_BATTLEPASS_PLAYER_BUILD: errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings+" bytes="+report.summary.totalSize);'''.replace('PLAYER_PATH',str(fixture/'Player/BattlePassDemo.x86_64'))
    check_code=check_code.replace('Debug.Log("PASS_BATTLEPASS_CLEAN_CONSUMER:',build_code+'\n        Debug.Log("PASS_BATTLEPASS_CLEAN_CONSUMER:')
(fixture/'Assets/Editor/BattlePassConsumerCheck.cs').write_text(check_code)
print('Fixture: '+str(fixture),flush=True)
log=fixture/'validation.log'
result=subprocess.run([str(args.unity),'-batchmode','-nographics','-projectPath',str(fixture),'-executeMethod','BattlePassConsumerCheck.Run','-quit','-logFile',str(log)])
output=log.read_text(errors='replace') if log.exists() else ''
summary={'fixture':str(fixture),'exitCode':result.returncode,'passed':result.returncode==0 and 'PASS_BATTLEPASS_CLEAN_CONSUMER:' in output,'log':str(log),'playerBuildRequested':args.build_player,'playerBuildPassed':args.build_player and 'PASS_BATTLEPASS_PLAYER_BUILD:' in output}
(root/'docs/evidence/battle-pass/clean-consumer.json').write_text(json.dumps(summary,indent=2)+'\n')
print(json.dumps(summary),flush=True)
if not summary['passed']:
    print('\n'.join(output.splitlines()[-70:]))
    raise SystemExit(result.returncode or 1)
print('PASS isolated battle pass consumer',flush=True)
