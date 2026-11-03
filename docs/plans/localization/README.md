# Kế hoạch Localization — JSON theo locale, Settings và scene test

Ngày: 2026-11-02. **Pilot đã triển khai và kiểm chứng; các gate release còn mở.**

## Mục tiêu nghiệm thu

Chạy menu **Dreamy → Localization → Play Settings Test**, mở Settings và chọn trong 12 ngôn ngữ: UI trong view và bên ngoài đổi ngay, không reload scene. Scene độc lập với PanelManager. Đóng/mở view và khởi động lại giữ locale đã áp dụng thành công.

## Tài liệu

- [Phân tích chức năng và luồng hiện tại](CURRENT_STATE.md).
- [Thiết kế dữ liệu, gateway và quyền sở hữu](DESIGN.md).
- [Bảng tiến độ và checkpoint](TRACKING.md).
- [Cách chạy scene, sửa JSON và import](RUN_TEST_SCENE.md).
- [Nghiên cứu GitHub và API component](COMPONENT_API_RESEARCH.md).

## Các phase

| Phase | Nội dung | Đầu ra chính |
| --- | --- | --- |
| [0](phase-00-baseline.md) | Baseline và dependency | Resolve, compile, tests hiện có |
| [1](phase-01-json-import.md) | JSON authoring/import | en.json, vi.json, validation, Unity Tables |
| [2](phase-02-runtime-service.md) | Service và lifecycle | Locale switch, fallback, persistence, cleanup |
| [3](phase-03-settings-gateway.md) | Gateway và contracts | Settings language state, adapter, presenter |
| [4](phase-04-settings-panel.md) | Panel và bindings | Selector và toàn bộ nhãn Settings được dịch |
| [5](phase-05-test-scene.md) | Scene độc lập | Live apply trong/ngoài Settings |
| [6](phase-06-game-integration.md) | Main game | GameInstaller và đường load Settings thật |
| [7](phase-07-verification.md) | Verification và bàn giao | Regression, screenshots, hướng dẫn setup |

## Milestone

- **M1 — Nền tảng:** Phase 0–2, JSON import được và service đổi locale ổn định.
- **M2 — Scene chạy được:** Phase 3–5 kèm các kiểm tra Phase 7 tương ứng, dùng Settings/gateway thật.
- **M3 — Tích hợp hoàn tất:** Phase 6 và nghiệm thu toàn bộ Phase 7.

Tests chạy theo từng phase; Phase 7 tổng hợp regression và bàn giao, không trì hoãn mọi verification tới cuối.

## Quyết định đã chốt

- Mỗi locale một JSON; các file dùng chung key, source/fallback là en. Fixture hiện có en/vi/de/es/fr/id/ja/ko/pt-BR/ru/th/zh-Hans.
- JSON → Editor validate/import → Unity Localization Tables → runtime binding.
- Settings gọi gateway; localization sở hữu locale/preference. Panel không trực tiếp ghi save.
- Giữ Unity Localization làm engine và giữ serialized references/GUID hiện có.
- Asset Tables, scanner toàn project và CSV conversion nằm ngoài pilot đầu tiên.

## Cách track

1. Bắt đầu từ Phase 0, cập nhật trạng thái ở TRACKING.md và file phase.
2. Đánh dấu checklist theo phần đã thực hiện; ghi revision, kết quả và giới hạn verification.
3. Lưu bằng chứng mới ở docs/evidence/localization/phase-XX/.
4. Chỉ đóng phase khi điều kiện hoàn thành được chứng minh; cập nhật checkpoint trước khi kết thúc đợt làm việc.

## Implementation pilot — 2026-11-02

Scene, JSON import, runtime service, Settings gateway/panel và main bootstrap đã được triển khai. Xem [cách chạy](RUN_TEST_SCENE.md), [tracker](TRACKING.md) và [validation](../../evidence/localization/VALIDATION.md). Kết quả hiện có: 29 EditMode và 9 PlayMode tests pass; các gate consumer/build/device và phạm vi mở rộng vẫn được ghi riêng.
