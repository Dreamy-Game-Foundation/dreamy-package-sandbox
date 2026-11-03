# Production Starter

Reusable game assets for Dreamy Localization 0.2.0, authored by Dreamy Game Studio. This sample does not install a demo scene, PanelManager or Settings dependency.

## Install once, then own the copy

1. Import TMP Essential Resources.
2. Import this sample from Package Manager. Move the entire folder to Assets/Game/Localization, preserving every .meta. Keep one copy only to avoid duplicate GUIDs.
3. Customize Source/<locale>.json, profiles, fonts and prefabs in Assets. Package updates do not overwrite or merge this copy.
4. Select Profiles/JsonImport.asset. Default table is Game; output is Assets/Localization/Generated. Run Dreamy → Localization → Validate Selected JSON Profile, then Import Selected JSON Profile before Play/build.
5. Put Prefabs/LocalizationBootstrap.prefab in the game's bootstrap scene, or initialize one LocalizationService from your composition root using Profiles/Runtime.asset. Use one owner, not both.
6. Put Prefabs/LocalizedLabel.prefab under an existing game Canvas. Change its key, font/style and authored fallback. It does not own canvas/safe area/navigation.

These prefabs can be used in production scenes. Translations and visual defaults are starter content for the game to replace/review; no device/build/linguistic release certification is implied.

## Editable contents

| Folder | Customize |
| --- | --- |
| Source | en/vi/de/es/fr/id/ja/ko/pt-BR/ru/th/zh-Hans; 18 shared starter keys, including Settings labels; add/remove locales in Import Profile too |
| Fonts | Licensed Liberation Sans, full Noto Sans CJK SC Regular, Noto Sans Thai; replace sources and retain licenses |
| Fonts/TMP | LocalizationFont → CJK/Thai fallback chain; dynamic atlases and source font references are local to this sample |
| Profiles | JSON source/output ownership, source/fallback locale, table name, runtime preload and preference key |
| Prefabs | Bootstrap lifecycle/profile and localized label presentation/reference/fallback |

Unlike the sandbox test fixture, the CJK source is a full font, not a character subset. It adds about 16 MB of source font before player packing; choose locales, source inclusion and atlas budgets for your game. SC glyph design is not a substitute for separate JP/KR typography. Thai complex shaping and all production translations still need product review. The supplied sample has no RTL adapter.

The font materials use standard TMP Essential Resources shaders. Include those resources in the consumer. Font licenses are alongside source files; the package's private license does not replace their SIL OFL licenses.

The runtime profile opts into rebuilding Unity's default SmartFormatter after backend initialization, to match the verified Unity 6.4 setup. Disable this option for custom formatter extensions.

## Existing project / update

Importer refuses to take ownership of locales or tables outside its output directory. If the game already owns the same locales elsewhere, merge JSON/config into that owner or set up its existing profile; do not create duplicate locale owners. Generated tables are not shipped in this sample because their Localization Settings and Addressables ownership belongs to each game.

Changing the table name requires changing Import Profile, Runtime profile preload and component references together. Moving the source folder preserves its GUID references; review string OutputDirectory paths separately.

Do not reimport over customized assets on upgrades. Diff the new Samples~ source with the game-owned copy and port changes deliberately. See Documentation~/customization-and-upgrades.md in the package.

For Dreamy Settings, import the separate Settings Integration sample and use the Settings package's own SettingsPanel prefab. Omitting that integration keeps Localization usable without Settings.
