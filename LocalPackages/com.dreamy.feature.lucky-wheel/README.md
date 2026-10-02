# Dreamy Lucky Wheel

`com.dreamy.feature.lucky-wheel` selects a weighted reward, persists the selected outcome, grants it through the shared economy wallet, then asks the host view to reveal that result. The animation never decides the reward.

## Bootstrap

Register the config before `IDataConfigService.InitializeAsync`:

```csharp
LuckyWheelInstaller.RegisterConfig(dataConfig);
```

After DataConfig, Datasave and a persistent `IResourceWallet` are ready:

```csharp
LuckyWheelInstaller.Install();
```

The wallet must treat a repeated `ResourceGrantRequest.TransactionId` as success without granting twice. A pending result is saved before the wallet is called, so a restart retries the same transaction and reveals the same segment.

## Sample

Import **Lucky Wheel Feature** from Package Manager. `LuckyWheelPanel.prefab` is a prefab variant of `com.dreamy.feature/Runtime/Prefabs/BaseFeaturePanel.prefab`. The sample contains six rewards and a coroutine-driven ease-out spin animation. Its in-memory save and wallet are demo-only; replace them with production services.

Device UTC and local random selection are appropriate for offline casual games, not server-authoritative or regulated rewards.
