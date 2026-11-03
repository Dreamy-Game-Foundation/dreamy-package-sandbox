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

## 12 ngôn ngữ và dropdown — 2026-11-03

- [x] Thêm de/es/fr/id/ja/ko/pt-BR/ru/th/zh-Hans; tổng 12 JSON, mỗi file 18 key và cùng selector `{amount}`.
- [x] Import locales, Unity String Tables và Addressables; giữ source/fallback `en`.
- [x] Thêm font fallback CJK/Thái, giữ GUID font gốc; kèm giấy phép và script tạo CJK subset.
- [x] Sửa item height 20 → 64, label padding/auto sizing và vùng cuộn 328; áp dụng vào production Settings và standalone view.
- [x] Menu Refresh Languages And Dropdown tự nhận JSON, giữ scene và fileID prefab hiện có.
- [x] 41/41 EditMode, 15/15 PlayMode pass; ảnh đầu/cuối danh sách và Settings Nhật/Thái đã xem trực tiếp.
- [x] [Evidence và checkpoint hoàn tất](../../evidence/localization/multilanguage/VALIDATION.md).

Số hiện tại là 41/15; các số ở checkpoint cũ là lịch sử. Bản dịch mới là nội dung kiểm thử; chưa native review hoặc player/device validation.

## Production Starter, metadata và Settings tùy chọn — 2026-11-03

- [x] Author Dreamy Game Studio; source version Localization 0.2.0 và Settings 0.3.0; README/CHANGELOG đồng bộ, chưa tạo tag/publish.
- [x] Tách rõ code package giữ ổn định khi update và Assets game được tùy chỉnh; ghi quy trình diff/merge sample khi nâng cấp.
- [x] Production Starter có JSON, font nguồn Latin/CJK/Thái đầy đủ, TMP fallbacks, profiles, bootstrap/label prefab; không chứa PanelManager hoặc scene demo bắt buộc.
- [x] Settings Integration là sample opt-in, adapter chuyển khỏi host vào sample và root dùng bản đã import; giữ hai Runtime package không phụ thuộc lẫn nhau.
- [x] Settings package sample prefab có selector thật; thiếu gateway/options thì ẩn dropdown/label/error và xóa option cũ.
- [x] 41/41 EditMode, 15/15 PlayMode pass với ca no-gateway trên SettingsPanel thật; consumer mới không cài Settings import 12×18 và chạy en→fr→ja thành công.
- [x] [Receipt và checkpoint](../../evidence/localization/production-starter/VALIDATION.md).

Consumer mới kiểm chứng Unity 6000.4.12f1, không thay cho gate Unity 6000.0/player/device. Starter là prefab/data sản phẩm có thể tùy chỉnh; không gộp scene test vào PanelManager. Các số version ở folder Assets/Samples cũ không tự đổi khi nâng package.

## Sandbox production + Foundation + Country Flags — 2026-11-03

- [x] Promote imported Production Starter / Settings Integration / Country Flags into game-owned Assets/_Project/Localization folders, preserving GUIDs.
- [x] Canonical source/profile moved to Production/Source and Production/Profiles/JsonImport.asset; 12 locales × 50 keys, existing generated table ownership unchanged.
- [x] Bootstrap, Foundation, Settings and manager-free test scene use production font/profile assets.
- [x] Foundation static labels and dynamic score/health/status use stable keys and runtime arguments.
- [x] Country Flags sample: 271 rounded 256px PNGs, MIT license, pinned source/blob hashes and Sprite metas; game icon catalog maps 12 locales explicitly.
- [x] Dropdown option and selected caption icons; text-only and no-gateway paths retained.
- [x] Fresh regression: 41/41 EditMode và 17/17 PlayMode pass; original EditMode attempt stalled, targeted restart and streamed-hash snapshots recovered verification.
- [x] Main game Ready; ảnh Foundation Việt/Anh và dropdown có cờ đã kiểm tra. PanelManager UIRoot canvas được chuẩn hóa 1080×1920 để Settings vừa khung nhìn nhỏ.

See docs/evidence/localization/sandbox-flags for current checkpoint and verification status.

## Dọn sample giữ chỗ — 2026-11-03

- [x] Xóa Basic Localization (chỉ có README) và .meta ở package lẫn bản import trong Assets.
- [x] Bỏ entry manifest và phần giới thiệu README; ghi changelog, giữ ba sample có nội dung.
- [x] Kiểm tra JSON manifest, đường dẫn ba sample và diff whitespace; không đổi runtime/prefab.

## Commit toàn bộ — 2026-11-03

Theo yêu cầu người dùng, commit các working copy Audio/Feedback/Settings/Localization trước rồi commit sandbox cùng gitlink mới và mọi thay đổi hiện có. Xem [commit checkpoint](../../evidence/localization/commit-checkpoint-2026-11-03.md) cho hash submodule và scope. Chưa push/tag/publish; các ghi chú “chưa commit” ở checkpoint cũ là lịch sử trước yêu cầu này.
