from pathlib import Path
import tarfile,json,shutil,hashlib,subprocess,xml.etree.ElementTree as E
artifact=Path('/tmp/dreamy-ui-redesign-artifact/com.dreamy.debugger-0.3.0-preview.1.tgz')
root=Path('/tmp/dreamy-ui-acceptance-consumer');root.mkdir(exist_ok=True)
with tarfile.open(artifact) as tar:
 names=tar.getnames()
 bad=[n for n in names if any(x in n.split('/') for x in ['.git','__pycache__','data','node_modules']) or n.endswith(('.pyc','.sqlite','.db','.env'))]
 assert not bad,bad
 assert 'package/Samples~/QuickStart/link.xml' in names
 assert 'package/Documentation~/RELEASE.md' in names
 tar.extractall('/tmp/dreamy-ui-acceptance-unpacked',filter='data')
(root/'Packages').mkdir(exist_ok=True);(root/'ProjectSettings').mkdir(exist_ok=True)
(root/'Packages/manifest.json').write_text(json.dumps({'dependencies':{'com.dreamy.debugger':'file:'+str(artifact),'com.unity.test-framework':'1.6.0'},'testables':['com.dreamy.debugger']},indent=2))
shutil.copyfile('/tmp/dreamy-debugger-ui-consumer/ProjectSettings/ProjectVersion.txt',root/'ProjectSettings/ProjectVersion.txt')
shutil.copytree('/tmp/dreamy-ui-acceptance-unpacked/package/Samples~/QuickStart',root/'Assets/DebuggerSample',dirs_exist_ok=True)
for mode in ['EditMode','PlayMode']:
 result=Path('/tmp/dreamy-ui-redesign-artifact-'+mode+'.xml');result.unlink(missing_ok=True)
 run=subprocess.run(['/home/tutrinh/Unity/Hub/Editor/6000.4.12f1/Editor/Unity','-batchmode','-nographics','-projectPath',str(root),'-runTests','-testPlatform',mode,'-testFilter','Dreamy.Debugger','-testResults',str(result),'-logFile','/tmp/dreamy-ui-redesign-artifact-'+mode+'.log'])
 print(mode,'exit',run.returncode,E.parse(result).getroot().attrib if result.exists() else 'NO RECEIPT',flush=True)
 if result.exists():
  for t in E.parse(result).iter('test-case'):
   if t.attrib['result']=='Failed': print(t.attrib['name'],E.tostring(t.find('failure'),encoding='unicode'),flush=True)
 if run.returncode:raise SystemExit(run.returncode)
print('artifact-sha256',hashlib.sha256(artifact.read_bytes()).hexdigest(),flush=True)
