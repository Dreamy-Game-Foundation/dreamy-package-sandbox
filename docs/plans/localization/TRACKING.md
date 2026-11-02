# Theo dõi triển khai Localization

[Tổng quan](README.md) · [Cách chạy](RUN_TEST_SCENE.md) · [Validation](../../evidence/localization/VALIDATION.md)

Ngày cập nhật: 2026-11-02. **Pilot JSON → service → Settings gateway → scene → main game đã triển khai.**

- [x] Phân tích hiện trạng, tách kế hoạch thành Markdown và dọn dữ liệu công cụ cũ.
- [x] Baseline dependency/Editor và 15 EditMode tests.
- [x] JSON theo locale, validator, preview và Unity table import.
- [x] Service UniTask/lifecycle/fallback/preload cho pilot.
- [x] Optional Settings language gateway/service/view và host adapter.
- [x] Selector/prefab localize, scene kiểm thử và GameInstaller integration.
- [x] 29 EditMode + 9 PlayMode tests, main-game smoke và screenshots.
- [ ] Các gate hardening/release dưới đây.

| Phase | Trạng thái | Verification |
| --- | --- | --- |
| [0](phase-00-baseline.md) | Hoàn tất sandbox baseline | 15/15 baseline tests |
| [1](phase-01-json-import.md) | Core import hoàn tất | Coverage, read-only preview, stale plan, ID/GUID, idempotence pass |
| [2](phase-02-runtime-service.md) | Pilot hoàn tất; hardening còn mở | Init/dispose/cancel/same/unsupported locale pass |
| [3](phase-03-settings-gateway.md) | Pilot hoàn tất | External change, no gateway, gateway failure pass |
| [4](phase-04-settings-panel.md) | Pilot hoàn tất | English/Vietnamese labels và portrait screenshots |
| [5](phase-05-test-scene.md) | Hoàn tất scene/test slice | Live apply, arguments, reopen, preference/reload pass |
| [6](phase-06-game-integration.md) | Hoàn tất tích hợp | Bootstrap→Main Ready, label changes, audio/cleanup smoke |
| [7](phase-07-verification.md) | Pilot pass; release chưa hoàn tất | 29/29 EditMode, 9/9 PlayMode |

## Gate còn mở

- [ ] Backend seam và test toàn bộ initialization retry, store failure/concurrency edge cases.
- [ ] Cưỡng bức lỗi giữa apply/crash recovery để xác minh đường rollback.
- [ ] Fresh external consumer/Unity 6000.0 fixture.
- [ ] Player/mobile build, landscape layout và thiết bị thật.
- [ ] Portable package-only sample scene; hiện scene đã kiểm chứng nằm trong sandbox host.

Retained table leases, global persistent-variable adapter, full asset JSON import và scanner là mở rộng ngoài string pilot. Các policy chưa hỗ trợ bị reject rõ; không coi đã hoàn tất.

## Checkpoint

- Scene: Assets/_Project/Scenes/LocalizationSettingsTest.unity.
- Chạy bằng menu Dreamy → Localization → Play Settings Test; tránh Start Scene toolbar ép bootstrap.
- Không commit/push/tag/release. Working copies package có thay đổi chưa commit.
- Các lỗi quan sát và giới hạn bằng chứng ghi trong validation; không tính các lượt test bị kẹt/không có test là pass.

## Nhật ký

| Ngày | Nội dung | Bằng chứng |
| --- | --- | --- |
| 2026-11-02 | Tạo baseline và triển khai pilot JSON/Settings/scene/main | docs/evidence/localization |
| 2026-11-02 | Sửa Smart String warm-cache startup race | Initial failure JSON và final PlayMode XML |
| 2026-11-02 | Hoàn tất core verification | 29 EditMode, 9 PlayMode, main smoke |

## Component API và scene độc lập — cập nhật tiếp theo

- [x] Đối chiếu GitHub default branch/pinned commits và tài liệu Unity 1.5.13; giữ dependency đang resolve 1.5.8.
- [x] Reset/auto target/fallback và setter API cho cả sáu component; bảo toàn field/GUID cũ.
- [x] Tách LocalizationSettingsTest khỏi PanelManager, dùng StandaloneSettingsView và presenter trực tiếp.
- [x] 41/41 EditMode, 13/13 PlayMode pass; mỗi fixture xác nhận không có PanelManager.
- [x] Ghi [research/API contract](COMPONENT_API_RESEARCH.md) và [evidence mới](../../evidence/localization/component-api/VALIDATION.md).

Các số 29/9 ở mục pilot phía trên là baseline lịch sử; số hiện tại là 41/13. Không mở rộng tuyên bố kiểm chứng sang asset-table load failure/player build/device.
