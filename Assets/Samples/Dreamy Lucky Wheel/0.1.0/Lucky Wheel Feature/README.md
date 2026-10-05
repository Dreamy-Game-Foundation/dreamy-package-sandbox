# Lucky Wheel Feature

This sample contains six weighted rewards and a three-second ease-out spin animation. The selected result is granted and persisted before the wheel starts rotating.

Open `LuckyWheelDemo.unity` and enter Play Mode to run the standalone sample. Press **SPIN** repeatedly to verify selection, reward reveal, and the post-selection wheel animation.

`Prefabs/LuckyWheelPanel.prefab` is a prefab variant of `Packages/com.dreamy.feature/Runtime/Prefabs/BaseFeaturePanel.prefab`. Keep `com.dreamy.feature` and `com.dreamy.ui` installed.

The controller intentionally uses in-memory save and wallet implementations so the imported sample can run without modifying the host bootstrap. Production integration must use `IDatasaveService`, a persistent idempotent `IResourceWallet`, and a trusted clock/random source when rewards require authority.

## Host-owned service integration

LuckyWheelPanel.Configure(ILuckyWheelService) owns its presenter and binds/unbinds it with panel activation. Await Init/PostInit, Configure with the host service, then Show. Use this route without LuckyWheelSampleController when integrating into an existing bootstrap and shared economy wallet. Disabling during reveal keeps the saved outcome available for replay on reopening; it does not request another grant.
