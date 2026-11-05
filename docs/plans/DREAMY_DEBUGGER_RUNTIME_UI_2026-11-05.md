# Runtime UI — hoàn tất yêu cầu ngày 2026-11-05

Phạm vi này đã triển khai và nghiệm thu trong Unity Editor 6000.4.12f1. Không đánh dấu toàn bộ plan production trước đó là hoàn tất.

## Hành vi hiện tại

- Giữ theme đỏ trên nền charcoal. Một logo Dreamy hiện ở góc trái: trên sidebar khi rail mở, trên header khi rail đóng. Hai vị trí luân phiên, không hiện đồng thời; không bật About/author/vendor UI.
- Report Bug bị loại khỏi danh sách tab runtime; fake-report shortcut cũng không còn trong runtime cheats. Capture/outbox/relay API và các action kiểm thử trong Editor vẫn giữ. Gmail tiếp tục pending.
- Nút ba gạch toggle được ở cả màn dọc và ngang. Chọn tab đóng rail. Animation dùng unscaled time, hoạt động khi game pause. Vùng nội dung co theo độ mở để pin/close không bị đẩy ra ngoài màn hình. Không còn overlay vô hình phủ lên nút ba gạch.
- HUD mặc định **Off** trong class config, installer, config sandbox và production sample đã import.
- Bật HUD tại `Options → Debugger → Performance HUD`: FpsOnly / Compact / Detailed. Vị trí preset vẫn chọn qua `HUD position`; `Reset HUD position` trả về preset ban đầu.
- Kéo HUD bằng tay nắm `::` ở góc trên phải. Khi kéo chuyển thành Free; vị trí được giữ trong session, không ghi vào asset authoring. Kéo và xoay/đổi kích thước màn hình được giới hạn trong safe area. Text, background và graphs không nhận raycast; chỉ grip chặn touch.
- FPS layout gọn, có typography/metrics/graph, không giữ khoảng trống cho RAM/audio đang tắt. Thêm mức legibility khi canvas Graphy bị thu quá nhỏ; scale authored và giới hạn safe area vẫn được áp dụng.

## Phần Graphy thực sự tái sử dụng

Nguồn: https://github.com/Tayx94/graphy/tree/1068f3de2bc9f1e4a905150abd59f31e6dead679

- GraphMobile shader và G_GraphShader helper; chỉ cô lập shader name và namespace để không xung đột với Graphy được import riêng.
- Roboto Regular/Bold và license Apache 2.0 đi kèm. MIT notice của Graphy được giữ.
- `GraphyHudGraph` cấp samples từ profiler hiện có cho renderer; không tạo Graphy singleton hay sampler FPS thứ hai, không thêm dependency Input System/Core/UI package.
- Graph refresh tối đa 10 Hz, text 4 Hz, RAM 1 Hz. Material được sở hữu bởi từng HUD, giải phóng khi destroy. Chưa có số đo CPU/GPU/GC trên thiết bị thật; không quảng bá thành performance acceptance.

## Lỗi đã quan sát và sửa

1. Config project/sample + installer bật Compact mặc định dù field mặc định Off: đã đổi cả ba owner.
2. Nút menu chỉ gọi Open, controller bị tắt ở màn ngang: đổi sang Toggle và không để breakpoint disable controller.
3. Rail mở đẩy close/pin ra ngoài viewport: content thu chiều rộng tương ứng và giữ cạnh phải.
4. Màn nhỏ làm chữ HUD quá nhỏ: thêm giới hạn legibility tại owner của HUD.
5. `OptionsPresentationBridge.Refresh` có thể chạy khi service presentation đã unavailable trong teardown: kiểm service trước khi cập nhật container. Regression dispose lease sau service quit đã pass.

## Nghiệm thu

Receipt: `docs/evidence/debugger/runtime-ui/verification.json`, `EditMode.xml`, `PlayMode.xml`.

- Consumer riêng chỉ có Dreamy Debugger và Unity Test Framework: **72 EditMode + 21 PlayMode pass**, 0 failed/skipped.
- Burger được hit qua EventSystem raycast ở cả lượt mở và đóng; test không chỉ gọi trực tiếp onClick. Test `timeScale=0`, nút close trong màn hình, một logo active, không Report Bug/fake shortcut, HUD mặc định Off, drag Free và clamp corners, reset và collector lifecycle.
- 28 prefab không Missing Script; không đổi GUID meta có sẵn so với snapshot trước lượt.
- Ảnh Editor: `drawer-open.png`, `portrait.png`, `hud-phone.png`, `hud-dragged.png`, `hud-tablet.png`. Dữ liệu resource là fixture synthetic; các counter FPS/RAM là của Editor và chịu ảnh hưởng khi MCP dừng frame, không phải benchmark của game/điện thoại.
- Main Editor xác nhận shader `Dreamy Debugger/Graphy Mobile` supported và render được. Không claim pixel-perfect SR/Graphy ở mọi thiết bị.
- Scene Bootstrap được mở lại, không dirty; layout/Simulator, lựa chọn Game view và Enter Play Mode options được khôi phục; các resolution test tạm đã xóa. Console cuối 0 error.
- Không gửi Telegram/email, không flush outbox, không sửa SR gốc hay project Unity khác. Không commit/push các thay đổi có trước.

## Giới hạn còn mở

Android/iOS touch, notch/orientation và profiling thiết bị thật; build/stripping IL2CPP; full settings parity và các gate privacy/production trong plan cũ vẫn chưa được nghiệm thu. Khi hot-reload code trong Play Mode làm service cũ bị cleanup, phải dùng fixture mới; lần capture đã dùng domain reload đầy đủ tạm thời và khôi phục setting sau đó. Không coi lượt này là xác nhận hot-reload/domain-reload-off parity toàn bộ.
