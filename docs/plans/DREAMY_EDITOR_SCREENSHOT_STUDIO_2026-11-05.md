# Dreamy Editor Tools — Game View Screenshot

Ngày: 2026-11-05. Trạng thái: **đã triển khai; compile/logic và GUI Ubuntu đã qua, Windows native paste chưa có host xác minh**.

## 1. Phạm vi đã chốt

Thêm chức năng **Game View Screenshot** vào `com.dreamy.editor-tools`, dùng trong Unity Editor trên Windows và Ubuntu. Chụp frame game hoàn chỉnh, gồm UI đã render, ở độ phân giải Game View thực tế.

Chỉ có một nguồn Game View. Bỏ desktop, cửa sổ hệ điều hành, Scene View/Scene Camera và mọi backend GDI, Windows.Graphics.Capture, D-Bus hoặc Screenshot Portal khỏi kế hoạch. Không cần bộ chọn nguồn hay quyền chụp desktop.

Bản đầu hỗ trợ ba chế độ **Copy**, **Save**, **Copy + Save**, cùng đếm ngược và preview. Copy là sao chép ảnh vào clipboard, không phải sao chép đường dẫn. Crop, history, annotation, burst và video chưa thuộc bản đầu.

## 2. Hiện trạng đã kiểm tra trong lượt nghiên cứu

| Mục | Bằng chứng tại workspace |
| --- | --- |
| Unity | `ProjectSettings/ProjectVersion.txt`: 6000.4.12f1 |
| Package | `LocalPackages/com.dreamy.editor-tools/package.json`: 1.0.1; HEAD lúc nghiên cứu `9872f81ac6e8a61676c3667feaf05a006f72bc71` |
| Cách cài | Manifest và lock trỏ local package; Newtonsoft khai báo 3.2.1, manifest project dùng 3.2.2 |
| Assembly | `Dreamy.EditorTools.Editor`, chỉ Editor; reference `Unity.Newtonsoft.Json` |
| Công cụ có sẵn | Scene, Build, Package, Data Debugger, menu và ShortcutManager; chưa có screenshot module |
| Compatibility | Catalog tại `../dreamy-template-project/compatibility/dreamy-packages.json` ghi source-verified đúng version/commit trên; chưa chứng minh screenshot |
| Unity tối thiểu | package.json ghi 6000.0 nhưng README/toolbar hiện yêu cầu 6000.4; lấy 6000.4 làm baseline, làm rõ khai báo khi release |
| Máy nghiên cứu | Ubuntu 26.04.1 LTS, session Wayland; chưa chạy thử capture |

Không tìm được `docs/agents/dreamy-workflow.md`, executable `dreamy` trong PATH hoặc file lesson trong `.dreamy/recall` ở lượt nghiên cứu. Chưa chạy được native lesson recall/checkpoint/harness; tài liệu này là checkpoint thủ công, không phải receipt triển khai thành công.

## 3. Hướng kỹ thuật

Dùng **một đường chụp Unity chung cho Windows và Ubuntu**, ưu tiên `ScreenCapture.CaptureScreenshotAsTexture`. API này chụp Game View và cần được gọi sau khi render hoàn tất. [Unity 6.4 ScreenCapture](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/ScreenCapture.CaptureScreenshotAsTexture.html)

`WaitForEndOfFrame` có thể ngừng tiếp tục khi chuyển sang Scene View và không chạy trong Editor batch mode. Vì vậy cần timeout realtime và đường hủy độc lập với coroutine chờ frame. [Unity 6.4 WaitForEndOfFrame](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/WaitForEndOfFrame.html)

Đây là thiết kế đề xuất, chưa phải API Dreamy đã có hoặc kết quả chụp đã xác minh. Cùng dùng Unity API không thay thế việc chạy kiểm thử trên cả hai OS.

## 4. Chức năng bản đầu

| Chức năng | Hành vi |
| --- | --- |
| Chụp | Một frame Game View trong Play Mode đang render; x1 |
| Đếm ngược | 0 / 3 / 5 giây; Cancel; không phụ thuộc timeScale |
| Độ phân giải | Theo Game View hiện tại; ghi kích thước thực tế; không tự đổi resolution/aspect |
| Chế độ | Copy / Save / Copy + Save; mặc định Copy; nhớ lựa chọn theo project |
| Định dạng | Ảnh SDR trong bộ nhớ; Save xuất PNG, tên không trùng, không ghi đè |
| Preview | Ảnh vừa chụp, Fit/100%, kích thước; đường dẫn chỉ khi đã lưu |
| Thao tác sau chụp | Copy Image hoặc Save As từ preview mà không cần chụp lại |
| Thư mục | Chỉ cần cho chế độ có Save; Open Image, Copy Path, Reveal Folder chỉ bật khi có file |
| Menu | Mở cửa sổ; Capture Game View; Open Capture Folder |
| Shortcut | ShortcutManager, mặc định chưa gán để tránh xung đột |

Menu gốc: `Tools/Dreamy/Capture/`. Cửa sổ tên **Game View Screenshot**. Shortcut và menu chụp nhanh dùng thiết lập delay/output đã lưu, không cần mở cửa sổ.

Bản đầu yêu cầu Play Mode, không pause và Game View đang render. Nếu chưa đủ điều kiện, báo hướng xử lý; không tự chạy game, bỏ pause hay thay đổi scene. Pause và Edit Mode chưa được cam kết hỗ trợ.

## 5. Luồng sử dụng

```text
Game View Screenshot
┌────────────────────────────────────────────────────────┐
│ Chế độ: [Copy ▾]   Đếm ngược: [0s ▾]       [Chụp]       │
│ Thư mục: [Screenshots/                         ] [...]  │
│ Trạng thái: Sẵn sàng / Cần Play Mode / Đợi frame       │
├────────────────────────────────────────────────────────┤
│                      Preview                           │
│                     Fit / 100%                         │
├────────────────────────────────────────────────────────┤
│ 1920×1080          [Copy Image] [Save As] [Mở thư mục]  │
└────────────────────────────────────────────────────────┘
```

1. Người dùng vào Play Mode và chuẩn bị Game View.
2. Bấm chụp hoặc shortcut, chạy countdown; khóa thao tác chụp trùng, hiển thị Cancel.
3. Đợi cuối frame, đọc ảnh một lần và thực hiện chế độ đã chọn. Copy chỉ truyền dữ liệu ảnh trong bộ nhớ vào clipboard; Save ghi PNG; Copy + Save dùng cùng frame cho cả hai.
4. Cập nhật preview với trạng thái Copied / Saved / Copied + Saved; chỉ hiện đường dẫn khi có file. Copy không kiểm tra quyền thư mục, không tạo thư mục hoặc file tạm. Không đưa cửa sổ Screenshot lên focus trong lúc chờ frame.
5. Nếu chuyển tab làm mất frame, UI giải thích cần mở Game View; timeout kết thúc job thay vì treo Editor.

## 6. Kiến trúc và lifecycle

```text
Window / Menu / Shortcut
           ↓
  GameViewCaptureCoordinator
       ↙       ↓        ↘
 Preconditions FrameBridge CaptureStorage
                  ↓            ↓
          ScreenCapture     PNG file
                  └────→ ImageClipboard / Result → Preview
```

Tất cả code thuộc `Dreamy.EditorTools.Capture`, trong assembly Editor hiện có. Capture dùng Unity API chung; clipboard ảnh có adapter theo OS riêng. Không thêm tham chiếu từ Runtime. Dependency clipboard phải được probe và ghi rõ, không tự cài công cụ hệ thống. Không dùng Core/Datasave/Debugger để quản lý chức năng Editor này.

Contract dự kiến:

- `GameViewCaptureRequest`: mode (Copy/Save/CopyAndSave), delay, output directory tùy chọn, capture ID; chưa cần source/backend registry.
- `GameViewCaptureCoordinator`: chỉ một job; validate chế độ → countdown → wait frame → capture → copy/save theo chế độ → result. Copy + Save có kết quả riêng cho từng đích, không chụp lại.
- Chờ cuối frame bằng public `Awaitable.EndOfFrameAsync(cancellationToken)` của Unity 6; đã chứng minh trong GUI consumer. Không tạo GameObject/component hoặc sửa scene. Timeout vẫn chạy độc lập qua Editor update. [Unity Awaitable](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Awaitable.EndOfFrameAsync.html)
- `CaptureStorage`: tạo tên duy nhất, ghi temp cùng thư mục, publish không ghi đè, dọn temp khi lỗi/hủy.
- `CapturePreferences`: mode, delay và output theo project ở `UserSettings/DreamyCapture/`.
- `IImageClipboard`: probe khả năng và copy dữ liệu ảnh trong bộ nhớ, có timeout/cleanup và adapter Windows/Linux. Không dùng clipboard text để thay cho image.
- `CaptureResult`: trạng thái capture, copy và save riêng; đường dẫn nullable, kích thước và lỗi từng đích nếu có. Preview sở hữu texture đang hiển thị và giải phóng khi thay ảnh/đóng cửa sổ.

Không reflection vào private `UnityEditor.GameView`. Không sửa scene/prefab hoặc ghi cầu nối vào serialized asset. Deadline chờ frame đề xuất 10 giây realtime; Cancel và timeout chạy qua Editor update độc lập với cầu nối cuối frame.

Stop Play, assembly reload và quit Editor hủy job, gỡ callback và giải phóng tài nguyên. Đóng cửa sổ hủy job do cửa sổ sở hữu; job từ menu/shortcut thuộc coordinator. Mọi callback muộn kiểm tra job ID/generation trước khi publish. Kiểm thử cả domain reload bật/tắt.

Unity API và texture lifecycle chạy trên main thread; không busy-wait. Khi PNG đã publish hoặc clipboard đã nhận ảnh, không thể coi hủy là rollback; giữ kết quả đích đã thành công. Lỗi preview không làm mất kết quả copy/save. Hủy trước publish file phải dọn temp.

## 6a. Clipboard ảnh Windows và Ubuntu

Ảnh vẫn chỉ lấy từ Game View. Clipboard là đích xuất, không mở lại phạm vi chụp desktop.

- **Windows:** spike Win32 clipboard với dữ liệu DIB tương thích ứng dụng đích; cấp phát/chuyển quyền sở hữu bộ nhớ đúng quy tắc `SetClipboardData`. Có retry giới hạn khi clipboard bận. [Microsoft SetClipboardData](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setclipboarddata)
- **Ubuntu:** ưu tiên spike helper `wl-copy --type image/png` trên Wayland hoặc `xclip -selection clipboard -t image/png -i` trên X11, nhận bytes qua stdin, không file tạm. Đây là dependency hệ thống tùy môi trường, không giả định đã có sẵn; máy nghiên cứu hiện không tìm thấy cả hai trong PATH. [wl-clipboard](https://github.com/bugaevc/wl-clipboard), [xclip](https://github.com/astrand/xclip)
- Chạy trực tiếp executable bằng argument cố định, không shell. Helper giữ clipboard có lifecycle riêng, không được kill ngay khi đóng preview; P0 phải chứng minh ownership, timeout, replacement và paste sau reload. Không hứa clipboard tồn tại sau khi quit nếu chưa kiểm chứng.
- Nếu helper không phù hợp, đánh giá native clipboard bridge trước khi chốt dependency. Không quay về dùng text clipboard hoặc file tạm mà vẫn báo Copy thành công.
- Probe trước capture: Copy thiếu backend thì giải thích dependency; Copy + Save thiếu backend yêu cầu người dùng đổi mode hoặc khắc phục, không âm thầm chuyển chế độ. Nếu lỗi phát sinh sau capture, giữ preview để Copy Again/Save As thủ công.
- Copy + Save thử hai đích độc lập khi chưa bị hủy: copy lỗi vẫn thử save; save lỗi vẫn thử copy. Thông báo cụ thể, ví dụ “Đã lưu, copy thất bại” hoặc “Đã copy, lưu thất bại”. Không tự xóa file hay khôi phục clipboard cũ.
- Nút Copy Path là thao tác riêng sau Save; không chạy tự động sau Copy Image vì sẽ ghi đè ảnh trong clipboard.

## 7. File và ngân sách tài nguyên

- Chế độ Copy không ghi ảnh hoặc file tạm xuống đĩa bởi công cụ; preferences vẫn có thể được lưu. Clipboard manager của hệ điều hành có chính sách lưu riêng.
- Với Save/Copy + Save, output mặc định `<project>/Screenshots/`; khi triển khai thêm hướng dẫn ignore, cho phép chọn thư mục ngoài project. Không tự AssetDatabase.Refresh.
- Tên: `{project}_GameView_{UTC-yyyyMMdd-HHmmss-fff}_{shortId}.png`, sanitize theo OS, không ghi đè.
- Bản đầu không cần metadata sidecar hoặc history index. Kích thước/đường dẫn nằm trong result và preview.
- Chỉ giữ preview hiện tại, không giữ texture của mọi lần chụp. Owner giải phóng texture sau encode hoặc chuyển quyền sở hữu rõ ràng cho preview.
- Giới hạn đề xuất 32 megapixel và ngân sách tạm 512 MiB/job, tính readback, texture và encode. Đây là mục tiêu cần đo ở P0, chưa phải benchmark. Từ chối trước allocation lớn khi biết kích thước.
- Kết quả: Success, PartialSuccess, PreviewWarning, Cancelled, NeedsPlayMode, NeedsUnpausedGameView, Busy, Timeout, UnsupportedEnvironment, ClipboardUnavailable, ClipboardFailure, SizeLimit, CaptureFailure, WriteFailure.

## 8. Kế hoạch triển khai

| Giai đoạn | Nội dung | Gate hoàn thành |
| --- | --- | --- |
| P0 — Spike | Cầu nối cuối frame, capture có UI, clipboard ảnh mỗi OS, timeout/cleanup | Paste ảnh thật và Save PNG trên Windows/Ubuntu; Copy không ghi file; không dirty scene |
| P1 — Logic | Coordinator, ba chế độ, clipboard adapters, storage, preferences | Copy không gọi storage; kết quả từng đích đúng; không job trùng/ghi đè |
| P2 — Giao diện | Chọn mode, preview, Copy Image/Save As, menu/shortcut | Copy/Save/Copy + Save hoàn chỉnh; không giành focus lúc đợi frame |
| P3 — Kiểm chứng | GUI hai OS, clean consumer, tài liệu và changelog | Đủ receipt theo ma trận; không leak tài nguyên; cài package thành công |

Đã triển khai P0–P2 và chạy kiểm chứng P3 trên Ubuntu. Chưa đổi version/tag hoặc phát hành. Windows native paste, Ubuntu 24.04 và GPU/URP khác vẫn cần receipt trước khi công bố support đầy đủ.

## 9. Kiểm chứng và nghiệm thu

Test nhỏ nhất phù hợp:

- EditMode với fake capture: Busy, Cancel trong countdown/chờ frame/trước publish, timeout, callback muộn, không complete hai lần.
- Storage: đường dẫn Unicode/dấu cách trên hai OS, collision, write failure, dọn temp và không ghi đè.
- Mode: Copy không gọi filesystem output dù thư mục không ghi được; Save không thay clipboard; Copy + Save dùng cùng frame. Fake clipboard kiểm tra failure/timeout và PartialSuccess; không tự lưu khi Copy lỗi.
- Fixture ảnh: màu/vị trí xác định, UI Screen Space Overlay, kiểm tra ảnh không lật dọc, đúng kích thước và decode được.

Ma trận GUI mục tiêu: Windows 11 x64 và Ubuntu 24.04/26.04 x64 với Unity 6000.4; đây chưa phải các cấu hình đã xác nhận hỗ trợ. Ubuntu Wayland là ca chính, X11 bổ sung khi có môi trường.

Trên cả hai OS cần kiểm tra:

1. Play Mode + URP + UI Overlay; landscape và portrait, ảnh khớp Game View thực tế.
2. timeScale = 0 khi Editor không pause; countdown vẫn chạy và chụp được nếu frame tiếp tục render.
3. Editor pause, chưa Play, chuyển Scene View, dừng Play, đóng cửa sổ và reload: từ chối/hủy/timeout rõ ràng, không treo.
4. Domain reload bật/tắt; bấm chụp liên tiếp; 50 lần chụp tuần tự để quan sát cleanup texture/bộ nhớ.
5. Output Unicode/dấu cách, không có quyền ghi, tên file trùng, UI scale khác nhau.
6. Paste ảnh vào ứng dụng đồ họa và ô nhập ảnh trình duyệt chấp nhận image; thử sau khi đóng cửa sổ screenshot, sau reload/quit, copy lại và clipboard bị app khác thay thế. Không dùng kiểm tra text/path làm bằng chứng image clipboard.
7. Copy-only không tạo file/thư mục/temp; lỗi copy không tự Save. Copy + Save: từng đích lỗi vẫn báo đúng đích đã thành công.
8. Clean consumer: compile, test assembly Editor nếu được thêm, không kéo Editor reference vào Runtime/player.

Lưu receipt với OS/session, GPU/render API, Unity/package commit, lockfile, bước thử, ảnh mẫu, kích thước và file hash. Host/version chưa có receipt phải ghi chưa xác minh.

Batch/headless chỉ kiểm tra pure logic và storage. Yêu cầu chụp Game View trong môi trường này phải trả UnsupportedEnvironment, không chờ end-of-frame.

Nghiệm thu khi cả Windows/Ubuntu copy được ảnh để paste và lưu được PNG có UI đúng nguồn/kích thước; cả ba chế độ đúng, Copy không tạo file; delay/cancel/timeout hoạt động; preview và thư mục chạy; không làm dirty scene hoặc thay game state; không rò texture/callback; package cài sạch được.

## 10. Checkpoint

- [x] Hoàn tất nghiên cứu package và Unity API trong lượt trước.
- [x] Thu gọn theo yêu cầu mới: chỉ Game View trên Windows/Ubuntu.
- [x] Bỏ desktop/Scene capture; chỉ clipboard ảnh có adapter OS.
- [x] Bổ sung Copy / Save / Copy + Save theo yêu cầu; Copy không cần lưu ảnh.
- [x] 2026-11-06: thêm Clipboard requirements và Copy Install Command trong cửa sổ; chọn lệnh theo Wayland/X11, Windows không cần cài thêm. Không tự chạy lệnh cài đặt.
- [x] Triển khai source Editor-only, cửa sổ/menu/shortcut, output và clipboard adapters.
- [x] Khai báo module ScreenCapture/ImageConversion, cập nhật README/changelog; giữ version hiện tại (Unreleased).
- [x] Compile và test logic trong clean consumer Unity 6000.4.12f1.
- [x] GUI Ubuntu Wayland: capture có UI, ba mode, clipboard readback sau quit, timeScale=0, Busy/Cancel, timeout và 50 capture tuần tự.
- [ ] Windows native paste và các host/version khác chưa có receipt. Xem `docs/evidence/editor-capture/` để đối chiếu bằng chứng hiện có.

Resume: đọc evidence và kiểm tra lại source fingerprint trước khi tiếp tục; ưu tiên Windows GUI/paste và URP/portrait smoke. Không dùng compile Linux để khẳng định native clipboard Windows đã hoạt động.
