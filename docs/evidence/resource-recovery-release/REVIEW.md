# Final follow-up: single-resource color-only panels

User steering supersedes the older default multi-deficit UI described below.
Coin/Gem now use separate prefab variants of ResourceSuggestionPanel, itself a BaseFeaturePanel
variant. Message/ResourceName/Balance + Shop/Later are editable TMP/Image controls. No custom
ResourceSuggestionFrame, resource icon, copied font or new art/shader assets remain.

- ResourceSuggestionPresenter is pure orchestration. It normalizes costs, shows only the selected
  resource, refreshes balances, closes before navigation, and passes context/current deficits.
- ResourceSuggestionPanelPresentation instantiates the selected prefab per opening instead of
  using PanelManager's type-only cache; Popup layer/stack/Back still use PanelManager.
- GameInstaller has Coin/Gem game variants bound in BootstrapScene. Recovery demo also routes
  Coin/Gem, preserving the legacy multi-deficit view as optional fallback.
- Final Unity targeted suites: **133/133 passed**, no skips, job c34df93a92d547d8b5acbf9384ca662d.
- Final clean consumer **PASS** at /tmp/dreamy-resource-consumer-6izfjan0: compile + Coin/Gem variant IDs/buttons, color-only Images and no Missing Script. Standard TMP Essential Resources supplied.
- Final managed Economy/Shop harness: **88 passed**, plus presenter/model harness.
- Rendered color-only panel inspected at 1920x1080, 1080x1920 and 800x600; screenshots alongside
  this report. This uses a temporary Canvas with ConstantPixelSize, matching the sandbox's
  serialized CanvasScaler mode; device safe-area insets remain untested.
- Play Mode smoke attempt in isolated fixture stalled entering Play Mode and emitted a Unity
  SearchDatabase ArgumentOutOfRangeException. The task-owned process was terminated; no pass
  is claimed for actual button clicks. Pure presenter decision/close/navigation tests passed.

The historical findings below remain relevant; 133/88 supersede the earlier 128/83 counts.

---

# Resource recovery source release review — 2026-10-16

Kết luận: đủ làm nền `get more resource` dùng lại cho nhiều game dùng tài nguyên số nguyên.
Chưa đạt toàn bộ Definition of Done/production: full-project tests còn lỗi UI nền;
Play Mode/player/device/billing/backend và migration trên save thật cần kiểm chứng tiếp.
Tag đánh dấu source đã review, không phải chứng nhận release ra store.

## Scope và version

| Package | Tag local đã tạo | Thay đổi chính |
| --- | --- | --- |
| Economy | v0.5.0 | Typed multi-resource spend/exchange, shortage/recovery, shared inventory/save v4 |
| Shop | v0.2.0 | Typed shortage result, recovery callback, semantic destination/navigation |
| Progression | v0.2.1 | Multi-reward journal/retry, shared Economy inventory sample |
| Connectivity | v0.1.0 | Initial tagged baseline: connectivity/recovery, optional UI samples, PanelManager integration |

Economy 0.4.0 và Progression 0.2.0 là các mốc triển khai chưa tag; release này chứa cả follow-up.
Connectivity chưa có tag trước đó nên dùng version hiện tại 0.1.0 cho baseline đầu tiên.
Đã tạo commit/tag local; SHA được lưu trong releases.json. Không push remote trong task này. Sandbox pin gitlinks của các commit này.

## Review chức năng và khả năng dùng nhiều game

- Một wallet/profile, detached candidate + receipt cùng lần save; thiếu một cost không trừ một phần.
  Normalize duplicate resource IDs, checked arithmetic, fingerprint gắn payload, replay qua restart,
  collision/ledger corruption/save uncertainty fail closed; listener lỗi không đảo kết quả commit.
- `ResourceActionFlow` giữ result giao dịch gốc, policy None/Prompt và không tự spend lại khi quay về.
  `ResourceAcquisitionCoordinator.OpenForResourceAsync` mở trực tiếp luồng nhận thêm, không cần mua thất bại.
- ResourceId do game định nghĩa; prompt hiển thị nhiều deficit, refresh khi balance đổi; nguồn không có
  destination được đánh dấu unavailable. Router cho game thay panel theo resource và có fallback.
- Gateway/navigation là adapter boundary: game có thể mở tab Shop, scroll/focus, panel khác hoặc NPC.
  Gateway sample chỉ điều hướng; không tự grant, charge hoặc xem Returned là receipt thanh toán.
  Pending/busy, cancel theo owner, repeated purchase/session replacement, Back/close/reopen có regression.
- Shop typed capability là tùy chọn; bool wallet cũ không bị suy diễn ExchangeFailed thành thiếu tiền.
  Progression journal tách XP đã commit khỏi delivery; retry entries có receipt và recovery qua restart.
- Core domain/Runtime assemblies giữ dependency direction; sample UI của Connectivity không ép game
  không dùng UI phải cài Dreamy UI/Feature/UGUI. Giữ GUID khi chuyển source/sample ownership.

Mỗi game vẫn phải cấu hình resource definitions, một persistence owner, destination map/gateway,
UI/art/localization và vòng đời cancellation. Đây là toolkit có điểm tích hợp, chưa phải plug-and-play
cho mọi game. `long` không đáp ứng BigNumber; metadata item instance, server/cloud authority,
IAP fulfillment/reconciliation và policy rollback/migration thuộc game/adapter riêng.

## Sửa khi review

- Shop dependency Economy đổi từ 0.4.0 chưa có tag sang baseline sẽ tag 0.5.0; lock đồng bộ.
- Đồng bộ README dependency Shop/Economy/Progression và thêm release notes phân biệt source tag/gate.
- Bổ sung reference `Dreamy.Connectivity.UI` vào asmdef sandbox sau khi code Online Reward mới
  dùng `ConnectivityRoot.Feature` xuất hiện trong lúc review; compile lại không còn Console errors.
- Thêm `validate-clean-consumer.py`: project tạm mới, package local + cache dependency đã resolve,
  import source sample nguyên bản, compile và kiểm tra prefab. Không copy code của Assets/_Project.
  Fixture cài cả bộ dependency Dreamy của sandbox, không chứng minh dependency tối thiểu từng package.

## Validation mới chạy

| Gate | Kết quả |
| --- | --- |
| Economy/Shop managed harness | PASS 83 NUnit cases + Shop presenter/model harness |
| Progression managed harness | PASS 59; 1 Unity-only case nằm ngoài managed harness |
| Targeted Unity EditMode, Unity 6000.4.12f1 Linux | PASS 128/128, 0 skip; job 3ac312a96d2f4c54955357f20b834ab8 |
| Connectivity dependency/metadata + Android/iOS C# | PASS; giữ prefab host customization đã import |
| Fresh Connectivity consumer không UI | PASS; /tmp/dreamy-connectivity-headless-v9mex0v3 |
| Fresh consumer + source UI samples của 4 package | PASS compile, required shortage bindings, prefab missing-script check; /tmp/dreamy-resource-consumer-xul2ly5o |
| Manifest/lock/asmdef checks | PASS local dependency lock khớp, Runtime không reference Editor |
| Source/doc diff whitespace | PASS C#/asmdef/JSON/Markdown; Unity-generated YAML có empty-field trailing spaces |

Lệnh tái chạy từ root sandbox:

```sh
python3 LocalPackages/com.dreamy.feature.economy/Tests~/validate-resource-recovery.py
python3 LocalPackages/com.dreamy.feature.progression/Tests~/validate-progression.py
python3 LocalPackages/com.dreamy.connectivity/Tests~/validate-connectivity.py --allow-imported-customizations --compile-platforms
python3 LocalPackages/com.dreamy.connectivity/Tests~/validate-headless-consumer.py --unity /path/to/Unity
python3 docs/evidence/resource-recovery-release/validate-clean-consumer.py --unity /path/to/Unity
```

## Gate còn mở và finding

1. Full-project EditMode job `e73810115624474a83e72bdae80efa3c`: completed 277, failed 5,
   đều trong package UI 1.0.5 chưa thay đổi: PanelPresenterTests.DisableAndFactoryTeardown_ReleaseExactlyOnce,
   FailedShow_ReleasesPresenter_AndCanRetry, ManagerShow_CachedReopen_CreatesOncePerOpening,
   NonPanelHost_ReopenDisableDestroy_UsesTheSharedLifecycle, Transition_VisualPreviousPanel_KeepsPresenterUntilClosed.
   Một test chạy riêng vẫn fail (`1b747740161c4c9bbec75fd44d1cbb00`). Fixture không reset counters
   trong SetUp và dựa vào MonoBehaviour callbacks trong EditMode; đây là hướng chẩn đoán,
   chưa kết luận runtime lỗi hay sửa fixture là đủ. Không bỏ qua lỗi này để tuyên bố full-project green.
2. Thử chạy fixture UI Editor trong PlayMode không khởi động test trong timeout
   (`d4c8685bd3e24ddd91a13492605de431`); đã dừng PlayMode và trả Editor về EditMode.
   MainScene startup trong lần thử đó có NullReferenceException ở FoundationTutorialAdapter:87;
   chưa chứng nhận smoke test sandbox với BootstrapScene/luồng end-to-end mới.
3. Chưa chạy player IL2CPP Android/iOS, iOS native link/settings, thiết bị thật, safe area mọi tỷ lệ,
   Addressables content build/load sau thay đổi cuối và SDK IAP/ad/backend thực tế.
4. Migration Economy v4/Progression v2 có fixture/regression, vẫn cần rehearsal save production,
   policy reconcile và rollout/rollback riêng. Multi-entry Progression bundle không atomic xuyên owner.

Fresh-consumer fixture ban đầu thiếu Physics2D module của DOTween; validator đã bổ sung Unity modules.
Assertion ban đầu yêu cầu displayCatalog đã được sửa vì đây là optional field có fallback ResourceId.
Các lỗi validator đó không được tính thành lỗi runtime package và lần chạy cuối đã pass.
