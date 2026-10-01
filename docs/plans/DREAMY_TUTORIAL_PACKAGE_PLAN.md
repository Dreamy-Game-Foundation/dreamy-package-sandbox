# Plan thiết kế Dreamy Tutorial

Trạng thái: đã triển khai v0.1.0 trong sandbox; xem `LocalPackages/com.dreamy.feature.tutorial/VALIDATION.md` để biết bằng chứng và gate còn lại. Ngày: 2026-10-01.

Giả định phạm vi: tutorial là hướng dẫn người chơi trong game (onboarding/FTUE), gồm lời nhắc, highlight UI, chờ thao tác và nhớ tiến độ.

## 1. Cơ sở thiết kế

- Sandbox dùng Unity `6000.4.12f1`; manifest và lock đang resolve các package Dreamy qua `LocalPackages`.
- Shop và Settings dùng Model–Presenter với view qua interface; concrete panel/controller nằm ở `Samples~`.
- `GameInstaller` đăng ký config trước `InitializeAsync`, cài feature sau khi service nền sẵn sàng, rồi công bố `BootstrapState.Ready`.
- `FoundationDemoRoot` là điểm tích hợp demo; khi mở Shop, Foundation panel bị hide và có thể bị hủy. Tutorial cần đợi target của panel mới thay vì giữ reference panel cũ.
- Áp dụng [quy tắc package](../rules/DREAMY_PACKAGE_DESIGN_RULES.md). Checkout này chưa tìm thấy `toolkit.json`, compatibility catalog hoặc skill `omg-*` trong các thư mục skill đã kiểm tra. API bên dưới là hợp đồng đề xuất; API tích hợp phải được đối chiếu lại khi triển khai.
- Worktree có thay đổi sẵn ở sample và submodule; plan không sửa các thay đổi đó.

## 2. Package và phạm vi v0.1.0

Tên: `com.dreamy.feature.tutorial`. Namespace: `Dreamy.Tutorial`.

Mục tiêu: tái sử dụng cùng engine chạy tutorial cho nhiều game, trong khi game quyết định khi nào bắt đầu, target nào được hướng dẫn và hành động nào đã thành công.

Trong v0.1.0:

- Nhiều flow có ID ổn định; mỗi flow là danh sách step tuyến tính.
- Một flow hoạt động tại một thời điểm; start trùng không tạo session mới; start flow khác trả `Busy`.
- Ba chế độ hoàn thành step: `Next`, `TargetClick`, `HostSignal`.
- Tooltip safe area, spotlight hình chữ nhật cho UI/3D và chặn raycast UI ngoài target.
- Skip theo policy của flow; suspend/resume khi host UI chưa sẵn sàng; lưu checkpoint giữa các step.
- Sample độc lập gồm UI Button/HostSignal và Collider 3D/TargetClick, với prefab/scene tạo qua Unity Editor, cùng adapter demo Foundation.

Để sau v0.1.0: branching graph, visual graph editor, outline shader, camera steering, swipe/drag, nhiều spotlight, auto queue, rewards, analytics SDK và rollout/A-B testing. Game có thể phát signal cho thao tác riêng mà không thêm luật gameplay vào package.

## 3. Ownership và dependency

| Thành phần | Trách nhiệm | Vị trí |
| --- | --- | --- |
| `TutorialCatalogConfig` | Validate flow/step, ID, policy, target và signal | Runtime/Config |
| `TutorialModel` | State machine, chọn step, kiểm tra token, skip và checkpoint | Runtime/Domain |
| `TutorialSaveData` | Trạng thái persistent và schema migration | Runtime/Persistence |
| `TutorialPresenter` | Render view, chờ target, nối input với service | Runtime/Presentation |
| `ITutorialView` | Contract hiển thị và input | Runtime/Contracts |
| `TutorialTargetRegistry` | Đăng ký target đang sống, tìm bằng ID, báo thay đổi | Integration/Runtime |
| `TutorialUITarget` / `TutorialWorldTarget` | Adapter RectTransform/Button và Camera/Collider, register/unregister theo lifecycle | Integration/Runtime |
| `TutorialOverlay` | Tooltip, spotlight, raycast filter và safe area | Integration/Runtime |
| `TutorialController` | Tạo/dispose presenter, gắn host lifecycle | Integration/Runtime |
| Adapter của game | Start trigger, navigation, signal gameplay và khóa input ngoài UI | Assets/_Project |

Dependency Runtime: Core, DataConfig, Datasave và Newtonsoft.Json. Integration Runtime khai báo thêm UGUI/Physics trong manifest. Domain không phụ thuộc Shop, Settings, Economy hoặc prefab. Bắt đầu với execution đồng bộ và event; chỉ thêm UniTask nếu có nhu cầu async công khai thật sự.

Integration Runtime có asmdef riêng tham chiếu Tutorial Runtime và UGUI. Sample không bắt buộc Dreamy Feature/UI; sample Editor dùng Unity Input System. Editor validator có assembly chỉ dành cho Editor. Kiểm tra dependency của sample khi import sạch; không kéo dependency UI vào domain chỉ để sample compile.

## 4. Contract đề xuất

`ITutorialService`:

- `TryStart(flowId)`: trả kết quả có nghĩa như `Started`, `AlreadyActive`, `AlreadyCompleted`, `Skipped`, `Busy`, `InvalidFlow`.
- `GetState()` và event `StateChanged`: snapshot readonly gồm flow, step, status và token của step hiện tại.
- `TryAdvance(stepToken)` dành cho `Next`; `ReportTargetClick(targetId, stepToken)` dành cho click đúng target.
- `ReportSignal(signalKey, stepToken)`: chỉ nhận signal đúng step/token đã active; cho phép target biến mất trong lúc navigation; host gọi sau khi thao tác thật sự thành công.
- `Suspend()` / `Resume()`: giữ checkpoint, không tính là complete hoặc skip.
- `TrySkip()`: kiểm tra policy, ghi trạng thái `Skipped` riêng với `Completed`.

Không expose API production để tùy ý đánh dấu cả flow completed. Reset/replay nằm trong công cụ development hoặc sample và chỉ xóa save tutorial.

`ITutorialView` nhận view state, phát `NextRequested`, `SkipRequested`, `RetryRequested`; click được nhận qua `ITutorialTarget.Clicked`. Target lookup dùng interface chỉ tại boundary presenter–host; domain không nhận `RectTransform` hoặc gọi ServiceLocator. Installer/root resolve service một lần rồi truyền dependency tường minh.

## 5. Config và save

Config JSON dự kiến: `tutorialCatalog`, chứa `catalogId`, `revision`, `flows` và các step.

Flow: `id`, `allowSkip`, `steps`. Step: `id`, `messageKey`, `targetId` tùy chọn, `completionMode`, `signalKey` tùy chọn và `blockOutsideTarget`. Tooltip v0.1 ở cạnh dưới safe area. Presentation key được host resolve ra text; chưa giả định có package Localization.

Validation: flow ID duy nhất trong catalog; step ID duy nhất trong flow; danh sách không rỗng; `TargetClick` phải có target; `HostSignal` phải có signal; enum hợp lệ; không bật spotlight/chặn ngoài target khi không có target. Reference scene/prefab được kiểm tra ở integration, không coi target chưa tồn tại lúc bootstrap là config lỗi.

Save: schema version, catalog ID/revision và map `flowId -> { status, nextStepId }`. Không lưu index, GameObject, RectTransform, listener hoặc token session. Hoàn thành step mới ghi checkpoint; resume có thể lặp phần trình bày của step chưa hoàn thành.

Khi config đổi: giữ trạng thái terminal của flow có ID cũ; checkpoint còn ID thì tiếp tục; step bị xóa thì dùng mapping migration tường minh, nếu thiếu mapping thì suspend flow với lỗi recoverable. Không tự động complete hoặc reset một flow có thao tác gameplay. Catalog được snapshot khi start, không thay definition giữa session.

Save thất bại: không chuyển sang step kế tiếp; trả lỗi và cho retry. Tutorial không thực thi hoặc lặp lại thao tác gameplay để retry save.

## 6. Execution, UI và lifecycle

Luồng trạng thái: `Idle -> WaitingForTarget -> ActiveStep -> WaitingForTarget/ActiveStep -> Completed`. `Suspended` và `Skipped` là các nhánh riêng. Step không có target đi thẳng vào `ActiveStep`.

- Mỗi lần activate step sinh token mới. Callback cũ, click kép và signal trùng không được advance thêm step.
- Presenter subscribe trước khi kiểm tra trạng thái hiện tại để tránh bỏ lỡ signal. HostSignal cần token của session/step được host capture, không lấy token mới khi callback cũ quay về.
- Target register ở `OnEnable`, unregister ở `OnDisable`/destroy. ID trùng trong cùng scope được báo lỗi; không chọn tùy ý một object.
- Target mất hoặc panel đóng: bỏ highlight, gỡ raycast blocker rồi vào `WaitingForTarget`; target quay lại thì render cùng step. Không tự bỏ qua step.
- Trong trạng thái chờ, host vẫn có thể điều hướng để khôi phục UI. Timer chẩn đoán cảnh báo target thiếu; không tự động tiến tutorial khi timeout.
- Overlay có sorting rõ, dùng tọa độ Canvas/camera phù hợp, cập nhật sau layout và khi resolution/orientation đổi. Tooltip nằm trong safe area, spotlight bám bounds target.
- `TargetClick` cho click xuyên đúng vùng target để button gốc xử lý, không gọi `Button.onClick.Invoke()` giả lập. Với hành động cần xác nhận thành công dùng `HostSignal`.
- Raycast filter chặn ngoài vùng target và cho nút Next/Skip hoạt động. Overlay chỉ kiểm soát UGUI; host chịu trách nhiệm khóa gameplay, keyboard/gamepad và xử lý Back theo policy khi tutorial modal.
- Presenter/controller dispose đối xứng: listener, target subscription, tween và async nếu có; sau destroy không callback vào view. App pause flush checkpoint; không coi pause là skip.

## 7. Sample và Foundation demo

Sample độc lập: `TutorialDemo`, `TutorialOverlay.prefab`, target component, config JSON, fake host action và hướng dẫn bootstrap rõ ràng. Không bắt buộc cài Shop để chạy sample package.

Foundation flow dự kiến:

1. Chào người chơi, nhấn Next.
2. Highlight nút Add Score, chờ signal từ host sau khi score cập nhật.
3. Highlight Open Shop, chờ host xác nhận Shop đã mở thành công.
4. Đợi target Close trên Shop, hướng dẫn đóng; host xác nhận Foundation panel đã trở lại.
5. Hiện lời nhắc hoàn tất và ghi `Completed`.

Không mua hàng hoặc ghi đè balance/save gameplay để trình diễn tutorial. Target được gắn trong prefab nguồn qua Unity; nếu sửa bản import ở sandbox thì đưa thay đổi được chấp nhận về `Samples~` và thử import sạch.

Integration order: register config -> initialize DataConfig -> tạo tutorial service với Datasave -> register service -> bootstrap Ready -> panel/target sẵn sàng -> host start flow. Host navigation không nằm trong TutorialModel.

## 8. Các phase triển khai

| Phase | Kết quả review được | Điều kiện hoàn thành |
| --- | --- | --- |
| 1 — Domain | Package manifest, asmdef, config, model, contracts và persistence | EditMode tests cho transition, validation, dedupe, save/resume và migration |
| 2 — Presentation | Presenter và fake view/target adapters | Test bind/dispose, target mất/quay lại, stale callback và save failure |
| 3 — Sample | Prefab overlay, target registry, controller, JSON và README | Import sạch, compile, reference đầy đủ; demo chạy bằng mouse/touch |
| 4 — Foundation | Adapter/root và target trên Foundation/Shop | Hoàn tất flow; mở lại app resume; không nhân listener qua nhiều lần mở/đóng |
| 5 — Hardening | Editor validator, changelog, compatibility evidence và release config | Runtime không có Editor reference; consumer gate và platform smoke test đã chọn |

## 9. Kiểm chứng trước release

- EditMode: ID/enum/config lỗi; start trùng/busy; sai token/signal; skip policy; progress save/load; thay revision; save failure.
- PlayMode: target chưa spawn/disabled/destroyed; chuyển panel; click xuyên spotlight; chặn click ngoài; Next/Skip; đóng view; click kép; callback sau dispose.
- UI: portrait/landscape, safe area, Canvas scaling/camera, target trong ScrollRect và layout đổi; tooltip không ra ngoài màn hình.
- Consumer: sample import một lần, không trùng class/asmdef/resource key; prefab không missing script/reference; Console và compile sạch.
- Demo: lần đầu start, lần sau không auto replay flow terminal, suspend/resume đúng checkpoint; reset development chỉ xóa dữ liệu tutorial.

Implementation gồm Runtime, Integration Runtime, Editor validator, sample UI/3D và Foundation adapter. Bằng chứng compile/test/prefab và giới hạn được ghi trong `LocalPackages/com.dreamy.feature.tutorial/VALIDATION.md`; chưa có device/release gate.

## 10. Target 3D trong v0.1

`ITutorialTarget` giữ ID, availability và click event; `ITutorialScreenTarget` ở Integration thêm screen rect và kiểm tra input. `TutorialWorldTarget` dùng camera được gán tường minh, collider bounds và raycast tới đúng collider. Target ngoài viewport hoặc cắt near/far plane sẽ chờ. Spotlight không phụ thuộc shader/render pipeline. Occlusion được kiểm tra lúc click; host dùng HostSignal cho di chuyển, đánh quái và tương tác gameplay.

Host quản lý camera, keyboard/gamepad, khóa gameplay và khôi phục scene/panel khi resume. Foundation adapter khôi phục Shop cho checkpoint Close Shop. Pointer ngoài màn hình và outline để phase sau.
