# Country Flags — rounded 256

271 PNG flags from [open-assets-hub/country-flag-collection](https://github.com/open-assets-hub/country-flag-collection/tree/main/rounded/256), pinned at commit `bf799e19c810fe406dd8d681e81c58e69d9a4450`. Original files are unchanged; SOURCE.json records every Git blob SHA and size. MIT license and upstream attribution are in LICENSE.txt.

Import the sample and move the copy into a game-owned Assets folder, preserving .meta. All images are configured as single Sprites, transparency enabled, mipmaps disabled, maximum size 256. Only directly referenced flags need to be included by the game's build; the sample is not a Resources folder.

## Settings dropdown icons

1. Import the Settings package's Settings Feature sample and Localization's Settings Integration sample. These optional code samples are separate from this asset-only collection.
2. Create **Dreamy → Settings → Language Icons**, add locale codes and sprites, using LocaleFlags.json as the editable starting mapping.
3. Open **Dreamy → Localization → Settings Integration**, select the game's Settings prefab/table/font and the Language Icons asset, then Apply. The tool wires caption/item images and reserves text padding.
4. Supply the initialized localization gateway at the composition root as documented by Settings Integration. The view renders each native language name plus its configured icon.

No icon mapping is required: a null icon set or missing locale icon leaves text options usable; without a language gateway, controls remain hidden. The icon set belongs to game presentation, not Settings Runtime. Country and language codes are separate: for example en→us and pt-BR→br here are editable display choices, not locale inference.

Use any of the sprites directly elsewhere in a production UI. Retain LICENSE.txt when redistributing them. Reimporting a sample does not merge game-owned icon mappings or assets.
