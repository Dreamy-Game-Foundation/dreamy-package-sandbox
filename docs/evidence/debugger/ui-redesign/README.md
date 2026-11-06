# Dreamy Debugger UI — nghiệm thu 0.3.0-preview.1

Ngày 2026-11-05, Unity **6000.4.12f1 / Linux**. Đây là candidate dùng thử nội bộ. Chưa commit, tag, push hoặc publish trong lượt triển khai này. Gmail vẫn pending; không gửi Telegram/email thật.

[Biên bản máy đọc được](verification.json) · [Plan](../../../plans/DREAMY_DEBUGGER_UI_REDESIGN_2026-11-05.md) · [Concept tham khảo](../../../design/dreamy-debugger-redesign-2026-11-05/README.md).

## Kết quả cuối

| Phạm vi | Kết quả | Bằng chứng |
|---|---|---|
| Source EditMode | 78/78 | [XML](source-EditMode.xml) |
| Source PlayMode | 36/36 | [XML](source-PlayMode.xml) |
| Consumer từ archive EditMode | 78/78 | [XML](artifact-EditMode.xml) |
| Consumer từ archive PlayMode | 36/36 | [XML](artifact-PlayMode.xml) |
| Relay, mock/local | 20/20, không destination thật | [Log](relay-tests.txt) |
| Linux development | Succeeded, có TaskCard/HUD/panel mới | [Receipt](builds/linux-dev.json) |
| Linux release | Succeeded, không package asset debugger trong BuildReport | [Receipt](builds/linux-release.json) |
| Android development | Succeeded, IL2CPP ARM64, có asset UI mới | [Receipt](builds/android-dev.json) |
| Prefab | 29, không missing script | MCP, ghi trong verification.json |
| Bảo toàn dữ liệu | 659 file SR/font/localization khớp hash; toàn bộ meta cũ không đổi | [Baseline](protected-files.json), verification.json |
| GUID import | Không trùng GUID trong package và không giao với SR | Chỉ thư mục Unity import; `Documentation~` là archive bị Unity bỏ qua |
| Gói xuất | 818 files, byte-for-byte khớp source; không local relay state | [File hashes](artifact-inputs.json) |

[Gói UPM để review](artifacts/com.dreamy.debugger-0.3.0-preview.1.tgz), SHA-256: `7719417080287a70231a2accf5a154ed32d1af49dc8d8ac2635df70b418d8518`.

Consumer chỉ có Debugger + Unity Test Framework trong direct dependencies; import sample Quick Start để kiểm tra cả phần game-owned. Gói không thêm dependency Dreamy Core/UI. Build outputs lớn được giữ tại `/tmp/dreamy-ui-acceptance-builds`; APK là `android-dev/DreamyDebugger.apk`. File receipt ghi BuildPipeline totalBytes; đây không phải kích thước APK nén. Hash/kích thước output thực tế ở verification.json.

## Ảnh chạy thật

Tất cả là Game View Unity, fixture offline có camera/light và số liệu giả, không dùng save game hoặc destination thật. FPS/memory là số đo của Editor fixture, không phải benchmark game/mobile. Ảnh AI nằm riêng trong thư mục concept.

| Màn | Ảnh |
|---|---|
| Cheats portrait | [390×844](screenshots/cheats-390.png), [320×640](screenshots/cheats-320.png) |
| Cheats landscape/tablet | [960×540](screenshots/cheats-960.png), [1024×768](screenshots/cheats-1024.png) |
| Drawer | [Mở hoàn toàn, scrim và nút đóng](screenshots/drawer-390.png) |
| Console | [Danh sách portrait](screenshots/console-390.png), [chi tiết có Back](screenshots/detail-390.png), [tablet](screenshots/console-1024.png) |
| HUD | [Detailed](screenshots/hud-detailed.png), [Compact](screenshots/hud-compact.png), [FPS only đã kéo](screenshots/hud-dragged.png) |
| Access/actions | [PIN](screenshots/pin-390.png), [confirmation](screenshots/confirm-390.png) |
| System | [Các hàng key/value](screenshots/system-390.png) |

Đã kiểm tra ảnh decode, kích thước và hash. Đã exercise/Repaint 5 trang form Settings bằng cấu hình transient, không có exception; chưa chứng nhận tương tác click/Undo/autosave hoặc ảnh native window: provider Linux hiện báo không hỗ trợ screenshot. Không dùng ảnh concept làm bằng chứng form thật.

## Những lỗi phát hiện và sửa trong nghiệm thu

- Di chuyển control vào slot inactive làm mất subscription, balance không refresh: sửa lifecycle subscribe/unsubscribe; có test action/balance/external change.
- Category header chồng layout, PIN cũ bị tint đậm: sửa asset authoring, không sửa YAML hoặc font game.
- Drawer chỉ trượt một phần và bị tab canvas phủ: giữ offset animation riêng; đặt drawer/scrim đúng sorting, khôi phục override sau activation. Test vị trí hoàn tất và thứ tự canvas.
- Header ở 320 px xuống dòng: co font có giới hạn; giữ logo và nút đóng.
- Console row chồng chữ, toolbar giãn khi mở detail: đồng bộ preferred row height với virtualizer, giới hạn text và flexible height của toolbar; có regression geometry.
- HUD chữ mờ khi reference canvas rất nhỏ rồi phóng bằng transform: dùng constant-pixel canvas cho HUD, tăng cỡ chữ phụ. Không đổi scaler của runtime panel/game.
- Shader HUD ở lần warm-up đầu hiện màu cyan trong Editor; ảnh nghiệm thu lấy sau khi shader hoàn tất, không có shader error message. Không đổi shader cho sự kiện này.
- Thử cleanup GameView dùng sai chỉ số custom-relative tạo 4 Console `Invalid index` của automation. Đã dùng đúng absolute index, xác nhận không còn resolution QA. Giữ lịch sử Console, không clear log của người dùng.

Source C#/Markdown/JSON qua `git diff --check`. Whole diff vẫn có trailing spaces ở các field YAML rỗng do Unity tự serialize; không sửa tay prefab YAML để làm sạch hình thức.

## Giới hạn và gate còn mở

- Thiết bị thật: tap/drag đa chạm, safe area/notch, xoay màn hình, DPI, keyboard/IME, input-system matrix; iOS chưa build/chạy.
- Profile thiết bị yếu: p95/p99, GC, VRAM, thermal/battery, capture/send khi đang chơi. Không tuyên bố không lag.
- Quan sát trước/sau incremental options chỉ là thời gian dispatch đăng ký trong Editor: 1000 options từ khoảng 1274 ms xuống 5–6 ms, còn công việc tạo/layout ở các frame tiếp theo. [Receipt trước](pre-budget-PlayMode.xml); không coi là tổng thời gian hoặc chứng nhận frame budget.
- Capture GPU cancellation/low-memory, native crash/ANR/OOM; không hứa tạo ảnh mới sau khi process chết.
- Production report: HTTPS relay thật, quyền truy cập destination, disclosure/consent/masking/withdrawal/purge và vận hành phải được nghiệm thu riêng.
- Unity 6000.4 cảnh báo deprecated object-finding API trong test; giữ API tương thích minimum 6000.0. Không mass-upgrade chỉ để tắt warning.
- Còn asset legacy trong catalog để bảo toàn tương thích; runtime không hiện author/report-package tab. Không tuyên bố đã dọn toàn bộ dữ liệu lưu trữ SR hoặc có full parity mọi setting của SR/Graphy.

Editor đã về BootstrapScene sạch, Edit Mode, target Android, khôi phục enter-play options/layout/Simulator, xóa fixture và custom resolutions QA. Không thay scene/game save/config thật để chụp ảnh.
