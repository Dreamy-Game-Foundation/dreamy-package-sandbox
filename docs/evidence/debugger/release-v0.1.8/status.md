# Dreamy Debugger 0.1.8

Status: complete — package main and annotated tag v0.1.8 pushed to origin at 1516f9fff4f62e7b3acb5f38b566509f5e47291b. Sandbox synchronization is committed separately.

Package manifest/runtime, changelog, README and current release/migration/tracking/platform/guide documentation are aligned at 0.1.8. The patch contains UI setter exceptions, re-reads authoritative values and isolates bool rendering from user input. Dependencies, assembly boundaries and serialized GUIDs are unchanged. Existing source behavior hashes match the 16 EditMode + 4 PlayMode test receipt from the prior fix task; those suites were not repeated for metadata preparation.

Fresh Linux Unity 6000.4.12f1 batchmode version test passed 1/1 (version-test.xml). Archive verification and local README links passed (archive-verification.json). Physical Android input, Crashlytics ingestion, rebuilt consumer APK and fresh Git-install compilation are not claimed.

The user requested committing/pushing all existing sandbox changes, including the Missions v0.4.0 gitlink, its evidence/patch/plan, and the dynamic localization font atlas reset. Missions commit 9d146b0 was already reachable at its remote main/tag. The font remains dynamic with its source TTF reference and multi-atlas mode enabled; it rebuilds glyphs at runtime. Planet Packages/ remains unmodified and will be updated by the user separately.

Whitespace review: the debugger package and sandbox source/documentation checks pass. Existing archived Missions failure XML stack traces and two raw patch files retain their original trailing/context whitespace; these historical artifacts were not rewritten merely to silence git diff --check.
