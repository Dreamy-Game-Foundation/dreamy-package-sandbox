"""Copy this implementation's local receipts and fingerprint the changed package files."""
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

destination = Path(__file__).resolve().parent
repository = destination.parents[3]
package = repository / 'LocalPackages/com.dreamy.debugger'
consumers = Path(sys.argv[1])
current = consumers / 'DreamyDebuggerOriginalInspection'
baseline = consumers / 'DreamyDebuggerBaselineInspection'
build = consumers / 'DreamyDebuggerInspectionBuild'
for source, name in [
    (current / 'editmode-all.xml', 'editmode.xml'),
    (current / 'playmode.xml', 'playmode-scene.xml'),
    (current / 'playmode-all.xml', 'playmode-regression-earlier.xml'),
    (baseline / 'baseline.xml', 'baseline-int64-failure.xml'),
    (build / 'windows-player.json', 'windows-player-earlier.json'),
]:
    if source.exists():
        shutil.copy2(source, destination / name)
for source in (current / 'SceneInspectionEvidence').glob('*.png'):
    shutil.copy2(source, destination / source.name)

changed = set()
previous = json.loads((destination / 'verification.json').read_text(encoding='utf-8')) if (destination / 'verification.json').exists() else {}
if (destination / 'source-sha256.json').exists():
    changed.update(json.loads((destination / 'source-sha256.json').read_text(encoding='utf-8')))
for arguments in [('diff', '--name-only', '-z'), ('ls-files', '--others', '--exclude-standard', '-z')]:
    data = subprocess.check_output(['git', '-C', str(package), *arguments])
    changed.update(name.decode('utf-8') for name in data.split(b'\0') if name)
hashes = {name: hashlib.sha256((package / name).read_bytes()).hexdigest()
          for name in sorted(changed) if (package / name).is_file()}
(destination / 'source-sha256.json').write_text(json.dumps(hashes, indent=2) + '\n', encoding='utf-8')
summary = {'branch': subprocess.check_output(['git', 'branch', '--show-current'], cwd=repository).decode().strip(),
           'package_baseline': previous.get('package_baseline') or subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=package).decode().strip(),
           'package_commit': subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=package).decode().strip(),
           'package_working_tree_clean': not bool(subprocess.check_output(['git', 'status', '--porcelain'], cwd=package).strip()),
           'unity': '6000.4.12f1', 'tests': {}}
for name in ('editmode.xml', 'playmode-scene.xml', 'playmode-regression-earlier.xml', 'baseline-int64-failure.xml'):
    file = destination / name
    if file.exists():
        root = ET.parse(file).getroot()
        summary['tests'][name] = {key: root.attrib.get(key) for key in ('result', 'total', 'passed', 'failed', 'start-time', 'end-time')}
summary['notes'] = [
    'Package source is committed; see package_commit and the checkpoint for source-push status.',
    'tested-source-sha256.json retains test-time bytes; only generated YAML trailing whitespace was normalized before commit.',
    'Earlier broad PlayMode run: one Int64 Cheats failure reproduced on the untouched 0.1.8 baseline.',
    'World-space runtime camera renders are not Device Simulator or physical-device evidence.',
    'Earlier Windows smoke predates later search/UI refinements and is not exact-final-source build proof.',
    'Android native IL2CPP attempt failed due to no space on C:. No Android runtime result.',
    'User requested no further builds. No build or public release is certified by this checkpoint.',
]
(destination / 'verification.json').write_text(json.dumps(summary, indent=2) + '\n', encoding='utf-8')
print(json.dumps(summary, indent=2))
