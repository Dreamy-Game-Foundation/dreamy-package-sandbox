# Dreamy Debugger — đề xuất cơ chế runtime tiếp theo

Ngày 2026-11-05. Baseline: package commit `8a0e19c`, version `0.2.0-preview.6`. Đây là đề xuất, chưa triển khai các cơ chế bên dưới. Dựa trên source/audit/receipts của package hiện tại, không giả định parity đầy đủ với SR hay Graphy. Gmail tiếp tục pending.

## Ưu tiên thực hiện

| Ưu tiên | Cơ chế | Mục đích và phạm vi |
|---|---|---|
| P0 trước production report | Chính sách thu thập có thể thay đổi khi đang chạy | Host cấp quyền riêng cho log/screenshot/upload; disable/cancel và xử lý outbox khi quyền thay đổi; suppression cho màn nhạy cảm; masking qua adapter. Không coi PIN debug hoặc AllowScreenshots là consent. |
| P0 trước phát hành game | Build policy và công tắc tắt khẩn cấp | Release mặc định không có cheat UI. Allowlist khả năng theo build; công tắc tắt được host cấp, fail closed và có hạn dùng nếu dùng remote config. PIN local không cấp quyền server; không thêm remote execution. |
| P1 | Breadcrumb / flight recorder nhẹ | Ring buffer sự kiện có cấu trúc: scene, state, màn UI, thao tác cheat và mốc load. Khi lỗi, đính kèm lịch sử gần nhất để biết điều gì dẫn tới lỗi. Không ghi input cá nhân hay chụp ảnh liên tục mặc định. |
| P1 | Phát hiện hitch và xu hướng hiệu năng | Theo dõi frame-time spike, p95/p99, GC delta, memory trend và khoảng loading do host đánh dấu. Gộp cảnh báo, cooldown, warmup; đóng băng snapshot khi vượt ngưỡng. Tách số liệu gần khỏi lịch sử dài; không gọi suy đoán leak là leak đã xác nhận. |
| P1 | Trang sức khỏe debugger | Cho biết module đang bật, lý do capture bị bỏ qua, pending/dead reports, lần relay chấp nhận gần nhất, trạng thái policy và chi phí sampler. Không hiển thị token. Phân biệt saved locally / relay accepted / destination delivered nếu có receipt từ relay. |
| P1 | Tìm kiếm và thao tác cheat nhanh | Search theo tên/category, collapse nhóm, favorites/session preset. Card tài nguyên gom balance + amount + Add; No Ads gom owned + Grant. Metadata group/width tùy chọn; giữ fallback control chuẩn và không buộc mỗi option một dòng. |
| P2 | Network diagnostics qua adapter | Request ID, endpoint đã lọc, status, duration, retry và loại kết nối. Không hook mọi HTTP client bằng reflection; host tích hợp tại network layer. Không thu header Authorization/cookie/body mặc định. Fault injection chỉ internal build và endpoint allowlist. |
| P2 | Save/config inspector có kiểm soát | Snapshot read-only của game state, config version và save schema. Mọi reset/grant đi qua API game, có confirm, audit và backup/transaction nếu game hỗ trợ. Không có arbitrary reflection setter lên toàn bộ object graph. |
| P2 | Snapshot và so sánh hai thời điểm | So sánh context, state, memory và module status trước/sau một hành động. Dữ liệu có schema, size cap và redaction; save/export theo yêu cầu. Không duyệt toàn bộ heap hay gọi getter tùy ý mỗi frame. |
| P2 | Tái hiện tình huống test | Time scale, pause/step theo khả năng host, seed RNG của game, giả lập offline/latency. Mỗi override có lease và tự restore khi đóng scope, relock hoặc rời scene. Không reset trạng thái người dùng đang chơi khi chưa xác nhận. |

## Thiết kế flight recorder nên làm trước

- Core event: monotonic timestamp, sequence, session ID, category, event ID và payload đã giới hạn. Tách thời gian monotonic dùng tính duration khỏi UTC dùng đối chiếu report.
- Bắt đầu với ring 256–512 sự kiện và giới hạn tổng byte; đây là cấu hình đề xuất, cần đo trên thiết bị. Loại sự kiện liên tiếp trùng và áp dụng quota theo producer.
- Host chủ động ghi scene/state/load milestones qua API typed; adapter Core/UI/Assets có thể thêm sau, không kéo các package đó thành dependency runtime bắt buộc.
- Khi có error/hitch/manual report, sao chép snapshot bounded để ghi file/encode ở worker; không serialize toàn bộ lịch sử trong threaded log callback.
- Dùng session/report/request correlation IDs do debugger/host cấp; không dùng advertising ID hoặc account ID mặc định.
- Breadcrumb lưu thao tác cheat và kết quả đã lọc, không lưu PIN, token, nội dung chat hay payload mua hàng.

## Quy tắc chi phí và vòng đời

1. Mỗi module có trạng thái Off / Background / Visible rõ ràng. Off không có timer, recorder hay allocation lặp lại của module đó. Background chỉ có khi host chủ động cho phép; ẩn UI không tự bật thu thập.
2. Shared sampler, fixed-capacity buffers, backpressure và dropped-count; cập nhật UI có nhịp. Batching registration trước khi tính chuyện pooling; profile 20/200/1000 options để quyết định tối ưu structural rebuild.
3. Không tìm object toàn scene, reflection scan, format chuỗi lớn hoặc rebuild layout trong Update. Cache capability và reference có owner; invalidation khi service generation đổi.
4. Screenshot là single-flight có memory cap, fallback text; không giữ GPU resource qua một capture mới khi request cũ chưa kết thúc. Không tự tăng tần suất chụp khi có lỗi dồn dập.
5. Module/cheat registration có IDisposable hoặc cancellation gắn owner; teardown không tạo service mới. Override game phải restore cả khi exception, scene unload và access relock.
6. Giữ đỏ làm brand, semantic colors cho metric. UI title/value tách cột; safe area, scale, drag, multi-touch và back/ESC phải có nghiệm thu.
7. Đo p95/p99 CPU/GC/VRAM, pin/nhiệt trên thiết bị thật trước khi đặt budget chính thức. Editor screenshot và unit test không chứng minh không lag.

## Phân pha và điều kiện đạt

### Phase 1 — ổn định và biết chuyện gì xảy ra

- Flight recorder + hitch detector + trang health; bổ sung lifecycle enter/exit Play 2×2, domain reload, focus/scene, nhiều pointer/cancel.
- Test deterministic: ring wrap, cap, dedupe, timestamp, policy off, report snapshot không đổi sau khi buffer tiếp tục chạy; hitch warmup/loading suppression/cooldown.
- Device test: HUD Off/Background/Visible, burst log và capture trên máy yếu. Không tạo crash trong scene người dùng.

### Phase 2 — cheat tiện dùng cho từng game

- Search/collapse/favorite và inline group metadata; sample card Resource/Ownership; API game-owned giữ nguyên quyền và disposal.
- Test Add/Confirm cập nhật đúng balance/status/owned; relock ngăn thao tác; unregister không để stale getter; nhiều options không gây rebuild theo từng value.
- Nghiệm thu portrait/landscape/notch, keyboard/IME, controller nếu hỗ trợ; so sánh profiler trước/sau với cùng dataset.

### Phase 3 — diagnostics production và adapter tùy chọn

- Hoàn thiện policy thu thập, withdrawal/cancel/purge, host masking; remote HTTPS relay và health/destination receipt; network/save adapters theo nhu cầu game.
- Test policy đổi trong lúc chờ frame/readback/encode/persist/upload, offline/restart, quota/disk full, low memory và late callback. Định nghĩa rõ hành vi với bundle đã relay-accepted; client purge không tự xóa được bản ở destination.
- Build và chạy thật Android IL2CPP; iOS trên Mac/device riêng. Nghiệm thu native crash/ANR/OOM bằng test build chuyên dụng, không hứa chụp mới khi process đã chết.

## Giữ ngoài scope mặc định

Arbitrary C# evaluation, remote shell/command execution, continuous screenshot/video recorder, arbitrary save/object editing, audio FFT/VR/XR và một generic rule editor lớn. Chỉ thêm khi có nhu cầu đã đo/kiểm chứng, owner rõ và gate phù hợp. Package chính tiếp tục độc lập; adapter là nơi nối Core/UI/Assets/Networking của từng game.

Đề xuất thứ tự gần nhất: **flight recorder → hitch detector + health → search/inline cheat → production policy + adapters**. Nếu bắt đầu rollout production report, hoàn tất P0 policy/build gates trước khi bật thu thập hoặc upload.
