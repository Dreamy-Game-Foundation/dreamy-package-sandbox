# Localization test fonts

`LocalizationFont.asset` keeps its existing Liberation Sans source and GUID. It falls back to `LocalizationCJK.asset` and `LocalizationThai.asset` for the 12-language fixture.

- `LocalizationCJKTest.otf`: renamed subset of Noto Sans CJK Regular, simplified Chinese face (index 2) from the locally installed Noto Sans CJK collection. Covers this fixture's source strings and CJK language names; it is not a full production CJK font. See `NotoCJK-LICENSE.txt` (SIL OFL 1.1).
- `NotoSansThai-Regular.ttf`: unchanged Noto Sans Thai Regular from the locally installed Noto core fonts. See `NotoThai-LICENSE.txt` (SIL OFL 1.1).

To regenerate after adding CJK text, install FontTools 4.68.0 in a temporary environment and run:

```sh
python tools/localization/subset_test_font.py /path/to/NotoSansCJK-Regular.ttc
```

Then run **Dreamy → Localization → Refresh Languages And Dropdown**. This discovers all JSON files, imports tables, validates glyph coverage, and updates the production and standalone Settings prefabs without rebuilding the scene.

Translations are test copy. Native-language review, locale-specific CJK font design, Thai shaping review and player/device validation remain production gates.
