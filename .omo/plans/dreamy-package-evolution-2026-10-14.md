# Dreamy package evolution - kế hoạch thực hiện

Ngày: 2026-10-14. Trạng thái: **Planned, chưa triển khai**.

Ràng buộc bổ sung khi thực thi: giữ code đơn giản, dễ đọc, clean và SOLID thực dụng. Chỉ tạo abstraction cho ranh giới thay thế thực tế; không thêm factory/manager/interface cho từng class, generic workflow engine hoặc extension point chưa có consumer. Recovery cần journal nhưng phải dùng state transition trực tiếp, không biến thành framework.

## TL;DR

Đã pull 17 repository `LocalPackages/com.dreamy.*` từ `origin/main`: tất cả trả exit code 0, `Already up to date`; HEAD bằng remote, ahead/behind `0/0`, worktree từng package sạch. Bằng chứng commit, tag, version, manifest và asmdef nằm trong `.omo/evidence/dreamy-package-evolution-2026-10-14/sync-baseline.json`.

Thực hiện theo thứ tự **A -> B -> C1 -> C2 -> C3 -> C4 -> D -> E -> F**, qua 30 đầu việc và 4 gate cuối. Economy sở hữu reward contract, journal schema và coordinator; Daily/Missions lưu pending journal cùng document chứa eligibility/claim state; game giữ ResourceSave, UnitProgressSave, SDK và battle bridge. Không đưa logic Planet Bunker vào package.

Ba điểm phải xử lý trước khi cutover: wallet reference implementation hiện sửa state trước khi save và chỉ giữ 4096 transaction ID; Daily sửa cached save trước khi persist; Missions đã có detached commit/reentrancy tests nhưng chưa có period/selection/reroll/distinct. Không rewrite phần Missions đã đúng.

Phạm vi công việc hiện tại chỉ là đồng bộ source và viết tài liệu này. Chưa thay package API/save, consumer pin, prefab, SDK; chưa commit, push, tạo tag hay phát hành. Các kiểu/tên đường dẫn mới bên dưới là **thiết kế đề xuất**, không phải API hiện có. Quy mô: XL, nhiều checkpoint.

## 1. Phạm vi và nguồn sự thật

- Source root: `D:/Game/dreamy-package-sandbox/LocalPackages/`; ký hiệu `P/<package>/...` bên dưới trỏ tới root này.
- Consumer `PB`: `D:/Game/Planet Bunker`.
- Consumer `T`: `D:/Game/dreamy-base-project/DreamyTemplateProject`, không phải Unity project ở thư mục cha.
- Đề xuất đầu vào: `PB/docs/plans/dreamy-package-evolution-plan.md`; đọc cùng `dreamy-migration-plan.md` và `dreamy-migration-progress.md` trong thư mục đó.
- Quy tắc: `docs/rules/DREAMY_PACKAGE_DESIGN_RULES.md`; chỉ dẫn từng repo và manifest/lock thắng README.
- Evidence thực hiện tương lai: `.omo/evidence/dreamy-package-evolution-2026-10-14/<checkpoint>/`. Mỗi run ghi source SHA, consumer pin/lock hash, Unity/platform, command, exit code và artifact. Evidence lịch sử không chứng minh run hiện tại.
- Không sửa `Library/PackageCache`, không đổi tag đã phát hành, không broad-upgrade dependency, không tự thêm Lucky Wheel/Tutorial, không sửa battle mechanics, pipeline hay SDK billing/backend.
- Core, Assets, Audio, UI, Settings, DataConfig, DataSave, Feedback, Editor Tools chỉ được mở scope khi có blocker cụ thể. Feature có một dependency gap đã chứng minh, nêu tại A1; không vì vậy chuyển reward contract sang Feature.
- Tất cả task thực thi vẫn để `[ ]`. Chỉ đánh dấu sau source commit được cho phép và validation tương ứng; tài liệu này không chứng nhận production.

### Kết quả đồng bộ source

Đây là latest `origin/main` quan sát ngày 2026-10-14, không có nghĩa mọi HEAD đều là release tag. Full SHA trong JSON evidence.

| Package | package.json | HEAD = origin/main | Tag tại HEAD |
|---|---:|---|---|
| com.dreamy.assets | 0.1.1 | 68b5a7457ed7 | v0.1.1 |
| com.dreamy.audio | 0.1.0 | 69128a81faeb | v0.1.0 |
| com.dreamy.core | 1.2.0 | ba844659fbc6 | v1.2.0 |
| com.dreamy.dataconfig | 0.2.1 | d261ae8bfd63 | v0.2.1 |
| com.dreamy.datasave | 0.2.0 | a88e22b0da20 | v0.2.0 |
| com.dreamy.editor-tools | 0.2.2 | a230ad458aee | v0.2.2 |
| com.dreamy.feature | 0.1.0 | d129106fb06c | v0.1.0 |
| com.dreamy.feature.economy | 0.2.1 | 97ff875f7373 | v0.2.1 |
| com.dreamy.feature.daily-reward | 0.1.0 | 4632d46cdb11 | v0.1.0 |
| com.dreamy.feature.lucky-wheel | 0.1.0 | 5f97a5e8064f | v0.1.0 |
| com.dreamy.feature.missions | 0.1.0 | def4e23198d5 | v0.1.0 |
| com.dreamy.feature.progression | 0.1.1 | 3f591a8998de | Không có |
| com.dreamy.feature.settings | 0.2.2 | 81e0f8cc36a8 | v0.2.2 |
| com.dreamy.feature.shop | 0.1.2 | 3fee66cfd839 | v0.1.2 |
| com.dreamy.feature.tutorial | 0.1.0 | 0c20c61f6917 | v0.1.0 |
| com.dreamy.feedback | 0.4.0 | 2d22a64f83d4 | v0.4.0 |
| com.dreamy.ui | 0.2.0 | 91bfb340955d | v0.2.0 |

Progression `v0.1.1` thực sự là `d56ef9bd145b413f23c5eed43c8d2e0b6571bf37`. HEAD mới hơn vẫn khai báo version 0.1.1; không gọi presenter/sample ở HEAD là API của tag. `git diff v0.1.1..HEAD -- Runtime/Domain/ProgressionModel.cs` rỗng, nhưng 21 file khác đã đổi, gồm presenter, sample, asmdef và package.json.

### Baseline hai consumer

Cả PB và T dùng Unity **6000.4.12f1**, cùng 12 Dreamy pin: Assets 0.1.1, Audio 0.1.0, Core 1.2.0, DataConfig 0.2.1, DataSave 0.2.0, Editor Tools 0.2.2, Feature 0.1.0, Economy 0.2.1, Settings 0.2.2, Shop 0.1.2, Feedback 0.4.0, UI 0.2.0. Lock SHA khớp bảng trên. **Daily, Missions và Progression chưa cài trong cả hai consumer**; generated csproj hoặc sample cũ không chứng minh đã cài.

PB HEAD `20e1264e12ad971d9bc02ca33fc5458df55c8b33`; T HEAD `f6486d0e8fa7e877360d7b775c96ad419474fe0f`. Compatibility PB ghi `observed`, không phải release-certified.

Giữ nguyên trạng thái có sẵn: sandbox có gitlink Settings khác commit repo cha; PB sửa `Assets/Rowlan/Tools/QuickNav Data/Preferences.asset`; T có `Assets/generation-report.json` và `.meta` chưa tracked. Không stash/reset/ghi đè chúng. Việc pull lần này không làm HEAD package nào thay đổi.

## 2. Findings từ source

| Bằng chứng đã đọc | Kết luận dùng cho kế hoạch |
|---|---|
| `P/com.dreamy.feature.economy/Runtime/DatasaveResourceWallet.cs:9,59-99,134-164` | Grant/exchange đổi balance/transaction cache trước save; cộng chưa checked; history bị cắt theo số lượng 4096. Cần sửa reference wallet trước khi dùng làm nền recovery. |
| `P/com.dreamy.feature.daily-reward/Runtime/Domain/DailyRewardModel.cs:39-66,133-173` | Grant trước claim-save; ApplyClaim sửa đối tượng đã Load; đổi schedule ID reset ngay; claim ID theo schedule/cycle/entry. |
| `P/com.dreamy.feature.daily-reward/Runtime/Config/DailyRewardScheduleConfig.cs` | Mỗi entry chỉ có một resourceId/amount; default reset-after-missed-day=true. PB phải cấu hình false, không đổi default toàn package. |
| `P/com.dreamy.feature.missions/Runtime/Domain/MissionModel.cs:20-43,49-109` | Có busy, detached clone/Commit, ID claim length-prefixed, event dedup bounded 4096; catalog đổi thì reset. Thiếu periodic/selection/distinct, không thiếu toàn bộ persistence discipline. |
| `P/com.dreamy.feature.missions/Tests/Runtime/MissionModelTests.cs:100-145` | Đã có save-failure-after-grant, progress-save-failure, reentry, bounded eviction và intentional catalog-reset tests; giữ và mở rộng, không xóa để rewrite. |
| `P/com.dreamy.datasave/Runtime/IDatasaveService.cs`; `DatasaveService.cs:215-241,334-366` | Save một key dùng temp/backup và chỉ cập nhật loadedData sau write; không có transaction nhiều key. Detached candidate là việc của owner; backup rollback độc lập vẫn có giới hạn recovery. |
| `PB/.../Progression/PlayerResourceService.Economy.cs:26-57`; `ResourceSave.cs` | Facade đã lưu mutation + ID cùng ResourceSave bằng candidate; không tạo economy.save thay thế. Chưa lưu fingerprint payload, cần bổ sung cho gateway nghiêm ngặt. |
| `PB/.../Progression/UnitUpgradeSystem.cs:49-87` | UnitProgressSave sở hữu cards/level/unlock, chưa có ledger delivery. Grant card phải cùng save owner này. |
| `PB/.../DailyReward/DailyRewardSave.cs`; `DailyRewardService.cs` | Legacy có next day/last claimed day/date UTC nhưng không có cycle count lịch sử. Không thể suy ra tổng số cycle bằng phỏng đoán. |
| `PB/.../Quest/QuestService.cs:74-101,158-245,322-327` | 3/6 daily, 5/10 weekly; UTC+7/Monday; một free reroll; ưu tiên conflict groups rồi relax. Policy này phải explicit trong host config, không gọi là strict. |
| `PB/.../Quest/QuestSave.cs` | Giữ WindowKey, SelectedQuestIds, Entries.Progress/Claimed/UniqueValues, RerollUsed khi migrate. |
| `PB/.../Bootstrap/GameInstaller.Features.cs:66-82`; battle call sites khảo sát | Có mapping nhiều objective; production emitter ExposeUnit/ExposeByTerrain chưa chứng minh. Quest hiện không nhận business event ID. Không báo đủ 10 objective hoạt động trước integration gate. |
| `P/com.dreamy.feature.shop/Runtime/Domain/ShopModel.cs` | Virtual purchase là một TryExchange; IAP grant theo store transaction + offer + entry index. Không tách debit/reward thành nhiều save để dùng bundle. |
| `T/.../Bootstrap/GameInstaller.Features.cs` | Template Shop vẫn simulated IAP; đây chỉ là fixture, không phải bằng chứng receipt/restore/process-death của SDK thật. |

`...` trong bảng chỉ là viết tắt diễn giải. Các task dưới đây dùng root/path đầy đủ hoặc prefix đã định nghĩa.

## 3. Quyết định thiết kế cho implementation

Các quyết định này là hướng thực hiện đề xuất, chưa tạo API hoặc thay đổi luật game.

### A. Ownership và dependency

```text
Economy.Runtime -> Datasave.Runtime
DailyReward.Runtime -> Economy.Runtime + DataConfig + Datasave + Core + UI.Presentation
Missions.Runtime -> Economy.Runtime + DataConfig + Datasave + Core + UI.Presentation
Shop.Runtime -> Economy.Runtime + DataConfig + Core + UniTask + UI.Presentation
Feature.Runtime -> DataConfig.Runtime (GUID đã resolve; manifest đang thiếu)
Host composition -> feature services + reward coordinator + concrete owner adapters
Host currency adapter -> ResourceSave
Host card adapter -> UnitProgressSave
```

Đây là graph rút gọn; graph manifest và từng asmdef đầy đủ trong JSON evidence. Không thêm chiều Economy -> Daily/Missions/Shop/Feature hay package -> `_Project`.

Contract, typed bundle, result, journal schema/coordinator và currency reference delivery nằm trong **Economy.Runtime**, không nằm Runtime.UI. Cùng asmdef hiện có để tránh assembly/GUID churn; giữ `noEngineReferences=true`. Không gọi toàn Economy package là dependency-free vì manifest vẫn có Core/UI. Chỉ adapter host biết item/unit hoặc SDK.

Journal của một claim được embed vào save của feature sở hữu claim: DailyRewardSaveData hoặc MissionSaveData. Coordinator không mở thêm journal.save độc lập; feature persist journal candidate và final claimed transition trong cùng document. Đây là reuse protocol/schema, không chuyển save ownership cho Economy.

Feature v0.1.0 có `Runtime/Dreamy.Feature.Runtime.asmdef` tham chiếu GUID `e34fd9426baf41dab67b6e4d8a9b4282`, chính là `com.dreamy.dataconfig/Runtime/Dreamy.DataConfig.Runtime.asmdef.meta`, nhưng package.json khai báo dependencies rỗng. A1 chỉ sửa metadata dependency tối thiểu này nếu giữ asmdef hiện tại; không chuyển ownership reward vì lỗi metadata đó.

### B. Reward schema, identity và tương thích

- Một entry bất biến có `entryId`, `kindId`, `targetId`, `amount` kiểu long dương; bundle có schema version và ordered entries. Validate nonempty IDs, unique entryId, known kind/owner, positive amount, target hợp lệ, checked overflow trước mutation. Không `object`, arbitrary JSON payload, game enum hoặc assembly-qualified type làm contract.
- `kindId` là stable string host đã đăng ký, ban đầu currency và item fixture/card adapter. `targetId` là resource/item stable ID; package không suy luận card ngẫu nhiên.
- Resolved bundle chỉ chứa entry cụ thể, resolver version và dữ liệu đủ để retry cùng kết quả. Mystery-card request chưa resolve không được đưa sang delivery như item ngẫu nhiên.
- Identity gồm profile/save-owner scope, feature kind, stable schedule/catalog identity, cycle hoặc window, entry/mission ID. Encode length-prefixed/versioned, không nối delimiter nhập nhằng. Revision ghi trong snapshot, **không đổi identity để phát lại reward**.
- Sub-transaction ID được derive từ request ID + stable entry ID; không dùng index cho bundle mới. Với Shop đã phát hành, giữ alias sang ID entry-index cũ, không tạo namespace mới khiến receipt cũ được cấp lại.
- Persist canonical request fingerprint và resolved fingerprint cùng payload/version. Cùng ID/cùng payload trả AlreadyDelivered hoặc resume; cùng ID/payload khác trả non-retryable PayloadMismatch, không mutation. Canonicalize fields/ordering có quy định, không phụ thuộc JSON property order hoặc GetHashCode.
- Giữ constructor, wallet API và single-resource config hiện có bằng adapter/overload. Legacy wallet bool chỉ chứng minh mức bảo đảm legacy; không tự coi mọi IResourceWallet là production-idempotent.
- Gateway result có Delivered, AlreadyDelivered, Pending, Failed kèm reason code và retryable flag. Configuration/unknown target/mismatch/overflow là lỗi xác định; I/O hoặc backend chưa biết outcome giữ pending/recovery, không tự tạo request ID khác.
- Giữ domain synchronous trên Unity main thread như hiện tại; backend có thể trả Pending và host tiến hành callback/recovery bằng cùng request ID. Chưa thêm UniTask vào Economy chỉ để dự phòng. Nếu một backend thực sự đòi đổi async contract, phải ADR riêng trước khi mở dependency.

### C. State machine và bảo đảm

```text
Validate eligibility/bundle
  -> IntentPersisted (request identity + unresolved fingerprint + claim context)
  -> Resolved (fixed bundle/version persisted)
  -> Delivering (per-entry status, stable sub-transaction ID)
  -> DeliveredAwaitingClaimCommit
  -> feature persists claimed + Completed journal atomically in its own document
```

- Busy guard thuộc mỗi feature owner và coordinator; callback reentry/double-click không tạo hai claim. Main-thread contract được ghi rõ; không tuyên bố thread-safe khi chưa có nhu cầu/thread contract.
- Với resolver ngoài máy, persist intent trước dispatch; backend nhận cùng request ID và phải có recovery/query contract. Với resolver local, chỉ grant sau khi fixed result đã persist. Không resolve lại một journal đã Resolved.
- Mỗi currency/card owner ghi mutation + sub-transaction ID + fingerprint trong **một save candidate của chính owner**, persist rồi commit memory/event. Grant success chỉ trả sau persistence contract đã thành công.
- Journal entry update thất bại sau owner grant: retry owner trả AlreadyDelivered. Feature claim-save thất bại sau mọi entry grant: journal vẫn DeliveredAwaitingClaimCommit, retry không grant lại; recovery hoàn tất claim trước báo success.
- Economy coordinator không tự ghi save class Daily/Missions. Feature lưu pending/entry acknowledgements bằng detached candidates trong chính feature save; sau delivery-complete, claimed transition + Completed journal commit trong cùng candidate. Startup đọc pending trước eligibility/reset. Không có khoảng hở giữa hai key feature/journal, không cần một independent acknowledgement save sau final commit.
- Pending lưu original claim date/cycle/window, next-state snapshot, revision và resolved payload. Midnight/reset/config change không chuyển reward sang ngày/kỳ mới; finalization old window không đánh dấu mission cùng ID ở new window. Không xóa old-window state đang có pending.
- Validate toàn bundle trước grant; nếu runtime owner failure sau một phần bundle thì giữ journal và lỗi rõ, không tự rollback reward đã cấp hay resolve phần còn lại thành reward mới.
- New strict-delivery ledger ban đầu giữ ID suốt lifetime profile/save namespace, **không prune theo số lượng/thời gian tùy ý**. Đo growth ở E. Chỉ compact nếu chứng minh request không thể replay và giữ tombstone/watermark đủ từ chối ID cũ. Feature completed journal có thể bỏ entry ack detail sau atomic finalization nếu claim identity/payload tombstone còn đủ để từ chối replay; không xóa durable owner receipts. Dedup progress event là ledger khác, không dùng claim retention làm event guarantee.
- Ledger legacy chỉ chứa ID không có payload không được đoán fingerprint. Giữ behavior duplicate legacy API; strict path dùng migration mapping/tombstone và từ chối collision không xác minh được, không cấp lại. Không thể bảo đảm hồi tố các ID đã bị wallet v0.2.1 prune hoặc claim legacy đã grant nhưng mất claim-save.
- Bảo đảm là recoverable idempotent delivery khi mọi owner giữ durable mutation+ID và cùng save lineage. Không có atomicity toàn bundle, không exactly-once mọi backend, không bảo đảm chống sửa save/rollback backup độc lập giữa journal và owner hoặc mất điện ngoài contract filesystem. Recovery từ backup phải theo bộ cùng generation; bất nhất cần reconciliation, không auto-regrant.

### D. Config revision, clock, selection và event

- Package default Missions = Permanent, toàn catalog active như cũ. Periodic là opt-in: offset/timezone policy, cutoff, weekStart, clock injected; không hardcode UTC+7/Monday.
- PB dùng Daily Reward UTC, Repeat, reset-on-missed=false; Quest fixed UTC+7, cutoff 00:00, Monday. Clock local vẫn không phải anti-cheat. Lưu last accepted window/claim date; rollback không mở lại kỳ cũ, forward mở đúng một current window không bù reward. Pending vẫn finish original window. Named timezone/DST chỉ triển khai nếu consumer thật cần; không thêm tz library mặc định.
- Catalog/schedule identity giữ ổn định. Revision additive giữ progress/claims; thay target/reward/remove selected ID giữa window cần explicit migration mapping. Default defer revision tới window/cycle tiếp theo, giữ snapshot đang active; incompatible update trả migration-required, không silent reset.
- Strict selection reject insufficient pool/conflict. PB cấu hình rõ `PreferGroupsThenRelax` để giữ ý đồ luật cũ; mọi result vẫn unique ID và đúng count. Source legacy `AddCandidates` không loại ID đã chọn ở pass trước khi relax; thêm fixture để không mang lỗi duplicate đó sang package, ghi đây là sửa invariant chứ không bảo toàn bug.
- Reroll đầu tiên chỉ free, một lần/window cho PB, incomplete/unclaimed/active, exclude toàn bộ selected trước reroll. Không có candidate hoặc persist lỗi thì không tiêu count và không đổi selection. Paid/ad reroll giữ disabled cho tới contract cost/receipt recovery riêng.
- Business event ID namespace gồm battle/session ID + action type + action ordinal, encode không collision; completed và won của cùng battle là hai event độc lập. Distinct-value unit ID là dữ liệu objective, không phải event ID.
- Permanent giữ bounded event retention legacy, công bố giới hạn 4096 theo constructor. Periodic giữ event IDs của active window và mọi pending/recoverable old window; chỉ retire window khi event source không còn replay trong guarantee công bố. Producer replay sang window mới phải mang original window/event context, không dùng receipt time để tăng sai kỳ.

## 4. Thứ tự và gate

| Checkpoint | Tasks | Phụ thuộc | Điều kiện qua gate |
|---|---|---|---|
| A | 1-4 | baseline | ADR/schema + strict currency/item crash matrix pass, legacy caller pass |
| B | 5-9 | A | Daily package + sample + PB migration/Home parity; resolver unavailable là lỗi thật |
| C1 | 10-11 | A, sau B theo yêu cầu | Permanent compatibility, window/clock/config/pending tests |
| C2 | 12-13 | C1 | Persisted selection/reroll, policy conflict explicit, no duplicates |
| C3 | 14-15 | C2, A | additive/distinct/dedup + journal recovery + event mapping |
| C4 | 16-18 | C3 | Quest migration, sample/UI/Home lifecycle parity |
| D | 19-21 | B, C4 | Gap reports; conditional code chỉ nếu có gap đã xác nhận |
| E | 22-24 | correctness A-D | Baseline thật, chỉ optimize hotspot, cùng fixture trước/sau |
| F | 25-30 | gates liên quan | source/version/docs, staged consumer rollout, rollback và device/SDK evidence |

Có thể release checkpoint A/B và kiểm chứng Daily trên template/game trước khi làm hết Missions. Không release capability Missions chưa đạt gate. Các lane đọc/gap report có thể chạy song song; writer cùng Economy model/save hoặc bootstrap phải tuần tự.

Dependency graph thực thi không có vòng: A1-4 -> B5-7 cùng adapter8 -> B9 -> C10-18 -> D19-21 -> E22-24 -> F25-27 -> **F29 rollback rehearsal -> F28 production rollout -> F30**. B9 và C18 tự bao gồm template-local source rehearsal trước game-local cutover; không chờ F27/full release. Nếu phát hành checkpoint sớm, chạy lại cùng checklist F25-30 trên checkpoint source đã đạt gate, không đòi các task tương lai chưa nằm trong release đó.

## 5. Todos

Mỗi task có implementation và regression tests cùng scope. `Commit` là ranh giới commit đề xuất cho execution **sau khi được người dùng cho phép commit**, không phải lệnh tự động của lần lập kế hoạch này. Category là gợi ý worker; implementation vẫn do executor chịu trách nhiệm.

### A - reward contract và recovery

- [ ] 1. Chốt ADR ownership và graph dependency.
  - References: graph ở mục 3A; `P/com.dreamy.feature/package.json`, `Runtime/Dreamy.Feature.Runtime.asmdef`; package.json/Runtime asmdef của Economy, Daily, Missions, Shop; DataConfig asmdef.meta đã resolve.
  - Work: trước edit inspect status/remote và tạo branch riêng trong từng repo sẽ sửa, không mang user changes vào branch bằng stash/reset. Ghi ADR trong Economy về ownership, guarantee, schema, sync/Pending contract, compatibility và retention; sửa riêng Feature manifest dependency DataConfig theo asmdef hiện có. Không đổi Core hoặc split assembly.
  - Acceptance/QA: Q-A1 fresh consumer resolve đủ dependency không cần dựa vào manifest host tình cờ có sẵn; graph không vòng/Runtime->Editor; fixture cố ý thiếu dependency phải bị validation phát hiện. Evidence `A/dependencies`.
  - Depends: baseline. Commit: dependency/ADR riêng theo repo. Recommended task executor category: deep-high, quyết định contract qua package boundary.

- [ ] 2. Thêm typed bundle validation và stable identity.
  - References: mục 3B; `P/com.dreamy.feature.economy/Runtime/{ResourceId,ResourceAmount,ResourceGrantRequest,ResourceExchangeRequest}.cs`.
  - Work: thêm schema/bundle/result/identity trong `Runtime/Rewards/` (mới); immutable resolved entry, canonical fingerprints/version; giữ legacy API, không arbitrary payload.
  - Acceptance/QA: Q-A2 round-trip currency+fake-item; reject duplicate entry, unknown kind/target, amount <=0, default struct/invalid ID, overflow, delimiter collision và same-ID/different-payload trước grant. Evidence `A/schema`.
  - Depends: 1. Commit: typed reward contract. Recommended task executor category: deep-high, serialization/identity invariants.

- [ ] 3. Làm reference currency delivery bền vững.
  - References: `P/com.dreamy.feature.economy/Runtime/{DatasaveResourceWallet,EconomySaveData}.cs`; mục 3C; DataSave `SaveInternal`.
  - Work: detached wallet candidate -> persist -> commit dictionaries/events; checked grant/exchange, retain atomic debit+all rewards; version/migrate payload ledger. New strict path không prune ID theo 4096; legacy constructor/API giữ contract được document, không nâng adapter không đủ guarantee thành strict.
  - Acceptance/QA: Q-A3 persist fail không đổi balance/cache/ID/event; reload+retry sau grant chỉ cấp một lần; exchange fail giữa rewards không trừ cost; retry >4096 strict transactions vẫn dedup; legacy ID collision không bị cấp lại. Evidence `A/currency`.
  - Depends: 2. Commit: durable wallet/reference owner. Recommended task executor category: deep-high, correctness dựa trên persistence invariant.

- [ ] 4. Triển khai journal/coordinator và crash matrix.
  - References: mục 3C; `P/com.dreamy.datasave/Runtime/{IDatasaveService,DatasaveService,SaveData}.cs`; mới `Economy/Runtime/Rewards/`, `Economy/Tests/Runtime/`.
  - Work: typed embedded journal cho feature, intent, fixed resolved payload, per-entry status, deliver-awaiting-claim và atomic finalization; fake feature save giữ pending+claimed cùng document, fake item owner có durable ID+payload cùng item state. Một owner dispatch boundary thực tế, không framework plugin tổng quát hay independent journal save.
  - Acceptance/QA: Q-A4 fault injection trước/sau intent save, resolver response/resolved save, mỗi owner commit, mỗi journal update và atomic feature finalization; tạo lại coordinator/wallet/item/feature từ persisted data sau từng fault. Count grant đúng một, payload không đổi, reentry Busy, resolver lỗi không grant. Test file-backed DataSave temp/backup boundary trên bản copy; không chỉ mock luôn trả success. Evidence `A/recovery-matrix`.
  - Depends: 2-3. Commit: coordinator + recovery tests. Recommended task executor category: deep-high, liên owner và crash states.

### B - Daily Reward

- [ ] 5. Thêm bundle config và versioned Daily save.
  - References: `P/com.dreamy.feature.daily-reward/Runtime/Config/DailyRewardScheduleConfig.cs`, `Runtime/Persistence/DailyRewardSaveData.cs`, existing `Tests/Runtime/DailyRewardModelTests.cs`.
  - Work: đọc legacy resourceId/amount hoặc bundle, reject nếu khai báo cả hai; preserve serialized IDs, migrate save v1, revision/active snapshot/pending context; không reset vì đổi schedule config.
  - Acceptance/QA: Q-B1 legacy JSON/save round-trip; ambiguity/unknown schema rejected; config reorder/remove/reward change cần explicit mapping/defer, không reset claimed. Evidence `B/config-migration`.
  - Depends: 4. Commit: Daily config/save migration. Recommended task executor category: deep-low, bám schema đã chốt.

- [ ] 6. Đổi Daily claim sang durable gateway.
  - References: `P/com.dreamy.feature.daily-reward/Runtime/Domain/{DailyRewardModel,DailyRewardClaimResult,DailyRewardViewState}.cs`, `Runtime/Contracts/IDailyRewardService.cs`, `Runtime/Installation/DailyRewardInstaller.cs`.
  - Work: detached candidate -> persist -> commit; busy guard; stable schedule/cycle/entry ID và pending original UTC day; retain legacy wallet overload; publish state/result chỉ sau commit, expose Pending/retry.
  - Acceptance/QA: Q-B2 same-day, midnight, skipped days non-reset, default reset=true, StopAfterLastDay, repeat cuối cycle, clock rollback, save failure after delivered, restart qua midnight, double-click/reentry; không state giả claimed hay chuyển old claim sang new day. Evidence `B/domain`.
  - Depends: 5. Commit: Daily claim recovery. Recommended task executor category: deep-high, eligibility/recovery tương tác ngày.

- [ ] 7. Cập nhật Daily presenter và hai sample.
  - References: `P/com.dreamy.feature.daily-reward/Runtime/Presentation/DailyRewardPresenter.cs`, Contracts/view/feedback; `Samples~/Daily Reward Feature/`; package samples metadata.
  - Work: bundle render, busy/error/retry/close, wallet-only compatibility; currency-only và bundle+fake-item route tách rõ. Không assume result.Reward.Value luôn tồn tại cho bundle; cleanup subscriptions/async theo owner.
  - Acceptance/QA: Q-B3 import mới compile, open/claim/retry/close/reopen không duplicate listener; fake owner failure hiển thị thật, currency-only audio path còn chạy; ảnh Game view nhiều aspect ratio/safe area, missing-script/ref check. Evidence `B/samples`.
  - Depends: 6. Commit: Daily presenter/samples. Recommended task executor category: deep-low, sample gắn contract đã cố định.

- [ ] 8. Thêm strict currency và card adapter của game.
  - References: `PB/Assets/_Project/Scripts/Gameplay/Progression/{PlayerResourceService.Economy,ResourceSave,UnitUpgradeSystem}.cs`; `UnitProgressSave` ở UnitUpgradeSystem.cs; resolver Daily/Quest hiện có.
  - Work: extend ResourceSave/UnitProgressSave với versioned delivery ID+fingerprint, detached candidate và overflow int/long; currency alias canonicalization; host resolver nhận request ID và chỉ trả concrete unit hợp lệ. Không tạo inventory.save hay economy.save.
  - Acceptance/QA: Q-B4 grant currency/card cùng bundle, fail/restart sau mỗi owner save không trùng; mọi alias coin/gold, gem/gems giữ balance; card unknown/overflow/resolver unavailable không mutate. Không cài random resolver giả cho production. Evidence `B/game-owners`.
  - Depends: 4, trước cutover 9; có thể làm song song với 5-7 ở file khác. Commit: host reward ownership. Recommended task executor category: deep-high, host persistence boundary.

- [ ] 9. Migrate Daily legacy và wire Home thật.
  - References: `PB/Assets/_Project/Scripts/Gameplay/Progression/DailyReward/`; `PB/Assets/_Project/Scripts/Bootstrap/GameInstaller.{Foundation,Features}.cs`; `PB/Assets/_Project/Scripts/UI/{Foundation/UIRouteRegistry,Screens/UIDailyReward}.cs`; DailyRewardServiceTests/HomeFeatureWiringTests.
  - Work: backup save; map NextClaimDay-1, LastClaimDateUtc, claimed eligibility và stable migration epoch, không đoán cycle lịch sử; persist destination + migration marker cùng candidate, retry idempotent trước bật route. Import/adopt sample vào `_Project/_Feature` qua Unity, preserve Home/Back.
  - Acceptance/QA: Q-B5 trước PB cutover, rehearsal source-local trên template copy cho currency-only/reset/stop mode; sau đó migrate PB copy twice/restart giữa migration không reset/ngày nhận lại/card trùng; UTC 7-day Repeat reset=false; no-resolver path thực; Home->Daily->Back giữ cùng Home và input, repeated opens; legacy routes retire chỉ sau parity. Historical torn legacy grants cần reconciliation, không claim tự recover. Evidence `B/pb-cutover`.
  - Depends: 7-8; F27 là release rollout sau này, không là dependency ngược của task này. Commit: Daily adapter/migration/route. Recommended task executor category: deep-high, save và Home cutover.

### C1 - Period và claim identity

- [ ] 10. Thêm period policy với injected clock.
  - References: `P/com.dreamy.feature.missions/Runtime/{Config/MissionCatalogConfig,Contracts/IMissionService,Domain/MissionModel}.cs`; PB QuestService window helpers.
  - Work: Permanent default, Daily/Weekly opt-in; offset/cutoff/weekStart config; half-open UTC window và stable ID, revision tách catalog identity. Không dùng đổi catalog ID để reset.
  - Acceptance/QA: Q-C1 fixed fake clock tại boundary -1 tick/đúng boundary/+1 tick, Monday và week-start khác, year/leap-day transition, UTC+7 và UTC, Permanent config/save cũ giữ behavior. Evidence `C1/period`.
  - Depends: 4, B gate. Commit: Missions period policy. Recommended task executor category: deep-high, lịch/window semantics.

- [ ] 11. Persist window transition và old pending claim.
  - References: `P/com.dreamy.feature.missions/Runtime/Persistence/MissionSaveData.cs`, MissionModel Commit; mục 3C-3D.
  - Work: versioned windows, active revision snapshot, high-water policy, pending archive; keep old period claim/finalizer isolated với current window; preserve Permanent transaction ID compatibility.
  - Acceptance/QA: Q-C2 rollback/forward, restart qua cutoff, config update khi pending, removed mission và migration-required; same mission next window claim được, old pending vẫn hoàn tất; failed rollover save không đổi active cache. Evidence `C1/window-save`.
  - Depends: 10. Commit: Missions window persistence. Recommended task executor category: deep-high, tương tác reset/recovery.

### C2 - Selection và reroll

- [ ] 12. Persist selection và explicit conflict policy.
  - References: Missions catalog/model/save; `PB/Assets/_Project/Scripts/Gameplay/Progression/Quest/QuestService.cs:158-245`; `PB/Assets/Resources/DataConfig/questConfigs.json`.
  - Work: pool/count/conflict, injected RNG, no duplicate ID; default Strict fails unsatisfied; PB PreferGroupsThenRelax explicit. Persist selection trước trả state/UI; restart không roll lại.
  - Acceptance/QA: Q-C3 3/6,5/10, constrained/no-candidate pool, deterministic RNG, relaxation cần fill không duplicate, fail save không publish uncommitted selection; Permanent toàn catalog như cũ. Evidence `C2/selection`.
  - Depends: 11. Commit: Missions selection. Recommended task executor category: deep-low, deterministic selection rules.

- [ ] 13. Thêm free reroll transactional.
  - References: same Missions files; PB QuestService.TryReroll; QuestSave.RerollUsed.
  - Work: active/incomplete/unclaimed eligibility, window limit, exclusion, NoCandidate/Busy; replacement+count cùng candidate save. Không bật cost/ad path.
  - Acceptance/QA: Q-C4 reroll một lần/window, completed/claimed/inactive/pending reject; exclude current selected, NoCandidate/save failure không mất lượt; restart giữ replacement/count, next window reset đúng. Evidence `C2/reroll`.
  - Depends: 12. Commit: Missions free reroll. Recommended task executor category: deep-low, scope state transition hẹp.

### C3 - Progress và reward

- [ ] 14. Thêm distinct objective và event dedup đúng scope.
  - References: MissionModel.ReportProgress + existing tests; MissionSaveData; PB QuestSave.UniqueValues; mục 3D.
  - Work: additive và distinct-value stable IDs, per mission/window sets serialized; saturating progress không overflow; business event namespace/window khác distinct ID. Persist trước StateChanged, giữ bounded legacy contract; no debounce làm mất durability.
  - Acceptance/QA: Q-C5 same unit qua hai event chỉ tăng distinct một, additive vẫn tăng; replay sau restart không tăng trong guarantee; completed/won cùng battle không triệt tiêu; unknown key không consume ID; old event replay không đẩy kỳ mới; old >4096 Permanent semantics document/test. Evidence `C3/progress`.
  - Depends: 13. Commit: Missions objectives/dedup. Recommended task executor category: deep-high, event/window identity.

- [ ] 15. Dùng reward gateway và map battle objectives.
  - References: MissionModel.Claim/IMissionService; PB QuestConfig objective enum, `PB/Assets/_Project/Scripts/Gameplay/Battle/BattleController.cs`, Bootstrap/GameInstaller.Features.cs.
  - Work: use same Economy journal; keep single-resource adapter and current Permanent claim ID alias. Host map PlayMatch, WinMatch, FireShot, DealDamage, DestroyUnit, ExposeUnit, CollectEnergyNode, HitUnit, ExposeByTerrain, UseDifferentUnit; generate business IDs từ authoritative events, không enum package.
  - Acceptance/QA: Q-C6 complete/claim/restart bundle fail không trùng; producer-to-service integration cho đủ 10 objective. Trace và nối missing ExposeUnit/ExposeByTerrain emitter bằng bridge tại điểm gameplay xác nhận, không thay mechanics; nếu chưa có authoritative event thì gate fail, không tick checkbox. Evidence `C3/reward-battle-bridge`.
  - Depends: 4,8,14. Commit: package reward path và host bridge riêng theo repo. Recommended task executor category: deep-high, gameplay authority boundary.

### C4 - Save, sample và Quest cutover

- [ ] 16. Migrate QuestSave bằng snapshot idempotent.
  - References: `PB/Assets/_Project/Scripts/Gameplay/Progression/Quest/{QuestSave,QuestService,QuestConfig}.cs`; Missions save migration; existing QuestServiceTests.
  - Work: giữ daily/weekly WindowKey, selected order/IDs, all progress/unique values/claimed/reroll; marker+destination trong cùng save; map old enum sang stable event/mission IDs. Snapshot trước startup auto-rollover; chỉ prune sau import/recovery. Unknown revision/ID trả migration-required, không reset/auto-complete.
  - Acceptance/QA: Q-C7 current và expired window, claimed/unclaimed, distinct progress, used-reroll; migrate twice và fault/restart không reselect, mất progress hoặc cấp thưởng; old/new claimed status đúng. Evidence `C4/quest-migration`.
  - Depends: 11-15. Commit: game Quest migration. Recommended task executor category: deep-high, preservation/migration invariants.

- [ ] 17. Làm Permanent và Periodic samples riêng.
  - References: `P/com.dreamy.feature.missions/Runtime/Presentation/MissionPresenter.cs`; `Samples~/Mission Feature/`, package.json.
  - Work: giữ sample Permanent đơn giản; periodic tabs/countdown/reroll/bundle/pending/error. Timer chỉ render state, domain quyết reset; presenter subscription/dispose, no per-frame full rebuild.
  - Acceptance/QA: Q-C8 import mỗi sample độc lập, compile, thao tác real panel và failure/retry, switch tabs/close/reopen không duplicate subscription; ảnh ratios/safe area, missing refs/scripts; fake item rõ demo. Evidence `C4/samples`.
  - Depends: 13-15. Commit: Missions samples/presentation. Recommended task executor category: deep-low, UI theo domain contract.

- [ ] 18. Adopt Quest sample và chuyển Home route.
  - References: PB Bootstrap, UI/Screens/UIQuest.cs, UI/Foundation/UIRouteRegistry.cs; `_Project/_Feature` và Addressables owner đang dùng; HomeFeatureWiringTests.
  - Work: migrate trước registration, single Missions authority; bridge game service API khi cần; adopt sample có chọn lọc giữ GUID/custom art và Resources key, không import đè sample đang dùng.
  - Acceptance/QA: Q-C9 rehearsal source-local trên template copy Permanent/periodic trước PB copy cutover; Home->Quest daily/weekly/reroll/claim/back; UTC+7/Monday parity, repeated open/close có một listener/service; old/new UI không đồng thời grant. Evidence `C4/pb-cutover`.
  - Depends: 16-17; không chờ full release F27. Commit: Quest Home cutover. Recommended task executor category: deep-high, domain/UI/asset ownership cutover.

### D - Conditional Progression và Shop

- [ ] 19. Đối chiếu Progression tag với XP của game.
  - References: tag `d56ef9bd...`, HEAD `3f591a89...`; `P/com.dreamy.feature.progression/Runtime/{Config/ProgressionCatalogConfig,Domain/ProgressionModel}.cs`; `PB/.../Progression/{PlayerProgressSave,PlayerLevelRules}.cs`.
  - Work: lập gap report theo đúng tag: XP thresholds, max, multi-level, milestone, integer bounds, legacy XP floor, source save authority. Existing tag đã có Experience/Sequential và milestone; không thêm lại capability đã có.
  - Acceptance/QA: Q-D1 comparison fixtures tại level boundary/multi-level/max/overflow và legacy floor; tag/HEAD diff evidence, không dùng HEAD presenter dưới pin cũ. Kết luận no-change hoặc exact proven gap. Evidence `D/progression-gap`.
  - Depends: C4 gate. Commit: gap report only. Recommended task executor category: deep-low, đối chiếu source/fixtures.

- [ ] 20. Chỉ đóng gap Progression đã xác nhận.
  - References: output 19; existing Progression tests, PlayerProgressSave.
  - Work: nếu không gap, ghi NotApplicable với evidence, không đổi package. Nếu có gap, nhỏ nhất là adapter giữ PlayerProgressSave/XP floor; chỉ mở package cho capability thật thiếu. Campaign stars/unlock/unit upgrades không vào player-level model; không hai save authority.
  - Acceptance/QA: Q-D2 round-trip save cũ, level/XP đúng threshold, multi-level reward recoverable nếu được bật; no-gap branch có báo cáo không tạo code. Evidence `D/progression-decision`.
  - Depends: 19. Commit: conditional gap only, không tự commit nếu NotApplicable. Recommended task executor category: deep-high, authority/migration nếu có thay đổi.

- [ ] 21. Review Shop fulfillment và chỉ chuyển khi có lợi ích.
  - References: ShopModel.cs, Tests/Runtime/ShopModelTests.cs; `PB/Assets/_Project/Scripts/Shop/DreamySdkShopGateway.cs`; migration-progress P5; T simulated gateway.
  - Work: document gap/redelivery; keep virtual TryExchange atomic; nếu áp dụng shared gateway cho IAP, preserve store transaction/offer/legacy entry IDs và reward snapshot. Nếu cần persisted Shop pending, thêm Shop-owned save và dependency DataSave rõ ràng; không coordinator journal độc lập. Không thay DreamySDK bridge, receipt validator hay mua hàng thật để test. Mixed-owner paid purchase không bật trong scope này: cần product decision riêng về partial fulfillment sau payment hoặc unified transaction owner, không âm thầm yếu đi atomic currency-only path.
  - Acceptance/QA: Q-D3 existing Shop tests và host callback/recreation/cancel/purchase-once tests; partial bundle/restart receipt cùng ID không grant hai lần; nếu thiếu SDK pending receipt query thì báo exact production blocker, không coi editor fake là pass. Evidence `D/shop-gap-recovery`.
  - Depends: 4,8,19; conditional integration giữ scope đã xác nhận. Commit: Shop change only if justified. Recommended task executor category: deep-high, store delivery identity/recovery.

### E - Tối ưu có đo đạc

- [ ] 22. Capture baseline cùng fixture rõ ràng.
  - References: completed A-D; permanent/periodic samples, PB Daily/Quest, Unity Profiler và DataSave owner.
  - Work: capture CPU/GC/save count+bytes/UI rebuild/ledger growth; fixture catalog 16,100,1000 missions, deterministic 10000 business events gồm replay/distinct/no-op; ghi seed/content/target distribution. Đo Editor domain tách device/UI; warmup riêng và cùng environment.
  - Acceptance/QA: Q-E1 raw captures/counter logs và test correctness totals; baseline có fixture/environment/SHA; không dùng historical Feedback tests làm số liệu mới. Budget chốt từ baseline và device yêu cầu, chưa bịa target hay % improvement. Evidence `E/baseline`.
  - Depends: correctness A-D. Commit: benchmark fixture/report. Recommended task executor category: deep-low, measured evidence.

- [ ] 23. Tối ưu domain chỉ tại hotspot đã đo.
  - References: output 22; Missions ReportProgress/GetState/Clone; Economy journal/ledger.
  - Work: nếu scan hotspot thì eventKey index; nếu dedup lookup hotspot thì runtime set với serialized representation giữ compatibility; no-op tránh duplicate saves/state rebuild. Batch progress chỉ khi đo chứng minh và commit atomic cả batch; distinct có thể cap tracking ở target chỉ khi không mất semantics reroll/migration.
  - Acceptance/QA: Q-E2 cùng fixture/SHA môi trường trước/sau, mọi recovery/migration test pass; save failure không mất acknowledged progress; không prune reward/event ID sai guarantee. Nếu không hotspot, no-change report là kết quả hợp lệ. Evidence `E/domain-before-after`.
  - Depends: 22. Commit: mỗi measured optimization độc lập. Recommended task executor category: deep-high, durability không đổi khi optimize.

- [ ] 24. Đo và kiểm tra UI/lifetime nhiều lần mở.
  - References: Daily/Mission presenters, sample panels; host Addressables/navigation owners.
  - Work: render countdown theo delta, pool/reuse rows chỉ khi baseline cần; kiểm subscription/tween/async cancellation/Addressables acquire-release theo owner qua 100 open/close trên test scene/save copy.
  - Acceptance/QA: Q-E3 counter/capture trước/sau không listener/object/handle tăng dần, no callback vào disposed view; cùng desktop Game view và mobile aspect, domain không bị timer UI reset; không dùng sleep timing-luck cho tests. Evidence `E/ui-lifetime`.
  - Depends: 22-23. Commit: scoped UI optimization only if needed. Recommended task executor category: deep-low, lifecycle instrumentation.

### F - Release, rollout và rollback

- [ ] 25. Chuẩn bị source release và migration documentation.
  - References: từng repo package.json/CHANGELOG/README/Samples~/Tests; evidence A-E; compatibility/source ledger.
  - Work: inspect branch/status/remote rồi branch riêng trong từng repo cần sửa; version/dependency tối thiểu khớp source/schema, sample declarations đúng folder; license/distribution review trước phát hành rộng. Không đổi các package nền không liên quan.
  - Acceptance/QA: Q-F1 focused+suite pass, Runtime/Editor/player compile, sample import mới, GUID/serialization diff, migration docs đủ guarantee/retention/rollback. Version đề xuất, chưa phải tag: Economy 0.3.0, Daily 0.2.0, Missions 0.2.0; Feature dependency-fix 0.1.1. Shop/Progression chỉ tăng nếu đổi, mức theo API/save diff thực tế. Evidence `F/source-release`.
  - Depends: gate mỗi checkpoint định release; full release sau E. Commit: verified increment từng repo chỉ khi được phép. Recommended task executor category: deep-low, package release mechanics.

- [ ] 26. Chốt artifact checkpoint trước publish.
  - References: 25; remote refs, source ledger, ADR và migration docs.
  - Work: chọn exact source SHA/version sau validation; ghi đề xuất tag mới, không overwrite tag. User approval là last step trước push/tag/publish; nếu chưa được phép thì giao source/evidence và giữ release gate pending.
  - Acceptance/QA: Q-F2 tag->commit->package.json->changelog/dependencies khớp; mismatch hoặc required test chưa có phải chặn publish. A/B có thể release riêng sau gate, C capabilities chưa pass không được quảng bá. Evidence `F/checkpoint-release`.
  - Depends: 25. Commit: release metadata nếu authorized; publish là action riêng. Recommended task executor category: deep-low, release ledger.

- [ ] 27. Rollout thử trên template theo checkpoint.
  - References: T manifest/lock; `Assets/_Project/Scripts/Bootstrap/GameInstaller.{Foundation,Features}.cs`; `Assets/_Project/Tests/Editor/`.
  - Work: backup/schema save copy; theo dependency order Feature metadata/Economy -> Daily -> Missions -> conditional Shop/Progression; resolve lock bằng Unity, chọn verified pin/SHA, không main. Test local source trước publish bằng isolated fixture, official pin sau tag mới. Merge adopted sample ngoài project trước, không duplicate asmdef/Resources.
  - Acceptance/QA: Q-F3 currency-only Daily reset-on-missed=true/Stop mode và Permanent Missions khác PB; periodic sample riêng; bootstrap/Back/shared wallet và existing Shop/template tests pass; fresh import/player compile. Template fake billing ghi demo. Evidence `F/template-<checkpoint>`.
  - Depends: checkpoint source gate 25, published pins sau 26; thực hiện trước PB cutover 9/18 và mỗi release. Commit: template pins+lock+selected adopted assets nếu authorized. Recommended task executor category: deep-high, consumer resolution/asset ownership.

- [ ] 28. Rollout Planet Bunker theo dependency order.
  - References: PB manifest/lock/compatibility, migration docs, tasks 9/18; `_Project/_Feature` đã adopt.
  - Work: backup Assets/Packages/ProjectSettings và bộ save cùng generation; update pin từng checkpoint sau template pass; preserve source of truth ResourceSave/UnitProgressSave/PlayerProgressSave, manual selective sample merge. Chỉ retire legacy writer sau parity+migration+recovery.
  - Acceptance/QA: Q-F4 EditMode/PlayMode/Home real flow, Daily UTC và Quest UTC+7/Monday, mystery resolver fail/retry, restart pending; regenerate packed Android Addressables, Android player/device smoke. SDK receipt/restore/process-death gate phải chạy khi CI inject SDK thật; không thay SDK để giả pass. Evidence `F/pb-<checkpoint>`.
  - Depends: 27,29, respective 9/18/conditional D changes. Commit: PB checkpoint pins+lock+adapters/migration assets nếu authorized. Recommended task executor category: deep-high, production consumer migration.

- [ ] 29. Rehearse rollback trên bản copy.
  - References: save envelope/version migration DataSave, template backup từ 27 và PB-copy backups từ 9/18, ledger/journal schemas.
  - Work: restore đồng bộ whole save generation + old Assets/Packages/pins trên copy; không rollback journal riêng hoặc chỉ hạ package tag. Nếu business yêu cầu preserve progress đã phát sinh sau upgrade, phải thêm verified export/backward-readable strategy trước release, không tự overwrite save mới.
  - Acceptance/QA: Q-F5 old client đọc backup schema cũ, new client tiếp tục backup/new saves đúng; no duplicate grant sau resume; mismatch lineage reject/reconcile. Không thao tác player save đang dùng. Evidence `F/rollback`.
  - Depends: 27 và PB-copy rehearsal 9/18; chạy trước 28 để không tạo vòng rollout->rollback->rollout. Commit: rollback runbook/fixture, không commit player saves. Recommended task executor category: deep-high, cross-schema recovery.

- [ ] 30. Tổng hợp checkpoint evidence và trạng thái release.
  - References: mọi evidence A-F; PB compatibility + migration-progress; T README/source ledger; source package SHAs.
  - Work: command/exit/test counts/discovery/Unity/platform per checkpoint; đánh dấu observed vs validated vs blocked, SDK/device gates riêng. Close checkboxes chỉ có source commit và validation; không gọi package production-ready nếu required gate pending.
  - Acceptance/QA: Q-F6 evidence links mở được, SHA/pin/lock đúng; missing/skipped/zero-test/inconclusive phải visible và gate fail, không ghi pass. Rollout A/B sớm không đóng toàn C-F. Evidence `F/release-summary`.
  - Depends: gates của release tương ứng, full plan sau 29. Commit: evidence/compatibility docs only nếu authorized. Recommended task executor category: deep-low, evidence audit.

## 6. Verification strategy và commands

### Đã chạy trong phiên lập kế hoạch

- Pull 17 repo: `git -C <repo> -c core.hooksPath=NUL pull --ff-only --no-rebase --tags origin main`, tất cả exit 0; monitor `mon_Z5VFXBJ82BDYMW81`.
- Mỗi repo: `git rev-parse HEAD`, `git rev-parse origin/main`, `git rev-list --left-right --count HEAD...origin/main`, `git status --porcelain`; 17/17 equality, delta 0/0, clean.
- `node D:/Game/dreamy-codex-toolkit/harness/dreamy-harness asmdef "<project>"`: sandbox **80**, PB **22**, T **14** asmdefs; cả ba status pass/exit 0. Monitor `mon_Z7ZKVCVEG94J2Z3C`.
- Giới hạn: asmdef harness là kiểm tra tĩnh Runtime->Editor, không chứng minh Unity compile, resolved dependency đầy đủ, sample behavior, player build hay reward correctness.
- Không chạy package EditMode/PlayMode hoặc mobile/billing tests trong lần lập kế hoạch; không có code package thay đổi. LSP daemon đang unreachable, không lấy đó làm lỗi C# hoặc tự sửa infrastructure ngoài scope.

### Execution test discovery

- Daily: `P/com.dreamy.feature.daily-reward/Tests/Runtime/DailyRewardModelTests.cs`, assembly `Dreamy.DailyReward.Runtime.Tests`.
- Missions: `P/com.dreamy.feature.missions/Tests/Runtime/MissionModelTests.cs`, assembly `Dreamy.Missions.Runtime.Tests`.
- Shop: `P/com.dreamy.feature.shop/Tests/Runtime/ShopModelTests.cs`, assembly `Dreamy.Shop.Runtime.Tests`.
- Progression: `P/com.dreamy.feature.progression/Tests/Runtime/ProgressionModelTests.cs`, assembly `Dreamy.Progression.Runtime.Tests`; tag/HEAD test asmdef khác nhau.
- Economy hiện **không có** `Tests/Runtime`; A tạo focused test assembly ở đó vì reward behavior mới sẽ không được suite hiện tại bắt. Không giả định generated csproj là .NET harness.
- Sandbox `Packages/manifest.json:testables` hiện có Audio/UI/LuckyWheel/Missions/Tutorial/Feedback; **thiếu Daily/Progression và Economy mới**. Executor thêm đúng testables/constraints khi chạy suite và chứng minh discovery bằng nonzero test count; không claim pass vì runner tìm 0 tests.
- PB existing suites: `Assets/_Project/Tests/EditMode/{DailyRewardServiceTests,QuestServiceTests,PlayerResourceWalletTests,HomeFeatureWiringTests}.cs`; bổ sung tests đúng thư mục/style đó.
- T existing suites: `Assets/_Project/Tests/Editor/{ShopBillingTests,TemplateSaveTests}.cs` cùng lifecycle/installer tests trong thư mục. Không có standalone `dotnet test` harness đã xác minh.

Chạy targeted suite trước, full package suite/consumer tests sau checkpoint; test không pin prose. Fault tests subscribe exact callback/state trước trigger, fake clocks/RNG, bounded event completion; không fixed sleep hay polling theo may rủi. Mocks phải giữ persistence thật của boundary được assert.

### Lệnh cho execution, chưa chạy

PowerShell trên workstation đã có Editor tại đường dẫn dưới. Dùng test project copy nếu project đang mở; không mở batchmode thứ hai trên cùng project. Khi dùng Unity MCP, initialize endpoint `http://127.0.0.1:8080/mcp`, đọc instances/custom-tools/editor state, target đúng project, lấy schema tools mới rồi mới chạy; tool catalog chat không hiện Unity không đồng nghĩa server mất kết nối.

```powershell
$Unity = "C:/Program Files/Unity/Hub/Editor/6000.4.12f1/Editor/Unity.exe"
$Project = "D:/Game/dreamy-package-sandbox"
# Evidence directory must exist before these future runs.
& $Unity -batchmode -nographics -projectPath $Project -runTests `
  -testPlatform EditMode -assemblyNames "Dreamy.DailyReward.Runtime.Tests;Dreamy.Missions.Runtime.Tests;Dreamy.Shop.Runtime.Tests;Dreamy.Progression.Runtime.Tests" `
  -testResults "$Project/.omo/evidence/dreamy-package-evolution-2026-10-14/package-editmode.xml" `
  -logFile "$Project/.omo/evidence/dreamy-package-evolution-2026-10-14/package-editmode.log"
# Add the Economy test assembly name after A creates and verifies its asmdef.
& $Unity -batchmode -nographics -projectPath $Project -runTests `
  -testPlatform PlayMode `
  -testResults "$Project/.omo/evidence/dreamy-package-evolution-2026-10-14/package-playmode.xml" `
  -logFile "$Project/.omo/evidence/dreamy-package-evolution-2026-10-14/package-playmode.log"
node D:/Game/dreamy-codex-toolkit/harness/dreamy-harness asmdef $Project
git -C $Project diff --check -- .omo
```

Cho PB/T, thay `$Project`, bỏ `-assemblyNames` để chạy full consumer suite và đặt artifact ngoài player save/assets; giữ output theo checkpoint. Parse XML outcome, failures, skipped và discovered count, đối chiếu exit code, không chỉ đọc log "started". Daily/Progression test assembly có includePlatforms=[]: xác minh actual runner classification theo tag/Unity discovery rồi chọn EditMode/PlayMode đúng, không ép suite chạy bằng một filter trả zero.

Addressables dùng owner build method/Editor menu đã có, inspect exact contract trước invocation; không bịa tên executeMethod. Gate yêu cầu packed Android content `result.Error` rỗng, compile/Console sạch và real Home loads sau build; Android content pass không phải Android player/device pass. SDK thật chỉ do CI/CD inject theo contract game hiện có.

## 7. Final verification wave

Các gate dưới đây là cho **implementation tương lai**, hiện chưa chạy/đánh dấu.

- [ ] F1. Audit scope, dependency, API và serialization.
  - Q-Final1: đối chiếu manifest/asmdef/source/tag/version/sample/compatibility; không package->game, Runtime->Editor, duplicate save authority hoặc GUID churn. Evidence `final/scope-audit`.
- [ ] F2. Audit durability, migration và fault tests.
  - Q-Final2: rerun đúng affected suite sau final change, nonzero discovery; kiểm crash matrix A/B/C và rollback generation; không exactly-once claim vượt bảo đảm owner. Evidence `final/recovery-audit`.
- [ ] F3. Manual QA trên Unity và consumer thật.
  - Q-Final3: Home Daily/Quest/Shop/Back, pending/retry/restart, permanent/reset sample, missing-script/reference check, content/player/device/SDK gates với outcome riêng. Evidence `final/runtime-qa`.
- [ ] F4. Đối chiếu parity và release readiness.
  - Q-Final4: map mọi tiêu chí người dùng ở phần 8 sang test/capture/SHA; unresolved mandatory gate khiến release chưa complete; optional D/E no-change phải có measured/report evidence. Evidence `final/readiness`.

## 8. Success criteria và coverage đầu vào

| Kết quả cần đạt | Task / QA chứng minh |
|---|---|
| Một reward contract reusable, ownership rõ, legacy currency chạy | 1-4 / Q-A1..A4 |
| Bundle currency+item, fixed resolved card, unknown/overflow/mismatch/concurrent/crash safe trong guarantee | 2-4,8 / Q-A2..A4,Q-B4 |
| Daily legacy config/save tương thích, detached claim, UTC/repeat/reset/stop đúng | 5-6,9 / Q-B1,B2,B5 |
| Daily UI/sample bundle/busy/retry và Home/Back thật | 7,9 / Q-B3,B5 |
| Missions Permanent + configurable period/window/revision/clock, pending qua reset | 10-11 / Q-C1,C2 |
| Selection persistent, strict/relaxed explicit, no duplicate, reroll parity | 12-13 / Q-C3,C4 |
| Additive/distinct/business dedup khác nhau, đủ 10 objective, rewards recoverable | 14-15 / Q-C5,C6 |
| QuestSave không mất window/selection/progress/unique/claim/reroll, UI lifecycle đúng | 16-18 / Q-C7..C9 |
| Progression/Shop chỉ sửa proven gap, đúng tag, không SDK/authority duplication | 19-21 / Q-D1..D3 |
| Optimize sau correctness có baseline/fixture/env/before-after, không hy sinh save | 22-24 / Q-E1..E3 |
| Version/docs/license, source commit/pins/lock, selective sample merge, 2 consumer gates | 25-28 / Q-F1..F4 |
| Backup/schema-aware rollback và explicit SDK/device/release blockers | 29-30,F1-F4 / Q-F5,F6,Q-Final1..4 |

### Quyết định còn cần bằng chứng trước production

- Backend mystery-card resolver: request ID, status/recovery và stable resolved payload chưa có implementation được xác minh. Adapter phải fail unavailable khi thiếu; không mở random client reward để vượt gate.
- Event producer ExposeUnit/ExposeByTerrain chưa chứng minh; task 15 phải nối authoritative emitter hoặc giữ cutover gate chưa đạt.
- Lifetime ledger/replay guarantee và disk growth phải đo; không được đổi thành prune 4096 để qua benchmark. Nếu cần bounded production retention, chốt contract replay/tombstone trước release.
- Local clock không authoritative; high-water policy không bảo vệ save tampering. Server trust chỉ được thêm qua host contract thật, không tự triển khai backend.
- Paid/ad reroll không thuộc bản đầu được đề xuất; muốn bật phải bổ sung transaction plan và approval cho scope đó.
- Exact release versions/tags chốt sau diff/gates; bảng version đề xuất không phải tag đã tồn tại.
- SDK billing, Android player/device smoke chưa chạy trong phiên này. Không đóng gate bằng simulated purchase, static harness hay evidence lịch sử.

Kế hoạch hoàn tất không đồng nghĩa implementation bắt đầu. Bước triển khai đầu tiên là A1-A4 trên source branch riêng, sau khi người dùng yêu cầu thực thi; không bắt đầu bằng import Daily/Missions vào game.
