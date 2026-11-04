# Dreamy GUID isolation — 2026-11-04

Completed an explicit identity migration, preserving existing importer metadata and fileIDs. 379 GUIDs remapped across 435 files; the old→new map and per-file before/after hashes are retained here. External references were updated only in project-owned debugger assets and imported Dreamy samples. External dependency GUIDs and source archives under Documentation~ were unchanged. Every modified file was checked byte-for-byte against the planned token substitution, including after Unity reimport.

Unity audit: 30 prefab assets, no missing scripts; one startup host with valid configuration/catalog. Project tests 6/6, clean consumer EditMode 65/65 and PlayMode 12/12 passed. Tests exercise the actual PIN, Options tabs, typed numbers and registry lifecycle. No new platform builds are claimed.

Initial audit helper missed an assembly reference; corrected its inspection to SerializedObject. The first clean-consumer run still referenced the old imported sample prefab; migrating that existing consumer's 16 affected files fixed the fixture before fresh nonzero successful receipts. These earlier failures are not acceptance.

Editor was stopped at a clean BootstrapScene. A second Unity executable wrapper still held the project lock after the API exit request; it was closed before batch verification. No files diverged from planned substitutions. Temporary verification source/meta removed, and normal sandbox Editor relaunched. Unrelated localization font changes excluded.

Original SR is not installed yet. It can retain its original GUIDs on import; compile/coexistence must still be checked after import. Keep comparison bootstraps in separate scenes or enable one at a time. Do not apply Dreamy's mapping to newly imported original SR.

Rollback: parent/package commits before this migration, with optional local byte backup at /tmp/dreamy-guid-isolation-backup.zip. Existing Dreamy consumers upgrading from earlier previews must migrate their references using the package Documentation~/GuidIsolationMap.json; fresh installs use the new GUIDs directly.
