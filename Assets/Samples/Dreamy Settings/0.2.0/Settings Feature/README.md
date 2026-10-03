# Settings Feature

The included `SettingsPanel` and `RateUsPanel` prefabs are ready to use. Their launchers create the rate-us panel from the Settings panel button.

For a custom scene setup, assign music/SFX toggles, GDPR, Restore Purchases, Open Store, Open Rate Us, and Close buttons to `SettingsPanel`. Use `SettingsController` with a scene `RateUsPanel` reference, or use the included launchers for prefab-driven setup.

For production, register the real host platform gateway before installing Settings and creating the panel:

```csharp
ServiceLocator.Register<ISettingsPlatformGateway>(platformGateway);
SettingsInstaller.Install();
```

`SimulatedSettingsPlatformGateway` is only for the sample. A production gateway maps its methods to `DreamySDK.Consent_ShowForm`, `DreamySDK.IAP_RestorePurchases`, and the game store-opening API.
