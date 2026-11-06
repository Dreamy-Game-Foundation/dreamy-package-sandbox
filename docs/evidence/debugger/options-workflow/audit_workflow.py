from pathlib import Path
import hashlib,json,shutil,tarfile,xml.etree.ElementTree as E,sys
root=Path('/run/media/tutrinh/NewData/Unity Project/Dreamy Foundation/dreamy-package-sandbox');pkg=root/'LocalPackages/com.dreamy.debugger';ev=root/'docs/evidence/debugger/options-workflow';h=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
archive=Path('/tmp/dreamy-workflow-artifact/com.dreamy.debugger-0.3.0-preview.3.tgz')
sys.path.insert(0,str(pkg/'Tools~'));from verify_package import verify
gate=verify(archive);inputs={}
with tarfile.open(archive) as tar:
 for member in tar.getmembers():
  if member.isfile():
   name=member.name.removeprefix('package/');digest=hashlib.sha256(tar.extractfile(member).read()).hexdigest();assert h(pkg/name)==digest,name;inputs[name]=digest
(ev/'package-inputs.json').write_text(json.dumps(inputs,indent=2)+'\n');(ev/'artifacts').mkdir(exist_ok=True);shutil.copyfile(archive,ev/'artifacts'/archive.name)
tests={}
for source,prefix in [('source','/tmp/dreamy-runtime-ui-'),('artifact','/tmp/dreamy-workflow-artifact-')]:
 for mode in ['EditMode','PlayMode']:
  p=Path(prefix+mode+'.xml');r=E.parse(p).getroot();assert r.attrib['result']=='Passed';name=source+'-'+mode+'.xml';shutil.copyfile(p,ev/name);tests[source+'-'+mode]={'receipt':name,'sha256':h(p),**r.attrib}
p=Path('/tmp/dreamy-workflow-game.xml');r=E.parse(p).getroot();assert r.attrib['result']=='Passed';shutil.copyfile(p,ev/'game-EditMode.xml');tests['game-EditMode']={'receipt':'game-EditMode.xml','sha256':h(p),**r.attrib}
projectInputs={}
for p in (root/'Assets/_Project/Debugger').rglob('*.cs'):
 relative=p.relative_to(root/'Assets');clone=Path('/tmp/dreamy-workflow-game-consumer/Assets')/relative
 assert h(p)==h(clone),p
 projectInputs[str(p.relative_to(root))]=h(p)
(ev/'game-inputs.json').write_text(json.dumps(projectInputs,indent=2)+'\n')
protected=json.loads((root/'docs/evidence/debugger/ui-redesign/protected-files.json').read_text());changed=[name for name,digest in protected.items() if h(root/name)!=digest]
old=json.loads((root/'docs/evidence/debugger/ui-redesign/baseline-files.json').read_text());changedMeta=[name for name,digest in old.items() if name.endswith('.meta') and (not (pkg/name).exists() or h(pkg/name)!=digest)]
assert not changed and not changedMeta,(changed,changedMeta)
(ev/'builds').mkdir(exist_ok=True);builds={}
for kind in ['linux-dev','linux-release','android-dev']:
 p=Path('/tmp/dreamy-workflow-builds')/kind/'receipt.json';receipt=json.loads(p.read_text());assert receipt['result']=='Succeeded',receipt
 shutil.copyfile(p,ev/'builds'/(kind+'.json'));binary=p.parent/('DreamyDebugger.apk' if kind=='android-dev' else 'DreamyDebugger.x86_64')
 builds[kind]={'receipt':'builds/'+kind+'.json','binary':str(binary),'sha256':h(binary),'bytes':binary.stat().st_size,**receipt}
assert not builds['linux-release']['debuggerAssets']
result={'candidate':'0.3.0-preview.3','unity':'6000.4.12f1 Linux','tests':tests,'builds':builds,'archive':{**gate,'file':'artifacts/'+archive.name},'integrity':{'sourceArchiveByteMismatches':0,'protectedFilesUnchanged':len(protected),'changedExistingPackageMetas':changedMeta,'gameSourceMatchesTestClone':True},'editorVerification':'Target MCP sandbox instance unavailable; isolated package consumers and a project copy were used. Main live Editor state was not altered or certified via scene tools.','limits':['Consumer Android build certifies sample/package compile and linking, not the game-specific reflected type on device.','No new real mobile/GC/thermal/native fault/iOS evidence.','Earlier receipts are historical; no commit/tag/push/publish performed.'],'observedFailures':['Initial MCP read/refresh routed to the other editor; targeting sandbox then failed because it was absent. No scene/prefab/data mutations or tests were performed there; all further verification used isolated consumers.','Main project batchmode refused its existing Editor lock; did not force-close it.','Full clone UPM import initially failed with filesystem quota -122; removed only owned older disposable consumer Library caches and retried successfully.','An npm pack invocation at the sandbox root lacked package.json; final invocation explicitly names the package directory.']}
(ev/'verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps({'tests':{k:v['passed'] for k,v in tests.items()},'builds':{k:v['result'] for k,v in builds.items()},'sha256':gate['sha256'],'protected':len(protected)},indent=2))
