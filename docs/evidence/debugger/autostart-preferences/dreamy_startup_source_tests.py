from pathlib import Path
import subprocess,xml.etree.ElementTree as E
for mode in ['EditMode','PlayMode']:
 result=Path('/tmp/dreamy-runtime-ui-'+mode+'.xml')
 result.unlink(missing_ok=True)
 run=subprocess.run(['/home/tutrinh/Unity/Hub/Editor/6000.4.12f1/Editor/Unity','-batchmode','-nographics','-job-worker-count','2','-projectPath','/tmp/dreamy-debugger-ui-consumer','-runTests','-testPlatform',mode,'-testFilter','Dreamy.Debugger','-testResults',str(result),'-logFile','/tmp/dreamy-runtime-ui-'+mode+'.log'])
 print(mode,'exit',run.returncode,E.parse(result).getroot().attrib if result.exists() else 'NO RECEIPT',flush=True)
 if result.exists():
  for t in E.parse(result).iter('test-case'):
   if t.attrib['result']=='Failed': print(t.attrib['name'], E.tostring(t.find('failure'),encoding='unicode'),flush=True)
 if run.returncode: raise SystemExit(run.returncode)
