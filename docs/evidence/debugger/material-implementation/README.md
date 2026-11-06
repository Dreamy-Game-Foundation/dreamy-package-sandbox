# Material Compact implementation and visual review

Status: implementation complete for the isolated Editor/runtime UI scope, 2026-11-07. Physical-device and native Editor visual gates remain unverified. Package source commit `cef4acb9a081a109bcdc2fb5abed09920ebfdd08`; baseline `de6b75f`, version remains 0.1.2 with an Unreleased changelog entry. Local commits only; no new tag or remote release.

Open [the interactive visual gallery](index.html). It shows unchanged PNG captures at their real dimensions, allows fit/100% display and includes a clearly labelled AI-concept reference. The screenshots are synthetic isolated Unity UI fixtures, not the user's gameplay or phone captures. The separately named generated concept is never acceptance evidence.

## Delivered

- Resource-backed Barlow/Semibold and JetBrains Mono skin, actual 32px Material PNG icons/sliced surfaces, compact cards and dark surfaces with red primary actions. Original fonts/notices retained. Three font sources total 482,640 bytes.
- Alpha Dreamy lettering extracted deterministically from supplied original artwork into 512×84 Wordmark.png. No replacement font or generated lettering. Production images use the texture with preserved aspect. No mipmaps or Read/Write; Android/iPhone importer overrides ASTC 4×4. These presets are not proof of device memory/quality.
- Responsive sidebar/header/card flow, single-row filters on short landscape, ordinary scrollable content rather than shrinking all labels. Small category pin icons and collapsed chips; expanded controls are lazy/paged inside a bounded scroll viewport, with fallback for incomplete cards. Same registration/confirmation/access owners; no game-state ownership change.
- Translucent full-screen-anchored default-off HUD, separate drag icon, header tap cycles and close button; allowlisted presentation preferences. Existing sampler and Graphy shader retained; histories start at the left.
- Centered, titleless PIN form with separate Cancel/backspace, long PIN/passphrase support; actual pointer raycast tests. Console dock has independent close control and correct circular markers. Added teardown null guards for console/dock filter events.
- Small-logo native Editor Settings header and six tabs; no new first-import theme/setup step. Existing script/prefab GUIDs preserved; original SR comparison package and game scenes untouched.

## Fresh receipts

| Check | Result / input |
|---|---|
| PlayMode, OpenGL | **73/73 passed**, PlayMode.xml, isolated local-package consumer, final committed source |
| Clean Git UPM import + compile + EditMode | **84/84 passed**, Git-EditMode.xml, exact final commit served read-only over loopback Git; not a GitHub/network authentication test |
| Installed Git source parity | **931 files**, no missing/different files; git-parity.json |
| Archive guard | Passed, 931 members, 3,070,881 bytes, SHA256 in archive-verification.json |
| Saved prefabs | 29 prefabs, no missing scripts, via DebuggerAssetBuilder.ValidatePrefabs |
| Rendered fixtures | 1280×720, 852×393, 640×300, 393×852; portrait/landscape rotation, drawer open/closed, keypad/passphrase, HUD/pins/dock. Screenshots and SHA256 manifest attached. |
| Sample-inclusive earlier EditMode | 92/92 passed before final toolbar/teardown refinement; Earlier-sample-EditMode.xml is retained as historical evidence, not substituted for final-source package acceptance. Eight extra tests belong to the imported Quick Start sample. |

PlayMode exercises actual GraphicRaycaster/EventSystem pointer routing for Cancel/Delete, HUD cycle/close and a Grant action after scrolling a 640×300 viewport. The latter opens a configured confirmation; cancel leaves resources unchanged. Lazy category expansion checks 100 options without allocating every control up front. Existing cheat owner/access/stale-confirm/report and preference tests also run.

No source/meta deletion or bulk GUID regeneration. Asset changes were authored through Unity Editor import/PrefabUtility APIs; source checks report no C#/Markdown whitespace errors. Unity's own serialized empty YAML fields retain its generated whitespace.

## Observed failures and corrections

- Re-running authoring originally lost the medium font and produced a confirmation heading assertion failure. Preserve medium references, explicitly apply the confirmation heading font and avoid faux bold; final matching test receipt passes.
- Landscape Amount/Grant wrapped excessively. Reduced preferred widths and used a single toolbar row for short views, while keeping scrolling and original controls.
- Dock close initially overlapped severity counts; reserve toolbar width. Circular row indicators were initially centered across the whole row; restore their left marker anchor while preserving aspect.
- A new scrolled Grant test initially expected confirmation from a fixture configured without it. Correct the fixture to require confirmation; production confirmation policy was not silently changed.
- Main sandbox Console contained ConsoleTabController.OnDestroy null filter-state teardown plus unrelated FoundationTutorialAdapter/localization formatting exceptions. Added console/dock teardown null guards and reran package checks. Do not claim the entire game's Console is clean; the unrelated game/demo errors remain outside this change.
- Native Editor API capture was all black, and Orca capture returned screenshot_failed. Excluded that image from the gallery/acceptance. The native Editor layout compiles, but its actual visual acceptance is still pending.

## Remaining verification limits

No physical Android/iOS execution, native keyboard, device notch/safe-area acceptance, mobile texture comparison, build-size/GPU residency, draw-call or frame-time/GC/thermal benchmark is claimed. The fixture camera uses the actual UI owners at controlled viewport sizes; it is not a physical phone. No real report was sent, no live gameplay resource was granted. The main sandbox stayed stopped during these final checks.

The specifically requested visual-review skill was not found in installed filesystem or Orca bundled/installed skill lists. Used unity-ui-reskin/references/visual-verification.md instead and supplied real renders plus a local interactive gallery. No background model or transcript collection was introduced. Referenced docs/agents/dreamy-workflow.md remains absent; this is the manual completion and observed-failure checkpoint.
