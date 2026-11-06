from pathlib import Path
import json,hashlib,tarfile,re,shutil,sys,xml.etree.ElementTree as ET,zipfile,subprocess
from PIL import Image
root=Path('/run/media/tutrinh/NewData/Unity Project/Dreamy Foundation/dreamy-package-sandbox');pkg=root/'LocalPackages/com.dreamy.debugger';ev=root/'docs/evidence/debugger/ui-redesign'
hashfile=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
archive=Path('/tmp/dreamy-ui-redesign-artifact/com.dreamy.debugger-0.3.0-preview.1.tgz')
(ev/'artifacts').mkdir(exist_ok=True);shutil.copyfile(archive,ev/'artifacts'/archive.name)
sys.path.insert(0,str(pkg/'Tools~'));from verify_package import verify
archive_gate=verify(archive)
inputs={};mismatch=[]
with tarfile.open(archive) as tar:
 for m in tar.getmembers():
  if m.isfile():
   name=m.name.removeprefix('package/');digest=hashlib.sha256(tar.extractfile(m).read()).hexdigest();inputs[name]=digest
   if not (pkg/name).is_file() or hashfile(pkg/name)!=digest:mismatch.append(name)
assert not mismatch,mismatch
(ev/'artifact-inputs.json').write_text(json.dumps(inputs,indent=2)+'\n')
protected=json.loads((ev/'protected-files.json').read_text());changed=[name for name,h in protected.items() if not (root/name).is_file() or hashfile(root/name)!=h];assert not changed,changed
baseline=json.loads((ev/'baseline-files.json').read_text());oldmeta=[name for name,h in baseline.items() if name.endswith('.meta') and (not (pkg/name).is_file() or hashfile(pkg/name)!=h)];assert not oldmeta,oldmeta
maps=[]
for folder in [pkg,root/'Assets/Packages/StompyRobot']:
 d={}
 for p in folder.rglob('*.meta'):
  if any(part.endswith('~') for part in p.relative_to(folder).parts):continue
  m=re.search(r'^guid: ([0-9a-f]{32})$',p.read_text(),re.M)
  if m:d.setdefault(m.group(1),[]).append(str(p.relative_to(folder)))
 maps.append(d)
dups={g:p for g,p in maps[0].items() if len(p)>1};overlap=sorted(set(maps[0])&set(maps[1]));assert not dups and not overlap,(dups,overlap)
tests={}
for source,prefix in [('source','/tmp/dreamy-runtime-ui-'),('artifact','/tmp/dreamy-ui-redesign-artifact-')]:
 for mode in ['EditMode','PlayMode']:
  p=Path(prefix+mode+'.xml');tree=ET.parse(p).getroot();assert tree.attrib['result']=='Passed',tree.attrib
  out=source+'-'+mode+'.xml';shutil.copyfile(p,ev/out);tests[source+'-'+mode]={**tree.attrib,'receipt':out,'sha256':hashfile(p)}
builds={}
for kind in ['linux-dev','linux-release','android-dev']:
 p=Path('/tmp/dreamy-ui-acceptance-builds')/kind/'receipt.json';v=json.loads(p.read_text());assert v['result']=='Succeeded';shutil.copyfile(p,ev/'builds'/(kind+'.json'))
 output=p.parent/('DreamyDebugger.apk' if kind=='android-dev' else 'DreamyDebugger.x86_64');builds[kind]={**v,'receipt':'builds/'+kind+'.json','output':str(output),'outputSha256':hashfile(output),'outputBytes':output.stat().st_size}
assert not builds['linux-release']['debuggerAssets']
assert any('TaskCard.prefab' in a for a in builds['linux-dev']['debuggerAssets'])
assert any('TaskCard.prefab' in a for a in builds['android-dev']['debuggerAssets'])
apk=Path(builds['android-dev']['output'])
with zipfile.ZipFile(apk) as z:android_libraries=[n for n in z.namelist() if n.startswith('lib/')]
assert any('arm64-v8a/libil2cpp.so' in n for n in android_libraries)
screens={}
for p in (ev/'screenshots').glob('*.png'):
 with Image.open(p) as im:screens[p.name]={'width':im.width,'height':im.height,'sha256':hashfile(p)}
state={'scene':'Assets/_Project/Scenes/BootstrapScene.unity','dirty':False,'playing':False,'buildTarget':'Android','enterPlayModeOptionsEnabled':True,'enterPlayModeOptions':1,'playModeStartScene':'','fixtureCount':0,'simulatorWindows':1,'qaCustomResolutionsRemaining':0,'prefabValidation':'29 prefabs; no missing scripts','observedVia':'Unity MCP after restoring owned fixture'}
result={'candidate':'0.3.0-preview.1','date':'2026-11-05','unity':'6000.4.12f1 Linux','baselinePackageCommit':'8a0e19ca99ab79296afdb62ee55e67fa97e6c137','archive':{**archive_gate,'path':'artifacts/'+archive.name,'sourceByteMismatches':mismatch,'fileMap':'artifact-inputs.json'},'tests':tests,'relay':{'total':20,'result':'passed','receipt':'relay-tests.txt','realDelivery':False},'builds':builds,'androidLibraries':android_libraries,'integrity':{'protectedFiles':len(protected),'changedProtectedFiles':changed,'existingMetaFiles':sum(n.endswith('.meta') for n in baseline),'changedExistingMetaFiles':oldmeta,'duplicatePackageGuids':dups,'srGuidOverlap':overlap},'screenshots':screens,'editorRestored':state,'limitations':['No physical mobile/iOS runtime verification; desktop screenshots are offline fixtures.','Editor native-window screenshot unavailable: Orca Linux provider reports observation.screenshot=false.','No device frame-time/GC/thermal/capture-under-load certification.','Unity 6000.4 emits deprecated object-finding API warnings in compatibility tests; minimum Unity 6000.0 API kept.','Whole diff whitespace check flags Unity-authored empty YAML fields; C#/Markdown/JSON diff check passes. Serialized YAML was not hand-normalized.','Original Editor restarted between continuations; SessionState lost. BootstrapScene/options/Simulator were observed restored, then a new isolated fixture was created and cleaned.','Cleanup first used custom-relative RemoveCustomSize indices and emitted four Invalid index errors; absolute indices corrected, owned QA entries now zero. Those automation Console entries are retained without clearing user history.','Editor shader warm-up initially rendered cyan; subsequent graph screenshot after warm-up and zero shader messages is used. No runtime shader changed for this.'],'networkDeliveryPerformed':False,'gitCommitOrPublishPerformed':False}
(ev/'verification.json').write_text(json.dumps(result,indent=2,ensure_ascii=False)+'\n')
print(json.dumps({'candidate':result['candidate'],'archiveSha256':archive_gate['sha256'],'tests':{k:v['passed'] for k,v in tests.items()},'builds':{k:v['result'] for k,v in builds.items()},'screenshots':len(screens),'protected':len(protected),'guids':'passed'},indent=2))
