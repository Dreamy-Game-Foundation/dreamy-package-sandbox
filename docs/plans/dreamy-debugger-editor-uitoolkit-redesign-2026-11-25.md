# Kế hoạch thiết kế lại Dreamy Debugger Editor bằng UI Toolkit

## Phạm vi

- `DebuggerWindow` là cửa sổ cấu hình dùng chung cho Settings, Cheat Panel, Access settings và Telegram & Reports. Chuyển sáu trang General, Access, Cheats, Performance, Reports và Layout sang UI Toolkit. Quick Start Acceptance Playground gọi cùng cửa sổ này.
- `ScreenshotWindow` là cửa sổ thao tác khi Play Mode. Chuyển form chụp PNG, điều khiển ẩn/khôi phục UI và bản xem trước sang UI Toolkit.
- Phần triển khai giới hạn trong Editor của `com.dreamy.debugger`. Overlay trong Game View, prefab, cấu hình đã serialize, cơ chế access, capture và relay tiếp tục dùng các owner hiện tại.

## Hiện trạng

- `DebuggerWindow.cs` dựng header, form và sáu tab bằng IMGUI với kích thước cố định. Nhãn và nội dung đi qua hai bảng index khác nhau. Bấm Cheats hiện mở Layout, Performance mở Cheat options, Reports mở Performance và Layout mở Reports. Menu Cheat Panel cũng đặt index của Performance.
- `ScreenshotWindow.cs` quét các group UI trong `OnGUI` và yêu cầu repaint qua `OnInspectorUpdate`. Capture PNG, external capture, Restore và ảnh mới nhất cùng nằm trong một luồng cuộn.
- `TelegramRelaySettings.cs` vừa vẽ form, vừa quản lý lệnh terminal, tiến trình và trạng thái của từng provider. Working tree đang có thay đổi Discord/Telegram. Lấy phiên bản đã tích hợp các thay đổi đó làm baseline trước khi chuyển Reports.

## Hướng thiết kế

Giữ chất graphite và màu hồng đỏ của Dreamy trong [concept Editor hiện có](../concepts/debugger-redesign-v2/editor-concept.png). Ở dark skin, dùng nền graphite, bề mặt sáng hơn một mức và đường phân cách mảnh. Ở light skin, dùng bề mặt xám sáng và tông đỏ đậm hơn để giữ độ tương phản. Dùng font Unity Editor và control native. Dùng màu nhận diện cho mục đang chọn và hành động chính, kèm focus ring rõ. Trạng thái lỗi, cảnh báo và đang chạy có nhãn riêng.

Dùng khoảng cách 4/8/12/16/24 pt, tên trang 18–20 pt và tiêu đề nhóm 13–14 pt làm điểm bắt đầu. Giữ một header gọn với tên công cụ, phiên bản và trạng thái Edit/Play. Dùng đường chia và khoảng trắng giữa các nhóm field. Ưu tiên nút có chữ cho thao tác như Locate, Restore và Open folder.

| Chiều rộng vùng nội dung | Settings | Screenshot |
| --- | --- | --- |
| Khoảng 360–599 pt | Dropdown chọn trang, form một cột | Capture và External Capture xếp dọc, ảnh ở dưới |
| Khoảng 600–799 pt | Sidebar 144–160 pt và form cuộn | Một cột với toolbar gọn, ảnh ở dưới |
| Từ khoảng 800 pt | Sidebar, form và preview bên cạnh ở trang cần preview | Control bên trái, Last screenshot bên phải |

Các mốc trên là giá trị thử trong prototype. Kiểm tra thêm khi dock thấp và hẹp hơn 360 pt. Vùng nội dung phải cuộn được, thao tác chính và Restore/Cancel phải tiếp cận được.

## Bố cục theo luồng sử dụng

| Trang | Bố cục và hành động |
| --- | --- |
| General | Thanh chọn config và Locate dùng chung. Loading Mode, tình trạng debugger/bootstrap, nút mở Runtime Console hoặc thêm bootstrap theo trạng thái. Import sample nằm trong foldout. |
| Access | Nhóm Activation, Credentials và Session. Giữ đường dẫn serialized và kiểu dữ liệu của PIN để giữ số 0 ở đầu. Dùng lưới chọn vị trí trigger 3×3 ở Layout và dùng lại cùng control ở Access nếu giữ cả hai lối chỉnh. |
| Cheats | Hiện trạng thái runtime/unlock và nút Open Cheat Panel. Danh sách registry chỉ đọc theo nhóm/tên/kiểu, có tìm kiếm theo nhóm hoặc tên và ảo hóa bằng `ListView` nếu fixture lớn. |
| Performance | Nhóm HUD mặc định, thao tác với HUD và preview có nhãn Sample. Tách rõ chế độ HUD của phiên Play Mode đang chạy với giá trị mặc định trong config. |
| Reports | Các nhóm Report capture, Runtime uploads, Local relay, Delivery test và Screenshot defaults. Telegram/Discord chọn trong Local relay, lệnh thủ công vào foldout. Trạng thái gửi và report ID nằm cạnh nút test. Hiển thị relay accepted và destination delivery bằng hai trạng thái có nghĩa riêng. |
| Layout | Lưới vị trí trigger và thông tin safe area của HUD/panel. Giữ thông tin về dock Console và pinned categories ở cạnh phần layout liên quan. |
| Screenshot | Thanh trạng thái Edit Mode, chưa có runtime, locked hoặc ready. Nhóm **Capture PNG** có profile, UI groups và delay. Nhóm **External Capture** có hide policy, shutter và auto-restore. Khi đang đếm giờ, chụp hoặc ẩn UI, hiện **Restore / Cancel** ở vùng hành động cố định. Last screenshot có preview theo tỷ lệ gốc, kích thước ảnh, Open image, Open folder và Capture again. |

## Thiết kế triển khai

- Dùng `CreateGUI()` cho mỗi window, UXML cho shell ổn định, USS dùng chung cho component và theme sáng/tối. Form động và trạng thái Play Mode dựng bằng C#. Giới hạn selector dưới root của Debugger và giữ asset UI trong `Editor/`.
- Dùng một định danh trang cho điều hướng, nhãn, nội dung và menu. Menu Cheat Panel mở đúng trang Cheats và giữ thao tác mở runtime Options trong Play Mode. Reports và Access giữ đường dẫn menu hiện có. Giữ trang đang chọn khi resize. Kiểm tra cách cung cấp lại `CaptureRect` theo tọa độ màn hình sau khi bỏ `OnGUI`, gồm dock và HiDPI.
- Dùng `ObjectField` cho config và bind `PropertyField` vào `SerializedObject`. Unbind khi đổi config hoặc tháo view. Giữ `DebuggerProjectSettings` là owner của Loading và config reference. Kiểm tra Undo/Redo, save/reopen và cảnh báo đối với giá trị runtime chỉ đọc lúc khởi tạo.
- Bọc các action hiện có bằng callback UI. Tách code vẽ relay khỏi xử lý terminal, giữ `EditorPrefs`, `SessionState` và credential store theo từng provider. Thay luồng `Repaint()` hiện tại bằng cập nhật trạng thái trên view. Kết quả async chỉ được cập nhật nếu view và request còn hiệu lực. Không đưa credential vào ảnh review hay status.
- Dùng event Editor cho Play Mode, scene/hierarchy và Undo ở chỗ thích hợp. Khi `ScreenshotMode` thiếu event, dùng scheduler gắn với view trong Play Mode để đọc trạng thái bên ngoài và countdown. Refresh cache UI groups theo thay đổi scene/hierarchy và lúc mở hoặc chuẩn bị thực thi thao tác. Chỉ cập nhật control khi trạng thái đổi. Dừng scheduler và gỡ subscription khi đóng hoặc reload. Tiếp tục để runtime sở hữu capture, hide/restore và preview texture.

## Thứ tự thực hiện

1. **Prototype trong Unity.** Chụp baseline hai cửa sổ, dựng shell chung cùng trang General và phần Capture PNG. Chốt màu, mật độ, focus và breakpoint bằng giao diện thật ở dark/light skin.
2. **Chuyển Settings.** Sửa mapping menu và trang, rồi chuyển General, Access, Layout, Performance và Cheats. Xác nhận binding, Undo và đổi config trước khi chuyển Reports.
3. **Chuyển Screenshot.** Dựng vùng thao tác cố định, group UI động và preview. Kiểm tra chụp và khôi phục ngay trong Play Mode.
4. **Chuyển Reports.** Tích hợp luồng Telegram/Discord hiện tại, sau đó chuyển local relay, delivery test và trạng thái async. Kiểm tra fake/local fixture trước khi dùng relay thật.
5. **Kiểm tra tích hợp.** Xóa phần IMGUI đã thay xong sau khi đối chiếu thao tác. Kiểm tra asset UXML/USS được import khi cài package vào consumer và Quick Start mở đúng Settings.

## Tiêu chí nghiệm thu

- Compile sạch và đọc Console mới trên sandbox Unity `6000.4.12f1`. Manifest/lock trỏ tới package local `0.1.8`; package khai báo Unity `6000.0`. Kiểm tra import và mở hai cửa sổ trên `6000.0` trước khi xác nhận tương thích cho bản UI mới.
- Mở từng menu và kiểm tra đúng trang. Đổi config A → B → A, sửa field, Undo/Redo, đóng/mở và domain reload. Các giá trị, kiểu dữ liệu và reference vẫn đọc được từ asset hiện có.
- Với Screenshot, test Edit Mode, Play Mode locked, selected groups rỗng, countdown, cancel, hide vô hạn, auto-restore và reload. Chụp PNG thật với fixture có Camera và Game View render. Kiểm tra PNG cùng Restore cả từ Editor lẫn Game View. Dùng các test liên quan trong `ScreenshotModeTests` làm hồi quy.
- Với Reports, test idle/sending/accepted/rejected/timeout bằng fixture, đổi trang khi đang gửi và đóng/mở khi tác vụ còn chạy. Kiểm tra cả hai provider, trạng thái chạy đơn, và phân biệt kết quả giả lập với kết quả relay.
- Lưu ảnh hai cửa sổ ở kích thước hẹp/vừa/rộng, dark/light và ít nhất một cấu hình HiDPI. Kiểm tra bằng bàn phím, tên asset/path dài, cảnh báo, trạng thái disabled và tác vụ đang chạy. Ghi kết quả visual cùng các receipt compile, Console và test tương ứng.

Kế hoạch dựa trên source, package manifest/lock, support matrix và API metadata từ Editor cài local. Chưa dựng prototype, compile hoặc chạy kiểm tra trực quan. Đường dẫn workflow được chỉ định trong AGENTS.md và Dreamy CLI chưa có trong workspace/PATH, nên phần recall dùng các observation local, gồm lưu ý fixture Camera và Game View repaint khi xác minh capture.
