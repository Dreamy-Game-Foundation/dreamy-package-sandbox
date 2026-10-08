# Lucky Wheel Feature

Production-ready integration layout for six weighted rewards and a coroutine-driven spin reveal. Install LuckyWheelFeatureInstaller using the host's persistent save/wallet, clock and random provider before opening the panel. LuckyWheelDemo.unity is a layout fixture that requires host bootstrap; it no longer silently starts a separate in-memory game.

The panel prefab is a variant of Dreamy Feature's BaseFeaturePanel. Keep Dreamy Feature and Dreamy UI installed. Result selection, granting and persistence remain in the service before visual reveal. Interrupted reveal resumes the saved transaction without selecting or granting a new reward. Presenter Show runs after the GameObject is active so pending reveal coroutines can start.

## Production integration and presenter lifecycle

The editable integration entry point is `LuckyWheelFeatureInstaller` in Samples~. Runtime `LuckyWheelInstaller` remains available for custom UI; games using the supplied views call only the feature installer. All presenters implement the engine-independent `IPanelPresenter` lifecycle in `Dreamy.UI.Presentation`.

```csharp
LuckyWheelFeatureInstaller.RegisterConfig(dataConfig); // Before dataConfig.InitializeAsync.
// After config/save/wallet readiness, using the same factory as other features:
LuckyWheelFeatureInstaller.Install(factory, config, save, wallet, clock, random);
// Or reuse a host-owned service: LuckyWheelFeatureInstaller.Install(factory, service);
```

Dependencies in this example belong to the composition root. No installer creates an in-memory wallet/save fallback. Model/service own rewards and checkpoints; views only render state and emit intent. Add direct asmdef references to the integration assembly and Dreamy.UI.Presentation wherever their APIs are used.

After assigning the shared factory to the scene's PanelManager, any caller can open `LuckyWheelPanel` with Show/Transition by address, or Show with a prefab. Each opening creates one presenter; close, disable, destroy or failed show release it. Cached reopen creates a fresh presenter. No per-feature controller is required.

Sandbox validation: `python3 LocalPackages/com.dreamy.feature.settings/Tests~/validate-settings.py --shop --features`. This compiles runtime/integration/sample assemblies against their declared references and runs pure managed model/presenter regressions. Unity scene/coroutine/raycast lifecycle still requires Editor/PlayMode validation.
