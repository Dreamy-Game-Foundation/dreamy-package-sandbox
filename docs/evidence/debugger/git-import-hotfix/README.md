# Git import hotfix — 2026-11-06

Status: complete. Package 0.1.1 fix and parity check pushed to origin/main at d793eb1a3cdad43a4b7ff4e235137594eb60dc6f. No tag was created. Original Git 0.1.0 commit 5912da131b70d7574f3e25bd18f7c046ce34024a remains unchanged.

## Observed failure

User reported CS0234 Controls.Data and CS0246 OptionsControlBase in a Git-installed package. Read-only Console inspection of planet-bunker-tu-code showed those exact errors. No files/editor state in that project were changed. A separate new consumer `/tmp/dreamy-debugger-git-consumer` using the same remote Git commit reproduced the failure (exit 1), without test framework, testables or imported sample. Before parity check finds precisely eight tracked control scripts plus their eight metas missing. Compiler errors and parity receipt are attached.

The source Git tree contains the files; the UPM Git-resolved package has an empty Controls/Data directory. The package's broad .gitignore `data/` rule filters this directory through UPM Git packaging. The earlier npm tgz retained all files, so tarball-only testing did not cover Git installation. Previous source/tarball passes were real but insufficient to claim the Git import path was accepted. This is a release-check failure; future acceptance must include actual fresh Git resolution and installed-source parity.

## Fix and fresh acceptance

Scope ignore to `/Tools~/relay/data/`. Keep all asmdefs, class namespaces and asset/script GUIDs unchanged; no cache/meta deletion or reference workaround. Bump package/runtime to 0.1.1, update README/changelog/release/migration and add Tools~/verify_git_install.py. UPM injects `_fingerprint` into package.json: the verifier permits only the exact source commit there, compares all authored manifest fields semantically and compares other file bytes. Repository .gitignore itself is intentionally omitted by UPM and excluded from parity.

A new local Git consumer with no sample/tests compiled successfully after the fix. Then `/tmp/dreamy-debugger-git-final-consumer` resolved the final exact remote HTTPS Git commit from GitHub. The final Git receipt checks 885 files, zero missing/different. The imported Quick Start is copied byte-for-byte from the same committed template; all package and sample tests pass: 92 EditMode and 64 PlayMode under Unity 6000.4.12f1/Linux, Test Framework 1.6.0. Exact manifest/lock and fresh XML receipts are attached.

A new 0.1.1 npm tarball also passes the archive guard (hash/size attached), at `/tmp/dreamy-debugger-release-v0.1.1/com.dreamy.debugger-0.1.1.tgz`. This does not replace the Git acceptance evidence. No new phone/IL2CPP/device/performance acceptance is claimed.

## Consumer update

Set the Git dependency to `https://github.com/Dreamy-Game-Foundation/com.dreamy.debugger.git#d793eb1a3cdad43a4b7ff4e235137594eb60dc6f`, or use Package Manager Install package from Git URL with that URL. Unity must resolve the new hash; keeping 5912da1 keeps the defect. Keep game/sample/config customizations and all meta files. Do not manually patch Library/PackageCache.

Referenced docs/agents/dreamy-workflow.md is absent. This is the manual failure observation and completion checkpoint; no native recall/verification receipt is fabricated.
