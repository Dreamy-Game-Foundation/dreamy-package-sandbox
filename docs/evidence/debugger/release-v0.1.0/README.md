# Dreamy Debugger v0.1.0 release preparation

Status: complete — documentation, metadata and a private distribution archive are prepared. No tag, push or publication was performed.

Package/runtime version is 0.1.0. This is the user's requested numbering reset from 0.3.0-preview.6; earlier 0.2.x tags remain unchanged. README covers local/Git/tarball installation, automatic/manual startup, sample import-to-Play, adding attributed partials, complete scene-owned host, asmdefs/link.xml, service bindings, access/closing/disabling, Console/HUD/local preferences APIs, reporting/capture and local/remote Telegram setup. Changelog, migration, platform, tracking and release docs agree. Author and existing third-party restrictions remain unchanged.

Validation used Unity 6000.4.12f1 Linux and Test Framework 1.6.0 with the final tarball as the consumer dependency. Imported the archive's Quick Start. Fresh receipts: 92/92 EditMode and 64/64 PlayMode passed, including version alignment and sample lifecycle. The three complete README game partial/host files compiled in their own asmdef; copies are attached. Relay/setup suite: 20/20 passed offline using local/mocked transport, no Telegram delivery. Documentation links, JSON version alignment, git diff whitespace and the release archive guard passed. Every archived file was compared byte-for-byte to the committed package source; verification.json records the commit/hash.

Artifact: /tmp/dreamy-debugger-release-v0.1.0/final/com.dreamy.debugger-0.1.0.tgz. Manifest/lock, archive contents/sha256 and XML receipts are attached; the binary is not committed into the evidence directory. npm pack uses the package's explicit file allowlist.

No new Android/iOS/player build, device touch/IL2CPP, visual, performance, native crash or production relay/privacy acceptance is claimed. Older build receipts remain historical. The actual game must validate its own service bindings and preserved reflected types on its target devices. Existing modified sandbox scenes/config/samples and unrelated staged work were not included.

The referenced docs/agents/dreamy-workflow.md is unavailable in this checkout. This serves as the manual completion checkpoint rather than claiming a CLI/harness receipt that did not run.
