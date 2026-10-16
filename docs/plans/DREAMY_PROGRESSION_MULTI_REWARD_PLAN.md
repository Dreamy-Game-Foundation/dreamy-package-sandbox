# Dreamy Progression: nhiều reward, retry và recovery

Ngày: 2026-10-16. Trạng thái: đã triển khai working tree Progression 0.2.0; chưa commit/tag/publish.

Kết quả: `docs/evidence/progression-multi-reward/VALIDATION.md`. Runtime/config/save v2,
reward service/presenter, migration contract và sample đã bổ sung. 59 managed tests và
60 test gọi trực tiếp trong Unity Editor pass; PlayMode Test Runner chưa khởi động được.
Các mục bên dưới giữ làm thiết kế và tiêu chí đối chiếu; API thực tế xem README package.

## 1. Baseline và vấn đề đã xác minh

- `com.dreamy.feature.progression` hiện là `0.1.2`; khai báo Economy `0.3.0`, trong khi package Economy local là `0.4.0`. Manifest và lock đều dùng local packages. Khi triển khai phải pin minimum version thực sự chứa reward protocol và resolve lại lock.
- `ProgressionLevelEntry` chỉ có `rewardResourceId`/`rewardAmount`; level đầu không được có reward. Có hai mode Sequential và Experience.
- `ProgressionModel.AdvanceNext`/`GrantExperience` gọi wallet trước khi commit XP, level và grant ID. Vượt nhiều level có thể đã cấp vài reward rồi thất bại ở level sau, trong khi progression chưa lưu.
- Save Progression version 1 chỉ có catalog, level index, XP và processed grant IDs. Model mutate đối tượng save trực tiếp; grant history bị giới hạn mặc định 4096.
- Transaction ID cũ: `progression:{catalogId}:level:{level.Id}`. ID này không chứa save key/profile scope.
- Economy source đã có `ResolvedRewardBundle`, `RewardIdentity`, `RewardDeliveryGateway`, `IRewardDestination`, `RewardDeliveryCoordinator`, `RewardJournal`; wallet Datasave triển khai destination `currency`.
- Journal có state `IntentPersisted → Resolved → Delivering → DeliveredAwaitingClaimCommit → Completed`, acknowledgement từng entry. Coordinator giao tuần tự, dừng ở entry đầu lỗi; acknowledgement hợp lệ là một prefix liên tục.
- Datasave có `Version`, `Migrate(int fromVersion)`, save envelope và atomic write từng document. Không có transaction atomic xuyên Progression, wallet và inventory.
- Daily Reward có ví dụ tích hợp coordinator, clone candidate, persist journal và recovery; tái sử dụng pattern, không tạo dependency Progression → Daily Reward.
- Không tìm thấy `toolkit.json` hoặc compatibility catalog trong repository, cũng không có skill `omg-*` trong hai skill root đang cài. Những nhận xét API trên dựa vào source local đã đọc, chưa phải xác nhận tương thích từ toolkit.
- Workspace hiện có nhiều thay đổi của người dùng ở Economy, Shop, Connectivity và scene. Implementation phải giữ nguyên các thay đổi đó và ghi baseline trước khi sửa.

## 2. Quyết định kiến trúc

```text
Game composition root
  ├─ catalog + Datasave → ProgressionModel
  └─ destinations → Economy RewardDeliveryGateway → ProgressionModel

Progression: eligibility, XP/level, milestone records, journal persistence, UI state
Economy: bundle, identity, coordinator, routing, delivery result
Game destinations: currency/inventory/chest/cosmetic mutation + durable receipts
```

1. Commit XP/level độc lập với kết quả cấp reward. Mỗi progression commit lưu cùng document: XP mới, level mới, processed grant ID và toàn bộ reward intents cho các milestone vừa vượt.
2. Chỉ gọi destination sau khi progression commit thành công. Reward lỗi không rollback XP, không đổi kết quả advance đã thành công thành thất bại.
3. Một milestone có một record và một Economy journal riêng. Một grant vượt ba level sinh đủ ba record trong cùng commit; milestone không có reward không sinh bundle rỗng.
4. Dùng nguyên `ResolvedRewardBundle`, `RewardDeliveryCoordinator`, `RewardJournal`. Không viết coordinator, receipt format hay state machine delivery thứ hai trong Progression.
5. Mỗi destination lưu effect và receipt có fingerprint trong cùng owner document. Receipt trùng cùng payload trả `AlreadyDelivered`; ID trùng payload khác trả `PayloadMismatch`.
6. Progression không tham chiếu class inventory, card, chest, cosmetic của game. Không chuyển các nghiệp vụ đó vào Core/Economy. Runtime tiếp tục `noEngineReferences=true`, không tham chiếu Editor.
7. Một model writer cho một progression save key. Chặn re-entry ở model; Busy không được mutate state. Không hỗ trợ hai model cùng ghi một document.

### Mapping reward

| Reward | kindId đề xuất | targetId ví dụ | Owner |
| --- | --- | --- | --- |
| Coin | `currency` | `currency.coin` | Wallet của game hoặc DatasaveResourceWallet |
| Gem | `currency` | `currency.gem` | Cùng currency destination |
| Card | `card` | `card.knight` | Card inventory của game |
| Chest | `chest` | `chest.silver` | Chest inventory của game |
| Cosmetic | `cosmetic` | `skin.hero.red` | Cosmetic collection của game |

Kind/target là stable strings, không hard-code enum năm loại trong package. Coin/Gem không đăng ký hai destination cùng `currency`. Chest reward là cấp chest item; mở chest và roll nội dung thuộc game. Cosmetic đã sở hữu cần policy rõ trong destination: no-op có receipt hoặc conversion do game quyết định, tránh retry vô hạn.

## 3. Config mới và tương thích config cũ

Thêm `bundle` optional vào `ProgressionLevelEntry`, gồm `schemaVersion`, `resolverVersion`, `entries`; entry có `entryId`, `kindId`, `targetId`, `amount`.

```json
{
  "id": "rank-3",
  "level": 3,
  "requiredExperience": 300,
  "bundle": {
    "schemaVersion": 1,
    "resolverVersion": "progression-static-v1",
    "entries": [
      { "entryId": "coins", "kindId": "currency", "targetId": "currency.coin", "amount": 100 },
      { "entryId": "gems", "kindId": "currency", "targetId": "currency.gem", "amount": 5 },
      { "entryId": "card", "kindId": "card", "targetId": "card.knight", "amount": 2 },
      { "entryId": "chest", "kindId": "chest", "targetId": "chest.silver", "amount": 1 },
      { "entryId": "skin", "kindId": "cosmetic", "targetId": "skin.hero.red", "amount": 1 }
    ]
  }
}
```

- Giữ đọc `rewardResourceId`/`rewardAmount`. Chuẩn hóa thành bundle một entry có ID ổn định, kind `currency`, resolver version `progression-legacy-v1`.
- Phân biệt field không có với field có giá trị 0/null khi validate. Khai báo cả bundle và legacy fields là lỗi; không cộng hai nguồn ngầm.
- Không reward là hợp lệ; bundle được khai báo nhưng rỗng/null entries là lỗi. Level đầu tiếp tục không có reward.
- Kiểm tra stable level IDs, duplicate entry IDs trong cùng bundle, amount dương, kind/target không rỗng, schema được hỗ trợ, threshold tăng nghiêm ngặt và overflow.
- Cùng target có thể xuất hiện ở nhiều entry khác ID; destination validate tổng amount theo contract Economy.
- Giữ các accessor legacy khi dùng config cũ; với bundle nhiều entry, accessor `Reward` phải báo dùng sai rõ ràng. Thêm `HasReward`, `IsBundle`, `GetBundle()` có semantics được tài liệu hóa.
- Giai đoạn này dùng reward concrete, không thêm random resolver hoặc network API. Nếu sau này cần authority resolution phải persist intent trước authority call và yêu cầu replay cùng request ID; không dùng random lại khi retry.

## 4. Save version 2 và identity

Giữ các field v1, bổ sung:

- `CurrentLevelId` và snapshot identity/thứ tự/threshold của catalog đang áp dụng để kiểm tra config drift, tránh remap index âm thầm.
- Collection `MilestoneRewards`; mỗi record chứa stable milestone ID, level tại thời điểm unlock, catalog/occurrence identity, payload snapshot đã chuẩn hóa, `RewardJournal`, lý do lỗi gần nhất nếu cần hiển thị sau restart.
- Metadata migration: version/policy đã áp dụng, legacy reached-level baseline và các milestone đang chờ đối soát legacy.
- Clone sâu toàn bộ save, records, payload và journals. Không giữ alias mutable tới config, model state hoặc đối tượng Datasave đã load.

Identity đề xuất:

```text
requestId = RewardIdentity.Request(
  stableScope, "progression", catalogId, occurrenceId, milestoneId)
entryTransactionId = RewardIdentity.EntryTransaction(requestId, entryId)
requestFingerprint = RewardIdentity.Fingerprint(
  "progression-milestone", requestId, frozenBundle.Fingerprint)
```

- `stableScope` mặc định là save key; game có nhiều profile phải dùng profile-scoped key ổn định.
- `occurrenceId` là progression run/season cố định, mặc định `permanent`; chỉ đổi khi game chủ động bắt đầu progression mới. Không đổi theo grant ID, timestamp, app restart hoặc chỉnh config.
- Payload fingerprint bảo vệ reuse identity sai nội dung; không phải cơ chế chống sửa save.
- Record Completed/tombstone phải tồn tại đủ lâu để không unlock lại milestone. Không prune journal/receipt tùy tiện; retention cần policy theo save lineage.
- Giới hạn 4096 processed grant IDs hiện tại chỉ chống replay XP trong cửa sổ history, không bảo đảm toàn lịch sử sau restart. Để đáp ứng chống trùng mặc định, đề xuất lưu đầy đủ XP grant receipts ở chế độ strict. Giữ bounded retention chỉ dưới option công khai kèm giới hạn guarantee; thêm test replay sau ngưỡng cũ. Backend grant sequence/watermark là tối ưu riêng nếu game có authority.

## 5. Luồng commit và delivery

### A. AdvanceNext / GrantExperience

1. Check mode, grant ID, duplicate, amount, checked arithmetic và catalog compatibility.
2. Tạo detached candidate từ state đã commit; tính target level và toàn bộ milestones vừa vượt.
3. Chuẩn hóa/freeze payload cho từng milestone; tạo journal `IntentPersisted` bằng `RewardDeliveryCoordinator.Begin`. Callback ở bước chuẩn bị chỉ gắn journal vào candidate, chưa có delivery.
4. Lưu candidate một lần, gồm XP/level + grant receipt + tất cả intents. Có thể dựng candidate qua helper riêng; không persist từng milestone rời trước progression commit.
5. Khi Save thành công mới thay state in-memory, publish ProgressChanged và trả advance thành công. Tiếp đó có thể thử delivery như convenience, nhưng kết quả delivery là kết quả riêng.
6. Nếu Save lỗi, không gọi destination và không phát event thành công. Thêm status `PersistenceFailed`/`RecoveryRequired` phù hợp; giữ nguyên giá trị enum cũ khi thêm status.
7. Save có thể đã ghi rồi mới ném exception: chặn mutation tiếp theo đến khi reload/reconcile bằng grant ID và fingerprint. Không đoán failed-save đồng nghĩa chưa ghi. Test cả lỗi trước và sau durable write.

### B. Deliver một milestone

1. Restore coordinator từ journal đã lưu, kiểm tra record và fingerprint.
2. `IntentPersisted`: Resolve bằng payload snapshot, persist journal resolved. Không đọc lại reward từ catalog mới.
3. `Deliver`: coordinator validate rồi giao tuần tự; mỗi entry thành công lưu acknowledgement ngay qua detached Progression candidate.
4. Destination đã lưu nhưng ack save lỗi: retry lại đúng entry transaction ID; destination trả AlreadyDelivered, rồi Progression lưu ack.
5. Tất cả entries đã ack: `Complete` lưu Completed cùng trạng thái hoàn tất milestone trong một document. Bước này không tăng XP/level lần nữa.
6. Complete save lỗi: giữ `DeliveredAwaitingClaimCommit`; retry chỉ hoàn tất journal, không giao lại entries đã ack.

### C. Retry và startup recovery

- `RetryReward(milestoneId)` và `RetryPendingRewards()` hoạt động không cần grant XP mới. Service đọc state không tự tạo side effect.
- Host gọi recovery sau khi Datasave và destinations đã sẵn sàng; sample tự gọi tại startup sau wiring. Recovery không chạy trong constructor.
- Duyệt milestone theo thứ tự unlock. Trong một bundle giữ đúng thứ tự coordinator; lỗi một bundle không ngăn retry các milestone độc lập khác, trừ lỗi persistence khiến state không còn chắc chắn thì dừng toàn batch.
- Pending retryable dùng retry thủ công/startup; không loop mỗi frame. UnknownKind/UnknownTarget/PayloadMismatch/InvalidJournal là blocked với reason, không tự retry vô hạn hoặc đánh dấu Delivered.
- Reward blocked vẫn giữ nguyên entitlement và không chặn grant XP tiếp theo nếu progression persistence còn hợp lệ.
- Load lại catalog khác ID/mode, reorder/remove level hoặc thay threshold không tương thích phải yêu cầu migration rõ ràng; bỏ hành vi tự reset save của model hiện tại. Pending payload cũ vẫn được recovery độc lập nếu document hợp lệ.
- Reward mới từ config chỉ áp dụng cho milestone chưa unlock. Không sửa snapshot pending, không tự cấp bù reward vừa thêm cho level đã qua.

## 6. API và UI

Tên API dưới đây là đề xuất triển khai, chưa tồn tại trong source baseline.

- Giữ `IProgressionService`, `GetState`, `AdvanceNext`, `GrantExperience`, `ProgressChanged`.
- Thêm interface nhỏ `IProgressionRewardService`: query milestone reward states, event `RewardsChanged`, `RetryReward`, `RetryPendingRewards`. Model triển khai cả hai; tránh bắt tất cả implementation IProgressionService bên ngoài sửa ngay.
- View state immutable: milestone ID/level, entries, pending count, acknowledged count, `IsBusy`, `CanRetry`, reason và retryability; phân biệt chưa unlock, pending, blocked, delivery đã ack nhưng chưa complete, completed.
- UI đơn giản có thể hiển thị Pending/Delivered; Delivered công khai chỉ khi Completed đã lưu. Khi all ack nhưng chưa complete, hiển thị Pending hoàn tất để không báo thành công sớm.
- Kết quả advance chỉ nói XP/level commit. Reward query/result riêng cho biết delivery; duplicate XP grant không ngầm thử cấp lại reward.
- Giữ presenter/view cũ dùng HUD XP. Reward presenter/view bổ sung riêng, unsubscribe đúng lifecycle. Sau reload query state đủ để render, không phụ thuộc event trong phiên trước.
- Event UI chỉ phát sau durable commit. Lỗi listener không được đổi kết quả commit thành failed hoặc kích hoạt replay; áp dụng pattern notification isolation của Economy.
- Thêm installer/constructor nhận gateway và scope/options rõ ràng. Composition root đăng ký destinations; leaf không resolve global services.
- Constructor/installer IResourceWallet cũ giữ để source compatibility. Strict recovery chỉ tự dùng được nếu wallet thực sự triển khai IRewardDestination; bool TryGrant thuần không có durable receipt thì báo cần adapter/migration, không gắn nhãn strict. Tài liệu nêu rõ chuyển đổi hành vi khi reward lỗi: XP vẫn commit.

## 7. Migration v1 → v2

1. `ProgressionSaveData.Version = 2`; `Migrate(1)` chỉ chuyển cấu trúc, giữ XP, level index, processed grant IDs; không truy cập game services trong SaveData.Migrate.
2. Model initialization đối chiếu catalog và tạo baseline stable level IDs. Level đã đạt trong save v1 được coi là đã xử lý theo hành vi v1, không tự tạo bundle mới cho lịch sử. Giữ metadata legacy thay vì dựng journal Completed giả thiếu payload.
3. Điểm đặc biệt: v1 có thể đã cấp reward của các level phía trước nhưng save level chưa tiến. Trước khi phát identity mới cho các reward đó, cần đối soát ID cũ `progression:{catalogId}:level:{levelId}` với owner.
4. Đề xuất contract migration riêng do host cung cấp: trả rõ AlreadyDelivered, NotDelivered hoặc Unverifiable. AlreadyDelivered chỉ xác nhận đúng legacy entitlement hoặc policy migration mà host chấp nhận; NotDelivered cần bằng chứng history đầy đủ. Receipt không có fingerprint không chứng minh được payload tương ứng.
5. Với Unverifiable giữ blocked `MigrationRequired`/`LegacyReceiptUnverifiable`; không tự coi chưa cấp, không cấp bằng ID mới. Game có thể dùng explicit trust policy cho tombstone hoặc công cụ đối soát; quyết định phải persist cùng metadata và có fixture.
6. Contract strict hiện tại của Economy không có API công khai đối soát ID legacy tùy ý. Chỉ bổ sung query/import tối thiểu nếu destination reference cần; không dùng reflection/đọc field private. Không dùng adapter đổi ID rồi gọi bool wallet như bằng chứng exactly-once.
7. Save v2 đã tạo record dùng identity mới ổn định và không lặp migration. Phân biệt fresh save với migrated save để fresh install không bị gate legacy.
8. Migration persistence lỗi: giữ bản nguồn, không phát reward. Future-version, corrupt journal, catalog mismatch không được reset/overwrite thành progress mới. Test bằng file/envelope thực tế vì Datasave có backup fallback.
9. Rollback qua backup độc lập giữa các owner có thể tạo khác generation; guarantee giới hạn trong cùng save lineage. Không tuyên bố journal tự giải quyết restore tùy ý nhiều file.

## 8. Sample và tài liệu tích hợp

Thêm `Samples~/Progression Rewards`, giữ Basic Progression làm kiểm tra tương thích:

- Catalog Experience vượt ít nhất ba level trong một grant; bundle minh họa đủ Coin/Gem/Card/Chest/Cosmetic và một level không reward.
- Game/sample-owned destinations: currency dùng Economy; card/chest/cosmetic dùng demo inventory persistent, effect + receipt cùng save candidate. Runtime package không tham chiếu demo assembly.
- Panel hiển thị XP/level và trạng thái từng milestone/entry; nút Grant XP, Retry milestone, Retry pending, Reload saved services.
- Fault injection một lần tại Card, sau destination save trước journal ack và tại final completion commit; có lựa chọn restart/reload để quan sát recovery.
- Sau reload giữ cùng save key, scope và occurrence. Hiển thị số lượng và receipt để chứng minh phần đã nhận không tăng lại.
- README mô tả wiring destinations, receipt contract, migration checklist, config cũ/mới, delivery failure reasons và sự khác nhau giữa advance thành công với reward completed.
- Cập nhật package README, CHANGELOG và samples manifest; giữ .meta GUID hiện có, tạo .meta cho file mới, không sửa bản sample đã import của người dùng tự động.

## 9. Test và tiêu chí nghiệm thu

| Nhóm | Kịch bản bắt buộc | Kết quả cần chứng minh |
| --- | --- | --- |
| Config | Legacy, bundle 5 loại, no reward, mixed fields, empty bundle, duplicate IDs, amount/schema sai | Chuẩn hóa đúng; reject config sai trước mutation |
| Multi-level | Một XP grant vượt 3+ level; exact threshold; max level; Sequential | XP/level + mọi intent cùng commit, không sót milestone |
| Tách commit | Destination lỗi ngay hoặc giữa bundle | XP/level và grant ID vẫn đã lưu; reward pending |
| Duplicate | Cùng grant ID trước/sau restart, sau history limit cũ, retry cùng milestone nhiều lần | XP không cộng lại theo policy strict; reward không trùng |
| Partial bundle | Coin/Gem thành công, Card lỗi, Chest/Cosmetic chưa chạy | Ack prefix đúng; retry bắt đầu từ entry chưa ack |
| Cross-milestone | Milestone A blocked, B có thể giao | B tiếp tục; A vẫn recoverable |
| Ack crash | Effect + receipt lưu, journal ack chưa lưu | Retry nhận AlreadyDelivered, amount không tăng |
| Resolve crash | Intent save, resolve save lỗi hoặc restart | Payload giữ nguyên, chưa delivery khi unresolved |
| Complete crash | All ack, final commit lỗi | Pending hoàn tất; retry không cấp lại |
| Persistence | XP commit lỗi trước write và sau durable write | Không delivery sớm, reconcile trước mutation tiếp |
| Restart | Restart tại mỗi journal state | Recovery đúng, không XP mới, không đổi transaction ID |
| Config drift | Thay reward/reorder/remove level/catalog/scope | Snapshot pending giữ nguyên; mismatch có policy rõ |
| Destination | Unknown kind/target, cumulative overflow, fingerprint mismatch, cosmetic đã sở hữu | Reason đúng; không silently discard |
| Migration | V1 level đã đạt, partial legacy grants, tombstone không kiểm chứng, migrate hai lần | Không cấp lại lịch sử, không double grant qua đổi ID |
| Serialization | JSON roundtrip và Datasave envelope v1/v2 trên disk, backup/future version | Không alias, không reset dữ liệu hoặc overwrite sai |
| Lifecycle/UI | Retry re-entry, listener throw, bind/unbind, reload | Busy an toàn, event sau commit, UI khôi phục đúng |

Thay test cũ kỳ vọng reward lỗi làm XP không đổi bằng kỳ vọng XP đã commit + pending reward. Giữ regression mode, threshold, overflow, max level và config validation hiện có.

Validation theo tầng:

1. Pure domain/fault-injection tests dùng fake save có serialize/deserialize, không trả cùng reference như fake cũ. Mô phỏng fail tại từng commit và ghi xong mới throw.
2. Targeted Unity Progression suite theo test assembly hiện có; thêm integration với Economy coordinator và DatasaveResourceWallet thực. Không chỉ mock gateway cho toàn bộ suite.
3. Datasave disk fixture trong thư mục tạm cho migration, save/load, backup và future-version; cleanup fixture deterministic.
4. Economy RewardContractTests/RewardJournalTests regression nếu thay API migration/reference owner; Daily/Missions targeted regression khi có thay protocol dùng chung.
5. Import sample, compile, kiểm tra Console và smoke PlayMode: vượt nhiều level → fail giữa bundle → restart → retry → balances/ownership đúng một lần.
6. Kiểm tra Runtime không tham chiếu Editor/game assemblies, manifest/asmdef/.meta và diff. Ghi rõ gate nào chưa chạy, không dùng test result lịch sử làm bằng chứng mới.

## 10. Thứ tự triển khai và deliverables

| Bước | File/module chính | Hoàn tất khi |
| --- | --- | --- |
| P0 — Baseline/contracts | Progression/Economy manifests, public contracts, test fixtures | Chốt version protocol, identity, retention và legacy migration policy; ghi dirty baseline |
| P1 — Config | Config/ProgressionCatalogConfig.cs, config tests | Legacy + bundle cùng chạy; validation đầy đủ |
| P2 — Persistence | Persistence/ProgressionSaveData.cs, milestone record, migration tests | V2 roundtrip/clone/migrate và pending payload ổn định |
| P3 — Advance | Domain/ProgressionModel.cs, ProgressionAdvanceResult.cs | Một commit XP/level/grant ID/intents, reward lỗi không cản tiến độ |
| P4 — Delivery | Model + helper delivery riêng, reward service contracts | Coordinator recovery đủ states, ack/final commit retry đúng |
| P5 — UI/wiring | Reward view states/presenter, Installation/ProgressionInstaller.cs | Query/retry hoạt động, UI cũ tiếp tục compile |
| P6 — Migration integration | Legacy migration adapter/policy; Economy query tối thiểu nếu cần | Đối soát partial legacy không phát thưởng trùng |
| P7 — Sample/docs/release | Samples~/Progression Rewards, README, CHANGELOG, package.json | Demo persistent + fault injection chạy; test evidence và release notes đủ |

Thực hiện tuần tự P0 → P1/P2 → P3 → P4 → P5/P6 → P7, với test đi cùng mỗi bước. Commit tách theo trách nhiệm và repo package; cập nhật gitlink sandbox sau khi package changes đã review được. Chưa commit, tag, publish hoặc triển khai runtime trong công việc lập kế hoạch này.

Đề xuất Progression lên `0.2.0` vì thay đổi semantics reward failure và save schema; version Economy tối thiểu xác định theo commit/release thực chứa protocol, không dựa riêng số version local đang dirty. Nếu thêm API migration cho Economy, release package đó trước rồi mới pin Progression.

Ngoài phạm vi: backend authority, random loot resolver, chest opening, tự chuyển cosmetic trùng thành currency, reset season tự động, transaction atomic toàn bundle và đồng bộ đa thiết bị. Các destination/game có thể triển khai riêng theo cùng receipt contract.
