# Dreamy Debugger UI v2 — concept boards

2026-11-06. Status: design proposal complete; runtime implementation pending.

- [Runtime concept](runtime-concept.png): compact Cheats panel, alpha-style wordmark, category chip, small HUD, console dock and titleless keypad.
- [Editor concept](editor-concept.png): compact tabbed settings, Access fields, HUD/overlay preview and typography direction.
- [Detailed phased plan](../../plans/DREAMY_DEBUGGER_UI_REDESIGN_V2_2026-11-06.md).

Images were generated with the built-in imagegen tool and copied here from its generated-images directory. They are AI design proposals, not Unity/game screenshots, functioning prototypes, imported assets or acceptance evidence. The generic gameplay backgrounds are illustrative. No runtime, prefab, importer, GUID, font, config or assembly code was changed in this design step.

Design direction: graphite surfaces, restrained red accents, transparent white Dreamy wordmark, flat controls, IBM Plex Sans/Mono direction and one consistent outline icon family. Actual fonts, glyph coverage, original Dreamy silhouette, raster compression and icon licenses must be verified during implementation; the generated letters/icons are not those production assets.

## Functional corrections to concept shorthand

The plan is the behavior contract. Generated artwork simplifies some controls:

- Runtime resource balance is **read-only**; Amount is a separate bounded input. Grant still needs the existing authorization/confirmation path; the concept button does not bypass it.
- Credentials retain Numeric/Passphrase only. Do not add a way to disable authentication as part of reskinning.
- HUD safe area stays Off by design; the dropdown-like artwork is a status illustration, not a request to add another HUD setting. Panel safe area remains On.
- Hidden tap entry remains enabled/invisible by default; artwork and implementation must keep that behavior.
- Keypad x and backspace represent separate Cancel/Backspace actions with independent hit surfaces. No title/technical label should appear inside the keypad.
- HUD header taps cycle modes; close is separate; graph/body pass-through and drag/click ownership follow the plan. The art does not specify exact touch sizes.

Do not cut these boards into runtime textures. Use authored low-resolution shapes/icons and measured Unity layouts. Texture-memory targets in the plan are budgets/calculations, not measured achieved compression. See plan phases and acceptance gates before implementation/release.
