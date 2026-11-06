# Dreamy Debugger — Material Compact v3

Status: new visual proposal, not applied to the debugger package. Prepared in response to the request to find a different UI and regenerate concept only from demonstrably available resources.

## View the result

- [Actual Unity resource specimen](unity-resource-specimen.png): rendered with Unity 6000.4.12f1/OpenGL in a separate consumer; synthetic data, real imported fonts and sprites.
- [Generated layout concept](generated-concept.png): AI interpretation constrained by the specimen. **Not a Unity render.** The generator mistakenly copied the specimen's “REAL UNITY RENDER” line into the concept. That text is false for this file; only the separately named specimen is a real Unity screenshot. Minor sheen/text/icon differences in the generated image are not shipped resources.

## Different resource direction

This is a small assembled skin, not a third-party turnkey Unity UI kit. It uses Google Material Icons rather than Tabler, Barlow rather than IBM Plex/Geist, and authored 32px rounded surfaces rather than Kenney/default Unity beveled sprites. The shapes have editable SVG sources; their final PNGs, actual fonts and notices are in resources/. No giant background texture or new UI framework is needed.

Sources: [Material Design Icons](https://github.com/google/material-design-icons) (Apache 2.0), [Barlow](https://github.com/jpt/barlow) / [Google Fonts files](https://github.com/google/fonts/tree/main/ofl/barlow) (OFL), [JetBrains Mono](https://github.com/JetBrains/JetBrainsMono) (OFL). Downloaded artifacts are hashed in resource-manifest.json. Local SVG circle/fill/outline shapes are original simple geometry created for this specimen. No source from the rejected candidates below is bundled.

Also inspected [VantUI Modern Dark UI Kit](https://vantui.itch.io/modern-dark-ui-kit) and [Softline Flat Night](https://www.fab.com/listings/1b73b6a0-0c81-4bcc-9db3-45854ea0f0ca). VantUI's downloaded unitypackage lacked a clear included license, so its assets were not selected/bundled. Softline is an alternative listing, not an acquired or verified implementation resource. No purchase/account access was used.

## What is proven

- Separate disposable consumer imported 13 real PNG sprites (all 32×32) and three real font files. The specimen code compiled and the Unity batch render exited 0. Existing debugger package, game scenes, prefabs, GUIDs and manifest in the shared workspace were unchanged.
- Barlow Regular 104,068 bytes, Barlow SemiBold 108,348 bytes, JetBrains Mono Regular 270,224 bytes: **482,640 bytes combined font sources**. The tested Vietnamese/number string had no missing glyphs according to the fonts' cmap; this is not proof of every Unicode character or every device renderer.
- Sprites are imported Read/Write off, mipmaps off, 32px max. The attached Unity receipt reports RGBA32 and 9,144 bytes per texture including Unity overhead in that Editor observation. Pixel payload alone is 4 KiB per 32² RGBA32 sprite; do not confuse file size, payload and total runtime memory.
- Shapes and actual icon files render as usable uGUI sprites. Source harness uses sliced surfaces, proper circular dots and the actual imported font names.

## What is still pending

- This is a **static visual specimen**, not functional debugger integration. Editor Settings is illustrated using uGUI on the specimen board; it is not yet an EditorWindow. Grant, category pin, HUD modes and keypad callbacks are not wired to game/debugger logic here. Do not treat decorative Buttons in this board as actual input acceptance.
- Actual Dreamy wordmark alpha extraction is not demonstrated; the specimen uses available Barlow text “DREAMY” deliberately. Preserve the original logo as the future branding source; do not substitute an AI-drawn mark.
- Runtime reskin, responsive/portrait integration, real-pointer Cancel/Delete, complete asset mapping, compressed target comparison, atlas packing, build/draw-call/GC/device acceptance and native Editor controls remain open. The broad v2 resource gate is **partially demonstrated**, not passed in full.
- No mobile compression claim: the specimen uses uncompressed tiny textures to establish a crisp baseline. Measure target overrides and compare edges before choosing ASTC/ETC/desktop formats. Fonts/atlas residency and build inclusion are not measured by the source font sizes above.

## Reproduce

In an isolated Unity 6000.4 consumer with uGUI and its built-in modules, copy resources/*.png, *.ttf and license texts to Assets/UiCandidateV3. Put UiCandidateSpecimen.cs in Assets/Editor. Run Unity with graphics available: `-batchmode -force-glcore -projectPath <consumer> -executeMethod Dreamy.DesignPreview.UiCandidateSpecimen.Build -logFile <path>`. The harness creates an empty fixture scene and writes /tmp/dreamy-ui-material-preview/unity-resource-specimen.png plus the resource receipt. Run only in a disposable consumer: it replaces the active fixture scene and exits the Editor.

The prior design plan remains the functional specification for pin chips, dock console, titleless keypad, HUD gestures and layout. This proposal replaces only the suggested visual resource direction if the user selects it; it does not authorize publishing or silently implementing the reskin.
