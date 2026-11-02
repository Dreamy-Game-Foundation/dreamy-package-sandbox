# Phase 0 — Chốt baseline và resolve (P0)

[Tổng quan](README.md) · [Theo dõi tiến độ](TRACKING.md) · [Thiết kế chung](DESIGN.md)

Trạng thái: **Hoàn tất trên sandbox**.

Phụ thuộc: Không; bắt đầu từ baseline hiện tại.

Công việc:

- [x] Xác minh manifest/lock và các asmdef; ghi Unity Localization thực resolve 1.5.8 so với minimum khai báo 1.5.5 và đối chiếu cache/Editor. Không sửa dependency chỉ vì hai version khác nhau.
- [x] Kiểm tra Unity.TextMeshPro dependency riêng so với assembly đã có trong uGUI 2.0.0; chỉ sửa sau bằng chứng resolve sạch.
- [x] Bật testables localization và chạy 15 EditMode tests hiện có; ghi lỗi có sẵn.
- [x] Xác minh cách chạy scene riêng, bootstrap startup hooks, EventSystem/Input System và PanelManager; đọc owner code trước khi sửa scene/prefab.

## Đầu ra

baseline receipt gồm exact commit/version, resolve, compile, tests và lỗi tồn đọng.

## Điều kiện hoàn thành

- [x] không có compile blocker liên quan dependencies mới; không dùng lock viết tay làm bằng chứng resolve.

## Ghi nhận thực hiện

- Revision/package versions: ghi trong `docs/evidence/localization/source-identity.json` (working copies chưa commit).
- Files thay đổi: đã triển khai trong localization package, Settings package và host theo phase.
- Lệnh kiểm tra và kết quả: xem validation và receipts bên dưới.
- Bằng chứng: lưu tại `docs/evidence/localization/phase-00/` khi thực hiện.
- Vấn đề còn mở: chưa ghi nhận.
- Ngày cập nhật: 2026-11-02; trạng thái theo phạm vi/gate ở trên.

## Kết quả thực hiện

15/15 baseline EditMode; manifest/lock và Editor target đã đối chiếu. Unity 6000.4.12f1; Localization resolve 1.5.8; TMP 5.0.0 builtin. Kết quả sau triển khai: 29 EditMode + 9 PlayMode.

Xem [validation](../../evidence/localization/VALIDATION.md) và [cách chạy](RUN_TEST_SCENE.md).
