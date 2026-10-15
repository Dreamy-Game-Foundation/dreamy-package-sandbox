# Reward evolution: kiến trúc và kế hoạch tiếp tục

Ngày: 2026-10-15. Trạng thái: quyết định kỹ thuật cho kế hoạch tiếp tục;
chưa phải chứng nhận implementation, release hoặc consumer cutover.

Tài liệu này bổ sung `docs/PACKAGE_EVOLUTION_NEXT_STEPS_2026-10-14.md`.
Thứ tự hiện tại theo yêu cầu người dùng: giải thích, đánh giá độ phức tạp,
chốt kiến trúc và lập plan trước khi tiếp tục thay đổi runtime.

## 1. Baseline đã quan sát

- Sandbox: `feature/package-evolution-20261014`, SHA
  `06693e03e9c7b2a9a9805f51c931ea5b71cbf549`.
- Feature, Economy, Daily, Missions, Progression, Shop đang ở
  `feature/reward-evolution-20261014`, khớp gitlink sandbox.
- Unity `6000.4.12f1`; MCP xác nhận đúng đường dẫn sandbox, Editor idle,
  không PlayMode hoặc test job đang chạy tại lúc kiểm tra.
- Đã đọc manifest và packages-lock: Dreamy dùng local package dependencies;
  mỗi declaration Dreamy có lock entry và local package.json tương ứng.
  Điều này không chứng minh Package Manager vừa resolve lại checkpoint.
- Economy.Runtime tham chiếu Datasave.Runtime; Daily/Missions tham chiếu
  Economy, DataConfig, Datasave, Core và UI.Presentation. Ba asmdef này giữ
  `noEngineReferences=true`. Kiểm tra tĩnh named references trong package Runtime
  không thấy chiều tới package Editor; chưa phải audit đầy đủ GUID/plugin deps.
- Console read trả 0 error entries; chưa refresh/compile hay chạy suite mới.
  Các con số test ngày 14/10 trong bàn giao vẫn là bằng chứng lịch sử.
- Không có toolkit.json/compatibility catalog trong sandbox. Catalog global
  đã đọc là bản cũ, không có record các feature package này; đánh giá dưới đây
  dựa trên source checkpoint, không tuyên bố API mới đã được toolkit chứng nhận.
- Không tìm thấy omg-* skill đang được cài trong hai skill root hoạt động;
  không sử dụng bản nằm trong Trash làm hướng dẫn hiện hành.
- Repo `/run/media/tutrinh/NewData/planet-bunker-tu-code` quan sát được ở `main`,
  SHA `edfb80e303d6838dc075cc07b15bcbde441ec685`, không có ref `*20261014*`
  trong refs local hiện có. Chưa fetch hoặc sửa repo đó.
- Template consumer trong bàn giao là repo lồng
  `dreamy-base-project/DreamyTemplateProject`; chưa xác định được bản tương ứng
  trên máy này. Repo sibling `dreamy-template-project` không được coi là thay thế.
- Hai stash sandbox/feedback được giữ nguyên. Chưa sửa source package,
  scene, prefab, manifest, lock, save của người chơi hoặc nhánh consumer.

## 2. RewardDelivery và journal giải quyết việc gì?

Ví dụ Daily trao 100 gold + 1 card. Gold và card thuộc hai save owner khác nhau.
Nếu crash sau khi gold đã lưu nhưng trước khi Daily lưu claimed, gọi lại Claim
trực tiếp có thể cộng gold lần hai. Đánh dấu claimed trước khi cấp thì có thể mất
reward. Một cờ bool trong Daily không xử lý được cả hai phía.

RewardDelivery là protocol chuyển một claim thành các lần giao reward cụ thể,
có identity ổn định và khả năng tiếp tục sau lỗi. Journal là bản ghi tiến độ
của chính claim đó. Receipt là bằng chứng một owner đã nhận một entry.

| Thành phần hiện có | Trách nhiệm |
| --- | --- |
| `ResolvedRewardEntry/Bundle` | Payload cụ thể đã cố định: entryId, kindId, targetId, amount; bundle bất biến có fingerprint. |
| `RewardIdentity` | Encode identity không nhập nhằng; tạo request ID, entry transaction ID và fingerprint của payload. Fingerprint không phải cơ chế chống sửa save. |
| `IDailyRewardBundleResolver` | Chuyển ý định reward thành kết quả cụ thể, ví dụ card do authority chọn. Gọi sau intent save; cùng request ID phải replay cùng kết quả nếu resolved save lỗi. |
| `RewardJournal` | Request, payload resolved, acknowledgement từng entry và state. Embed trong save feature. |
| `RewardDeliveryCoordinator` | Điều phối Begin → Resolve → Deliver → Complete qua callback persistence của feature. Không tự sở hữu save file. |
| `RewardDeliveryGateway` | Validate bundle và route entry theo kind tới destination đã đăng ký. |
| `IRewardDestination` | Validate và giao cho owner thực sự. Mutation và receipt cùng owner document. |
| `RewardDeliveryResult` | Success/pending/failure, reason và retryability; feature chỉ báo Claimed khi final commit thành công. |
| `PendingClaim` | Giữ ngày/kỳ/revision và transition gốc để recovery không phụ thuộc config/thời gian mới. |
| `DatasaveResourceWallet` | Reference currency owner: detached candidate, checked arithmetic, balance + receipt cùng save. Host có wallet riêng dùng adapter tới owner đó. |
| Missions period/window/selection/event data | Giữ Daily/Weekly/Permanent, lựa chọn và reroll; event identity chống tăng progress lặp, distinct value đếm đơn vị khác nhau. Đây là nghiệp vụ Missions, không phải delivery receipt. |

Luồng durable dự kiến:

```text
Kiểm tra eligibility
  → lưu Intent + original claim context
  → resolve nếu cần, rồi lưu concrete payload
  → giao từng entry: owner lưu mutation + receipt
  → feature lưu acknowledgement từng entry
  → feature lưu claimed + Completed journal cùng document
  → UI/feedback nhận kết quả Claimed
```

State hiện có: `IntentPersisted → Resolved → Delivering →
DeliveredAwaitingClaimCommit → Completed`; bundle một entry có thể bỏ qua
state trung gian Delivering.

Nếu gold save thành công nhưng journal acknowledgement lỗi, retry gửi lại
cùng transaction ID; gold owner dùng receipt trả AlreadyDelivered, không cộng lại.
Nếu final claim commit lỗi, retry hoàn tất claim dựa trên journal.
Sau Resolved, journal là authority payload; không roll card lại hoặc đọc reward mới
từ config để thay payload. Nếu card owner tiếp tục unavailable thì claim còn Pending.

Guarantee là giao có thể recovery và không lặp hiệu ứng khi các owner tuân thủ
contract persistence/receipt và cùng save lineage. Không có atomicity toàn bundle:
gold có thể đã nhận trong khi card còn pending. Validate trước không khóa tài nguyên
của tất cả owner. Không có bảo đảm tự động cho backend, power loss mọi filesystem,
hoặc restore từng save riêng lẻ từ các generation khác nhau.

## 3. ADR: giữ protocol nhỏ, ownership rõ

**Quyết định:** giữ contract, bundle, identity, coordinator và journal schema trong
Economy.Runtime hiện tại. Daily/Missions sở hữu eligibility, pending context,
recovery và claimed transition. Host composition root inject gateway, clock,
resolver và destinations; leaf không tự tìm global service.

```text
UI/View → Presenter → Daily/Missions service → Economy delivery protocol
                                            ↓ injected gateway
                          currency destination | card destination
                                ↓                      ↓
                          currency owner          card owner
                          mutation+receipt        mutation+receipt

Daily/Missions save = feature progress + pending context + embedded journal
```

- Package không tham chiếu `_Project`, game enums, card database hoặc SDK.
  Host adapter là nơi biết ResourceSave/UnitProgressSave cụ thể.
- Datasave chịu serialize/write/load; không quyết định eligibility hoặc quyền cấp.
  Snapshot config trong save là dữ liệu lịch sử của claim/window, không chuyển
  quyền chỉnh config từ DataConfig sang save.
- Một writer cho mỗi owner document. Busy guard hiện là per instance; không bảo vệ
  hai model/wallet instance cùng ghi một save key. Installer phải dùng một service
  và một owner instance cho mỗi scope; UI mở lại không dựng owner mới song song.
- UI chỉ gửi intent, render pending/reason và phát feedback sau final success.
  Đóng panel không huỷ claim đã persist. Recovery thuộc service, không thuộc view.
- Giữ synchronous/main-thread contract hiện tại. Nếu authority thật cần network
  async, bổ sung integration theo contract đã xác minh, giữ identity và durable
  state; không chặn main thread để ép network vào resolver sync.
- Giữ legacy wallet API để tương thích; không quảng bá bool TryGrant thành strict
  lifetime delivery. Host mới muốn recovery phải chọn explicit gateway path.
- Giữ virtual Shop purchase trong một TryExchange atomic tại cùng wallet owner.
  Durable IAP là scope có điều kiện, cần receipt thật và Shop-owned pending state.
- Progression chỉ thay khi có gap được chứng minh; PB giữ PlayerProgressSave là
  authority XP/level, không tạo XP save song song.

Các phương án không chọn:

| Phương án | Lý do |
| --- | --- |
| Chỉ `Grant()` rồi save claimed | Có cửa sổ crash gây cấp lặp hoặc mất reward. |
| Một reward.save/journal.save chung mới | Thêm owner/write boundary và reconciliation; không giúp mutation của game owner atomic hơn. |
| Chuyển protocol lên Core hoặc tách package mới ngay | Chưa có lợi ích đủ bù dependency, assembly và migration churn. |
| Generic workflow/event-sourcing framework | Scope hiện chỉ cần giao reward và resume; thêm framework làm khó đọc và vận hành. |
| Rollback gold nếu card lỗi | Tạo giao dịch bù mới, có thể lỗi tiếp; người chơi có thể đã tiêu gold. Giữ pending và resume. |

## 4. Độ phức tạp và giới hạn cần giữ

Journal + receipt + stable identity là phần cần thiết với mixed-owner rewards
và recovery. Tăng độ phức tạp so với demo single currency là có lý do. Các điểm
dưới đây cần thu gọn hoặc làm rõ, không cần rewrite toàn bộ hệ thống:

1. `Begin/Resolve/Persist` trả `Delivered/AlreadyDelivered` để báo operation thành
   công dù chưa giao reward. Trước mắt ghi rõ contract và test final-success
   boundary; chưa đổi enum/public API khi consumer chưa được kiểm kê.
2. Daily giữ payload ở cả PendingClaim và journal sau Resolve. Missions giữ Bundle
   cùng journal và cả WindowSnapshot lớn cho mỗi pending/completed claim.
   Chốt journal là authority sau Resolve; kiểm tra mismatch/corrupt input và
   payload gốc trước Resolve. Không xoá field serialized mù quáng.
3. Completed Missions claims vẫn được giữ trong PendingClaims và clone qua JSON;
   snapshot/window/receipt growth cần đo riêng. Chỉ compact khi chứng minh không
   mất replay protection và có migration fixture; không prune owner receipts.
4. Không đồng nhất bounded event dedup với lifetime reward receipt. Permanent
   event history hiện bounded; window/namespace của producer vẫn phải đúng.
5. Legacy compatibility làm model có hai nhánh. Giữ nó ở boundary hiện có trong
   đợt này; chỉ retire bằng migration/version policy cụ thể.
6. Save implementation hiện ghi temp/backup rồi chuyển file; cần filesystem
   restart tests, không suy từ in-memory store tests ra mọi power-loss guarantee.

## 5. Plan tiếp tục, theo dependency

Các checkbox sau đều chưa hoàn thành ở thời điểm tạo tài liệu. Discovery/Console
read ban đầu không tự đóng bất kỳ gate runtime nào.

### P0 — correctness trên đúng source, trước cleanup

- [x] Capture source SHAs, manifest/lock hashes và Console baseline; xác nhận
  scene dirty/prefab stage trước Editor mutation. Không lưu scene Feedback cũ
  chỉ vì nó đang mở sau branch switch. Resolve/compile đúng checkpoint.
- [x] Discover và chạy targeted Economy, Daily, Missions, Progression, Shop;
  Economy/Missions theo EditMode đã quan sát, Daily/Progression theo PlayMode;
  xác minh discovery thực tế cho Shop. 0 tests là gate không đạt, không phải pass.
- [x] Chạy Daily resolver regressions mới ở đầu DailyRewardModelTests:
  intent trước authority; resolved save trước grant; failed resolved save giữ
  request ID; restart/config change không resolve lại payload đã lưu; missing
  authority giữ pending, không client-random fallback.
- [ ] Chạy crash matrix owner-save, journal acknowledgement, final claimed save;
  recreate toàn bộ owner/model/store từ persisted serialization. Bổ sung các
  tình huống chưa covered: invalid/mismatched journal và notification exceptions
  sau owner commit, kiểm tra effect/receipt/claim boundary.
- [ ] Kiểm tra failure/restart bằng Datasave filesystem trong thư mục temp riêng,
  gồm save failure/backup restore, không dùng save thật của người chơi.

Exit: compile không có lỗi; tất cả targeted tests discovered >0, không fail;
ghi discovered/passed/failed/skipped + source SHA. Failure còn lại phải được sửa
trước khi đổi schema hoặc wire consumer.

### P1 — làm rõ contract và giảm complexity có kiểm soát

- [ ] Document operation success so với delivered/claimed; audit caller chỉ
  phát success sau Complete. Giữ public API cho tới khi có nhu cầu đổi rõ ràng.
- [ ] Chốt invariant PendingClaim/Journal: trước Resolve cần original intent,
  sau Resolve đọc journal; có validation rõ cho snapshot không hợp lệ/mismatch.
- [ ] Lập fixture save v1/v2 và WIP checkpoint trước mọi thay đổi serialized data.
  Chỉ giảm payload/snapshot dư khi có round-trip + failure/restart coverage.
  Nếu chưa giảm an toàn thì giữ schema và ghi rõ debt, không ép refactor.
- [ ] Audit installer single-writer, model lifetime và presenter close/rebind;
  không thêm shared resolver abstraction chỉ để phục vụ một consumer.

### P2 — samples và Template rehearsal

- [ ] Import từng sample vào consumer sạch với đúng local source/pins; kiểm tra
  missing script/reference/GUID, pending/retry, close/reopen và label kết quả mới.
- [ ] Xác định đúng Template consumer checkpoint, không sửa sibling main thay thế.
  Rebind Home/panel/row/Addressables theo runbook bằng Unity, giữ GUID/art keys.
- [ ] Run existing suites + sáu test consumer được bàn giao; Daily StopAfterLastDay
  và reset=true, Permanent Missions, shared wallet, Home/Back/Shop/Settings.
- [ ] 100-cycle panel lifecycle trên panel thật: listener/object/tween/handles,
  visual và safe area. Unit fake view không thay thế gate này.

### P3 — Planet Bunker sau khi có đúng checkpoint

- [ ] Xác định/checkout đúng repo và branch chứa `8cb3e5f...`; kiểm kê dirty state,
  package pins và scope writable trước mutation. Không áp dụng thay đổi lên main
  chỉ vì nó là checkout hiện tìm thấy.
- [ ] Preserve ResourceSave legacy WalletTransactionIds/WalletBalances và
  UnitProgressSave; mutation + receipt cùng owner candidate. Unknown legacy
  fingerprint phải reject/reconcile, không đoán và không regrant.
- [ ] Verify Daily/Quest migration idempotent trên bản copy, rollback/midnight,
  original-window recovery, revision/selection/reroll/distinct/claimed preservation.
- [ ] Hoàn tất battle bridge bị dở; test producer-to-service đủ 10 objective,
  player/enemy filtering, delayed callback identity và original window.
- [ ] Chỉ sau test mới đổi installer sang strict constructor và thay subscription
  legacy cùng lúc, bảo đảm một writer cho completion/win/grant.
- [ ] Home/runtime cutover, card authority pending/error/retry, copy rehearsal và
  rollback cả bộ saves cùng generation. SDK/device proof ghi riêng.

### P4 — đo hiệu năng sau correctness

- [ ] Rerun baseline trên đúng Linux/Editor hiện tại; Windows timings ngày 14/10
  là tham khảo lịch sử, không dùng làm đối chứng trực tiếp.
- [ ] Thử đổi thứ tự predicate MissionModel.Report để so EventKey trước
  SelectedIds.Contains. Dùng cùng fixture/seed/hash, so operation categories,
  correctness totals/save counts, không thêm index/cache khi chưa cần.
- [ ] Tách JSON clone/commit và ledger/window snapshot growth khỏi matching cost.
  Allocation provider phải qua calibration; invalid counter không ghi zero.
- [ ] Chỉ compact lịch sử khi chứng minh replay rejection vẫn còn; không đổi
  lifetime receipt guarantee để có benchmark đẹp.

### P5 — release sau consumer gates

- [ ] Audit tasks 25–30/F1–F4 theo evidence hiện tại; không kế thừa checkbox cũ.
- [ ] Exact pins, migration docs, metadata/version sau runtime gates; commit package
  trước gitlink sandbox nếu được yêu cầu lưu checkpoint tiếp theo.
- [ ] Packed Android Addressables, player/device smoke và production SDK receipt,
  restore/redelivery/process-death là gate riêng, không suy từ simulated billing.
- [ ] Không merge main, publish hoặc sửa tag đã phát hành theo plan này.

## 6. Evidence và trạng thái kết thúc bước thiết kế

Mỗi run tiếp theo ghi vào `.omo/evidence/dreamy-package-evolution-2026-10-15/`:
source SHA từng repo, manifest/lock hash, Editor/platform, discovery và outcomes,
failure log, fixture identity, các gate chưa chạy. Không cộng test counts từ
nhiều lần run thành một final-suite result.

Đã hoàn thành ở bước này: đọc handoff/plan gốc, review source ownership và recovery,
định hướng kiến trúc, xác định complexity debt và viết plan có thứ tự/gate.
Validation hiện có chỉ gồm manifest/local-path checks, limited named-asmdef check,
MCP project/readiness/Console read và whitespace check của tài liệu.
Chưa chạy lại Unity tests/build, chưa sửa runtime và chưa hoàn thành migration.

## 7. Cập nhật execution sau khi người dùng cho tiếp tục

Mục 1 và mục 6 phía trên mô tả thời điểm thiết kế, không phải trạng thái execution
cuối. Đã tìm đúng MCP và compile/import checkpoint. Evidence mới nằm tại
`.omo/evidence/dreamy-package-evolution-2026-10-15/README.md` và raw JSON cùng thư mục.

- Baseline EditMode: Economy 41, Missions 52, Shop 8 đều pass, không fail/skip.
- Baseline PlayMode: Daily 38, Progression 21 đều pass.
- Bốn regression cases mới tái hiện lỗi trước sửa. Final PlayMode: Daily 42 và
  Progression 21 đều pass, không fail/skip; source dirty hashes được ghi riêng.
- Đã sửa Daily dùng journal sau Resolve; kiểm fingerprint intent trước authority
  call; invalid intent trả result rõ ràng thay vì exception hoặc cấp payload mới.
- Đã sửa completed StopAfterLastDay state/rows và label sample. Đồng bộ panel và
  hai README source/imported; không đổi schema save hay GUID prefab/script.
- Smoke prefab thật: Pending → retry, coin=100/card=1; 100 vòng presenter-host
  bind/dispose + panel enable/disable không tăng listener/row. Chưa phải bằng chứng
  PanelManager transition/Home/Addressables/native-handle lifetime hoặc visual QA.
- Console cuối 0 error entries; scene và Bootstrap preference đã khôi phục.
- P1 đã làm phần authoritative payload và trạng thái UI; chưa compact snapshots
  hoặc đổi API result. Các gate chưa đánh dấu phía trên vẫn còn việc.
- PB/Template integration, benchmark và release/device gates chưa hoàn tất.

## 8. Feature tối giản theo quyết định tiếp theo của người dùng

Quyết định này thay thế hướng sửa metadata Feature → DataConfig của checkpoint
14/10: rà toàn bộ C# trong Feature cho thấy không có consumer của DataConfig,
nên bỏ cả dependency package và asmdef reference thừa thay vì duy trì nó.
Unity Package Manager resolve lại packages-lock; không nâng version dependency.

Giữ nguyên FeatureId/FeatureState/FeatureMetadata/IFeaturePresenter/IFeatureView
và các prefab/meta cũ. Bổ sung FeaturePresenter C# thuần cho lifecycle lặp thực tế:
Show bind một lần rồi Refresh; Dispose unbind một lần; cho phép reopen; cleanup
nếu Bind hoặc Refresh trong Show lỗi. Unbind phải chịu được partial bind và không
throw. Đây là synchronous subscription lifecycle, không quản lý async work/save.

DailyRewardPresenter và MissionPresenter dùng base mới; các view kế thừa
IFeatureView<TState>. Runtime, test và sample integration asmdefs khai báo Feature
reference trực tiếp; source/imported samples đồng bộ reference. Không thêm registry,
DI, config/save, clock, reward ownership hoặc UI dependency vào Feature.

Verification của source mới: Feature 4 + Missions 54 EditMode pass (58 total),
Daily 42 PlayMode pass; fail=0, skip=0. Reflection assembly Feature chỉ có
reference `netstandard`. Bootstrap preference được khôi phục sau PlayMode.
Evidence: `feature-simplification-editmode.json`, `feature-simplification-playmode.json`
và `feature-simplification.md` trong evidence ngày 15/10. Chưa publish hoặc đổi tag.
