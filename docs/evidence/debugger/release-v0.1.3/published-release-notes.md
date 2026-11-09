Dreamy Debugger 0.1.3 refreshes the runtime UI with Material Compact and fixes sidebar rows extending past the header on devices with a left safe-area inset.

### Changes

- Barlow / JetBrains Mono, small Material PNG icons and an alpha Dreamy wordmark extracted from original artwork.
- Compact resource cards and category pin chips with lazy, paged expansion.
- HUD tap-to-cycle and close controls; separate move icon and aligned history graphs.
- Centered titleless PIN keypad with separate Cancel/Delete, plus a closeable console dock.
- Inset-aware sidebar width and icon placement across portrait, landscape and scaled canvases.
- Updated setup/API README, migration notes and version metadata. Existing GUIDs, cheat IDs and dependency boundaries are preserved.

### Installation

Unity Package Manager → Install package from Git URL:
`https://github.com/Dreamy-Game-Foundation/com.dreamy.debugger.git#v0.1.3`

Alternatively install the attached `com.dreamy.debugger-0.1.3.tgz` from tarball. Back up/merge imported samples and game-owned prefab variants before updating.

### Verification

- Unity 6000.4.12f1 / Linux: 92 EditMode tests including imported Quick Start and 74 PlayMode tests passed on the candidate.
- Fresh GitHub tag installation: 84/84 package EditMode tests passed; 931 installed files match the release commit.
- Archive guard passed; all 931 shipped files match the verified source.
- SHA-256: `3fa261f89c82bbd66c308ad1fc68841320e83c6cc07a3c8df9369c94e20c680d`.
- Release commit: `55c6e281e41097229364bf97541d6b2331161eec`.

Physical Android/iOS, native keyboard/notches, mobile texture quality, IL2CPP and device performance remain separate acceptance gates. Preserve the bundled third-party notices and applicable upstream distribution restrictions.
