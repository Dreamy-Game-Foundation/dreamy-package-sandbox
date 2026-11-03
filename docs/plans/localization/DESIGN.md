# Thiết kế JSON, service, Settings gateway và scene test

[Tổng quan](README.md) · [Hiện trạng](CURRENT_STATE.md)

API và đường dẫn dưới đây là thiết kế dự kiến; chưa phải chức năng đã triển khai.

Luồng chọn ngôn ngữ:

SettingsPanel.LanguageRequested(code)
→ SettingsPresenter
→ ISettingsService.SetLanguageAsync
→ SettingsModel
→ ISettingsLanguageGateway
→ LocalizationSettingsLanguageGateway (host/integration)
→ ILocalizationService
→ Unity Localization SelectedLocale/tables
→ LocalizedTmpText trên panel và scene.

Luồng dữ liệu:

en.json + vi.json → Editor validator/importer → generated String Tables/Locales → runtime service/bindings.

- Localization sở hữu locale hiện tại, trạng thái transition, lookup, fallback và preference thông qua ILocalePreferenceStore.
- Settings sở hữu thao tác người dùng và trạng thái hiển thị selector; không sở hữu một bản preference locale khác.
- Gateway chuyển contract localization sang contract Settings và chuyển tiếp Changed, không tự load table hay ghi save.
- Composition root tạo/hủy service và adapter; presenter chỉ gỡ subscription của nó, không Dispose gateway dùng chung.
- UI text dùng một đường subscription Unity Localization; không vừa bind trực tiếp vừa được presenter ghi đè cùng một TMP target.
- Đổi locale cần lifetime của application/service; đóng panel hủy việc đợi/cập nhật panel, không được khiến locale toàn app ở trạng thái nửa chừng. Caller cancellation trước commit hủy yêu cầu; sau commit không rollback riêng UI. Contract phải kiểm thử rõ.

## Dữ liệu và file layout dự kiến

Assets/_Project/Localization/Production/Source/en.json
Assets/_Project/Localization/Production/Source/vi.json
Assets/_Project/Localization/Generated/ (Unity settings, Locales, shared data, String Tables)
Assets/_Project/Localization/Profiles/LocalizationTestProfile.asset
Assets/_Project/Scenes/LocalizationSettingsTest.unity
Assets/_Project/Tests/Localization/

JSON root là object key → string, UTF-8. Không bọc metadata và không nhét locale settings vào từng entry. Locale lấy từ filename; profile khai báo nguồn locale, danh sách file, default/fallback và table đích.

Ví dụ key: settings.title, settings.music, settings.sfx, settings.haptics, settings.language, settings.close, settings.privacy, settings.restore, settings.rate, demo.title, demo.open_settings, demo.greeting, demo.reward.

- Source locale xác định key chuẩn. Missing/extra key và placeholder mismatch đều có report đường dẫn/locale/key; policy thiếu bản dịch mặc định chặn import sample.
- Duplicate JSON property phải báo lỗi, không chấp nhận last-wins làm mất bản dịch.
- JSON rỗng, root sai kiểu, value không phải string, locale filename trùng/không hợp lệ phải báo lỗi trước mutation.
- Source/fallback locale bắt buộc đủ key. Intentional empty string cần rule riêng; không tự coi mọi chuỗi rỗng là missing nếu về sau cho phép.
- Tất cả entry pilot vào một String Table collection. Table/entry IDs được giữ ổn định qua reimport; không xóa/recreate collection mỗi lần.
- Smart String metadata cho pilot cấu hình tường minh trong authoring profile/catalog theo stable key; không dùng regex thấy dấu ngoặc để tự đoán Smart. Formatter vẫn là Unity Smart Strings.
