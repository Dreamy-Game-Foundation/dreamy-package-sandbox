from pathlib import Path
import subprocess,os,json
base=Path('/tmp/dreamy-ui-acceptance-builds');base.mkdir(exist_ok=True)
for target,method in [('linux-dev','BuildLinuxDevelopment'),('linux-release','BuildLinuxRelease'),('android-dev','BuildAndroidDevelopment')]:
 out=base/target;out.mkdir(exist_ok=True)
 env=dict(os.environ,DREAMY_BUILD_OUTPUT=str(out))
 args=['/home/tutrinh/Unity/Hub/Editor/6000.4.12f1/Editor/Unity','-batchmode','-nographics','-quit','-projectPath','/tmp/dreamy-ui-acceptance-consumer','-executeMethod','Dreamy.Debugger.Editor.DebuggerBuildVerification.'+method,'-logFile',str(base/(target+'.log'))]
 if target=='android-dev':args+=['-buildTarget','Android']
 run=subprocess.run(args,env=env)
 receipt=out/'receipt.json'
 print(target,'exit',run.returncode,receipt.read_text() if receipt.exists() else 'NO RECEIPT',flush=True)
 if run.returncode:break
