# Phase 4 — Settings panel và binding thực tế (P0)

[Tổng quan](README.md) · [Theo dõi tiến độ](TRACKING.md) · [Thiết kế chung](DESIGN.md)

Trạng thái: **Hoàn tất pilot; còn kiểm tra layout khác**.

Phụ thuộc: [phase-03-settings-gateway.md](phase-03-settings-gateway.md).

Công việc:

- [x] Mở rộng SettingsPanel với TMP_Dropdown hoặc view selector tương đương; map code↔option rõ ràng, render không phát request.
- [x] Thêm LocalizedTmpText cho title, music, SFX, haptics, privacy, restore, rate, close, language và thông báo thao tác.
- [ ] Chuẩn bị font/fallback có glyph tiếng Việt, kiểm tra dấu và layout text dài ở aspect ratio phổ biến.
- [x] Busy chỉ khóa selector; không vô hiệu hóa music/haptics không liên quan. Error giữ locale đã commit trước đó và cho retry.
- [x] Đồng bộ sample source package và imported sandbox copy bằng import/migration có kiểm soát; giữ script GUID và prefab bindings, kiểm tra nested prefab ownership.
- [x] Scene-specific localized variant/wrapper có thể thêm bindings để Settings package không bị ép phụ thuộc Localization chỉ vì sample. Nếu source view thêm field optional, prefab cũ vẫn chạy.

## Đầu ra

Settings prefab hỗ trợ language và localized variant cho pilot.

## Điều kiện hoàn thành

- [x] Settings đang mở đổi toàn bộ nhãn; close/reopen không mất locale, không listener trùng và không phá toggle/platform actions.

## Ghi nhận thực hiện

- Revision/package versions: ghi trong `docs/evidence/localization/source-identity.json` (working copies chưa commit).
- Files thay đổi: đã triển khai trong localization package, Settings package và host theo phase.
- Lệnh kiểm tra và kết quả: xem validation và receipts bên dưới.
- Bằng chứng: lưu tại `docs/evidence/localization/phase-04/` khi thực hiện.
- Vấn đề còn mở: chưa ghi nhận.
- Ngày cập nhật: 2026-11-02; trạng thái theo phạm vi/gate ở trên.

## Kết quả thực hiện

Dropdown English/Tiếng Việt, TMP bindings, busy/error và fonts đã nối. Sample source/imported code đồng bộ, prefab giữ GUID. Portrait screenshot có bằng chứng; landscape/device layout chưa kiểm tra.

Xem [validation](../../evidence/localization/VALIDATION.md) và [cách chạy](RUN_TEST_SCENE.md).
