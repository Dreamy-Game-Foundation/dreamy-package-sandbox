from pathlib import Path
import subprocess,os,json
base=Path('/tmp/dreamy-startup-builds');base.mkdir(exist_ok=True)
for target,method in [('linux-dev','BuildAutomaticLinuxDevelopment'),('linux-release','BuildAutomaticLinuxRelease'),('android-dev','BuildAutomaticAndroidDevelopment')]:
 out=base/target;out.mkdir(exist_ok=True)
 env=dict(os.environ,DREAMY_BUILD_OUTPUT=str(out))
 args=['/home/tutrinh/Unity/Hub/Editor/6000.4.12f1/Editor/Unity','-batchmode','-nographics','-job-worker-count','2','-quit','-projectPath','/tmp/dreamy-startup-final-consumer','-executeMethod','Dreamy.Debugger.Editor.DebuggerBuildVerification.'+method,'-logFile',str(base/(target+'.log'))]
 if target=='android-dev':args+=['-buildTarget','Android']
 run=subprocess.run(args,env=env)
 receipt=out/'receipt.json'
 print(target,'exit',run.returncode,receipt.read_text() if receipt.exists() else 'NO RECEIPT',flush=True)
 if run.returncode:break
