# Plan thêm và hoàn thiện Dreamy Feedback

Ngày: 2026-10-07. Trạng thái: đã thực hiện package, hai sample và Foundation integration. Feedback đã được commit/push tại `1d029ba`; integration sandbox được xuất bản trên nhánh `feat/feedback-integration`. Kết quả validation được ghi tại `LocalPackages/com.dreamy.feedback/VALIDATION.md` và `.omo/evidence/feedback/`. Chưa tạo tag hoặc kiểm tra thiết bị mobile. Các nhận xét khảo sát bên dưới mô tả baseline trước implementation.

## 1. Mục tiêu và phạm vi

Thêm [com.dreamy.feedback](https://github.com/Dreamy-Game-Foundation/com.dreamy.feedback) vào `LocalPackages` dưới dạng Git submodule, dùng local UPM dependency như các package Dreamy hiện tại. Sửa lỗi và chuẩn hóa package; cung cấp sample có scene, prefab, config, UI và hướng dẫn chạy; tích hợp với Foundation demo và các service đang có.

Feedback chịu trách nhiệm biểu diễn kết quả đã được domain xác nhận. Không sở hữu wallet transaction, save, reward, navigation hoặc luật gameplay.

Đầu ra: package local đã chỉnh sửa, hai sample importable, Foundation integration, tests, README/changelog/migration và báo cáo validation. Commit/push/tag là bước phát hành riêng; không coi sửa chưa commit trong submodule là nội dung đã được gitlink repo cha lưu lại.

## 2. Bằng chứng khảo sát

| Nguồn | Quan sát |
| --- | --- |
| Sandbox HEAD | `baed3c0a57eb1caf46611eff64ac6d7de73767d2` |
| `ProjectSettings/ProjectVersion.txt` | Unity `6000.4.12f1` |
| `.gitmodules`, manifest và lock | 16 Dreamy submodule; Feedback chưa được cài; dùng `file:../LocalPackages/...` |
| Feedback upstream | `main`, SHA `a39f7c45e99c445be5b9baba6052ed75b4c6c093`, package `0.1.0` |
| Feedback Runtime | VFX, haptic, floating text, icon fly, screen flash/fade, camera shake, sequence; Core + UniTask + UGUI/TMP |
| Feedback sample | Một script `BasicFeedbackSample.cs` và README; chưa có scene/prefab/database/sample asmdef hoặc test assembly |
| Toolkit canonical | `../dreamy-codex-toolkit/toolkit.json`, version `0.1.0-alpha.2`, status alpha; toolkit HEAD `51e8e3997abb3c3a87e1860e05e0b3f815504b31` |
| Compatibility | Feedback registry pin khớp upstream SHA; registry Core/UI/Audio chưa khớp các SHA sandbox, UI có drift được ghi nhận. Chưa thể gọi toàn bộ tổ hợp là tested/supported |
| Quy tắc local | `docs/rules/DREAMY_PACKAGE_DESIGN_RULES.md`: sample source ở `Samples~`, asset tạo bằng Unity, host glue ở `_Project`, cleanup có owner |
| Worktree ban đầu | Economy, Lucky Wheel, Missions, Progression có sửa sẵn; Tutorial có file chưa track. Phải giữ nguyên các thay đổi ngoài phạm vi |

Repo Feedback được clone để đọc source tại thư mục tạm `C:/Users/trinh/AppData/Local/Temp/dreamy-feedback-review-20261007`; chưa đăng ký nó vào sandbox.

API Core/UI/Audio/Economy trong plan là source quan sát hoặc thiết kế dự kiến. Compatibility của tổ hợp hiện tại phải được xác nhận bằng compile, tests và consumer sample; không suy từ README/registry cũ.

## 3. Vấn đề cần xử lý

| Ưu tiên | Bằng chứng source | Công việc |
| --- | --- | --- |
| P0 | Runtime asmdef tên `Dreamy.Feedback`, references `Unity.ugui` và `UnityEngine.ParticleSystemModule` | Chuẩn hóa thành `Dreamy.Feedback.Runtime`; đối chiếu assembly thật. UGUI cache sandbox có tên `UnityEngine.UI`; không sao chép `Unity.UGUI` từ sample khác. Bỏ engine module khỏi asmdef references nếu Unity tự cung cấp; giữ dependency module trong manifest |
| P0 | `package.json` khai báo TMP `3.0.6`; sandbox UGUI `2.0.0` đã chứa `Unity.TextMeshPro` | Dùng dependency UGUI phù hợp Unity 6000, loại dependency TMP legacy sau khi kiểm resolve sạch |
| P0 | `FeedbackSequenceService.PlayAsync` suppress cancellation của delay rồi gọi action mà không kiểm lại | Hủy trong delay phải chặn action kế tiếp; cleanup theo session; thêm regression |
| P0 | `CameraShakeService` task cũ gọi `Stop()` lên `activeCancellation` dùng chung; original position chụp khi initialize | Session cũ không được dừng/reset session mới. Stop/handle phải thuộc lượt play; restore vị trí theo ownership camera hiện tại |
| P0 | `VfxService`, `FloatingTextService`, `IconFlyService.Clear` chỉ destroy object trong pool | Theo dõi cả instance đang chạy, cancel task, cleanup active + idle, chặn release callback hồi sinh pool sau Clear/Dispose |
| P0 | VFX/floating text handle giữ instance; release có thể push lại nhiều lần | Stop idempotent, handle cũ không tác động instance đã được thuê lại; dùng generation/session hoặc cơ chế tương đương nhỏ nhất |
| P1 | `FeedbackServiceRegistry` chỉ register; sample không cleanup, tự tìm root rồi dereference | Installer/root sở hữu service và registration, cleanup đối xứng, validate dependency trước initialize |
| P1 | `ScreenFeedbackService.Flash` trả handle không có stop action; view callback có thể chạy sau cancellation | Handle Stop có hiệu lực, không completion sau hủy, overlay reset alpha và không chặn input |
| P1 | UI floating text/icon fly dùng trực tiếp `transform.position`; FeedbackRoot tự tạo Transform thường | Thiết lập canvas/RectTransform đúng; tách rõ world/screen/local conversion, dùng camera tường minh và kiểm nhiều aspect ratio |
| P1 | `HapticService.Play` gọi cùng `Handheld.Vibrate` cho mọi loại | Document fallback đúng; toggle enabled, throttle và platform guard; không quảng cáo intensity khác nhau khi chưa có backend hỗ trợ |
| P1 | Author `Dreamy Studio`, docs/sample còn skeleton | Đồng bộ `Dreamy Game Studio`, README, changelog, package samples và menu theo quy ước đã chọn |

Các lỗi lifecycle trên là kết luận từ đọc source, chưa có runtime reproduction. Bước sửa phải bắt đầu bằng test thể hiện failure tương ứng.

## 4. Ownership và dependency

| Thành phần | Owner / vị trí |
| --- | --- |
| Database, preset, ID, validation | Feedback Runtime/Editor; ScriptableObject cho prefab và Unity refs |
| Playback, cancellation, handle, pool | Feedback service; lifecycle owner gọi cleanup khi shutdown |
| Service registration | Composition root; không resolve ServiceLocator trong VFX/pooled item/leaf UI |
| UI demo, presenter/controller, sample builder | `Samples~` với asmdef Runtime/Editor riêng |
| Shared wallet, save, audio, navigation | `GameInstaller`, `FoundationFeatureDemos`, host adapter ở `Assets/_Project` |
| Static game tables / persistent player state | DataConfig / Datasave hiện có; Feedback không tạo source of truth mới |

Dependency Runtime mục tiêu: Core `1.1.2`, UniTask `2.5.10`, UGUI `2.0.0`, ParticleSystem module, thêm engine module nếu source thực sự dùng và cần khai báo. Kiểm phiên bản resolved thực tế thay vì upgrade dependencies hàng loạt.

UI/Audio/Economy/Datasave/DataConfig là dependency của sample tích hợp hoặc host; không thêm vào Feedback Runtime chỉ để sample compile. README liệt kê prerequisites sample và thử import trong consumer chỉ cài Feedback cùng dependency nền.

Giữ namespace/type và GUID hiện hữu khi có thể. Đổi assembly name cần migration cho asmdef refs, generated code, serialized managed types nếu có. Version dự kiến `0.2.0` nếu giữ đổi tên assembly/public lifecycle contract; chốt theo diff cuối cùng, không tự đổi tag đã tồn tại.

Không bắt buộc Addressables/Cinemachine/DOTween trong Feedback. Có thể dùng asset trực tiếp ở sample; host dùng Dreamy Assets/UI theo flow sẵn có. Không refactor hệ thống camera của game để sửa package.

## 5. Các bước thực hiện

### Phase A — Thêm submodule và chuẩn hóa package

1. Chụp status repo cha và từng submodule liên quan; kiểm Editor đúng project và không có scene/prefab dirty trước thao tác asset.
2. `git submodule add https://github.com/Dreamy-Game-Foundation/com.dreamy.feedback.git LocalPackages/com.dreamy.feedback`; checkout SHA đã khảo sát nếu upstream đã đổi.
3. Sửa package metadata/asmdef/dependency blocker trước khi thêm vào UPM, tránh để project resolve package lỗi giữa chừng.
4. Thêm manifest `com.dreamy.feedback: file:../LocalPackages/com.dreamy.feedback`; thêm `testables` sau khi có test assembly. Unity resolve lock; xem diff để tránh thay đổi package khác.
5. Chạy static asmdef/dependency validation, Unity refresh/compile, đọc Console. Lưu baseline và error riêng của Feedback so với lỗi có sẵn.

Gate A: submodule đúng URL/SHA, manifest/lock local khớp, package được Unity nhận và không tạo compile error mới.

### Phase B — Sửa Runtime và Editor

1. Thêm regression cho canceled delay, shake supersession, stale/repeated handle và Clear khi effect đang chạy.
2. Implement lifecycle contract nhỏ nhất: cleanup toàn bộ service, session ownership, release idempotent, cancel trước destroy; reinitialize có policy rõ.
3. Sửa screen flash cancellation/Stop, camera restore, haptic fallback và world-to-UI positioning.
4. Validation kiểm ID rỗng/trùng, prefab/component/ref bắt buộc, duration/prewarm/count, sequence action và ID tham chiếu. Generator chỉ ghi output thuộc owner được chỉ định.
5. Service registration cleanup chỉ gỡ service do owner đã đăng ký; owner cũ không unregister service của owner mới.

Gate B: regression pass, compile sạch, Runtime không tham chiếu Editor, shutdown không còn task/effect/callback của session cũ.

### Phase C — Hai sample đầy đủ

**Basic Feedback:** demo độc lập importable từ Package Manager; có scene, FeedbackRoot prefab, VFX particle prefab, floating text TMP prefab, icon sprite, database/preset và sequence. UI có nút thử đủ 7 nhóm, Play All, Stop/Clear, toggle haptic, trạng thái hướng dẫn. Có camera/world target và vùng UI để thấy coordinate conversion. Haptic thực tế cần device; Editor hiển thị trạng thái phù hợp.

**Feedback + Economy:** UI panel theo phong cách Foundation với wallet balance, Grant, Retry và Close. Flow: domain grant -> số dư thay đổi đã xác nhận -> text/icon fly/SFX/haptic/sequence. Tái sử dụng UI, Audio và Economy; bootstrap độc lập dùng demo wallet/save được ghi rõ, còn sandbox nhận shared service từ host. Không tạo wallet/save thứ hai trong Foundation.

`DatasaveResourceWallet.TryGrant` hiện trả `true` cho transaction trùng nhưng không phát `BalanceChanged`. Không dùng bool này để phát hiệu ứng thành công lần nữa. Adapter nhớ balance trước/sau hoặc subscription scoped theo balance change; bỏ qua replay/không tăng số dư. Icon tới HUD không được quyết định lúc grant tiền. Close/cancel effect không rollback giao dịch đã xác nhận.

Đặt Runtime/Editor sample asmdef riêng, khai báo prerequisites rõ. Builder tạo assets bằng AssetDatabase/PrefabUtility/EditorSceneManager ở đường dẫn mới, không overwrite asset người dùng; phục hồi scene active sau build. Đưa asset đã xác minh về `Samples~` bằng workflow Unity giữ GUID, rồi thử import sạch.

Gate C: cả hai sample import mới, compile, mở scene và Play được; không thiếu script/font/prefab/ref; thử portrait/landscape, safe area, close/back và reopen nhiều lần.

### Phase D — Foundation integration

1. Thêm installer/root Feedback nhận databases, camera và canvas refs; khởi tạo trong thứ tự bootstrap và cleanup ở shutdown.
2. Nối demo Feedback vào `FoundationDemoPanel`, `FoundationFeatureDemos`, Runtime asmdef và `FoundationFeatureDemoBuilder`; reusable UI/presenter giữ ở package sample.
3. Host adapter dùng wallet/save/audio hiện tại; shared balance change cho thấy kết hợp reward từ Economy và flow Daily Reward/Missions/Lucky Wheel khi có tăng số dư thật.
4. Audio dùng `IAudioService` đã bootstrap; không tự dựng AudioService thứ hai. Haptic toggle thuộc demo/host; Settings hiện chưa có contract haptic nên không giả định API tồn tại.
5. Chỉ sửa Economy hoặc sample khác nếu integration buộc phải cần contract mới; giữ các diff đang có. Ưu tiên adapter ở host để giới hạn phạm vi.

Gate D: BootstrapScene Ready, mở Feedback từ Foundation, reward tăng wallet một lần, Retry không tăng/không phát lại, UI và audio hoạt động; các demo hiện tại không regression.

### Phase E — Validation và tài liệu

1. Chạy static gates, targeted EditMode/PlayMode, clean sample import và consumer compile.
2. Smoke standalone/player build cho baseline; device Android/iOS chỉ đánh dấu pass khi có evidence thực, đặc biệt vibration.
3. README install local/Git, dependency/sample prerequisites, bootstrap/cleanup, migration asmdef; CHANGELOG và VALIDATION ghi SHA/Unity/check/result/limitations.
4. Cập nhật README sandbox và bảng package/demo. Compatibility/source ledger toolkit là deliverable tiếp theo chỉ cập nhật khi có commit tương ứng và scope cho repo toolkit.
5. Review scoped diffs, GUID/serialized refs/asmdefs, status submodule; ghi rõ nội dung chưa commit và gate release chưa chạy.

Gate E: báo cáo phân biệt pass/fail/not-run, có log/test XML/ảnh và trạng thái cụ thể cho package, sample, sandbox, clean consumer.

## 6. Ma trận kiểm tra

| Kiểm tra | Kết quả cần có |
| --- | --- |
| Assembly/dependency | Runtime không phụ thuộc Editor; external asmdef refs khớp assembly thật; UPM resolve không TMP duplicate |
| Database/IDs | Rỗng/trùng/missing prefab/duration sai bị báo; valid sample không issue |
| Sequence cancel trong delay | Không chạy action/callback sau hủy; play mới không bị task cũ ảnh hưởng |
| Camera shake A -> B, Stop A | B tiếp tục; hoàn tất/stop khôi phục đúng baseline; destroy camera không exception |
| Handle Stop 2 lần, stale handle sau reuse | Không double-release; không dừng effect mới; pool không cấp cùng instance cho hai play |
| Clear/Dispose khi đang chạy | Active và idle đều dọn; không callback tạo lại pool; reinitialize theo policy |
| Screen cancel/disable | Không completion sau cancel; alpha reset; UI vẫn click được |
| World/UI coordinates | Text/icon hiển thị đúng ở overlay/camera canvas, nhiều aspect ratio |
| Grant rồi Retry | Balance +100 đúng một lần; một nhóm feedback cho grant mới; không replay feedback do duplicate transaction |
| Hide/reopen/unload | Subscription không nhân đôi; không orphan task/service registration |
| Audio/haptic disabled | Không âm thanh/rung trái setting; haptic fallback ghi đúng và kiểm trên device khi có |
| Consumer sạch | Basic chỉ cần nền; integration có dependency đã ghi; sample import không trùng assembly/GUID/Resources key |

## 7. Lệnh validation và baseline đã chạy

Chạy từ sandbox:

```powershell
node ../dreamy-codex-toolkit/harness/dreamy-harness asmdef .
node ../dreamy-codex-toolkit/harness/dreamy-harness package-check .
```

Baseline 2026-10-07: `asmdef` **pass**, parse 65 asmdef; `package-check` **pass**, source project đầy đủ. Đây là static check trước khi cài Feedback, không chứng minh Unity compile hoặc package đã tương thích.

Đã thử `package-check` trên clone Feedback độc lập: harness báo thiếu manifest/lock/ProjectVersion vì adapter này yêu cầu Unity project. Không dùng kết quả đó để kết luận package hỏng; kiểm package bên trong sandbox/consumer.

Sau implementation: chạy lại hai lệnh trên; Unity MCP chọn đúng instance trước refresh/compile, Console và targeted tests. Fallback batchmode/harness khi MCP không dùng được:

```powershell
# Cấu hình DREAMY_UNITY_PATH tới Editor 6000.4.12f1 thực tế trước các lệnh này.
node ../dreamy-codex-toolkit/harness/dreamy-harness compile .
node ../dreamy-codex-toolkit/harness/dreamy-harness test-editmode .
node ../dreamy-codex-toolkit/harness/dreamy-harness test-playmode .
```

Không khởi chạy batchmode đồng thời với Editor đang mở cùng project; chọn MCP hoặc consumer fixture riêng. Khi thiếu Editor/license/device, ghi gate chưa chạy/degraded cùng lý do. `Dreamy/Feedback/Validate All` hiện là menu kiểm tra asset, chưa coi là headless API có structured evidence.

## 8. Tiêu chí hoàn thành

- Feedback là submodule thật ở `LocalPackages`, manifest/lock thống nhất; metadata/Runtime/Editor/sample structure đồng bộ.
- Lỗi lifecycle được test và sửa; không thêm dependency vòng hoặc gameplay/save authority vào Feedback.
- Hai sample đủ assets, import sạch, chạy đầy đủ; Foundation dùng shared wallet/audio/UI và không reward/feedback trùng khi retry.
- README/changelog/migration/validation phản ánh diff và evidence thực tế; giữ nguyên sửa ngoài phạm vi.
- Không gọi release-ready khi consumer/player/device gate bắt buộc còn thiếu; ghi riêng thay đổi chưa commit trong submodule và bước phát hành còn lại.

## 9. Kết quả thực hiện

- Phase A–D đã thực hiện: submodule/UPM, Runtime/Editor, hai sample đầy đủ và Foundation dùng chung wallet/audio/feedback.
- Phase E: static checks pass; 4/4 EditMode, 14/14 PlayMode sandbox, 13/13 PlayMode consumer integration; Windows player build và smoke pass, không log error/exception.
- Import TMP Essential Resources một lần trước khi chạy sample. Sample dùng font ASCII tĩnh; không đóng gói thêm resource TMP Settings trùng với project. Input dùng Old/Both hoặc thay module khi dùng Input System-only.
- Kết quả, fixture và giới hạn nằm ở `LocalPackages/com.dreamy.feedback/VALIDATION.md` và `.omo/evidence/feedback/`. Mobile build/device và phát hành Git là các gate còn lại; implementation local đã hoàn tất.
