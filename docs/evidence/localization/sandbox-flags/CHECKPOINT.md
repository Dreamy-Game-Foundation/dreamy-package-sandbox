# Sandbox production setup, Foundation translations and flags

Status: complete — 2026-11-03.

Production Starter, Settings Integration and Country Flags are now game-owned sandbox assets. Canonical source/profile GUIDs moved into Production, DreamyGame remains the table owner, and 12 locales share 50 keys. Foundation static/dynamic text and production Settings dropdown flags apply live. Null icons and absent gateway retain their fallback behavior.

271 rounded/256 source PNGs verified by size/Git blob SHA at pinned upstream commit bf799e19c810fe406dd8d681e81c58e69d9a4450, with MIT license and Sprite metas, shipped as an asset-only sample. Settings contracts remain engine-independent.

Fresh receipts: 41/41 EditMode, 17/17 PlayMode, main Ready with en/vi screenshots and persisted UIRoot scaling readback. See VALIDATION.md for interrupted-test/restart correction, exact evidence and limits.

Editor stopped on BootstrapScene with original locale/start preferences restored. No commits/tags/push/releases. Existing unrelated changes preserved. Workflow/lesson registry remains unavailable; installed source/manifests and fresh receipts were used.
