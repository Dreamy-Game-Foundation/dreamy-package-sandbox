# Phase 7 — Verification, docs và bàn giao (P0 cho pilot)

[Tổng quan](README.md) · [Theo dõi tiến độ](TRACKING.md) · [Thiết kế chung](DESIGN.md)

Trạng thái: **Pilot đã kiểm chứng; release gates còn mở**.

Phụ thuộc: Chạy kiểm tra liên quan ngay từ từng phase; nghiệm thu toàn bộ sau Phase 6.

Test matrix bắt buộc:

- [x] en→vi→en: labels Settings và ngoài panel đúng.
- [x] Đóng/mở panel; reload scene; thoát Play/chạy lại: preference và subscription đúng.
- [x] Có callback locale từ ngoài Settings: selector cập nhật không phát request lặp.
- [x] Same locale/unsupported locale; bấm đổi nhanh; hủy caller; đóng panel khi loading; destroy root.
- [x] JSON invalid/duplicate/missing key/placeholder mismatch: reject trước ghi asset.
- [ ] Missing locale/table/translation và load failure: error/fallback đúng, không báo đổi thành công giả.
- [x] Không gateway: Settings cũ vẫn hoạt động.
- [ ] Font dấu tiếng Việt và text dài; chọn layout desktop/mobile phù hợp sample.
- [x] Reimport giữ GUID/entry IDs, unrelated assets không churn.
- [ ] EditMode contracts/importer và PlayMode bindings/scene; chạy smoke scene main sau phase 6. Build player nhỏ khi môi trường có module build; thiếu thì ghi unavailable.

## Đầu ra

receipts gắn commit/version, screenshot en/vi, README setup, schema/import docs, API migration/CHANGELOG.

## Điều kiện hoàn thành

- [ ] không đánh dấu runtime verified chỉ từ source/static check; mỗi hành vi claimed có test/scene receipt tương ứng.

## Ghi nhận thực hiện

- Revision/package versions: ghi trong `docs/evidence/localization/source-identity.json` (working copies chưa commit).
- Files thay đổi: đã triển khai trong localization package, Settings package và host theo phase.
- Lệnh kiểm tra và kết quả: xem validation và receipts bên dưới.
- Bằng chứng: lưu tại `docs/evidence/localization/phase-07/` khi thực hiện.
- Vấn đề còn mở: chưa ghi nhận.
- Ngày cập nhật: 2026-11-02; trạng thái theo phạm vi/gate ở trên.

## Kết quả thực hiện

29/29 EditMode và 9/9 PlayMode qua native TestRunner XML, screenshots và main smoke đã có. Chưa có fresh external consumer, player/mobile build, landscape/device, forced rollback/crash recovery.

Xem [validation](../../evidence/localization/VALIDATION.md) và [cách chạy](RUN_TEST_SCENE.md).
