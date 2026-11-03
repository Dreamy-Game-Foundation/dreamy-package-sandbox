# Settings Integration

Optional production composition for Dreamy Localization 0.2.0 and Dreamy Settings. Import only when the Settings package exposing ISettingsLanguageGateway is installed. This capability is included in Settings source version 0.3.0. The version is prepared locally; no v0.3.0 tag has been published yet.

## Dependencies and ownership

Runtime assembly Dreamy.Localization.SettingsIntegration references Dreamy.Localization.Runtime, Dreamy.Settings.Runtime and UniTask. The Editor companion references Localization Runtime/UI, Unity Localization and TMP. The Settings Feature sample also has its own Core/Audio/UI/Feature/Economy/UniTask dependencies (including the existing review-wallet adapter); supply those through the game manifest before importing that sample. Neither core package gains a mandatory dependency on the other. When Settings is not installed, do not import this optional sample; Production Starter works alone.

Copy/move the imported sample to a game-owned folder and keep its .meta. The adapter, setup tool and host composition are editable game integration; package upgrades do not merge these scripts automatically.

## Setup

1. Import the Settings package's Settings Feature sample. Use its own SettingsPanel prefab for production navigation; it includes a language selector whose controls start hidden.
2. Import Localization Production Starter, customize its JSON/font/profile and import tables.
3. Open Dreamy → Localization → Settings Integration. Choose the project-owned Settings prefab, String table (Game by default) and production TMP font. Apply to prefab adds localized bindings to recognized Settings labels and preserves existing keys/targets. Custom labels with different authored text need explicit LocalizedTmpText references.
4. Initialize localization before constructing the gateway. Pass the gateway to the Settings installer at the composition root.

```csharp
using Dreamy.Localization;
using Dreamy.Localization.SettingsIntegration;
using Dreamy.Feature.Settings.Integration;

// Fields of the game installer:
private LocalizationService localization;
private LocalizationSettingsLanguageGateway languageGateway;

// Inside the installer's async initialization after audio/UI setup:
localization = new LocalizationService(localizationProfile);
await localization.InitializeAsync(cancellationToken);
languageGateway = new LocalizationSettingsLanguageGateway(localization);
SettingsFeatureInstaller.Install(presenterFactory, audio, OpenRateUs,
    language: languageGateway);

// During root teardown, after closing/disposing Settings presenters:
languageGateway?.Dispose();
localization?.Dispose();
```

Reference Dreamy.Feature.Settings.Integration.Runtime for SettingsFeatureInstaller, Dreamy.Localization.SettingsIntegration for the adapter and Dreamy.Localization.Runtime / UniTask for service/async calls. The host still owns audio, presenterFactory and OpenRateUs. The sample does not introduce a second global locator.

If using SettingsInstaller directly, pass languageGateway: languageGateway. If composing SettingsModel directly, pass the same optional constructor parameter. When no gateway is supplied, the actual Settings view hides dropdown, language label and errors, clears stale options and leaves other settings usable. Language selection is not allowed to initialize a hidden Localization backend implicitly.

The adapter borrows the initialized localization service, listens to locale changes, exposes native language names and forwards cancellation; disposing it never disposes the service. The root disposes both in the order shown. When registering services in the host locator, unregister only instances created by that root.

The Settings package remains responsible for its own view/presenter lifecycle. Production SettingsPanel uses its normal UI navigation; independent feature test scenes use StandaloneSettingsView and direct presenter binding without PanelManager.

## Optional flag icons

Import the asset-only Country Flags sample if desired. Create Dreamy → Settings → Language Icons, map locale codes to Sprites (LocaleFlags.json supplies starter mapping), then select that asset in the Settings Integration window before Apply. The tool adds caption/item image targets and text padding. Without a catalog or a mapped icon, the view keeps text-only options. Settings Runtime contracts remain engine-independent.
