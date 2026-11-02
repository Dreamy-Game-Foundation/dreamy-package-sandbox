# Phase 6 — Tích hợp GameInstaller và main Settings (P1)

[Tổng quan](README.md) · [Theo dõi tiến độ](TRACKING.md) · [Thiết kế chung](DESIGN.md)

Trạng thái: **Hoàn tất tích hợp và smoke**.

Phụ thuộc: [phase-05-test-scene.md](phase-05-test-scene.md).

Công việc:

- [x] Sau khi scene riêng pass, thêm profile/service ownership vào GameInstaller.
- [x] Đăng ký service + gateway trước SettingsFeatureInstaller; dùng lại instance inject sẵn nếu host không sở hữu, không Dispose service của owner khác.
- [x] Add asmdef refs tối thiểu cho host/integration; Runtime Settings không reference Localization.
- [x] Gắn localized Settings prefab trong đường load thực tế; cập nhật Addressables theo config đang có nếu prefab thật được load qua Addressables, không tạo hệ load mới.
- [x] Localize một nhóm nhãn main scene để kiểm chứng cùng instance với Settings; không đồng loạt sửa mọi feature.
- [x] Startup fail có trạng thái/diagnostic rõ; cleanup khi GameInstaller fail hoặc destroy, tránh registration còn sót.

## Đầu ra

main bootstrap và Settings sử dụng luồng đã chứng minh ở scene test.

## Điều kiện hoàn thành

- [x] BootstrapScene → MainScene, đổi ngôn ngữ từ Settings áp dụng; scene transition và panel cache vẫn đúng; regression audio/haptics/review không phát sinh.

## Ghi nhận thực hiện

- Revision/package versions: ghi trong `docs/evidence/localization/source-identity.json` (working copies chưa commit).
- Files thay đổi: đã triển khai trong localization package, Settings package và host theo phase.
- Lệnh kiểm tra và kết quả: xem validation và receipts bên dưới.
- Bằng chứng: lưu tại `docs/evidence/localization/phase-06/` khi thực hiện.
- Vấn đề còn mở: chưa ghi nhận.
- Ngày cập nhật: 2026-11-02; trạng thái theo phạm vi/gate ở trên.

## Kết quả thực hiện

GameInstaller + BootstrapScene + actual Foundation prefab đã nối. BootstrapState.Ready và en/vi updates có smoke receipts; âm lượng đổi/khôi phục, gateway cleanup pass. Haptics thiết bị/SDK review chưa test.

Xem [validation](../../evidence/localization/VALIDATION.md) và [cách chạy](RUN_TEST_SCENE.md).
