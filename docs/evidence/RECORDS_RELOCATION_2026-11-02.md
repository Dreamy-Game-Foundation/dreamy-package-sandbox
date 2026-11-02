# Relocation of historical project records

Status: complete, 2026-11-02.

- Five historical plans moved to docs/plans, 178 evidence files moved to docs/evidence, and one helper script moved to tools.
- Existing destination files were checked before the move; no collisions or overwrites occurred.
- [Relocation inventory](records-relocation-2026-11-02.json) records every destination and original SHA-256. All files were byte-identical immediately after relocation.
- Markdown navigation and active code output paths were then updated. Raw JSON/XML test results, logs, screenshots and archived fixtures remain unchanged, including any historical workspace paths recorded by their original runs.
- Updated the benchmark output root and both copies of the Feedback screenshot helper to docs/evidence. Removed obsolete workflow handoff commands in the Audio and Localization plans.
- Verification: file counts, original hashes for unchanged files, localization Markdown links, diff whitespace checks and Node syntax check passed. No Unity tests, benchmark or player build were rerun for this path/documentation change.
- Localization implementation remains pending; see [the phase tracker](../plans/localization/TRACKING.md).
