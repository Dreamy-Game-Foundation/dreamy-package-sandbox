# Phase 2 — Runtime service và locale transition (P0)

[Tổng quan](README.md) · [Theo dõi tiến độ](TRACKING.md) · [Thiết kế chung](DESIGN.md)

Trạng thái: **Đã triển khai pilot; còn hardening**.

Phụ thuộc: [phase-01-json-import.md](phase-01-json-import.md).

Công việc:

- [ ] Chuẩn hóa async sang UniTask + CancellationToken và migration docs; thêm backend seam nhỏ cho unit tests.
- [x] Init có chia sẻ operation, state rõ ràng, retry có kiểm soát; Dispose unsubscribe Unity events và reset facade đúng ownership.
- [x] Áp dụng saved valid → Unity startup selection → default/fallback policy; validate empty locale list/alias collisions.
- [x] Triển khai fallback lookup thật; missing key/table có error code và requested/resolved locale.
- [x] Chọn policy một transition đang chạy: service serialize transitions; UI selector tạm khóa. Request queue, cancellation và shutdown phải không để state cũ ghi đè kết quả mới.
- [x] Chỉ phát Changed/persist theo transition commit một lần. Commit có nghĩa locale được áp dụng và required table sẵn sàng; không khẳng định mọi binding đã render xong cùng frame.
- [x] Preference store là nguồn ghi duy nhất; expose trạng thái initializing/changing/ready/failed để gateway map.

## Đầu ra

service contract, implementation, focused tests.

## Điều kiện hoàn thành

- [ ] saved locale round-trip; init fail/retry; cancel/dispose; same-locale không dư event/save; đổi locale thật cập nhật binding.

## Ghi nhận thực hiện

- Revision/package versions: ghi trong `docs/evidence/localization/source-identity.json` (working copies chưa commit).
- Files thay đổi: đã triển khai trong localization package, Settings package và host theo phase.
- Lệnh kiểm tra và kết quả: xem validation và receipts bên dưới.
- Bằng chứng: lưu tại `docs/evidence/localization/phase-02/` khi thực hiện.
- Vấn đề còn mở: chưa ghi nhận.
- Ngày cập nhật: 2026-11-02; trạng thái theo phạm vi/gate ở trên.

## Kết quả thực hiện

UniTask, token, shared init/retry, Dispose, preload/fallback và deduplicated locale events đã triển khai. Chưa có backend seam/fake để kiểm tra toàn bộ init failure/retry/store failure; global variable registry/lease policies không nằm pilot.

Xem [validation](../../evidence/localization/VALIDATION.md) và [cách chạy](RUN_TEST_SCENE.md).
