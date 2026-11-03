# Phân tích hiện trạng Localization

[Tổng quan](README.md) · [Thiết kế đích](DESIGN.md)

Ngày khảo sát: 2026-11-02. Các nhận xét dưới đây mô tả source tại baseline, chưa phải runtime verification.

## Baseline và giới hạn bằng chứng

- Sandbox HEAD: `5ff5ceb5d7f2ee3ab458e21ec9617b90052e5501`.
- Submodule: `LocalPackages/com.dreamy.localization`, HEAD `4a71c6fa3f4ee56c699368103e7920c9476816e0`, nhánh main, version 0.1.0. Đã clone; chưa sửa source package.
- Unity sandbox: 6000.4.12f1; fixture package: 6000.0.58f2. Không xem hai baseline là tương đương đã xác minh.
- Lock sandbox hiện đã có localization local, Unity Localization 1.5.8 và TMP 5.0.0 builtin. Đây là metadata resolve; chưa có receipt compile/test mới. Dependency Unity Localization của package khai báo 1.5.5.
- Dependencies package: Unity Localization 1.5.5, TextMeshPro 5.0.0, uGUI 2.0.0, audio module 1.0.0.
- Cache uGUI 2.0.0 của sandbox đã chứa `Runtime/TMP/Unity.TextMeshPro.asmdef`; dependency TMP độc lập cần được kiểm tra trên fixture sạch trước khi điều chỉnh.
- Không tìm thấy `docs/agents/dreamy-workflow.md` được AGENTS tham chiếu, compatibility/dreamy-packages.json hoặc lesson store trong phạm vi đã kiểm tra. Không có CLI `dreamy` trên PATH. Vì vậy đây là phân tích source, không phải chứng nhận compatibility hay harness receipt.
- Giữ nguyên prefab Connectivity đang có thay đổi của người dùng. Không refresh/import scene hoặc thay đổi asset trong bước phân tích.

## 2. Chức năng thực tế và luồng hiện tại

### Runtime

Nguồn: `Runtime/Contracts`, `Runtime/Services`, `Runtime/Configuration`, `Runtime/Locale`, `Runtime/Keys` của submodule.

1. `LocalizationBootstrap.Awake` có execution order -10000, có thể DontDestroyOnLoad; tạo LocalizationService nếu facade chưa được configure; gọi InitializeAsync bằng async void.
2. Service giữ lại một Task khởi tạo; đợi Unity Localization InitializationOperation; lấy danh sách locale thực tế từ Unity settings.
3. Chọn locale theo thứ tự: preference đã lưu và match được → Unity SelectedLocale hiện có → profile default match được → locale đầu tiên.
4. Matcher hỗ trợ không phân biệt hoa/thường, alias khai báo và hạ culture như en-US/en_US xuống en.
5. Service đăng ký SelectedLocaleChanged; callback lưu preference và phát LocaleChanged. Chưa phát sự kiện ban đầu qua callback này vì đăng ký sau khi chọn locale.
6. SetLocaleAsync đợi init và SemaphoreSlim, match locale, đổi SelectedLocale rồi ghi preference. Nếu đã là locale đó thì trả true ngay. Giá trị true hiện không thể hiện mọi table/binding đã tải và render xong.
7. GetStringAsync tạo LocalizedString với table/entry/arguments và đợi operation. GetAssetAsync gọi Unity AssetDatabase rồi trả asset; API chưa thể hiện quyền sở hữu/release.
8. Facade DreamyLocalization giữ service static; lookup bắt Exception, log và trả fallback. Service trực tiếp vẫn trả lỗi qua Task.
9. Preference mặc định là PlayerPrefs qua ILocalePreferenceStore; host có thể inject store khác.

### Binding

Nguồn: `Runtime.UI/*.cs`.

- TMP, uGUI Text, UnityEvent<string> dùng LocalizedStringBinding: subscribe khi enable, unsubscribe khi disable, hỗ trợ đổi reference và arguments.
- Image/SpriteRenderer/AudioSource dùng LocalizedSprite/LocalizedAudioClip; nhận AssetChanged, áp dụng fallback khi callback nhận null.
- Binding dựa trực tiếp vào Unity Localization, không đi qua ILocalizationService. Điều này cần được giữ có chủ đích hoặc thay đồng bộ; không nên tự động tạo thêm đường load trùng.
- Audio binding chỉ gán clip; không điều phối mixer, pool hay playback qua Dreamy Audio.
- Chưa có chính sách font theo locale, glyph coverage, RTL/layout hoặc xử lý kết quả cũ trong contract package.

### Variables và CSV

- LocalizationVariable<T> chỉ phát event khi giá trị đổi. Registry hỗ trợ đăng ký/ghi đè, đọc typed value, unregister và báo tên biến thay đổi.
- Registry chưa nối Unity Smart Format hoặc binding; đăng ký biến chưa tự làm chuỗi đổi theo biến.
- CSV parser hỗ trợ BOM, quoted comma, escaped quote, multiline và báo vị trí khi quote chưa đóng.
- CSV writer sắp xếp locale/table/key, xuất LF và escape field; model có ID, source, status, smart, asset_type, preload, comments, tags, context.
- Validator kiểm tra required/duplicate headers, locale column, schema, table/key, duplicate key, status, missing translation và tập placeholder theo regex.
- Ignore/deprecated được bỏ qua kiểm tra bản dịch. Unknown header hiện được coi là locale; field dư bị bỏ qua, field thiếu được điền rỗng; số/bool không hợp lệ bị chuyển về giá trị mặc định.
- Placeholder regex chưa đủ kiểm chứng numeric placeholders, escaped braces, nested plural/select và format hợp lệ của Unity Smart Strings.
- Chưa có importer/exporter Unity Tables, immutable preview/apply plan, setup window, scanner hoặc batchmode entry point. CSV writer hiện chỉ xuất document, chưa xuất Unity table asset.

### Test và sample

- Có 15 EditMode [Test] trong parser/validator/key/matcher/variables.
- Tests/PlayMode chỉ có asmdef, chưa có test C#.
- Sample giữ chỗ Basic Localization đã bị xóa; các sample hiện tại là Production Starter, Settings Integration và Country Flags.
- Roadmap gốc khai báo phase 0–3 đang làm, phase 4–8 pending. Đây là mục tiêu tài liệu, không phải chức năng đã hoàn tất.

## Đối chiếu các package

| Chủ đề | Bằng chứng hiện tại | Định hướng |
| --- | --- | --- |
| Composition | GameInstaller tạo service, register ServiceLocator, có OnDestroy; SettingsInstaller/ShopInstaller có overload inject | Host giữ quyền tạo/hủy service; integration dùng đúng instance |
| Async | Settings/Connectivity dùng UniTask và CancellationToken | Localization chuyển UniTask + token qua migration được công bố |
| Presentation | SettingsPresenter và FeaturePresenter bind/unbind; PanelPresenterFactory tạo presenter | Chọn ngôn ngữ dùng presenter/view cùng lifecycle |
| Persistence | Locale hiện có ILocalePreferenceStore; Settings có gateway tùy chọn | Chỉ một store có quyền ghi locale; không tạo hai preference độc lập |
| Gameplay | Shop/Missions/Progression/Tutorial trả domain state, ID và dữ liệu | Host/presentation map stable ID sang localization key; domain không tra chuỗi |
| Audio/Assets | Có Dreamy Audio/Assets riêng; Unity Localization đã dùng Addressables | Giữ một load owner; không load cùng asset qua hai hệ thống |
| Package | Runtime và Editor asmdef tách riêng | Giữ hướng dependency; feature khác không trở thành dependency của localization |

SettingsViewState, ISettingsService và ISettingsView hiện chưa có locale state hoặc language selection. Không coi đây là lỗi đã tích hợp; đây là phần cần xây.

## 4. Các vấn đề ưu tiên

### P0 — resolve và lifecycle

- Chưa xác minh dependency trên Unity sandbox; kiểm tra TMP độc lập so với TMP nằm trong uGUI.
- LocalizationService subscribe sự kiện static Unity nhưng không có Dispose/unsubscribe; facade không reset. Có nguy cơ giữ service cũ qua teardown hoặc Play Mode không domain reload.
- Task init lỗi được giữ mãi, nên lần gọi tiếp theo cùng nhận Task lỗi; chưa có retry/reset có kiểm soát.
- Bootstrap async void chưa catch lỗi, token hủy, kiểm soát owner hoặc cleanup. Nhiều bootstrap có thể tồn tại dù facade chỉ giữ một service.
- SetLocaleAsync và callback đều có đường SetLocale cho cùng lần đổi; cần test event/save count thực tế và gom quyền ghi về một nơi.

### P1 — contract có field nhưng chưa có hành vi

- FallbackLocaleCode và profile Tables/load policies chưa được service sử dụng.
- Global variable registry chưa nối formatter/binding.
- Locale event có thể phát trước khi mọi content mới sẵn sàng; chưa có state/result phân biệt changing/ready/failed/cancelled.
- Asset lookup không mô tả release owner, retry, retention và teardown. Chưa kết luận leak khi chưa kiểm tra semantics backend thực tế.
- Facade bắt mọi Exception làm caller khó phân biệt cấu hình lỗi, missing key và cancellation; fallback chỉ xảy ra với exception, không bảo đảm bắt missing-entry text do backend trả về.

### P2 — content pipeline và UX

- CSV parser/validator còn chấp nhận dữ liệu sai hoặc thiếu thông tin; CSV identity không phân biệt hoa/thường nhưng LocalizationKey equality có phân biệt, cần thống nhất/migration.
- Thiếu safe import/export, stable table/entry IDs, asset validation, font checks và build gate.
- Thiếu sample chạy được và PlayMode regression cho đổi locale, đóng/mở panel, pooled binding và teardown.

## Cập nhật phạm vi

JSON theo locale thay thế CSV-first trong pilot. CSV parser hiện tại là chức năng có sẵn; không phải nguồn dữ liệu chính cho scene mới. Thiếu import JSON, Settings language gateway và scene test sẽ được thực hiện theo từng phase.
