# Phase 1 — JSON authoring và import tối thiểu (P0)

[Tổng quan](README.md) · [Theo dõi tiến độ](TRACKING.md) · [Thiết kế chung](DESIGN.md)

Trạng thái: **Đã triển khai; core import đã kiểm chứng**.

Phụ thuộc: [phase-00-baseline.md](phase-00-baseline.md).

Công việc:

- [x] Thêm Editor JSON parser/document/validator, test duplicate key, wrong type, locale/key coverage và placeholders.
- [x] Tạo en.json/vi.json với toàn bộ nhãn của Settings đang có và nội dung demo.
- [x] Tạo profile import locale→file và source/fallback/table reference; dùng serializer đã có trong project sau khi kiểm tra package dependency, không tự thêm parser runtime.
- [x] Import dùng Parse → Validate all → Preview plan → Apply; target fingerprints chặn apply stale plan.
- [x] Tạo/cập nhật Unity locales/settings/tables, preserve table GUID/shared entry ID; không xóa orphan mặc định. Có backup/recovery cho failure giữa apply.
- [x] Có menu Import/Validate dùng cùng service; chưa làm Editor window lớn hoặc auto-import mỗi lần sửa file.

## Đầu ra

2 JSON, generated tables và import report.

## Điều kiện hoàn thành

- [x] JSON lỗi không thay asset; import lại cùng dữ liệu không churn ID/GUID; entry tiếng Việt và tiếng Anh tra được bằng cùng key.

## Ghi nhận thực hiện

- Revision/package versions: ghi trong `docs/evidence/localization/source-identity.json` (working copies chưa commit).
- Files thay đổi: đã triển khai trong localization package, Settings package và host theo phase.
- Lệnh kiểm tra và kết quả: xem validation và receipts bên dưới.
- Bằng chứng: lưu tại `docs/evidence/localization/phase-01/` khi thực hiện.
- Vấn đề còn mở: chưa ghi nhận.
- Ngày cập nhật: 2026-11-02; trạng thái theo phạm vi/gate ở trên.

## Kết quả thực hiện

Parser/importer + en/vi 18 keys. Preview không ghi asset, stale plan bị chặn, reimport giữ ID/GUID và byte-identical nội dung qua test. Có backup/restore path; chưa test cưỡng bức lỗi giữa apply.

Xem [validation](../../evidence/localization/VALIDATION.md) và [cách chạy](RUN_TEST_SCENE.md).
