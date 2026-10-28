# Architecture and lifetime

`Runtime` is engine-independent C#: Config → detached save snapshot → synchronous model → view state/presenter. It references Core (optional root installer), Feature, DataConfig, Datasave, Economy and Newtonsoft. `Integration/RewardPresentation` adds UniTask and Economy UI contracts without making the domain depend on UI.

One model/writer per profile and save key. One shared Economy owner per wallet key. Mutations are synchronous on the owner thread and reject reentrancy. Destination balance events may occur while a journal is in flight; they are not claim completion. Model StateChanged runs after the mutation releases its guard. Observer exceptions are reported via Diagnostic without changing a committed result.

Panel owns its view subscriptions and cached row objects. UIPanel uses `PanelPresenterFactory` → `BattlePassPanelPresenter` → `BattlePassPresenter : FeaturePresenter`; Dispose detaches feature/view subscriptions. Root owns the reward queue and disposes it on unload. The queue's UI cancellation never cancels domain delivery after Claim.

Economy owns coin/gem/item/entitlement balances and receipts. Battle Pass owns XP, event receipts, season snapshots, journals and batch cursor. Premium access reads the shared wallet; Shop owns the virtual purchase. Gameplay owns run IDs and completion outbox. No dependencies from Economy back into Battle Pass, Shop or Missions; no `_Project` references.

Sample sources are under `Samples~`. Concrete UI, demo seed, fake time advancement and Shop configuration remain sample code. No additional generic reward framework is introduced.

Sample panel/popup assets inherit BaseFeaturePanel; sample item/row/reward/entry assets inherit BaseFeatureItem as connected Prefab Variants. The Feature package owns the base GUIDs and UI shell, while sample variants own concrete view components/layout. Builder and consumer gates validate source links, one inherited SafeArea per panel and serialized bindings.
