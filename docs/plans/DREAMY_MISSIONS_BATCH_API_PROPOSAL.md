# Đề xuất upstream: Missions batch progress API

Ngày rà soát: 2026-11-17. Trạng thái: đã triển khai và kiểm thử source candidate trên v0.3.1; chưa phát hành hoặc migrate consumer. Xem [báo cáo thực thi](../evidence/missions-batch-api-2026-11-17/README.md).

## Phạm vi và bằng chứng

- Repository nguồn: `https://github.com/Dreamy-Game-Foundation/com.dreamy.feature.missions.git`.
- Base của draft được yêu cầu: `1e1b4b57b738a83d7325ee229a80852c397c739e`; `package.json` tại commit này xác nhận v0.2.0.
- Source submodule đang có trong workspace: `LocalPackages/com.dreamy.feature.missions`, HEAD `2474d139cd2c9ca3d1667296be1cd30d579fb7bd`, v0.3.1, working tree sạch lúc kiểm tra. Sandbox hiện dùng local package này; đây không phải bằng chứng consumer được mô tả đã đổi khỏi pinned v0.2.0.
- Đã tìm thấy draft tại `/run/media/tutrinh/NewData/planet-bunker-tu-code/docs/patches/missions-batch-api.patch` khi mở rộng tìm kiếm ngoài cây `Unity Project`. Patch được review, không apply: nó trình bày MissionModel như một file mới, gọi AdvanceWindow có side effect commit, xóa ID khỏi dedupe set khi eviction và vẫn dùng JSON clone. Bản triển khai mới dựa trên source v0.3.1, giữ reward-presentation API đã có.
- Workflow không có tại đường dẫn sandbox được dẫn. Đã đọc workflow của consumer tìm thấy và catalog `/run/media/tutrinh/NewData/dreamy-codex-toolkit/compatibility/dreamy-packages.json`; catalog chưa có record Missions. Recall không có lesson phù hợp. Runtime bằng chứng mới nằm trong báo cáo thực thi, chưa phải compatibility record của release.
- Code thay đổi chỉ trong source submodule `LocalPackages/com.dreamy.feature.missions`. Không sửa immutable `Packages`, `Library/PackageCache`, manifest, version hoặc changelog. Consumer adapter và pin v0.2.0 chưa đổi.

## Các điểm cần sửa trong source

`Runtime/Domain/MissionModel.cs` hiện có các đặc điểm cần giữ hoặc xử lý:

1. `Report` kiểm tra Busy trước validation, gọi `AdvanceWindow`, kiểm tra window, mã hóa ID bằng `RewardIdentity.Encode("mission-event", namespace, windowId, originalId)`, rồi mới kiểm tra matching/distinct.
2. `AdvanceWindow` tự clone, commit và thông báo. Gọi nguyên hàm này từ batch rồi commit progress sẽ tạo hai transaction/callback tại rollover, trái yêu cầu batch.
3. `Clone`/`Copy` dùng JSON round-trip; `Commit` còn clone thêm trước `datasave.Save`. Batch chỉ giảm số lần gọi chưa giải quyết toàn bộ allocation.
4. `Commit` save trước khi gán `save = next`, đã bảo vệ publication khi save lỗi trước write. Lỗi sau write vẫn có thể làm model giữ snapshot cũ trong khi storage đã có receipt mới.
5. Receipt Permanent bị loại theo `historyLimit`. Test `HistoryLimit_EvictsOldEventsWithExplicitBoundedSemantics` chủ động cho phép ID đã bị loại được tính lại.

## Public API đề xuất

```csharp
public readonly struct MissionProgressEvent
{
    public MissionProgressEvent(string eventKey, string originalId,
        string eventNamespace, string windowId,
        long amount = 1, string distinctValue = null);

    public string EventKey { get; }
    public string OriginalId { get; }
    public string EventNamespace { get; }
    public string WindowId { get; }
    public long Amount { get; }
    public string DistinctValue { get; }
}

public interface IMissionBatchProgress
{
    IReadOnlyList<MissionProgressStatus> ReportProgressBatch(
        IReadOnlyList<MissionProgressEvent> events);
}
```

Đây là phác thảo chữ ký; phần triển khai đầy đủ đã compile trong source candidate. `MissionModel` triển khai capability mới; không thêm member bắt buộc vào `IMissionService`. Giữ hai overload single-event và quy tắc legacy Permanent/raw ID. Dùng chung evaluator nội bộ khi phù hợp, nhưng không chuyển legacy ID thành namespaced ID.

DTO không có setter hay tham chiếu mutable. `WindowId` là window đã capture tại lúc phát sinh event, không tự lấy lại lúc flush. Constructor chỉ giữ dữ liệu; validation tại model trả `InvalidEvent`, kể cả `default(MissionProgressEvent)`. List null ném `ArgumentNullException`; list rỗng trả kết quả rỗng, không refresh/save/callback. Kết quả có đúng một status cho mỗi input, cùng thứ tự; không trả array mutable dùng chung với model.

## Một transaction cho cả batch

1. Giữ synchronous main-thread ownership. Khi Busy, trả Busy cho từng input và không đọc clock hoặc ghi save. Giữ Busy xuyên suốt save và callback, giải phóng bằng `finally`.
2. Với batch không rỗng, xác định input hợp lệ theo cùng thứ tự ưu tiên validation của single-event. Nếu tất cả đều invalid, không refresh. Với input hợp lệ, đọc clock một lần và xác định rollover bằng high-water mark đã persist; rollback không mở lại window cũ.
3. Tách hàm chuẩn bị rollover trên candidate khỏi wrapper `Refresh()` đang commit. Chuẩn bị tối đa một detached candidate; không gọi public `Refresh`/`ReportProgress` trong vòng lặp. Rollover và progress cùng thuộc transaction này.
4. Tạo `HashSet<string>(..., StringComparer.Ordinal)` từ receipt của window candidate. Với từng input hợp lệ: WrongWindow trước dedupe; mã hóa đúng identity hiện tại; kiểm tra receipt; tìm mission được chọn có cùng event key; kiểm tra distinct bắt buộc; áp dụng lên candidate.
5. Chỉ thêm ID vào receipt/set khi input được chấp nhận theo contract hiện tại. Input invalid, wrong-window hoặc không matching không chiếm ID. Nếu matching mission đều completed/claimed, vẫn ghi receipt và trả Updated như single-event hiện tại, nhưng không tăng progress. Distinct đã có cũng giữ hành vi receipt này.
6. Additive dùng `progress += Math.Min(amount, target - progress)` sau khi xác nhận progress chưa đạt target; không cộng trước rồi clamp. Distinct chỉ thêm giá trị chưa có theo ordinal, tăng một và dừng ở target. Một event có thể tác động nhiều mission; validation distinct phải hoàn tất trước khi thay đổi bất kỳ mission nào.
7. Khi Permanent vượt giới hạn, loại receipt cũ theo thứ tự như hiện tại, nhưng **không xóa ID khỏi set của batch**. ID đã thấy ở đầu batch vẫn phải bị chặn dù vừa bị eviction. Không giữ receipt bị loại sang batch sau bằng cache ngầm.
8. Nếu candidate thay đổi vì progress hoặc rollover: save đúng một lần, publish sau save thành công, rồi StateChanged đúng một lần. Nếu toàn bộ input bị từ chối và không rollover: không save/callback. Rollover có thể vẫn commit một lần dù mọi event hợp lệ đều trỏ window cũ.
9. Status Updated chỉ được trả sau commit thành công. Save lỗi làm lời gọi thất bại bằng exception, không trả một danh sách thành công dở dang. Lỗi subscriber xảy ra sau publication phải được phân biệt với lỗi persistence; không rollback progress đã commit.

So sánh sequential/batch chỉ yêu cầu cùng state và status tại clock cố định, cùng catalog, random seed và cùng retention horizon. Không yêu cầu cùng số callback/commit; batch cố ý gom lại. Batch dài vượt historyLimit cố ý chặn lặp trong cả batch mạnh hơn các single call vốn có thể quên receipt vừa eviction.

## Reload/retry và giới hạn chống trùng

Đề xuất an toàn ban đầu: nếu `Save` ném lỗi mà storage không chứng minh được chưa write, đánh dấu instance là cần reload. Chặn mọi mutation tiếp theo, gồm progress, claim, recovery, reroll và refresh, bằng lỗi recovery rõ ràng; không dùng Busy để che trạng thái này. Read chỉ cung cấp snapshot đã publish và phải được mô tả là có thể cũ. Host hủy instance, reload từ persistence có read-after-write đáng tin cậy, rồi retry **nguyên original IDs, namespace và captured window**. Nếu reload không đáng tin cậy hoặc lỗi, giữ trạng thái cần recovery.

Nếu storage đã ghi cả candidate, receipt khiến retry không tăng lần hai; nếu chưa ghi, retry áp dụng cả transaction. Không tách receipt ra khỏi progress thành save riêng. Constructor/recovery phải kiểm tra cả lỗi write của chính nó trước khi host dùng model. Callback ném lỗi sau publication không làm instance cần reload vì save đã được xác nhận; retry cùng IDs vẫn an toàn trong retention horizon. Contract này cần public exception/documentation rõ ràng và tests chứng minh, không chỉ dựa vào host tự invalidate.

**Điểm phải chốt trước release:** “replay mọi committed ID không bao giờ double-count” không thể bảo đảm vô hạn đồng thời giữ receipt Permanent hữu hạn cho arbitrary IDs. Phương án tương thích là bảo đảm trong receipt còn được giữ và toàn bộ batch hiện tại; ngoài horizon vẫn giữ semantics eviction đã phát hành. Nếu sản phẩm yêu cầu exactly-once vô hạn, cần thiết kế durable dedupe ledger hoặc sequence protocol riêng, retention/migration rõ ràng và benchmark bổ sung. Không quảng cáo bảo đảm vô hạn bằng một temporary HashSet.

## Detached copy thay JSON trong hot path

Thêm copier nội bộ được audit theo schema, không dùng serialize/deserialize cho progress candidate hoặc snapshot đưa vào Save:

- Copy dictionary missions, từng `MissionProgressData`, list distinct values, list processed IDs, selected IDs, window và các scalar revision/ticks/reroll.
- Copy period/selection policies vì chúng có public setters. Không chia sẻ mutable policy giữa candidate, snapshot persistence và model đang publish.
- Audit definitions và toàn bộ graph reward bundle trước khi chia sẻ; private setter không tự bảo đảm graph immutable. Giữ nguyên snapshot catalog/revision của window, không thay bằng catalog hiện tại.
- Giữ toàn bộ PendingClaims, WindowSnapshot, Bundle và RewardJournal, gồm state, request identity, fingerprint và các delivery receipt bên trong. Copy sâu mọi mutable node; chỉ chia sẻ node đã chứng minh immutable. Reward journaling tiếp tục là nguồn quyết định claim/recovery.
- Giữ trường legacy `Missions`/`ProcessedEventIds` và alias có chủ ý với active window trong từng graph riêng; không tạo alias xuyên transaction. Không tăng save schema chỉ vì thay copier.
- Audit concrete SaveData/dependency schema tại source release thực tế; thêm test sentinel cho mọi field và mutation-isolation hai chiều, kể cả store giữ reference hoặc sửa object được truyền vào Save. Constructor/load và claim phải không mất journal sau refactor.

## Regression bắt buộc

| Nhóm | Bằng chứng cần có |
| --- | --- |
| Sequential/batch | Cùng progress, claimed, distinct, ordered receipts, policies, definitions và pending journals; mixed statuses giữ đúng thứ tự |
| Identity | Duplicate trong batch, khác batch, sau reload; namespace/window khác; raw legacy ID và encoded ID không nhập nhằng; ký tự phân cách và Unicode |
| Retention | historyLimit nhỏ, batch lớn hơn limit, duplicate đầu/cuối batch; sau eviction giữ đúng bounded semantics đã công bố |
| Counters | amount 0/âm/long.MaxValue, target long.MaxValue, gần cap, nhiều matching missions, completed/claimed không tăng |
| Distinct | Trùng value khác ID, trùng ID khác payload, value null/blank, mixed additive/distinct cùng key, dừng lưu values khi cap |
| Window | WrongWindow, exact boundary, rollover kèm progress chỉ một save/event, rollover với toàn wrong-window, clock rollback, weekly/offset/cutoff, empty/all-invalid không refresh |
| Reentry | Save và StateChanged gọi lại progress/batch/claim/refresh đều Busy; callback đọc state sau commit; callback ném lỗi không làm Busy bị kẹt |
| Persistence | Throw trước write, write rồi throw, reload lỗi, retry cùng IDs; không publication/callback khi Save thất bại; chặn mutation trên instance cần reload |
| Copy/journal | Candidate và store snapshot tách biệt; đủ fields; pending/completed claims qua rollover/reload; claim/reward recovery không grant lần hai |
| Consumer | Quest và XP partial-save regressions vẫn pass khi mission đã commit nhưng Quest/XP save lỗi và host retry |

## Đo allocation và số commit

Chạy trên source release đã chọn, ghi Unity/runtime/backend, dependency commits, fixture, seed, warm-up và phương pháp đo. Dùng cùng save ban đầu cho hai nhánh sequential và batch; loại constructor/load/warm-up khỏi vùng đo hot transaction và báo riêng nếu cần.

- Receipt journal 4.096 và 50.000 IDs (Periodic hoặc Permanent có historyLimit tương ứng), kèm distinct values và pending claim journal đủ lớn để lộ chi phí copy.
- Workload 30, 60, 100 shots; công bố rõ số event/shot, chạy cả một event/shot và nhiều event/shot như fire/hit/kill. Bao gồm duplicate, no-match và rollover riêng.
- Đo allocated bytes/operation bằng công cụ phù hợp runtime, thời gian với nhiều lần lặp, Save calls, StateChanged calls và payload persist. Đo fake store để tách copier, sau đó real Datasave để thấy chi phí serializer còn lại.
- Với tất cả event mới, không rollover: sequential có N progress commits; batch có đúng 1. Duplicate-only không rollover: 0. Rollover kèm batch: đúng 1. Không tính constructor/claim recovery commits vào các số này.
- Công bố số đo thô và tỷ lệ bytes/time so với baseline; chưa đặt con số cải thiện khi chưa đo. JSON có thể vẫn thuộc persistence serializer; mục tiêu là bỏ JSON round-trip copy trong hot model transaction.

## Trình tự triển khai và phát hành

1. Nhận/review draft patch, chọn source branch và dependency compatibility; đối chiếu drift từ base v0.2.0 tới release hiện tại. Chốt retention và reload contract trước code.
2. Viết regressions cho transaction/failure, triển khai copier và batch evaluator; giữ single-event APIs và external adapter compile fixture.
3. Chạy focused Runtime/EditMode tests và reward-presentation/claim recovery suites liên quan, benchmark và kiểm tra diff/asmdef/serialized compatibility. Lưu receipts có source commit và kết quả thực tế.
4. Chỉ sau khi gates pass mới cập nhật changelog/version trong upstream, phát hành tag đã validate. API additive thường phù hợp minor version, nhưng chọn version theo source release thực tế; không giả định v0.2.1 hay hạ consumer hiện tại xuống v0.2.0.
5. Consumer được mô tả chỉ đổi pin sau khi release khả dụng: thay thân `MissionProgressBatch.Commit` bằng capability upstream, giữ original IDs/captured window, thay host invalidation bằng reload contract đã công bố. Chạy lại Quest/XP partial-save regressions trước khi bỏ adapter fallback.

Checkpoint: source patch review, implementation, 90/90 package EditMode regressions, 30/30 minimum-dependency tests, 103/103 isolated migrated-consumer tests và benchmark đã hoàn tất cho source candidate. Upstream release/tag, consumer pin/migration, real consumer pin/adoption và device profiling còn là gate phát hành riêng; không tuyên bố production release đã sẵn sàng.
