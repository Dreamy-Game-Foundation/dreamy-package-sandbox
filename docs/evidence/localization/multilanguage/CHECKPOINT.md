# Multilanguage dropdown checkpoint

Status: complete — 2026-11-03.

Delivered 12 locale JSON/table setup, CJK/Thai font fallbacks, 64px nonoverlapping dropdown rows and bounded scrolling on both production and standalone Settings prefabs. Test scene remains manager-free and byte-identical. Existing prefab IDs/GUIDs and shared key IDs preserved.

Verification: 41/41 EditMode, 15/15 PlayMode; four inspected portrait screenshots. See VALIDATION.md for receipts, observed failures/corrections and exact limits.

No commits/pushes. Unrelated package edits and pre-existing EditorSettings enabled=1/options=0 diff preserved. Editor stopped on LocalizationSettingsTest, locale preference en.

Workflow docs/agents/dreamy-workflow.md and local lessons/compatibility registry were unavailable. Used installed source, manifest/lock and fresh Unity receipts.
