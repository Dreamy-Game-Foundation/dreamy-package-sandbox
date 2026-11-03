"""Generate the scene's CJK test font; requires fonttools==4.68.0.

Usage: python subset_test_font.py /path/to/NotoSansCJK-Regular.ttc
Re-run after adding CJK strings. The font intentionally covers this fixture only.
"""
import json
import sys
from pathlib import Path

from fontTools import subset
from fontTools.ttLib import TTFont

root = Path(__file__).resolve().parents[2]
source = root / "Assets/_Project/Localization/Source"
text = "".join("".join(json.loads(p.read_text()).values()) for p in source.glob("*.json"))
text += "日本語한국어中文（简体）0123456789"
font = TTFont(sys.argv[1], fontNumber=2)  # Simplified Chinese face, includes kana/Hangul.
options = subset.Options()
options.name_IDs = [0, 1, 2, 3, 4, 5, 6, 13, 14]
subsetter = subset.Subsetter(options=options)
subsetter.populate(text=text)
subsetter.subset(font)
# Give this modified subset a distinct family name; retain copyright/license records.
for record in font["name"].names:
    if record.nameID in (1, 3, 4, 6):
        value = "DreamyLocalizationCJKTest" if record.nameID == 6 else "Dreamy Localization CJK Test"
        record.string = value.encode(record.getEncoding())
font.save(root / "Assets/_Project/Localization/Fonts/LocalizationCJKTest.otf")
