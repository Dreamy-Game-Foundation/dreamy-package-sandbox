# Multilanguage and dropdown validation — 2026-11-03

Completed: 12 locales, 18 shared string keys, immediate Settings/scene updates, readable scrollable dropdown. The test scene remains independent of PanelManager/UIPanel.

## Changes

- New JSON/locales/tables: de, es, fr, id, ja, ko, pt-BR, ru, th, zh-Hans beside en/vi. English remains source/fallback. All files retain the same keys and `{amount}` selector.
- Both imported production SettingsPanel and generated StandaloneSettingsView now use 64px option rows, 52px label space with padding, font auto sizing 24–30 and a clamped vertical scroll viewport capped at 328px. Previously rows were 20px with 30px text.
- Existing Liberation Sans font keeps its GUID and gains CJK/Thai fallbacks. Font sources, licenses and CJK subset generation instructions are in Assets/_Project/Localization/Fonts. The CJK subset source is 82 KB; no full 19 MB collection was added.
- Builder discovers JSON files. Refresh Languages And Dropdown imports and updates existing prefabs in place without rebuilding the scene.

## Fresh successful receipts

- [EditMode](editmode.xml): 41/41 passed, start 2026-11-03 04:32:39Z.
- [PlayMode](playmode.xml): 15/15 passed, start 2026-11-03 04:33:56Z, end 04:34:04Z. Includes all prior cases plus every language selected through the actual dropdown, applied title/reward, persisted locale, glyph coverage, nonoverlapping option geometry/text and scrolling to the last row.
- Static JSON validation: exactly 12 files × 18 matching keys, amount placeholder retained.
- [Serialization receipt](serialized-preservation.json): both Settings prefab object/fileID sets preserved; existing prefab/font/scene/shared-data meta files unchanged; shared key/ID asset and test scene byte-identical to baseline.
- Manual portrait smoke at 1284×2778: ready=true, 12 native-language options, PanelManager count=0. Inspected [top](dropdown-top.png), [bottom](dropdown-bottom.png), [Japanese](settings-ja.png), [Thai](settings-th.png) screenshots directly.
- Final Console: zero errors. Package metadata-folder warnings and Unity Localization 1.5.8 SerializeReference/base-class warnings remain; they are outside this layout/data change.
- git diff --check passed after normalizing serializer-added trailing spaces in the two edited prefabs. No GUID/fileID/content semantics changed during whitespace normalization.

## Observed failures and corrections

- Initial font-tool installation hit a read-only default cache, then sandbox DNS restrictions. Installation succeeded using /tmp cache/target and approved network execution; project dependencies were not changed.
- Repeating font preparation exposed installed TMP 5 TryAddCharacters returning the input as missing when no new glyphs need adding. Changed validation to inspect actual character tables across fallbacks; repeat refresh succeeded.
- Initial EditMode run: 40/41 passed; the existing fixture expected two locales. Updated the fixture to 12 locales/18 keys; [initial failed receipt](editmode-initial-failure.xml) retained and fresh matching run passed.
- Native test run bookkeeping initially overlapped: the passing EditMode XML was routed to the runner's playmode output filename. Its embedded platform=EditMode and 41 cases identify it; copied intact to editmode.xml. Reran PlayMode sequentially and retained its distinct platform=PlayMode, 15-case receipt above.
- A manual MCP smoke called TMP_Dropdown.Show in the same frame as instantiating the view, before TMP Start initialized its tween runner, causing an invocation-only NullReferenceException. Waiting a frame resolved it; normal input and the PlayMode test yield before opening. No product lifecycle patch was needed.
- Initial clone-over-existing generated prefab remapped duplicate child names. Restored the clean starting prefab, changed the builder to update it in place, re-applied only intended presentation changes, and verified all original fileIDs above.

## Limits and final state

Translations are test copy, not native-language-reviewed release copy. CJK font is a fixture subset using a simplified Chinese face; locale-specific Han design and detailed Thai shaping need production review. No RTL languages were added. No player build, device or landscape validation is claimed.

Editor stopped on LocalizationSettingsTest. Manual smoke restored locale en. Temporary toolbar/start/reload settings were restored; the pre-existing EditorSettings diff (enabled=1/options=0) is preserved. No commit, push, package upgrade or release was performed. Existing unrelated package edits were left in place.
