from pathlib import Path
import hashlib,json,shutil,tarfile,xml.etree.ElementTree as E,re,sys
root=Path('/run/media/tutrinh/NewData/Unity Project/Dreamy Foundation/dreamy-package-sandbox');pkg=root/'LocalPackages/com.dreamy.debugger';ev=root/'docs/evidence/debugger/attribute-migration';digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
archive=Path('/tmp/dreamy-attribute-artifact/com.dreamy.debugger-0.3.0-preview.2.tgz')
sys.path.insert(0,str(pkg/'Tools~'));from verify_package import verify
gate=verify(archive);inputs={}
with tarfile.open(archive) as tar:
 for m in tar.getmembers():
  if m.isfile():
   name=m.name.removeprefix('package/');h=hashlib.sha256(tar.extractfile(m).read()).hexdigest();assert digest(pkg/name)==h,name;inputs[name]=h
(ev/'package-inputs.json').write_text(json.dumps(inputs,indent=2)+'\n')
(ev/'artifacts').mkdir(exist_ok=True);shutil.copyfile(archive,ev/'artifacts'/archive.name)
tests={}
for source,prefix in [('source','/tmp/dreamy-runtime-ui-'),('artifact','/tmp/dreamy-attribute-artifact-')]:
 for mode in ['EditMode','PlayMode']:
  p=Path(prefix+mode+'.xml');r=E.parse(p).getroot();assert r.attrib['result']=='Passed',r.attrib;name=source+'-'+mode+'.xml';shutil.copyfile(p,ev/name);tests[source+'-'+mode]={'receipt':name,'sha256':digest(p),**r.attrib}
protected=json.loads((root/'docs/evidence/debugger/ui-redesign/protected-files.json').read_text());assert all(digest(root/p)==h for p,h in protected.items())
old=json.loads((root/'docs/evidence/debugger/ui-redesign/baseline-files.json').read_text());assert all(digest(pkg/p)==h for p,h in old.items() if p.endswith('.meta'))
(ev/'builds').mkdir(exist_ok=True);builds={}
for kind in ['linux-dev','linux-release','android-dev']:
 p=Path('/tmp/dreamy-attribute-builds')/kind/'receipt.json';j=json.loads(p.read_text());assert j['result']=='Succeeded';shutil.copyfile(p,ev/'builds'/(kind+'.json'));binary=p.parent/('DreamyDebugger.apk' if kind=='android-dev' else 'DreamyDebugger.x86_64');builds[kind]={'receipt':'builds/'+kind+'.json','binary':str(binary),'sha256':digest(binary),'bytes':binary.stat().st_size,**j}
assert not builds['linux-release']['debuggerAssets']
result={'version':'0.3.0-preview.2','unity':'6000.4.12f1 Linux','archive':{**gate,'file':'artifacts/'+archive.name},'tests':tests,'projectTests':{'passed':10,'total':10,'receipt':'project-tests.json','jobId':'c9e0bb5130d944c1bbb58ed446ae3f13'},'builds':builds,'integrity':{'sourceMatchesArchive':True,'protectedFilesUnchanged':len(protected),'existingPackageMetasUnchanged':True,'walletSourceMovedWithSameMeta':True},'scope':{'gameOptionIdsPreserved':32,'completeCards':10,'projectSampleDependencyRemoved':True,'editorScene':'BootstrapScene clean; Edit Mode','realSaveGrantOrDestinationDeliveryPerformed':False},'limitations':['Game-specific GameCheatOptions has not been built/run on device; Android consumer certifies the package and imported sample only.','No performance benchmark or native screenshot changes claimed; prior UI images are historical.','Package APIs and sample authoring changed after preview.1; old preview.1 receipts do not certify preview.2.','Deprecated Unity object-finding API warnings are retained for minimum Unity 6000.0 compatibility.'],'observedFailures':['Initial attribute fixture had ambiguous Category (NUnit vs ComponentModel); qualified name fixed. A 9-test result while compilation was blocked is rejected as stale; fresh matching run contains 10 tests.','First npm pack used sandbox directory lacking package.json; corrected to package directory before final artifact creation.'],'commitOrPublishPerformed':False}
(ev/'verification.json').write_text(json.dumps(result,indent=2,ensure_ascii=False)+'\n');print(json.dumps({'tests':{k:v['passed'] for k,v in tests.items()},'project':10,'builds':{k:v['result'] for k,v in builds.items()},'archiveSha256':gate['sha256']},indent=2))
