# Component setup/API — research and integration, 2026-11-02

Status: implemented; 41/41 EditMode and 13/13 PlayMode tests passed. Scope: the six existing string/asset components. No engine replacement or dependency upgrade.

## Source review

Default branches and exact commits were fetched from GitHub on 2026-11-02. A mirror's newest commit is not an assurance that it is newer than every Unity distribution channel.

| Source | Revision reviewed | Strength | Decision |
| --- | --- | --- | --- |
| [Unity Localization UPM mirror](https://github.com/needle-mirror/com.unity.localization/tree/9416385e511f0ddad3bc9b9f27f37e1791eb876f) | 9416385, 2026-09-02; package 1.5.13 | Reference property setters manage subscriptions; atomic table/entry updates; lifecycle-owned event binding | Adopt those patterns against installed 1.5.8 APIs; preserve Unity table/formatter backend |
| [UnityLocalization](https://github.com/tranvietanh0/UnityLocalization/tree/19d6d2928490bbc60b819ca5a3de959251ce1dc0) | 19d6d29, 2026-05-07 | Small TMP binding, runtime arguments and shared validator/CLI workflow | Keep simple component use; add missing Reset/target/fallback APIs. Do not import its fixed en/vi CSV workflow into JSON authoring |
| [Retrofit Localization](https://github.com/alexgofman/unity-retrofit-localization/tree/b1cf8af0fc056ab39549039e51654f6d1dd4bc4a) | b1cf8af, 2026-10-02 | Cached component discovery and explicit fallback reporting make old UI safer | Adopt cached target lookup and preserve original content for fallback. Its separate phrase sweep, pool rewrite and formatter engine do not fit the existing Unity tables architecture |

Official docs reviewed: [SetReference](https://docs.unity3d.com/Packages/com.unity.localization@1.5/api/UnityEngine.Localization.LocalizedReference.SetReference.html), [LocalizeStringEvent](https://docs.unity3d.com/Packages/com.unity.localization@1.5/api/UnityEngine.Localization.Components.LocalizeStringEvent.html), [scripting](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/Scripting.html). The 1.5 docs currently identify 1.5.13; installed sandbox still resolves 1.5.8. The mirror is unaffiliated with Unity; official documentation and installed source are the API authority.

This is an original Dreamy implementation informed by these patterns; no third-party package or source file was copied into the project.

## Async operation explanation

AsyncOperationTask converts Unity AsyncOperationHandle completion/failure into an awaitable Task used by the UniTask service. It waits for initialization, table or asset loading without blocking the frame. It neither creates a background thread nor takes ownership of cached asset handles. A caller's cancellation stops that caller waiting; it does not imply cancelling the shared backend operation. The shared initialization Task also permits multiple consumers to await the same initialization safely.

## Component contract

- Reset is the Unity authoring callback and a callable method: find the matching component on the same GameObject, capture its current text/sprite/clip, keep the key and UnityEvent listeners.
- Awake/OnValidate fill missing targets and capture fallback once. An existing serialized fallback wins; explicit empty/null values are tracked as intentional.
- CaptureFallback is the explicit action that overwrites the snapshot from the current target. Refresh, locale changes and assigning the same target do not recapture translated output.
- SetTarget supports explicit external targets; captureFallback=false preserves an existing fallback when retargeting.
- SetReference supports table/key strings, LocalizationKey, Unity TableReference/TableEntryReference and replacement LocalizedString/LocalizedSprite/LocalizedAudioClip instances. Replacement detaches the old subscription before binding the new one; version checks reject obsolete callbacks.
- Configure assigns key/fallback/arguments before subscription, avoiding a premature Smart String request. ClearReference restores fallback on an active component. ApplyFallback is also public.
- LocalizedStringEvent has no text target to discover. It preserves listeners, exposes ValueChanged and accepts an explicit fallback.
- TMP binding supports either TMP 3D or uGUI and requires an existing TMP_Text component. Runtime examples must add TextMeshProUGUI/TextMeshPro before adding the binding.
- Serialized target, localizedString/localizedSprite/localizedClip and fallback field names stay in their original component owners. Non-serialized asset subscription plumbing is shared without migrating prefab fields into a generic base class.

## Verification

- New EditMode tests: all six Reset/setup paths, unchanged keys/listeners, legacy serialized fallback, explicit empty/null values, same-target calls, external target capture and Unity destroyed-object lookup.
- New PlayMode regressions: live reference replacement, obsolete reference mutation, disable/re-enable, target switching, ClearReference, string-event lifecycle and asset fallback capture.
- Existing Settings/JSON tests run with the new component code.
- Initial fixture failures were retained: abstract TMP_Text cannot be auto-created by RequireComponent, and SendMessage(OnValidate) asserts in EditMode. Fixtures now create the concrete TMP target and exercise the public setup path.
- Full failure/release coverage for real localized asset-table loads remains outside the string pilot. Current components rely on Unity's asset cache/loading events; this change adds no independent asset loader.

See [component evidence](../../evidence/localization/component-api/VALIDATION.md) for fresh receipts and limitations.

## Standalone scene constraint

LocalizationSettingsTest now uses StandaloneSettingsView, SettingsPresenter and SettingsModel directly. Its root contains no PanelManager/factory navigation; the generated standalone prefab contains no UIPanel. The builder and migration command preserve the existing test canvas/content while replacing the manager with a plain overlay. Every PlayMode fixture asserts that no manager is created.
