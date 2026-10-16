#!/usr/bin/env python3
"""Compile current packages and unmodified UI samples in an isolated Unity consumer.
Run from sandbox root; requires a licensed Unity Editor and resolved package cache.
This checks compilation/serialized prefab bindings, not Play Mode or native builds.
"""
import argparse
import json
from pathlib import Path
import shutil
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('--unity', required=True, type=Path)
args = parser.parse_args()
root = Path.cwd()
fixture = Path(tempfile.mkdtemp(prefix='dreamy-resource-consumer-'))
for folder in ['Assets/Editor', 'Packages', 'ProjectSettings']:
    (fixture / folder).mkdir(parents=True, exist_ok=True)
manifest = json.loads((root / 'Packages/manifest.json').read_text())
dependencies = {}
for name, version in manifest['dependencies'].items():
    if name.startswith('com.dreamy.'):
        dependencies[name] = 'file:' + str(root / 'LocalPackages' / name)
    elif name in ['com.cysharp.unitask', 'com.demigiant.dotween']:
        dependencies[name] = 'file:' + str(next((root / 'Library/PackageCache').glob(name + '@*')))
    elif name.startswith('com.unity.modules.') or name in ['com.unity.addressables', 'com.unity.ugui', 'com.unity.nuget.newtonsoft-json', 'com.unity.test-framework']:
        dependencies[name] = version
(fixture / 'Packages/manifest.json').write_text(json.dumps({'dependencies': dependencies}, indent=2) + '\n')
shutil.copyfile(root / 'ProjectSettings/ProjectVersion.txt', fixture / 'ProjectSettings/ProjectVersion.txt')
# The samples require the standard TMP Essential Resources (settings/font/shaders).
shutil.copytree(root / 'Assets/TextMesh Pro', fixture / 'Assets/TextMesh Pro')
for name, sample in [
    ('com.dreamy.feature.economy', 'Resource Recovery'),
    ('com.dreamy.feature.shop', 'Shop Feature'),
    ('com.dreamy.feature.progression', 'Progression Rewards'),
    ('com.dreamy.connectivity', 'Connectivity Feature'),
    ('com.dreamy.connectivity', 'Connectivity Diagnostics'),
]:
    shutil.copytree(root / 'LocalPackages' / name / 'Samples~' / sample, fixture / 'Assets/Samples' / sample)
(fixture / 'Assets/Editor/ResourceConsumerCheck.cs').write_text('''using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class ResourceConsumerCheck {
 public static void Run() {
  foreach (var name in new[] {"Dreamy.Economy.Runtime", "Dreamy.Economy.Recovery.Unity",
    "Dreamy.Shop.UI.Integration", "Dreamy.Feature.Shop.Integration.Runtime",
    "Dreamy.Progression.Rewards.Sample", "Dreamy.Connectivity.Feature", "Dreamy.Connectivity.UI"})
   if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == name))
    throw new Exception("Missing assembly: " + name);
  var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Resource Recovery/NotEnoughResourcesPanel.prefab");
  var panel = prefab.GetComponent<Dreamy.Economy.UI.NotEnoughResourcesPanel>();
  if (panel == null) throw new Exception("Missing shortage panel");
  var serialized = new SerializedObject(panel);
  foreach (var field in new[] {"rowContainer", "rowPrefab", "statusText", "cancelButton"})
   if (serialized.FindProperty(field).objectReferenceValue == null) throw new Exception("Unbound shortage field: " + field);
  // displayCatalog is optional: the row falls back to resource IDs and grouped amounts.
  foreach(var name in new[]{"Coin","Gem"}) {
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Resource Recovery/ResourceSuggestions/"+name+"SuggestionPanel.prefab");
   if(PrefabUtility.GetPrefabAssetType(asset)!=PrefabAssetType.Variant)throw new Exception("Not a resource variant: "+name);
   var suggestion=asset.GetComponent<Dreamy.Economy.UI.ResourceSuggestionPanel>();
   if(suggestion.ResourceId.Value!=(name=="Coin"?"currency.coin":"currency.gem"))throw new Exception("Wrong resource");
   var data=new SerializedObject(suggestion);
   foreach(var field in new[]{"acquireButton","laterButton","balanceText"})
    if(data.FindProperty(field).objectReferenceValue==null)throw new Exception("Unbound suggestion field: "+field);
   if(asset.transform.Find("SafeArea/Content/Dialog/ResourceArt")!=null)throw new Exception("Unexpected resource art");
   foreach(var image in asset.GetComponentsInChildren<UnityEngine.UI.Image>(true))
    if(image.sprite!=null)throw new Exception("Expected color-only Image");
  }
  foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] {"Assets/Samples"})) {
   var asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
   foreach (var transform in asset.GetComponentsInChildren<Transform>(true))
    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) != 0)
     throw new Exception("Missing script: " + asset.name);
  }
  Debug.Log("PASS_RESOURCE_UI_CLEAN_CONSUMER: assemblies and prefab bindings");
 }
}''')
print('Clean UI consumer fixture: ' + str(fixture), flush=True)
log = fixture / 'validation.log'
result = subprocess.run([str(args.unity), '-batchmode', '-nographics', '-projectPath', str(fixture),
    '-executeMethod', 'ResourceConsumerCheck.Run', '-quit', '-logFile', str(log)])
output = log.read_text(errors='replace') if log.exists() else ''
if result.returncode or 'PASS_RESOURCE_UI_CLEAN_CONSUMER:' not in output:
    print('\n'.join(output.splitlines()[-65:]))
    raise SystemExit(result.returncode or 1)
print('PASS clean UI consumer compile, imported source samples and prefab bindings', flush=True)
