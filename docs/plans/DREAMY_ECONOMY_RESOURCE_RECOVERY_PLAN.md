# Plan mở rộng Economy: spend, thiếu tài nguyên và điều hướng Shop

Ngày: 2026-10-15. Trạng thái: **đã triển khai source và sample, validation sandbox đạt; chưa phát hành**.

Mục tiêu: tiêu tài nguyên qua một owner giao dịch, có thể bật/tắt panel thiếu tài nguyên; mở đúng khu vực mua/bổ sung theo resource và thiết kế Shop của từng game. Giữ compatibility với consumer hiện có.

## 1. Cơ sở và khoảng trống hiện tại

Áp dụng [quy tắc thiết kế package](../rules/DREAMY_PACKAGE_DESIGN_RULES.md) và [plan Connectivity](DREAMY_CONNECTIVITY_PACKAGE_PLAN.md). Manifest/lock đang resolve package local; version/source quan sát ngày lập plan:

| Package | Version `package.json` | Commit submodule |
| --- | --- | --- |
| Economy | `0.3.1` | `22e8a0d1b94cbd2df42ed019859a00e6918e252a` |
| Shop | `0.1.3` | `0d9e7d7b0bf2fc02a2ab1e6a95ed7fd04bce26f3` |
| UI | Kiểm lại khi triển khai | `1677264fdbc4c49a92766b8ffbdc915ae0ddca6f` |

Source đã đối chiếu:

- `IResourceWallet` chỉ có `TryGrant` và `TryExchange`, trả `bool`.
- `ResourceExchangeRequest` hiện có một cost và bắt buộc ít nhất một reward. Không phù hợp để giả lập spend bằng reward rỗng hoặc reward giả.
- `ResourceAmount` dùng `long`, yêu cầu amount > 0; không được dùng nó để biểu diễn current balance = 0 hoặc thiếu hụt = 0.
- `DatasaveResourceWallet` tính detached candidate, lưu trước rồi cập nhật balance và phát event. Có busy guard, history legacy giới hạn và strict receipt reward riêng. Không thay ngầm semantics của chúng.
- `ShopModel` đổi `TryExchange(false)` thành `ExchangeFailed`; result chưa có dữ liệu thiếu hụt. `ShopPresenter` chưa có recovery thiếu tài nguyên.
- Sample Shop sử dụng presenter; `Assets/_Project/Scripts/UI/UIShopPanel.cs` là custom panel có holder gem/gold riêng. Đây là bằng chứng cần adapter navigation theo game, không hardcode tab enum vào Economy.
- Chưa tìm thấy toolkit/compatibility registry trong checkout; các API mới bên dưới là đề xuất và phải kiểm lại source/versions lúc code. Không tuyên bố hỗ trợ mọi loại economy: bản đầu vẫn là resource số nguyên `long`; BigNumber/server wallet có adapter và validation riêng.

## 2. Các quyết định cần giữ xuyên suốt

1. **Economy domain không mở panel hoặc Shop.** Giao dịch trả kết quả có kiểu; application flow/presenter quyết định presentation.
2. **Panel thiếu tài nguyên tùy chọn theo từng lời gọi**, không phải global switch bắt buộc toàn game.
3. **Điều hướng theo ResourceId và dữ liệu thiếu hụt**, không theo tên tab/prefab/enum riêng game.
4. **Host cung cấp adapter bổ sung tài nguyên**: Shop, ads, đổi tiền, màn chơi kiếm thêm hoặc không có đường bổ sung.
5. **Giữ API legacy**; thêm capability mới thay vì thêm member bắt buộc vào interface đang được nhiều game implement.
6. **Không tự tiêu lại sau khi người chơi mở Shop**. Mặc định quay lại màn hình gốc để xác nhận; mode tự tiếp tục chỉ bật tường minh và revalidate toàn bộ điều kiện.
7. Tiêu tiền, đổi tiền và grant reward chỉ thành công sau commit của owner; không coi UI animation hoặc đóng Shop là bằng chứng mua thành công.

## 3. Ownership, assembly và dependency

```text
Economy.Runtime              <- Economy.UI.Runtime
       ^                               ^
       |                               |
Shop.Runtime                 Host acquisition adapter
       ^                       /             \
       |                 Custom Shop       Sample Shop
       +-----------------------+---------------+
```

| Owner | Trách nhiệm | Vị trí dự kiến |
| --- | --- | --- |
| Wallet | Validate, commit, idempotency, balance events | Economy Runtime |
| Transaction result/deficit | Snapshot immutable của kết quả và thiếu hụt | Economy Runtime |
| Spend flow | Gọi wallet, chọn policy, phối hợp recovery | Economy Runtime synchronous core + UI orchestration async riêng |
| Resource acquisition contracts | Request/result trung lập với Shop | Economy Runtime |
| Not-enough presenter/coordinator | Modal session, input, refresh, gọi acquisition gateway | Economy UI Integration assembly mới |
| Not-enough view/prefab | Render và UI events | UI assembly và `Samples~` |
| Shop resource navigation | Resolve request sang offer/tab/scroll/layout | Shop integration/sample hoặc `_Project` |
| Giá và lựa chọn sản phẩm | Catalog/rule của game; SDK thanh toán thật | Shop service/host gateway |

Economy Runtime giữ `noEngineReferences: true`, không thêm dependency Shop hoặc UniTask chỉ để mở popup. Async acquisition/UI contract nằm assembly mới `Dreamy.Economy.UI.Integration` reference Economy Runtime, Economy.UI.Runtime, Dreamy.UI.Presentation, UniTask; các dependency UI/TMP thực dùng được khai báo rõ. Không vòng reference với Shop. `IResourceAcquisitionGateway` async đặt ở assembly integration; request/result DTO nằm Runtime.

Economy UPM hiện đã dependency UI/Core/Datasave. Nếu integration mới dùng UniTask thì phải thêm dependency UPM đúng version; không coi sample có sẵn UniTask ở sandbox là đủ. Shop integration reference Economy UI Integration để implement gateway; Economy không reference Shop.

Root sở hữu một wallet/profile, catalog và gateway; inject cho flow/presenter. Không tạo wallet trong popup, không resolve ServiceLocator trong button/item. Resource display catalog chỉ phục vụ icon/tên/format; balance đọc từ wallet/provider.

## 4. API giao dịch bổ sung, compatibility và nguyên tử

### 4.1 Capability mới

Đề xuất interface mới, chưa tồn tại:

```csharp
public interface IResourceTransactionWallet
{
    ResourceTransactionResult Spend(ResourceSpendRequest request);
    ResourceTransactionResult Exchange(ResourceTransactionRequest request);
}
```

`DatasaveResourceWallet` implement thêm interface này. Không thay chữ ký `IResourceWallet`; không ép custom wallet cũ implement tính năng mới. Root kiểm capability khi install, thiếu thì trả `Unsupported`, không giả lập bằng check balance rồi gọi API `bool`.

- `ResourceSpendRequest`: transaction ID, danh sách costs bất biến, business context ID phục vụ truy vết. Ít nhất một cost; amount dương; ID hợp lệ.
- `ResourceTransactionRequest`: transaction ID, costs và rewards; exchange cần cả hai danh sách không rỗng. Không expose boolean “skip validation”.
- Hỗ trợ nhiều cost để dùng được nâng cấp cần coin + gem. Gộp cost trùng ID bằng checked arithmetic trước khi so balance; reward trùng tương tự. Payload fingerprint canonical có thứ tự ổn định.
- `ResourceTransactionResult`: `Committed`, `AlreadyCommitted`, `InsufficientResources`, `InvalidRequest`, `Busy`, `Overflow`, `PersistenceFailed`, `PayloadMismatch`, `LegacyReceiptUnverifiable`, `Unsupported`.
- Kèm transaction ID, danh sách deficit, mã lỗi. `IsSuccess` chỉ gồm `Committed/AlreadyCommitted`; UI feedback phải phân biệt replay và commit mới.
- `ResourceDeficit`: `ResourceId`, `Required: long`, `Available: long`, `Missing: long`; available cho phép 0. Chỉ thêm row có missing > 0. Snapshot phản ánh lần thực thi, không được dùng như reservation.

### 4.2 Thuật toán trong wallet

1. Validate toàn bộ request, normalize costs/rewards và fingerprint; default struct cũng phải bị kiểm tra.
2. Lấy guard/serialize mutation trên owner thread; kiểm receipt trước balance để replay giao dịch đã thành công không bị báo thiếu tiền.
3. Cùng ID + fingerprint giống: `AlreadyCommitted`, không trừ/grant/event lần hai. Cùng ID khác payload: `PayloadMismatch`. Collision với receipt legacy không xác minh payload: fail closed, không đoán thành công.
4. Tính toàn bộ deficits từ cùng snapshot balance; nếu thiếu thì trả tất cả thiếu hụt, không mutation hoặc ghi success receipt.
5. Tạo detached candidate, trừ mọi cost và cộng mọi reward bằng checked arithmetic; overflow bất kỳ tài nguyên nào hủy toàn giao dịch.
6. Persist balance candidate và receipt trong cùng save owner/commit, sau đó publish runtime state. Save fail không đổi balance và không phát event thành công; chỉ trả `PersistenceFailed` khi save contract bảo đảm failure chưa commit. Nếu provider có outcome mơ hồ, reload/reconcile trước khi cho retry.
7. Phát một change event mỗi resource thay đổi sau commit. Subscriber exception không được biến giao dịch đã commit thành `PersistenceFailed`; isolate/log notification boundary và giữ kết quả commit.
8. Clear busy trong finally; không await/UI callback trong vùng mutation. Không check balance ngoài rồi trừ trong một lời gọi độc lập.

Wallet giao dịch local đồng bộ; server authoritative wallet cần capability async khác ở adapter/application flow, không block main thread bằng `.Result`. Không tự xem adapter local là đủ cho server economy.

### 4.3 Ledger, save và migration

- Thêm transaction receipts có fingerprint dành cho API mới; không tái sử dụng bounded history legacy như bằng chứng idempotency vĩnh viễn.
- Receipt có transaction ID, loại transaction, fingerprint canonical và schema version. Kiểm collision qua các ledger grant/reward/spend/exchange dùng chung wallet namespace.
- Trước khi chọn giữ receipt không giới hạn, xác định retention theo lifetime profile; compact chỉ khi có policy owner chứng minh replay cũ không còn được phép. Không tự xóa receipt để giảm save size rồi tiếp tục hứa chống lặp.
- Migrate save cũ: giữ nguyên balances, legacy IDs/tombstones và strict reward receipts; collection mới khởi tạo rỗng. Không suy ngược fingerprint từ balance hiện tại. Request API mới đụng ID cũ trả `LegacyReceiptUnverifiable`.
- API legacy giữ signature và semantics đã document; không âm thầm redirect tất cả legacy sang ledger mới nếu làm đổi duplicate behavior hoặc exception behavior.
- Test restore save cũ, restart, payload collision, save failure, receipt corruption và bounded legacy history. Nếu cần schema version mới, migration phải có fixture trước khi release.

## 5. Spend và kết quả gameplay

API spend chỉ bảo đảm trừ resource trong wallet. Nó **không bảo đảm atomic với gameplay/save owner khác**.

- Mua item nằm cùng wallet: dùng exchange costs + rewards trong một commit.
- Nâng cấp hệ thống lưu riêng: feature sở hữu journal/workflow ID và recovery; đánh dấu từng bước để không vừa trừ tiền vừa mất upgrade khi crash. Không dùng callback `SpendThenDoAnything()` và gọi đó là transaction nguyên tử.
- Hành động có thể thay giá/điều kiện: owner cấp request mới theo quote/revision sau xác nhận. Retry cùng giao dịch giữ ID/payload; một ý định mới hoặc giá thay đổi dùng ID mới và xác nhận lại.
- Cancel trước commit không trừ tiền; cancel sau commit chỉ hủy presentation, không rollback tự động. Hoàn tiền là giao dịch riêng với key/rule riêng.

## 6. Panel thiếu tài nguyên là tùy chọn

Contract orchestration đề xuất:

```csharp
public enum InsufficientResourcePresentation
{
    None,
    Prompt
}

// Schematic: signature/type sẽ được chốt cùng implementation.
// ExecuteAsync(request, presentation, ownerToken) -> ResourceActionFlowResult
```

Không nhét UI policy vào `ResourceSpendRequest` domain. `ResourceActionFlowResult` giữ **kết quả transaction ban đầu** và **kết quả recovery riêng**; mở Shop thành công không đổi transaction thất bại thành thành công.

Luồng:

```text
Request -> wallet Spend/Exchange
  Committed/AlreadyCommitted -> trả kết quả nghiệp vụ
  InsufficientResources + None -> trả deficits, không mở UI
  InsufficientResources + Prompt -> NotEnoughResourcesPanel
       Cancel/Back -> đóng, không mutation
       Acquire(resourceId) -> gateway mở đường bổ sung
       Quay về -> đọc balance, revalidate, hiển thị trạng thái mới
  Lỗi khác -> trả đúng lỗi, không báo nhầm “thiếu tiền”
```

Ví dụ policy:

- Auto-upgrade chạy nền: `None`, caller nhận thiếu hụt.
- Người chơi bấm Upgrade: `Prompt`.
- Shop purchase hiện có: host opt-in presenter callback/recovery coordinator; không bật mặc định cho mọi consumer cũ.
- Không có UI gateway nhưng caller chọn Prompt: báo `PresentationUnavailable` trong recovery result, giữ `InsufficientResources` gốc; không throw mất nguyên nhân.

Chống trùng: một popup session active, request khác trả `Busy` hoặc chỉ nhận result mà không mở thêm panel theo policy rõ. Không overwrite pending callback. Mọi request/deficit mang context ID để không nhầm giữa hai nút nâng cấp.

## 7. Contract mở Shop/bổ sung tài nguyên theo game

### 7.1 Request trung lập với layout

DTO `ResourceAcquisitionRequest`:

- `TargetResourceId` và `MissingAmount` cho resource người chơi vừa chọn.
- `Deficits` đầy đủ để shop có thể hỗ trợ bundle/multi-resource.
- `SourceContextId`, optional original offer/action ID để analytics/navigation; không giữ reference GameObject hoặc callback business.
- Không chứa `TabIndex`, tên prefab, enum gem/gold hoặc giá IAP.

Boundary async ở UI Integration:

```csharp
public interface IResourceAcquisitionGateway
{
    ResourceAcquisitionAvailability GetAvailability(ResourceId resourceId);
    UniTask<ResourceAcquisitionResult> OpenAsync(
        ResourceAcquisitionRequest request,
        CancellationToken cancellationToken);
}
```

`ResourceAcquisitionAvailability`: `Available`, `Unavailable`, `Unknown`, kèm loại hành động/label presentation. `ResourceAcquisitionResult`: `Returned`, `Cancelled`, `Unsupported`, `NoMatchingDestination`, `Failed`, `Busy`. Đây là kết quả điều hướng, không phải receipt mua hàng.

API convenience dự kiến ở coordinator: `OpenForResourceAsync(resourceId, missingAmount, sourceContextId, token)` và `OpenForDeficitAsync(deficit, context, token)`. Caller muốn mở Shop trực tiếp không cần tạo lỗi spend giả. Tên public cuối không bắt buộc có chữ Shop vì cùng gateway có thể mở màn kiếm năng lượng.

### 7.2 Adapter Shop mẫu

- `ShopResourceAcquisitionGateway` nằm Shop integration/sample, nhận navigator/view adapter của host.
- `ShopResourceDestinationMap`: map ResourceId sang semantic destination ID, optional offer filter và focus rule. Config sở hữu bởi game/catalog; không dùng display name làm key.
- Adapter resolve destination, mở hoặc đưa Shop hiện có lên trước, đợi data + UI layout sẵn sàng, chọn tab/category, filter nếu policy cho phép, scroll tới offer, highlight. Không gọi `Find()` theo tên child hoặc assume tab thứ 2 là gem.
- Nếu Shop đang mở: focus trên instance hiện tại; không spawn panel thứ hai. Request focus có generation/token, request đã cancel không đổi tab muộn.
- Chỉ gợi ý offer thật sự cấp target resource; không chọn offer yêu cầu chính resource đang thiếu nếu làm vòng lặp. Không có route phù hợp: `NoMatchingDestination`, giữ Cancel và thông điệp rõ.
- Không tự mua product hoặc chọn “gói tối ưu” theo missing amount; giá, khuyến mại và quyết định mua thuộc Shop/game.
- Shop purchase service vẫn xác nhận SDK/backend và grant theo transaction owner. Acquisition gateway không seed balance hoặc grant tài nguyên để demo đường production.

### 7.3 Nhiều kiểu Shop cần hỗ trợ

| Layout game | Cách adapter xử lý |
| --- | --- |
| Tab Coin/Gem | Map resource -> tab ID, đợi tab ready, scroll/highlight |
| Một danh sách ScrollRect | Filter/match reward resource, scroll tới item |
| Mỗi currency có panel riêng | Map resource -> panel address/type tại host |
| Shop 3D/NPC | Navigation game mở tương tác/NPC bằng semantic route |
| Energy chỉ nhận qua ads/chờ | Gateway mở màn bổ sung Energy; không ép vào Shop |
| Không bán tài nguyên đó | Availability unavailable, ẩn nút mua thêm |

Sample custom của sandbox có `gemOfferHolder/goldOfferHolder`; triển khai adapter trong `_Project` để focus holder phù hợp. Không chuyển enum `EResourceOffer` hoặc config project vào Economy/Shop Runtime.

## 8. Sau khi trở về từ Shop

Mặc định `ReturnForConfirmation`:

1. Gateway hoàn tất khi người chơi quay về/đóng đích; receipt hoặc balance thay đổi được xử lý độc lập bởi owner Shop/wallet.
2. Đọc lại balance, giá và điều kiện gốc; không dùng thiếu hụt cũ để kết luận đã đủ.
3. Nếu màn hình gốc còn sống, render lại nút xác nhận; nếu owner đã hủy, không mở lại popup hoặc chạy action.
4. Nếu receipt tới muộn: observable balance cập nhật view còn sống. Không chờ vô hạn, không coi đóng Shop là mua thành công.

Mode tùy chọn `RevalidateThenResume` chỉ dùng khi host có callback revalidation và operation identity rõ. Sau khi đủ tài nguyên vẫn phải validate feature state/quote rồi gọi lại transaction có receipt bảo vệ. Không giữ giá cũ nếu catalog đổi; không tự resume giao dịch tiền thật. Default remains manual confirmation.

Nhiều thiếu hụt: panel hiển thị danh sách, người chơi chọn một loại để bổ sung. Quay về tính lại toàn bộ; không tự động mở shop lần lượt cho mọi loại. Balance tăng từ nguồn khác trong lúc popup mở phải cập nhật rows và disable đường bổ sung đã không còn cần.

## 9. Presenter, prefab và vòng đời UI

`NotEnoughResourcesPresenter` nhận read-only balance source, snapshot chi phí, view, acquisition gateway, display provider và owner token. Wallet mutation chỉ qua transaction flow. View phát `AcquireRequested(resourceId)` và `CancelRequested`.

Hierarchy prefab mẫu:

```text
NotEnoughResourcesPanel [UIPanel/view, CanvasGroup]
  Backdrop
  SafeArea
    Dialog
      Title [TMP]
      Description [TMP]
      ResourceRows [LayoutGroup]
        DeficitRow prefab
          Icon, Name, Available, Required, Missing
          AcquireButton
      Status [TMP]
      CancelButton
```

- `DeficitRow` không gọi wallet/SDK/ServiceLocator, chỉ render row và phát intent.
- Các trạng thái: thiếu một/nhiều resource, không có route, opening shop, navigation failed, đã đủ, đang đợi update, owner closed. Nút Acquire khóa trong lúc mở route; Cancel/Back rõ ràng.
- Safe area ở panel hoặc parent owning container; font/icon fallback; số 0 hiển thị hợp lệ; formatter host hỗ trợ style tiền của game.
- Không giữ popup model chỉ bằng static pending request. Session context inject khi mở qua UI bridge/factory, mỗi lần mở có presenter riêng và generation.
- Dừng listener/event/async khi close/disable/destroy; load UI lỗi giải phóng session; callback muộn không update view. Điều hướng dùng suspend/close popup theo policy host, không để hai modal cùng tranh Back.
- Prefab/scene được tạo qua Unity Editor/MCP/Editor API; giữ `.meta`, serialized refs, không sửa YAML tay. Copy-ready chỉ sau fresh import/PlayMode/Game view evidence.

## 10. Samples và tài liệu bàn giao

### Economy: sample `Resource Recovery`

- Scene với wallet demo được ghi nhãn, nút Spend Only, Exchange, Spend Silent và Spend With Prompt.
- Chuyển kịch bản đủ coin, thiếu gem, thiếu coin+gem, lỗi save và duplicate transaction.
- `NotEnoughResourcesPanel.prefab`, `DeficitRow.prefab`, display catalog, root installer, custom acquisition gateway mở panel demo.
- Chứng minh không có route thì ẩn nút bổ sung; quay về khi chưa mua/đã mua; owner bị đóng; update balance từ nguồn khác.
- Không dependency Shop trong Economy sample này; panel acquisition giả chỉ là demo có nhãn, không được đăng ký ở production GameInstaller.

### Shop: sample `Resource Navigation`

- Hai kiểu UI: tab currency và danh sách cuộn. Cùng request ResourceId dùng hai adapter khác nhau.
- Tái sử dụng prefab/presenter có owner rõ; nếu phụ thuộc sample Shop Feature thì ghi thứ tự import và dependency, ưu tiên sample tự đủ không kéo asmdef trùng.
- Demo offer grant đúng resource, no matching destination, Shop đã mở, focus sau layout, cancel trong load và quay về màn spend.
- Route chỉ điều hướng; mua vẫn qua service/gateway. Fake purchase phải ghi demo, không có auto-success production.

### Consumer sandbox

- `_Project` adapter cho UIShopPanel hiện có; kiểm gem/gold holder và source config bằng API thực lúc implement.
- Không giữ hai bản script/asmdef cùng tên khi import/move sample. Dùng Unity Move cùng `.meta` nếu đưa sample vào `_Project`.
- Scene/prefab demo direct reference để chạy ngay; hướng dẫn Addressables là lựa chọn riêng, game tự cung cấp address và ownership release.

README/API docs cần code ví dụ install capability, gọi spend/exchange, `None/Prompt`, mở trực tiếp theo ResourceId, custom Shop adapter, multi-resource, migration save, receipt lifetime, cancellation và giới hạn transaction qua nhiều owner.

## 11. Migration Shop và release

- Thêm trạng thái Shop thiếu tài nguyên và property deficits theo hướng additive. Append enum value, không đổi numeric value đã có. Không đổi `IShopService` hiện tại chỉ vì navigation.
- Shop service dùng capability mới nếu được inject. Với wallet legacy: tiếp tục trả `ExchangeFailed` như cũ; không suy diễn bool false thành thiếu tiền bằng cách đọc balance sau lỗi.
- Presenter nhận optional recovery callback/coordinator riêng cho not-enough; mặc định null giữ hành vi cũ. Không tái sử dụng callback success feedback để xử lý lỗi.
- Không làm public result chỉ còn constructor mới khiến sample/consumer cũ compile fail; giữ factory cũ hoặc cung cấp migration minh bạch nếu bắt buộc breaking.
- Dự kiến Economy `0.4.0` và Shop `0.2.0` cho capability mới; version cuối theo semver policy của repo. Save migration/release notes phải nêu rõ tác động, không bump chỉ patch nếu hành vi public đổi đáng kể.
- Cập nhật dependency minimum Shop -> Economy mới nếu source gọi type mới; đồng bộ package.json, changelog/tag, compatibility khi có, commit package rồi consumer gitlink/lock. Không dùng dirty submodule làm release evidence.

## 12. Các phase triển khai

| Phase | Deliverable | Gate |
| --- | --- | --- |
| E0 — Baseline | Public API ledger, save fixture cũ, dependency/commit snapshot | Compatibility decisions rõ |
| E1 — Transaction contracts | Request/result/deficit, new capability, normalization | Unit tests validation/long boundary/multi-cost |
| E2 — Wallet | Atomic spend/exchange, fingerprint receipts, migration | Save/restart/duplicate/collision/failure tests |
| E3 — Optional presentation | Flow policy, async acquisition boundary, presenter | None không mở UI; Prompt đúng nguyên nhân; lifecycle tests |
| E4 — Shop routing | Resource destination adapter, optional not-enough callback | Tab/scroll/no route/already open tests |
| E5 — Prefabs/samples | Editor-generated assets, scenes, docs, validator | Fresh import riêng/cùng nhau, PlayMode và screenshots |
| E6 — Consumer/release | Sandbox custom shop adapter, validation report, release notes | Existing feature regressions + clean consumer/player compile |

E1/E2 có thể triển khai độc lập Connectivity. E3/E4 không bắt buộc Connectivity; chỉ đường purchase/load cần mạng mới inject flow ở [plan Connectivity](DREAMY_CONNECTIVITY_PACKAGE_PLAN.md). Host điều phối một modal recovery tại một thời điểm để tránh vòng lặp thiếu tiền -> Shop -> mất mạng -> Shop -> thiếu tiền.

## 13. Test matrix và Definition of Done

### Giao dịch và persistence

- Single/multi cost, duplicate ID trong request, balance 0, amount âm/default struct, overflow cost/reward, đủ đúng giá, thiếu nhiều loại.
- Không partial debit/grant khi một cost thiếu hoặc overflow; không event trước save; notification lỗi sau commit không khiến trừ lần hai.
- Replay cùng payload qua restart chỉ mutate một lần; payload mismatch/collision legacy fail closed; strict reward receipts không bị thay semantics.
- Save failure/reload, migration fixture save cũ, ledger corruption, bounded history legacy và reentrancy/busy guard.
- Gameplay owner khác bị crash sau spend: workflow recovery test của feature, không chỉ test wallet.

### Presentation và navigation

- `None` không mở panel; lỗi save/invalid/busy không hiện thiếu tiền; thiếu UI vẫn trả result gốc.
- Prompt đủ data, zero balance render đúng, multi-deficit chọn đúng ResourceId; route unavailable ẩn nút; không offer phù hợp không mua nhầm.
- Shop mở mới/đang mở; tab/scroll/focus chờ layout; request cancel không focus muộn; IAP receipt tới muộn không bị suy ra từ result navigation.
- Mở/đóng lặp, Back/Cancel, scene unload, listener cleanup, hai yêu cầu đồng thời, popup bị destroy trong lúc Shop load.
- Quay về không mua, mua chưa đủ, đủ từ nguồn khác, giá đổi, feature gốc không còn hợp lệ; default không tự spend lại.

### Validation commands và giới hạn

- Harness hiện có: `python3 LocalPackages/com.dreamy.feature.settings/Tests~/validate-settings.py --shop` dùng regression Shop/UI liên quan sau khi có thay đổi. Kiểm lại harness có cover code mới trước khi gọi pass.
- Harness mới dự kiến `LocalPackages/com.dreamy.feature.economy/Tests~/validate-resource-recovery.py` phải được viết, không phải lệnh đã có. Chạy pure managed contracts/migration và compile graph phù hợp.
- Unity EditMode/PlayMode xác minh lifecycle/prefab, fresh consumer import xác minh dependency thật; Android/iOS player compile theo support matrix. Không gọi asset copy-ready chỉ từ harness managed.
- Giữ regression Daily Reward/Missions/Progression/Lucky Wheel/Shop đang dùng wallet legacy; không thay save hoặc grant behavior ngoài scope.

**Done khi** silent spend và prompt spend cùng dùng một transaction owner; kết quả thiếu hụt chính xác và atomic; replay không trừ hai lần; một custom Shop và sample tab/scroll nhận cùng ResourceId request; import mới có prefab đầy đủ refs và flow chạy thật; API legacy compile và regression pass; báo cáo phân biệt gate pass với chưa chạy.

## 14. Thứ tự ưu tiên chung

1. Economy transaction result + spend capability + save/receipt tests.
2. Connectivity domain/gateway/recovery tests; native settings triển khai cùng device validation.
3. Not-enough presenter và acquisition contract; Retry presenter.
4. Prefab/sample của hai tính năng và adapter custom Shop sandbox.
5. Clean consumer import, mobile builds/devices, docs/version/release ledger.

Đoạn trên mô tả kế hoạch ban đầu. Kết quả triển khai và các gate thực tế được ghi ở mục 15; chưa phát hành package/tag.


## 15. Kết quả triển khai 2026-10-15

### Source và quyết định API

- Economy source `0.4.0`, Shop source `0.2.0`; gitlink baseline vẫn Economy `22e8a0d`, Shop `0d9e7d7`, UI `1677264`. Thay đổi nằm trong working tree package/submodule, chưa commit/tag/push và không phải release evidence.
- `IResourceTransactionWallet`, request immutable, deficit/result có kiểu; multi-cost/reward normalization, checked arithmetic và canonical SHA-256 fingerprint. `IsNewCommit` tách feedback mới khỏi replay. Legacy wallet không phải implement member mới.
- Datasave wallet giữ một mutation owner, detached candidate, schema 3 và ledger transaction không tự prune. Collision qua namespace strict reward/legacy/spend/exchange. Save exception đọc lại durable state; nếu không reconcile được, trả thêm `PersistenceUncertain` và khóa mọi mutation cho tới khi root recreate/reconcile. Custom store phải đọc durable state khi Load.
- `ResourceActionFlow`: `None/Prompt`, giữ transaction gốc; presentation/recovery không retry và không grant. Presenter refresh đầy đủ deficits, khóa session/navigation trùng, hủy theo owner và tháo listeners. Các interface mới chỉ ở boundary wallet/view/acquisition/navigation.
- `Dreamy.Economy.UI.Integration` giữ async orchestration; `Dreamy.Economy.Recovery.Unity` giữ view/row Unity. Domain Runtime vẫn không dùng Unity, UI, Shop hay UniTask. UPM khai báo UniTask/uGUI trực tiếp và nâng minimum UI lên 1.0.5 cho Presentation API.
- Shop result append enum thiếu tài nguyên, giữ factory cũ và fallback bool. Presenter/installer nhận callback recovery tùy chọn có lifetime token riêng, độc lập success feedback.
- `Dreamy.Shop.UI.Integration`: immutable semantic destination map, gateway match actual reward và loại đường vòng tiêu chính currency thiếu. `IShopResourceNavigator` là điểm host chọn tab/scroll/panel/NPC; không tạo abstraction cho từng class.
- Sample Shop có navigator scroll-list; Resource Recovery sample có Coin/Gem tabs. Sandbox có adapter cho UIShopPanel gem/gold holders và `GameInstaller.ResourceAcquisition` để mở trực tiếp. Các adapter chờ layout, hỗ trợ instance hiện có và kiểm token trước focus.
- Prefab/scene được tạo bằng Unity Editor, không ghép YAML; source sample nằm `Samples~/Resource Recovery`, có scene, row prefab, panel safe area + ScrollRect, formatter/catalog fallback. Demo tạo wallet riêng, grant chỉ qua nút **DEMO**; gateway không giả lập mua hàng.
- Không triển khai auto-resume, pricing/IAP/backend, gameplay journal cho một feature chưa tồn tại hoặc adapter BigNumber/server; mặc định quay lại xác nhận. Host sở hữu các phần này như quyết định thiết kế ban đầu.

### Bằng chứng validation

Unity `6000.4.12f1`, project `dreamy-package-sandbox`.

| Gate | Kết quả |
| --- | --- |
| Managed harness resource recovery | PASS 63 test cases + Shop presenter regression harness; compile source Economy/Shop/integration và Shop sample |
| Unity EditMode Economy/Shop/recovery + DailyReward/Missions/Progression/LuckyWheel | PASS **127/127**, gồm save envelope schema 2 và lỗi ghi file thật; lượt cuối 5.43 giây |
| Compile Editor | PASS sau khi bổ sung direct `Dreamy.UI.Presentation` reference của test assembly |
| Runtime/Editor dependency boundary | PASS kiểm asmdef/source; domain và orchestration assemblies giữ noEngineReferences |
| Resource Recovery sample import qua Package Manager | PASS import mới vào `Assets/Samples/Dreamy Economy Contracts/0.4.0/Resource Recovery`; không Missing Script, row reference đúng, có SafeArea |
| Sample source/import synchronization | PASS code/prefab/scene/asmdef Resource Recovery và Shop source khớp bản import |
| Play Mode sample | PASS balance 0, một/hai deficit, mở Coin tab, quay về, live update đủ resource không tự spend, xác nhận lại mới commit; Console không error trong flow |
| Diff whitespace | PASS repo cha và hai package |
| Harness Settings `validate-settings.py --shop` | BLOCKED bởi mismatch có sẵn `SettingsPlatformGateway.cs` giữa package sample và bản import; giữ nguyên Settings. Shop presenter/model regression được chạy riêng bởi harness mới; native consumer suites chạy độc lập |
| Player managed compile Linux | PASS `PlayerBuildInterface.CompilePlayerScripts`, StandaloneLinux64, **95 assemblies**, không có Editor DLL trong output; `/tmp/dreamy-economy-player-compile`. Không phải full native/IL2CPP build. Warnings có sẵn ở UI tween, CW plugin và duplicate using trong GameInstaller |
| Consumer project độc lập, import riêng/cùng nhau, Android/iOS build/device, SDK billing | CHƯA CHẠY; không tuyên bố production/copy-ready cho mọi consumer |
| Commit/tag/release | CHƯA THỰC HIỆN |

Ảnh Play Mode: [một resource](../evidence/economy-resource-recovery/single-resource.png), [nhiều resource](../evidence/economy-resource-recovery/multi-resource.png).

API/composition/migration và cách dùng sample: `LocalPackages/com.dreamy.feature.economy/Documentation~/resource-recovery.md`.

```sh
python3 LocalPackages/com.dreamy.feature.economy/Tests~/validate-resource-recovery.py
```

E0–E4 đã có source và behavior tests. E5 đã có prefab/scene, import sandbox và Play Mode evidence. E6 đã có custom consumer adapter, metadata/docs; các gate external consumer/full build/mobile device và release còn được liệt kê riêng ở bảng trên. Không dùng dirty submodule như bằng chứng cho package đã phát hành.


## 16. Shop shortage popup và prefab variant — 2026-10-16

Sửa thiếu sót của lượt trước: ShopPresenter có callback recovery nhưng GameInstaller chưa cấp callback; prefab shortage vẫn là Canvas độc lập nên không theo panel lifecycle chung.

- `NotEnoughResourcesPanel` hiện kế thừa `UIPanel`, layer Popup, có cache; mở/đóng bằng Show/Hide, Back qua PanelManager. Khi mở acquisition, popup tạm ra khỏi Back stack, trả input về Shop; khi quay về mới khôi phục.
- `NotEnoughResourcesPanel.prefab` là **Prefab Variant** của `Packages/com.dreamy.feature/Runtime/Prefabs/BaseFeaturePanel.prefab`, kế thừa SafeArea/backdrop/tween. Không còn Canvas riêng. Unity xác minh GUID và local script ID giữ nguyên; row/status/cancel/canvasGroup/tween references đủ.
- `ResourceRecoveryPanelPresentation` chịu trách nhiệm create/reuse popup, session presenter và chờ hide kết thúc; dùng được cho flow ngoài Shop. Economy thêm dependency `com.dreamy.feature` 0.2.0 cho base prefab; Unity integration khai báo Core/UniTask trực tiếp.
- `GameInstaller.RecoverShopResourcesAsync` được truyền vào ShopFeatureInstaller. BootstrapScene có direct serialized reference đến variant, không load prefab bằng tên/Find. Wallet không bị tạo mới trong UI.
- Shop acquisition dùng catalog thực và navigator của **ShopPanel hiện tại**; “Get more” không spawn Shop thứ hai. Purchase lock cho phép mua offer khác trong lúc bổ sung; khóa duplicate offer gốc và chặn kết quả recovery cũ ghi đè purchase mới.
- Scene/Editor builder của Resource Recovery sample được cập nhật để dùng PanelManager trên Canvas chủ. Code/asmdef, variant shortage và scene demo được đồng bộ giữa source sample/bản import. Giữ nguyên các chỉnh sửa consumer có sẵn ở DeficitRow.prefab, ShopPanel.prefab và Connectivity.

Validation ngày 2026-10-16:

| Gate | Kết quả |
| --- | --- |
| Managed harness Economy/Shop/recovery | PASS **64 test cases** + ShopPresenter regression harness |
| Unity EditMode Shop.Runtime.Tests + Shop.Recovery.Tests | PASS **15/15** |
| Unity compile | `scriptCompilationFailed=false`, không có Console error trong flow kiểm tra |
| Variant inspection | Variant đúng BaseFeaturePanel; 1 SafeArea, không Canvas root, không thiếu refs; giữ GUID/local script ID |
| Shop thật qua Bootstrap/MainScene | Bấm PurchaseButton của gem-to-coin-exchange khi Gem=1/cost=15: mở shortage Popup, Required=15, Available=1, Missing=14. Return không trừ tiền |
| “Get more” | Popup alpha=0, top stack=ShopPanel, Shop count=1, 17 purchase buttons interactable; status “Choose a resource pack”, Gem vẫn=1. Chỉ điều hướng, không mua |
| Sample dùng chung UIPanel | Prompt xuất hiện ở Layer Popup; Return và mở lại dùng cùng cached instance; Back chuẩn của PanelManager đóng popup và loại khỏi stack |
| Android/iOS và consumer độc lập | Không chạy thêm trong lượt sửa này |

Ảnh Shop mở shortage variant: [shop-not-enough-variant.png](../evidence/economy-resource-recovery/shop-not-enough-variant.png). Log managed: `docs/evidence/economy-resource-recovery/shop-shortage-managed.log`.
