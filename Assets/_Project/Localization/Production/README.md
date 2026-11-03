# Sandbox production localization

This is the game-owned copy of Production Starter, promoted from Assets/Samples. Customize here; package updates do not overwrite it.

- Source/: 12 locale JSON files, 50 shared keys, including all Foundation labels and dynamic score/health/status. Original canonical JSON/meta GUIDs were retained when moving.
- Profiles/JsonImport.asset: canonical import profile; table DreamyGame, output Assets/_Project/Localization/Generated, en source/fallback.
- Profiles/Runtime.asset: used by GameInstaller and the standalone test root. GameInstaller is the main-game owner; do not add a second LocalizationBootstrap prefab to BootstrapScene.
- Fonts/: full source fonts and TMP fallback chain, actively used by Foundation, Settings and the standalone scene. Keep licenses when customizing.
- Prefabs/: reusable starter prefabs. LocalizedLabel is already configured for DreamyGame/settings.title and the production font, ready to add under a sandbox Canvas.

Country Flags lives beside this folder. Profiles/LanguageIcons.asset in the parent folder maps the 12 locales to flag Sprites. Settings Integration is also game-owned beside this folder. Test scene remains independent of PanelManager.

Run Dreamy → Localization → Refresh Languages And Dropdown after editing locale data; it validates/imports and refreshes both Settings prefabs without rebuilding scenes. Configure Main Labels refreshes Foundation bindings.
