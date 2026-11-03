# Chạy thử localization và Settings

## Scene độc lập

1. Chọn **Dreamy → Localization → Play Settings Test**. Menu mở `Assets/_Project/Scenes/LocalizationSettingsTest.unity`, chạy đúng scene với reload sạch và khôi phục thiết lập Start Scene/reload trước đó khi dừng. Nếu mở scene rồi bấm Play thông thường, cần tắt Start Scene ép bootstrap trên toolbar.
2. Bấm **Open Settings / Mở Cài đặt**. View độc lập dùng trực tiếp SettingsPresenter; scene không có PanelManager hoặc UIPanel.
3. Mở dropdown và chọn một trong **12 ngôn ngữ**: Anh, Việt, Đức, Tây Ban Nha, Pháp, Indonesia, Nhật, Hàn, Bồ Đào Nha Brazil, Nga, Thái, Trung giản thể. Dropdown hiển thị tên bản ngữ; cuộn để xem các option còn lại. Nhãn trong Settings và nội dung ngoài panel đổi theo; không reload scene.
4. Đóng rồi mở lại Settings: selector giữ locale đã chọn.
5. Bấm **Add one coin / Thêm một xu** để kiểm tra chuỗi `{amount}`.
6. Dừng rồi chạy lại để kiểm tra preference. Nút **Reset saved language / Xóa ngôn ngữ đã lưu** chỉ xóa preference phục vụ lần khởi động tiếp theo, không tự đổi locale hiện tại.

## Sửa bản dịch

- Sửa các file `Assets/_Project/Localization/Production/Source/<locale>.json`, cùng bộ 50 key. `en` là source/fallback locale.
- Chọn asset `Assets/_Project/Localization/Production/Profiles/JsonImport.asset`.
- Chạy **Dreamy → Localization → Validate Selected JSON Profile** để kiểm tra không ghi assets.
- Chạy **Dreamy → Localization → Import Selected JSON Profile** để cập nhật Unity Tables trước khi Play/build.
- Không chỉnh tay tables sinh ra rồi coi JSON đã được cập nhật. Reimport giữ ID/GUID và không tự xóa orphan.
- Smart keys khai báo trong import profile; `demo.reward` dùng `{amount}`. Không dùng tiếng Anh làm key.
- Khi thêm locale hoặc ký tự mới, chạy **Dreamy → Localization → Refresh Languages And Dropdown** để tự nhận JSON, import tables, kiểm tra font và cập nhật cả hai prefab Settings mà không dựng lại scene. Font CJK là subset cho fixture; xem [hướng dẫn font](../../../Assets/_Project/Localization/Fonts/README.md) để tạo lại khi thêm chữ CJK.

## Main game

Mở `Assets/_Project/Scenes/BootstrapScene.unity`, Play và mở Settings trên Foundation panel. `GameInstaller` khởi tạo localization trước khi presenter được cài, truyền gateway vào Settings và hủy registrations do nó sở hữu khi kết thúc.

Main game hiện localize toàn bộ nhãn Foundation panel và điểm/máu/status động, cùng nhóm Settings. UI bên trong các feature khác (Shop/Missions/Progression...) chưa được dịch hàng loạt.

## Tạo lại scene/config

Dừng Play và lưu mọi scene đang sửa. Dùng **Dreamy → Localization → Build Settings Test Scene** để tạo lại scene test từ JSON/profile và tạo prefab StandaloneSettingsView riêng cho test. Các menu **Configure Main Bootstrap** và **Configure Main Labels** dùng để nối profile vào bootstrap và localize nhóm nhãn Foundation. Builder sẽ thay nội dung scene test; không chạy lại builder nếu đang giữ các tùy chỉnh thủ công chưa chuyển vào builder.

## Kiểm thử

- EditMode: `Dreamy.Localization.Tests.EditMode` và `Dreamy.Template.Localization.Editor.Tests` — 41 tests pass.
- PlayMode: `Dreamy.Template.Localization.Tests` — 17 tests pass, gồm đổi đủ 12 locale, glyph coverage, icon/caption, text-only fallback, Foundation dynamic text, dòng dropdown không chồng và cuộn đến option cuối.
- Native fallback: `Dreamy.Template.Editor.LocalizationVerification.RunPlayMode()` chạy TestRunner API và ghi `docs/evidence/localization/phase-07/playmode.xml`.
- Khi chạy TestRunner, tắt **Start Scene → Enable start scene** để toolbar không ép BootstrapScene. Các lượt kiểm thử đã chạy với thiết lập này được tạm tắt rồi khôi phục.

Xem [tracker](TRACKING.md) và [validation](../../evidence/localization/VALIDATION.md) để biết giới hạn kiểm chứng.

## Ranh giới test scene

Test scene dùng `Assets/_Project/Localization/Generated/StandaloneSettingsView.prefab`, bật/tắt GameObject và bind/unbind SettingsPresenter trực tiếp. Không dùng PanelManager.Instance, PanelPresenterFactory hay UIPanel.Show/Hide. Các test scene mới theo cùng nguyên tắc này.

## Dùng trong sản phẩm và cập nhật package

Import **Production Starter** từ Dreamy Localization 0.2.0. Bản trong Assets có thể sửa JSON/font/profile/prefab và chuyển nguyên folder với `.meta` vào thư mục game. Starter dùng font nguồn CJK đầy đủ, không dùng subset riêng của scene test. Xem README package để import tables trước khi chạy bootstrap/label prefab.

Để nối với Settings 0.3.0, import sample **Settings Integration** riêng, dùng gateway và menu cùng tên để gắn table/font vào prefab Settings thuộc game. Không có gateway thì SettingsPanel tự ẩn các control ngôn ngữ. Code Runtime/Editor trong package thuộc owner package; không sửa cache để giữ tùy chỉnh game. Khi update, diff sample mới rồi merge vào bản game, không import đè.

## Cờ và các asset sandbox hiện tại

- Dữ liệu/font/profile production: Assets/_Project/Localization/Production.
- Nguồn bản dịch: Production/Source, 12 locale × 50 key; import profile ở Production/Profiles/JsonImport.asset, output/table giữ DreamyGame.
- Cờ: Assets/_Project/Localization/Country Flags; map runtime ở Assets/_Project/Localization/Profiles/LanguageIcons.asset.
- Sample Country Flags trong package chứa 271 PNG/Sprite và license MIT; bản sandbox đã được chuyển vào thư mục game, giữ GUID.
- Settings Integration là bản game-owned tại Assets/_Project/Localization/Settings Integration. Không reimport thêm bản trùng asmdef/GUID vào cùng project.
- MainScene UIRoot (Canvas của PanelManager) dùng ScaleWithScreenSize 1080×1920; test scene tiếp tục dùng Canvas độc lập và không có PanelManager.
