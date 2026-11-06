from pathlib import Path
import json,shutil,subprocess,xml.etree.ElementTree as E
source=Path('/run/media/tutrinh/NewData/Unity Project/Dreamy Foundation/dreamy-package-sandbox');target=Path('/tmp/dreamy-workflow-game-consumer');target.mkdir(exist_ok=True)
for folder in ['Assets','ProjectSettings']:shutil.copytree(source/folder,target/folder,dirs_exist_ok=True)
(target/'Packages').mkdir(exist_ok=True)
manifest=json.loads((source/'Packages/manifest.json').read_text())
for key,value in manifest['dependencies'].items():
 if value.startswith('file:'):manifest['dependencies'][key]='file:'+str((source/'Packages'/value[5:]).resolve())
(target/'Packages/manifest.json').write_text(json.dumps(manifest,indent=2))
# Keep git dependency resolutions pinned to the source lock, but let UPM resolve local absolute paths in this clone.
shutil.copyfile(source/'Packages/packages-lock.json',target/'Packages/packages-lock.json')
result=Path('/tmp/dreamy-workflow-game.xml');result.unlink(missing_ok=True)
run=subprocess.run(['/home/tutrinh/Unity/Hub/Editor/6000.4.12f1/Editor/Unity','-batchmode','-nographics','-projectPath',str(target),'-runTests','-testPlatform','EditMode','-testFilter','Dreamy.Template.Debugging.Tests','-testResults',str(result),'-logFile','/tmp/dreamy-workflow-game.log'])
print('exit',run.returncode,E.parse(result).getroot().attrib if result.exists() else 'NO RECEIPT',flush=True)
if result.exists():
 for case in E.parse(result).iter('test-case'):
  if case.attrib['result']=='Failed':print(E.tostring(case.find('failure'),encoding='unicode'))
