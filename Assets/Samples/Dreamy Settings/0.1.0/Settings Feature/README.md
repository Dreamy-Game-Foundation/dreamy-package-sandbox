# Settings Feature

Copy this folder into the game and create a `SettingsPanel` prefab variant from `BaseFeaturePanel`.

Assign TMP status text, music/SFX sliders, GDPR, Restore Purchases, Open Store, and Close buttons to `SettingsPanel`. Add `SettingsController` to the same panel root.

Register the host platform gateway before installing Settings:

```csharp
ServiceLocator.Register<ISettingsPlatformGateway>(platformGateway);
SettingsInstaller.Install();
```

`SimulatedSettingsPlatformGateway` is only for the sample. A production gateway maps its methods to `DreamySDK.Consent_ShowForm`, `DreamySDK.IAP_RestorePurchases`, and the game store-opening API.
