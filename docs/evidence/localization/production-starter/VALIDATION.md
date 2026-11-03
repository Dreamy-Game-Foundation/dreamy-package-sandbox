# Production starter, optional Settings and metadata — 2026-11-03

Completed local source versions: **Dreamy Localization 0.2.0**, **Dreamy Settings 0.3.0**. Author: Dreamy Game Studio. No tag, commit, push or published release was created.

## Delivered ownership boundary

- Runtime/Runtime.UI/Editor, asmdefs and manifests stay package-owned for reviewed updates. README distinguishes this from technical read-only enforcement.
- Production Starter is imported into Assets and becomes game-owned: editable JSON, full-source Latin/CJK/Thai fonts with licenses, TMP fallbacks, runtime/import profiles, bootstrap prefab and localized label prefab. No demo controller, global UI manager or Settings dependency is required.
- The CJK source is a full ~16 MB Noto Sans CJK SC font, separate from the sandbox's small test subset. It supports future text additions; regional typography, shaping and player font budgets remain game responsibilities.
- Settings Integration is an opt-in sample. The existing host adapter was moved into the imported sample with its script meta GUID preserved; the host now references its sample assembly/namespace. It exposes a reusable gateway and Editor setup tool for the game's Settings prefab/table/font.
- Settings Runtime remains independent of Localization. The original Settings package sample prefab now contains a real selector. Without a language gateway/options, the view hides dropdown, label and error and clears stale options. Presenter lifecycle and independent test-scene ownership remain intact.
- README/CHANGELOG/migration and legacy-plan status were reconciled. Settings sample-specific Economy/Core/Audio/UI/Feature/UniTask requirements are explicit. The legacy localization TestProject~ lock remains marked as unrefreshed.

## Fresh receipts

| Check | Result | Receipt |
| --- | --- | --- |
| Native EditMode | 41/41 passed, 11:22:51–11:22:58Z | editmode.xml |
| Native PlayMode | 15/15 passed, 11:26:24–11:26:35Z | playmode.xml |
| Fresh consumer, no Settings installed | Exit 0; 12×18 JSON import and sample bootstrap/label en→fr→ja; Japanese glyphs present | consumer-smoke.json, consumer-editor.log |
| Final starter inputs | All packaged starter file hashes match the final fresh-consumer inputs | consumer-input-identity.json |
| Serialization | Existing 91 Settings package prefab fileIDs preserved, 55 new objects; imported production/standalone prefab IDs unchanged | serialized-preservation.json |
| Metadata/static | Author/version/changelog, sample paths, local doc links, runtime→Editor boundaries and no Localization component GUID in base Settings prefab pass | source review and source-identity.json |
| Unity package registration | Localization 0.2.0 and Settings 0.3.0 observed through PackageInfo | Editor tool readback |
| Console / whitespace | Sandbox Console zero errors; root and both package diff --check pass | final tool reads |

The enhanced no-gateway regression uses the actual SettingsPanel prefab and presenter; it checks dropdown/label hidden, stale options cleared after availability changes, and no PanelManager created. Existing live switch, reopen, cancellation, persistence and component lifecycle cases remain covered.

The consumer is a new project under /tmp with only Localization and required Unity/UniTask dependencies; no Settings, Dreamy host runtime or host scene is copied. It imports the packaged Production Starter at a different Assets path plus TMP Essential Resources shaders, creates Unity tables/Addressables in the new project's output owner, instantiates supplied bootstrap/label prefabs and switches languages in Play Mode. consumer-manifest.json, consumer-packages-lock.json and consumer-verification.cs describe the fixture. Source fonts/profiles/prefabs were authored through Unity APIs; sample meta GUIDs are portable and distinct from host test assets.

## Observed failures and corrections

- Initial optional Editor sample assembly lacked its direct Unity.Localization reference (CS0012). Added the reference, refreshed all assets/asmdefs and verified compile plus setup-tool invocation succeeded.
- Creating and destroying authoring-only objects marked the otherwise unchanged test scene dirty. Native EditMode TestRunner showed a Save dialog and stopped responding to MCP. Used the computer-use skill to save the scene; the action provider timed out, but the scene became clean, its file remained unchanged and the native 41-case receipt completed. No repeated blind click was used.
- Unity batchmode under the filesystem/IPC sandbox could not write its user database or reach licensing IPC. Retried the dedicated /tmp consumer with approved escalation; both fresh consumer runs exited 0.
- Final consumer log includes licensing handshake/token retry messages and a Unity QuickSearch startup ArgumentOutOfRangeException. No C# compile errors were present; entitlement initialization recovered, JSON import and Play Mode smoke completed, and Unity exited 0. These Editor startup diagnostics are retained in raw logs rather than described as a clean log.
- The first consumer preceded a TMP asset auto-save during sample import. Synchronized the final font asset and ran a second entirely new consumer from the final packaged sample; consumer-input-identity.json matches those final inputs.
- The bridge's DeleteAsset guard required an explicit intentional flag to remove only the task-created temporary authoring folder. That folder is absent from final sources.
- Unity serializer trailing spaces were normalized in edited prefabs without altering GUID/fileID/content semantics. Existing unrelated package edits and pre-existing EditorSettings diff were preserved.

## Limits and final state

Source versions are prepared, not released. Complete package-only test scenes, Unity 6000.0 revalidation, player/mobile builds, devices, landscape, native translation review, regional CJK/complex shaping, forced importer rollback, full asset-table failure/release and backend retry/store edge cases remain unverified. The reusable sample assets may be used/customized in production; these open gates do not constitute production certification.

Editor is stopped on LocalizationSettingsTest, scene clean. Temporary test toolbar/start options were restored; persisted pre-existing EditorSettings enabled=1/options=0 diff remains. No current consumer process remains running. Historical receipts are retained in their earlier evidence folders.
