# Phase 3 — Gateway và Settings contracts (P0)

[Tổng quan](README.md) · [Theo dõi tiến độ](TRACKING.md) · [Thiết kế chung](DESIGN.md)

Trạng thái: **Hoàn tất pilot**.

Phụ thuộc: [phase-02-runtime-service.md](phase-02-runtime-service.md).

Các tên dưới đây là API đề xuất, chưa tồn tại:

- ISettingsLanguageGateway: capability, current code, available options, Changed, SetLanguageAsync(code, token).
- SettingsLanguageOption: stable code + native display name (English/Tiếng Việt), không dùng index làm identity.
- Settings language state: selected code/options, availability, busy và error code. Có thể là nested immutable state trong SettingsViewState.
- LocalizationSettingsLanguageGateway: adapter nằm host/integration, phụ thuộc Settings + Localization; Settings runtime chỉ biết interface gateway của nó.

Công việc:

- [x] Thêm gateway optional vào SettingsModel/Installer/FeatureInstaller, cuối constructor/overload để hạn chế phá call sites cũ.
- [x] Thêm thao tác language async và Changed forwarding vào ISettingsService hoặc capability interface được presenter inject; chọn một đường duy nhất khi khóa API.
- [x] Mở rộng view/presenter cho language request và state render; bind/unbind theo lifecycle hiện có, chặn completion cũ bằng generation/token.
- [x] Gateway chịu mapping status/error; cancellation không hiển thị như load failure, error code được UI map qua localization key.
- [x] Không có gateway: selector ẩn/disabled, audio/haptics/platform/review vẫn dùng được.
- [x] Subscribe locale Changed từ ngoài panel để selector không bị stale; update render dùng WithoutNotify để không tạo vòng lặp.

## Đầu ra

contracts, gateway adapter, installer wiring và fake-based Settings tests.

## Điều kiện hoàn thành

- [x] không dependency cycle; model/presenter không gọi Unity Localization/PlayerPrefs; optional gateway và existing call sites vẫn hợp lệ.

## Ghi nhận thực hiện

- Revision/package versions: ghi trong `docs/evidence/localization/source-identity.json` (working copies chưa commit).
- Files thay đổi: đã triển khai trong localization package, Settings package và host theo phase.
- Lệnh kiểm tra và kết quả: xem validation và receipts bên dưới.
- Bằng chứng: lưu tại `docs/evidence/localization/phase-03/` khi thực hiện.
- Vấn đề còn mở: chưa ghi nhận.
- Ngày cập nhật: 2026-11-02; trạng thái theo phạm vi/gate ở trên.

## Kết quả thực hiện

Chọn optional ISettingsLanguageService và ISettingsLanguageView, giữ nguyên contracts cũ. Gateway/adapter/model/presenter đã nối; PlayMode no-gateway/failure recovery pass.

Xem [validation](../../evidence/localization/VALIDATION.md) và [cách chạy](RUN_TEST_SCENE.md).
