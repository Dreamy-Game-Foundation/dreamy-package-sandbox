# Sandbox production setup, Foundation and country flags — 2026-11-03

Completed in this sandbox. Localization source remains 0.2.0 / Settings source 0.3.0, locally prepared and untagged.

## Delivered

- Imported Production Starter, Settings Integration and Country Flags copies are game-owned under Assets/_Project/Localization. Canonical JSON sources and the existing import profile moved into Production/Source and Production/Profiles/JsonImport.asset with their original GUIDs; the generated DreamyGame table/output owner stays unchanged.
- 12 locales × 50 keys: 32 Foundation additions plus the original 18. Foundation static labels, score, health, startup/save/load/online-reward status all use stable keys. Runtime arguments preserve the current score/health/status across language changes. Score/health bindings start empty in the prefab because anonymous arguments are runtime state; the presenter configures key/arguments atomically before Smart String loading.
- GameInstaller and the independent localization test root now reference the production runtime profile. Foundation, Settings, standalone scene and reusable label use the production full-source font/fallback chain. The reusable label is configured for DreamyGame/settings.title. No second bootstrap owner was inserted into the game.
- Country Flags sample ships 271 unchanged rounded/256 PNGs, Unity single-Sprite metas, source manifest, editable locale mapping and MIT license. Origin: open-assets-hub/country-flag-collection, commit bf799e19c810fe406dd8d681e81c58e69d9a4450. Every downloaded file's size and Git blob SHA was verified; 3,911,678 bytes total. See flags-source.json / sample SOURCE.json.
- SettingsLanguageIcons is editable UI sample data; each dropdown option and selected caption can have a Sprite. Runtime Settings contracts gained no engine/image dependency. Null catalog/missing icon keeps text options; absent gateway still hides controls and clears stale state. Default mappings include en→us, vi→vn, ja→jp, ko→kr, pt-BR→br, zh-Hans→cn and explicit direct maps for the other six locales.
- Settings Integration authoring tool accepts an optional icon asset and wires image/text padding. Host setup reserves 64px rows and a 328px bounded scroll region. Existing prefab objects were kept.
- MainScene's actual PanelManager UIRoot CanvasScaler was ConstantPixelSize, clipping the 1080-high Settings card in the 452×839 Game View. Set that canvas to ScaleWithScreenSize, 1080×1920, match .5. The separate UICanvas's existing user change was preserved. The standalone test scene remains manager-free.

## Fresh verification

| Check | Result | Receipt |
| --- | --- | --- |
| EditMode | 41/41 passed, 12:26:06–12:26:14Z | editmode.xml |
| PlayMode | 17/17 passed, 12:27:24–12:27:37Z | playmode.xml |
| Main game | Ready, vi; all 12 options have correct flags; current caption vn; all visible Foundation text translated and score/status 10 retained | main-smoke.json |
| Persisted scene launch | Ready after stopping/saving/relaunching; UIRoot ScaleWithScreenSize 1080×1920, actual scale .42764923 at 452×839 | persisted-main-smoke.json |
| Serialization | Existing Foundation/Settings/standalone prefab IDs retained; canonical JSON GUIDs and 18 old shared entry IDs preserved; 32 new keys | serialized-preservation.json |
| Flag files | 271 source blobs verified; PNG bytes unchanged, Sprite importer metas present | flags-source.json / sample SOURCE.json |
| Final compile/Console | No C# project compile errors and final sandbox Console zero errors | final-console.json |
| Source/format | Root and both package diff --check; current file identity | source-identity.json |

PlayMode extends previous coverage with caption/option flags, flag-vs-text bounds, clearing the previous caption image without gateway, text-only fallback without catalog and Foundation en→vi→ja with current score/health/status. The Foundation test never initializes a PanelManager. Existing locale/persistence/cancel/setup/subscription cases still pass.

Inspected actual game screenshots: foundation-en.png, foundation-vi.png, settings-flags-en.png, settings-flags-vi.png, dropdown-flags-vi.png. Flags/text do not overlap; Foundation buttons and the Settings Close button fit the small portrait viewport. Screenshots were captured from the main game's real SettingsPanel, not a separate mockup.

## Observed failures/corrections

- The first EditMode attempt using the old Base64 snapshot did not produce a fresh XML. Editor responses stalled while its project lock stayed held and resident memory was about 6 GB. Desktop/process discovery initially missed the Editor, so the initial “closed” diagnosis was corrected after lock-owner inspection. No OOM cause is asserted.
- Two attempted GUI launches exited while the original project lock was still held; batchmode likewise refused a duplicate instance. The original sandbox Editor alone was stopped with SIGTERM after confirming saved scenes, then reopened. No lock was deleted and no other project's Editor was stopped.
- Snapshot tests now stream SHA256 per file instead of retaining full Base64 font/image content or emitting giant array-diff values. Fresh EditMode then passed in about 8 seconds; the interrupted attempt is not counted as passing.
- Installed TMP 5 OptionData requires text/image/color; the implementation uses that three-argument constructor. Caption visibility is Image.enabled, not GameObject.activeSelf; assertions follow installed source.
- Manual MCP snippets hit an unavailable TestRunnerApi.IsTestRunActive member, a duplicate Newtonsoft type reference and a destroyed SettingsPanel lookup after Close. These were invocation-only failures, corrected using SessionState, the bridge's serialized return value and a newly opened real panel. No project source compile failure or product lifecycle patch resulted.
- A dirty MainScene preflight became clean before the save API ran; the pre-existing UICanvas scaler edit was already on disk and retained. The later UIRoot scaler change is intentional and verified after a fresh launch.
- Unity serializer trailing spaces in edited prefabs were normalized without changing IDs or content semantics.

## Final state and limits

Editor stopped on BootstrapScene, scene clean. Original saved locale and temporary test/start settings restored. Existing unrelated package changes retained. No commit/tag/push or release was performed.

This verifies Editor integration and a small portrait viewport, not a player/device/landscape build, native translation review, complete CJK typography/Thai shaping, importer crash recovery or asset-table lease/release behavior. Other feature panels' internal UI text remains outside the Foundation-panel translation scope. Country flags are editable presentation choices, not automatic country/language inference.
