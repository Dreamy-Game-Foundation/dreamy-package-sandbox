# Chạy thử localization và Settings

## Scene độc lập

1. Chọn **Dreamy → Localization → Play Settings Test**. Menu mở `Assets/_Project/Scenes/LocalizationSettingsTest.unity`, chạy đúng scene với reload sạch và khôi phục thiết lập Start Scene/reload trước đó khi dừng. Nếu mở scene rồi bấm Play thông thường, cần tắt Start Scene ép bootstrap trên toolbar.
2. Bấm **Open Settings / Mở Cài đặt**. View độc lập dùng trực tiếp SettingsPresenter; scene không có PanelManager hoặc UIPanel.
3. Chọn **English / Tiếng Việt**. Nhãn trong Settings và nội dung ngoài panel đổi theo; không reload scene.
4. Đóng rồi mở lại Settings: selector giữ locale đã chọn.
5. Bấm **Add one coin / Thêm một xu** để kiểm tra chuỗi `{amount}`.
6. Dừng rồi chạy lại để kiểm tra preference. Nút **Reset saved language / Xóa ngôn ngữ đã lưu** chỉ xóa preference phục vụ lần khởi động tiếp theo, không tự đổi locale hiện tại.

## Sửa bản dịch

- Sửa `Assets/_Project/Localization/Source/en.json` và `vi.json`, cùng bộ key. `en` là source/fallback locale.
- Chọn asset `Assets/_Project/Localization/Profiles/JsonImport.asset`.
- Chạy **Dreamy → Localization → Validate Selected JSON Profile** để kiểm tra không ghi assets.
- Chạy **Dreamy → Localization → Import Selected JSON Profile** để cập nhật Unity Tables trước khi Play/build.
- Không chỉnh tay tables sinh ra rồi coi JSON đã được cập nhật. Reimport giữ ID/GUID và không tự xóa orphan.
- Smart keys khai báo trong import profile; `demo.reward` dùng `{amount}`. Không dùng tiếng Anh làm key.

## Main game

Mở `Assets/_Project/Scenes/BootstrapScene.unity`, Play và mở Settings trên Foundation panel. `GameInstaller` khởi tạo localization trước khi presenter được cài, truyền gateway vào Settings và hủy registrations do nó sở hữu khi kết thúc.

Main game hiện chỉ localize nhóm nhãn pilot: Settings, tiêu đề Foundation và nút mở Settings. Những feature/nhãn khác chưa được dịch hàng loạt.

## Tạo lại scene/config

Dừng Play và lưu mọi scene đang sửa. Dùng **Dreamy → Localization → Build Settings Test Scene** để tạo lại scene test từ JSON/profile và tạo prefab StandaloneSettingsView riêng cho test. Các menu **Configure Main Bootstrap** và **Configure Main Labels** dùng để nối profile vào bootstrap và localize nhóm nhãn Foundation. Builder sẽ thay nội dung scene test; không chạy lại builder nếu đang giữ các tùy chỉnh thủ công chưa chuyển vào builder.

## Kiểm thử

- EditMode: `Dreamy.Localization.Tests.EditMode` và `Dreamy.Template.Localization.Editor.Tests` — 41 tests pass.
- PlayMode: `Dreamy.Template.Localization.Tests` — 13 tests pass.
- Native fallback: `Dreamy.Template.Editor.LocalizationVerification.RunPlayMode()` chạy TestRunner API và ghi `docs/evidence/localization/phase-07/playmode.xml`.
- Khi chạy TestRunner, tắt **Start Scene → Enable start scene** để toolbar không ép BootstrapScene. Các lượt kiểm thử đã chạy với thiết lập này được tạm tắt rồi khôi phục.

Xem [tracker](TRACKING.md) và [validation](../../evidence/localization/VALIDATION.md) để biết giới hạn kiểm chứng.

## Ranh giới test scene

Test scene dùng `Assets/_Project/Localization/Generated/StandaloneSettingsView.prefab`, bật/tắt GameObject và bind/unbind SettingsPresenter trực tiếp. Không dùng PanelManager.Instance, PanelPresenterFactory hay UIPanel.Show/Hide. Các test scene mới theo cùng nguyên tắc này.
