# dreamy.debugger — Kế hoạch phát triển theo phase

Ngày cập nhật: 2026-11-04. Trạng thái: hoàn tất nghiên cứu và plan; chưa triển khai hoặc xác minh runtime.

## 1. Định hướng đã chốt

- Tạo package độc lập `com.dreamy.debugger`, namespace `Dreamy.Debugger`, thương hiệu **Dreamy Debugger**; không bắt buộc Dreamy Core, UI, Feature, Datasave, Assets hay Graphy.
- Fork và tái cấu trúc SRDebugger/SRF thành code do Dreamy quản lý. Thay định danh SR/StompyRobot trong sản phẩm bằng Dreamy hoặc tên theo trách nhiệm, có migration và attribution. Hướng này thay thế phương án chỉ bọc adapter trong plan trước.
- Gesture cấu hình được, hỗ trợ tap 5 lần; PIN dài hơn và passphrase; HUD FPS có thể dùng độc lập với panel debug.
- Chụp ảnh, lưu lỗi và gửi Telegram/email theo cơ chế best effort. Trải nghiệm người chơi có ưu tiên cao hơn việc thu đủ diagnostics.
- Runtime của package là phần dùng chung, consumer không sửa trực tiếp. Sample/glue/config/theme ở project là phần được tùy chỉnh.
- Phát hành tăng dần qua các phase có đầu ra và gate. Native crash, ANR và OOM không được quảng bá là đã hỗ trợ chỉ vì thu được managed exception.

## 2. Bằng chứng và giới hạn nghiên cứu

### 2.1. Source đang import

| Nội dung | Quan sát tại repository | Hệ quả cho triển khai |
| --- | --- | --- |
| Phiên bản | `Assets/Packages/StompyRobot/SRDebugger/Scripts/Version.cs`: 1.12.1 | Giữ baseline/hash để truy nguồn patch |
| Tap | `UI/Controls/MultiTapButton.cs`: `RequiredTapCount`, `ResetTime`, dùng unscaled time | Có nền tảng multi-tap; settings chưa expose số tùy ý |
| Trigger | `Settings.cs`, `Services/Implementation/DebugTriggerImpl.cs`: double/triple tap, tap-and-hold | Thêm config và state machine gesture, bỏ switch chỉ hỗ trợ ba kiểu |
| PIN | `Settings.EntryCode`, `PinEntryServiceImpl.VerifyPin`: đúng 4 số | Phải sửa cả validation, service, UI và migration settings |
| Keypad | `UI/Controls/PinEntryControl.cs`: numeric, dot array; truy cập `Keyboard.current` | Thêm UI độ dài linh hoạt, passphrase; kiểm mobile không có keyboard device |
| Screenshot | `Internal/BugReportScreenshotUtil.cs`: full screen Texture2D, ReadPixels, Apply, EncodeToPNG | Không dùng nguyên pipeline này cho auto-report cần ít giật |
| Report | `IBugReporterHandler`, `IDebugService`: có điểm thay handler | Tái sử dụng ý tưởng tách transport; không giữ endpoint vendor làm mặc định |
| Assemblies | SRDebugger runtime tham chiếu SRF + Input System; Editor có assembly riêng; disable constraint có sẵn | Cần tách dependency và kiểm strip thật trong player |

Project tại lần khảo sát: Unity **6000.4.12f1**; manifest/lock cùng ghi uGUI **2.0.0**, Input System **1.19.0**, URP **17.4.0**, Test Framework **1.6.0**. Package hiện diện không chứng minh tương thích. Chưa chạy compile, Play Mode, build hay profiling.

Workflow `docs/agents/dreamy-workflow.md` và registry `compatibility/dreamy-packages.json` chưa tìm thấy trong repo ở lượt khảo sát trước; CLI `dreamy` chưa có trên PATH. Chưa recall lesson/checkpoint bằng toolkit; checkpoint thủ công nằm cuối file. Không đưa ra claim API tích hợp của package Dreamy khác.

### 2.2. Graphy: tham khảo và chọn lọc

[Graphy README](https://github.com/Tayx94/graphy) mô tả FPS/memory/audio dạng số và graph, layout, hotkey, device info và trigger theo điều kiện. Chọn học cách tách HUD và mức hiển thị; không import toàn bộ tool. Không đưa audio monitor hay auto-screenshot theo FPS vào MVP. Đây là quyết định thiết kế Dreamy, chưa phải benchmark so sánh.

Mốc nghiên cứu upstream: `1068f3de2bc9f1e4a905150abd59f31e6dead679`. Nếu lấy code/asset, phải ghi file và commit thực sự được lấy, giữ [MIT notice](https://github.com/Tayx94/graphy/blob/1068f3de2bc9f1e4a905150abd59f31e6dead679/LICENSE), kiểm license asset riêng. Không bắt consumer cài Graphy; không chạy hai profiler collector song song.

## 3. Kiến trúc package độc lập

### 3.1. Cấu trúc dự kiến

```text
com.dreamy.debugger/
  package.json
  Runtime/
    Foundation/         bootstrap, lifecycle, settings snapshot, contracts
    Console/            bounded log buffer, filter, export
    Access/             gesture, unlock, session
    Metrics/            sampling, history buffer
    UI/                 overlay, HUD, views, reusable prefabs
    Reporting/          report policy, capture, durable outbox, uploader
  Editor/               settings, validators, migration, build checks
  Tests/                EditMode, PlayMode, consumer fixtures
  Samples~/
    QuickStart/         bootstrap + demo scene
    ProjectCommands/    game-specific commands
    CustomTheme/        editable views/theme/config
    ReportIntegration/ relay config + fake transport demo
  Documentation~/
  THIRD_PARTY_NOTICES.md
  CHANGELOG.md
```

Các tên trên là đề xuất, không phải API đã tồn tại. Không chia quá nhiều asmdef nếu chưa cần; tối thiểu tách runtime foundation, UI, reporting, Editor và test. Dependency hướng từ UI/reporting tới foundation; foundation không tham chiếu UI/reporting/sample. Editor tham chiếu runtime; runtime không tham chiếu UnityEditor.

- Dependency Unity dự kiến: foundation dùng module Unity cần thiết; UI dùng uGUI, đánh giá dùng Text hiện có hay TMP sau inventory. Không thêm TMP chỉ để đổi tên.
- Input System là backend tùy chọn, dùng adapter/asmdef có điều kiện và version define; không giữ unconditional reference khiến consumer thiếu Input System không compile. Có backend legacy khi project cho phép và điểm gọi gesture từ host UI.
- Hỗ trợ built-in/URP chỉ được công bố sau kiểm capture/overlay trên từng pipeline. HDRP/WebGL để trạng thái chưa xác minh; nền tảng không có worker thread cần chế độ report chỉ text hoặc trì hoãn.
- Dùng composition và dependency truyền từ bootstrap; không thêm service locator/DI framework để thay SRF. Chỉ đưa helper SRF thực sự dùng vào foundation nội bộ.
- Debugger sở hữu overlay Canvas riêng, lifecycle riêng, không phụ thuộc BaseFeaturePanel/BaseFeatureItem. Đây là ngoại lệ có chủ đích cho diagnostic tool độc lập; vẫn kiểm safe area, input, raycast và scene transition theo quy tắc repo.
- Tích hợp Dreamy khác là module bổ sung về sau, không có trong manifest bắt buộc. Game gửi context qua provider/command contract.

### 3.2. Ranh giới runtime ổn định và phần được sửa

“Immutable runtime” nghĩa là consumer không sửa source package; không có nghĩa toàn bộ runtime state bất biến.

| Khu vực | Chủ sở hữu | Cách tùy chỉnh hợp lệ |
| --- | --- | --- |
| Runtime/Editor dùng chung | Maintainer package | Thay đổi có version, changelog, kiểm regression; không patch từng game |
| Config schema, contracts | Package | Project tạo asset config; bootstrap validate rồi copy thành snapshot chỉ đọc |
| Log, metric, unlock session, outbox | Service runtime tương ứng | Mutable có giới hạn; không ghi ngược config asset hoặc lưu vào package |
| Prefab/theme mặc định | Package | Project tạo variant hoặc copy có hướng dẫn, không sửa base package |
| Samples~ | Package giữ bản mẫu; project sở hữu bản import | Sửa command, bootstrap, theme, scene và provider trong Assets của game |
| Runtime data | App storage | Outbox/settings preference trong persistent storage; có schema, quota và cleanup |
| Credentials gửi tin | Relay/server | Không nằm trong sample, prefab, config hoặc binary game |

Runtime không tham chiếu type trong sample. Sample không tự chạy khi chỉ cài package; import và gắn bootstrap là thao tác rõ ràng. Mỗi sample có README, namespace và asmdef riêng. Upgrade package không ghi đè phần đã import vào project.

## 4. Thiết kế chức năng

### 4.1. Gesture và mở khóa

- Preset QA đề xuất: **5 tap liên tiếp trong vùng góc nhỏ → PIN `0000`**. Cho phép cấu hình count 1–20, khoảng cách tối đa giữa hai tap và timeout toàn chuỗi; giá trị khởi đầu 0,4 giây/tap, 2,5 giây/chuỗi, phải thử trên thiết bị.
- Tùy chọn nâng cao: giữ hai ngón rồi multi-tap; triển khai sau gesture đơn giản. Desktop có hotkey cấu hình được.
- Reset chuỗi khi quá hạn, đổi scene/focus, bị cancel hoặc pointer lệch vùng vượt tolerance; chống đếm nhiều pointer và click tổng hợp từ touch thành hai tap.
- Dùng unscaled time; không capture touch toàn màn hình. Vùng trigger nằm trong safe area, tránh nút game; ưu tiên host-forwarded input hoặc passive observer. Nếu dùng vùng raycast riêng phải kiểm che input; khi xung đột thì chuyển vị trí hoặc disable trigger.
- PIN linh hoạt 4–32 số, preset `0000`; passphrase tùy chọn 8–64 ký tự với trường nhập riêng, không kéo dài keypad bốn chấm hiện tại. Không log credential, không lưu plain text trong PlayerPrefs.
- Unlock state machine: Locked → CollectingGesture → CredentialPrompt → Unlocked → Relocked/Cooldown. Nhập sai 5 lần khóa tạm 30 giây, thời gian unscaled; hết phiên, app background hoặc hết TTL thì khóa lại theo preset.
- Lưu verifier có salt cho credential local; không nhúng mật khẩu đội QA dùng chung dưới dạng rõ. Build client vẫn có thể bị reverse engineer: local unlock giảm mở nhầm, không dùng để cấp quyền server, IAP hoặc lệnh kinh tế thật.
- Đóng prompt phải trả input focus, không đổi time scale; HUD visibility và unlock có policy riêng. Release mặc định không có trigger/privileged command; gesture không thay thế build gate.

### 4.2. Console, commands và thông tin game

- Console: search/filter theo mức, collapse trùng, stack trace, copy/export, buffer count + byte cap, hiển thị số log bị bỏ.
- Command registry có ID, nhóm, mô tả, owner, quyền theo build; đăng ký/gỡ khi lifecycle thay đổi. Hỗ trợ bool/number/enum/action trước; không reflection scan toàn app mỗi frame.
- Runtime debugger không chứa cheat cụ thể. Thêm tiền, reset save, đổi locale, reload scene là sample hoặc code game; destructive action cần xác nhận ở UI và điều kiện cho phép tại command handler.
- System/build info: app/Unity/platform/build ID, scene, session, version package. Provider thu dữ liệu Unity ở main thread, cache cho report; không query scene hoặc object trong thread nhận log.

### 4.3. Performance HUD bật/tắt riêng

- Mode: **Off**, **FPS Only**, **Compact** (FPS + frame time + memory), **Detailed** (graph ngắn và metadata chọn lọc).
- Bật/tắt qua debugger, hotkey hoặc API host; đóng debugger không bắt buộc đóng HUD. HUD mặc định không nhận raycast, không chiếm focus và không tự pause game.
- Thu frame delta nhẹ mỗi frame; cập nhật text 2–4 Hz, memory 1 Hz; history fixed-size, graph chỉ rebuild khi hiển thị và có mẫu mới. Đây là config thử nghiệm, không phải số tối ưu đã đo.
- FPS là số frame chia tổng unscaled duration trong cửa sổ; spike thể hiện qua frame time. Không đặt tên CPU/GPU time nếu chỉ đo deltaTime; metric không hỗ trợ hiển thị N/A.
- Off thì tắt UI và collector không có consumer; nếu report đang dùng metric history phải hiện rõ collector vẫn hoạt động. Có ref-count/subscription cho consumer để tránh sampler trùng.
- Không đưa full profiler, audio analysis, tự chụp theo mỗi spike vào bản đầu. Cảnh báo hiệu năng có hysteresis/cooldown và chỉ tạo text event trước.

### 4.4. Khi nào có thể chụp lúc lỗi nặng?

| Loại sự cố | Khả năng thực tế của package managed | Policy đề xuất |
| --- | --- | --- |
| QA report thủ công | Game còn render thì có thể yêu cầu ảnh mới | Chụp trước khi mở form, cho preview/loại ảnh |
| Exception/error khi game còn chạy | Có thể schedule capture ở frame sau; không bảo đảm đúng frame lỗi | Deduplicate, cooldown; tiếp tục report nếu không có ảnh |
| Render/GPU lỗi hoặc mất graphics context | Capture có thể thất bại hoặc treo | Timeout logic, bỏ ảnh, không chờ GPU đồng bộ |
| Main thread hang/ANR | Coroutine/UI/capture trên main thread không tiến triển | Chỉ đánh dấu suspect từ watchdog nếu có; lấy breadcrumb/ảnh cũ; native integration riêng |
| Native crash/process abort | Không bảo đảm chạy C# callback, capture hoặc upload | Dùng SDK/native crash provider nếu cần; gửi dữ liệu đã lưu ở lần mở tiếp theo |
| OOM/OS kill/battery loss | Có thể không có callback; cấp phát ảnh làm tình hình xấu hơn | Không chụp/nén khi low memory; chỉ dùng dữ liệu đã có, có thể thiếu report |

`Application.logMessageReceivedThreaded` là nguồn Unity log đa luồng, không phải universal crash handler; callback phải thread-safe và không gọi Unity API tùy ý. [Unity log callback](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-logMessageReceivedThreaded.html).

Android ANR liên quan UI thread bị chặn; xem [Android ANR](https://developer.android.com/topic/performance/vitals/anr). Thiết kế suy ra: watchdog C# không thay thế báo cáo OS và không thể tự ép render đang treo chụp ảnh.

**Ảnh gần nhất trước lỗi:** chỉ opt-in cho QA, không mặc định quay/chụp liên tục. Nếu spike benchmark đạt, cho lưu một ảnh downscale định kỳ thưa hoặc ở checkpoint do game báo. Muốn ảnh còn sau process death phải persist trước; cache RAM không đủ. Mỗi report phải có thời điểm ảnh, độ cũ và lý do thiếu ảnh; không gọi ảnh cũ là “ảnh đúng lúc crash”. Dirty-session marker chỉ gợi ý shutdown bất thường, không chứng minh crash.

### 4.5. Pipeline capture không chặn gameplay

```text
Log/QA event → policy + dedupe → immutable report snapshot
                             → optional capture → encode → durable local outbox
Outbox → upload scheduler → relay → Telegram / email / destination khác
```

- Log callback chỉ enqueue dữ liệu nhỏ vào bounded queue và cập nhật bộ đếm; không screenshot, nén, ghi file, gửi mạng hoặc mở UI tại callback.
- Capture coordinator main thread chỉ một request in-flight. Lấy final game frame theo pipeline, xử lý overlay exclusion có kiểm chứng; capture cả UI Screen Space Overlay là một gate riêng.
- GPU downscale vào RenderTexture tái sử dụng, rồi `AsyncGPUReadback` khi device hỗ trợ; readback có độ trễ vài frame. Không dùng WaitForCompletion/WaitAllRequests trong gameplay. [Unity AsyncGPUReadback](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rendering.AsyncGPUReadback.html).
- Async readback chỉ giải quyết một bước; GPU copy, allocation, encode và IO vẫn cần đo. Device không hỗ trợ/error thì auto-report bỏ ảnh; sync capture chỉ cho thao tác QA có chủ đích, được đo và cấu hình riêng.
- Worker chỉ nhận raw bytes có ownership rõ; dùng codec được tài liệu xác nhận thread-safe. Không bọc mọi Texture2D/Unity API trong Task.Run. Cần kiểm API và nền tảng trước khi chọn encoder: [EncodeArrayToJPG](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ImageConversion.EncodeArrayToJPG.html); [EncodeNativeArrayToJPG](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ImageConversion.EncodeNativeArrayToJPG.html) yêu cầu gọi main thread theo tài liệu đã đọc.
- Chất lượng thử nghiệm: cạnh dài tối đa 960 px, JPEG 65–75; cap ảnh và tổng report; không full-resolution PNG mặc định. Đo peak RAM gồm RT, raw buffer, encoded bytes, serialize/upload copy.
- Giới hạn một encode worker; có cancel/generation token khi shutdown. Không dispose buffer/RT khi GPU hoặc worker còn dùng; callback muộn chỉ cleanup, không chạm owner đã hủy.
- Ghi file ngoài main thread khi nền tảng cho phép; temp + commit/rename với bảo đảm filesystem được kiểm thử, journal/version/schema để phục hồi file dở. Không tuyên bố durable trước khi ghi thành công.
- Khi frame quá tải, low memory, app background, capture timeout hoặc queue đầy: giảm chất lượng, bỏ ảnh hoặc defer. Không retry ảnh liên tục, không làm game đợi diagnostics.

### 4.6. Gửi Telegram bot / Gmail

**Có thể hỗ trợ**, tách khỏi runtime collector. Thiết kế chính: game gửi HTTPS tới relay; relay fan-out Telegram/email. Package vẫn hoạt động local khi không có relay.

| Phương án | Dùng ở đâu | Quyết định |
| --- | --- | --- |
| Relay → Telegram Bot API | Backend nội bộ | Khuyến nghị; text summary + ảnh tùy chọn + report file/link |
| Relay → Gmail API hoặc mail provider → hộp thư Gmail | Backend nội bộ | Khuyến nghị; người nhận không cần đăng nhập Gmail trong game |
| Game → bot trực tiếp | Chỉ prototype/dev bằng credential tạm cấp ngoài repo | Không ship mặc định; token vẫn có thể bị trích xuất từ client |
| Game → Gmail OAuth/SMTP | Không chọn cho production mặc định | Tránh nhúng mailbox credential/token dài hạn và auth flow không phục vụ gameplay |
| Export local / share do QA kích hoạt | Không backend | Luôn có làm fallback; UI share theo platform là tùy chọn |

Telegram có `sendPhoto`, `sendDocument` và thông tin `retry_after` khi flood control; relay xử lý payload limit và rate limit theo API hiện hành. [Telegram Bot API](https://core.telegram.org/bots/api#senddocument).

Gmail API có `messages.send`, payload MIME/base64url; auth/quota do relay chịu trách nhiệm. Không nhầm “gửi tới Gmail” với “bắt buộc dùng Gmail API”. [Gmail sending guide](https://developers.google.com/workspace/gmail/api/guides/sending).

- Report ID duy nhất; server ack chỉ nghĩa relay đã nhận bền vững. Trạng thái delivery từng destination được theo dõi riêng; lỗi Telegram không bắt game gửi lại vô hạn.
- Idempotency theo report ID và destination; queue state Pending/Sending/Accepted/Retry/DeadLetter. Đặt outbox về trạng thái có thể retry sau app restart; tránh gửi trùng khi mất response.
- Một upload in-flight; timeout, retry exponential backoff + jitter, cap lần thử, tôn trọng Retry-After. Network async vẫn tốn CPU/RAM/băng thông, nên serialize/nén có giới hạn và ưu tiên gửi ở menu/idle hoặc lần mở sau.
- Offline/server lỗi/429/5xx: giữ local trong quota; 4xx vĩnh viễn đưa dead letter có giới hạn. Không log lỗi uploader trở lại đường auto-report gây recursion.
- Không giữ app sống, không blocking flush lúc quit, không cam kết upload khi app đã suspend/kill. Backend là deliverable deploy riêng, không làm dependency Unity.
- Caption/link không chứa credential, email cá nhân hoặc save raw. Host quyết định policy cho ảnh màn hình nhạy cảm; vùng cần che phải được loại trước persist/upload. Release default không chụp/gửi tự động cho đến khi host chủ động bật policy.

## 5. Đổi SR/SRF thành Dreamy và nâng cấp có kiểm soát

- Lập inventory class/namespace/asmdef/menu/Resources/shader/prefab/setting/define/reflection string; phân loại giữ, thay, loại bỏ. Không replace chuỗi `SR` toàn repository.
- Entry point mới như `DebuggerHost`; helper nội bộ dùng tên theo việc làm như `LogBuffer`, `CaptureCoordinator`, `UnlockController`; namespace đã chỉ rõ Dreamy nên không thêm prefix Dreamy cho mọi class.
- Loại service framework/helper SRF không dùng; thay dần owner/lifecycle rõ ràng, không copy nguyên SRF rồi chỉ đổi tên.
- Đổi namespace/type serialized bằng công cụ migration phù hợp; đánh giá `MovedFrom`, `FormerlySerializedAs`, Unity MonoScript mapping, asmdef và settings asset. Giữ GUID khi move, kiểm reference thật; attribute không tự chữa mọi trường hợp.
- Resource path, prefab lookup, shader name, menu, generated code, reflection/AOT preserve và define cần mapping riêng. Không đổi GUID chỉ để branding.
- Phase migration cho phép SR ở nguồn baseline và mapping/attribution. Gate sản phẩm cuối: không còn SR/StompyRobot trong API, runtime implementation, hierarchy, menu và asset path được ship; ngoại lệ chỉ notices, lịch sử nguồn và migration tool có tài liệu.
- Giữ copyright/license notice đúng yêu cầu; rename không làm mất nghĩa vụ nguồn vendor. README SRDebugger hiện ghi hạn chế phân phối và icon; phase 0 phải ghi phạm vi quyền nội bộ, không suy diễn đổi tên là được tái phân phối.
- Sau migration, consumer chỉ cài Dreamy, không cần thư mục StompyRobot để compile/chạy. Không để song song hai bootstrap hoặc hai collector.

## 6. Preset và ngân sách đề xuất

| Preset | Gesture/command | HUD | Reporting |
| --- | --- | --- | --- |
| Editor/Dev | Cho phép theo config | Tùy chọn | Local + fake transport mặc định |
| QA | 5 tap + PIN `0000` tùy chỉnh; command allowlist | Off hoặc FPS Only theo config | Capture best effort, relay do host bật |
| Release Off | Strip trigger/command/debug UI | Off | Collector/outbox tắt |
| Release Diagnostics | Không privileged command; access riêng do host cho phép | Off mặc định | Text-first, ảnh opt-in, throttle mạnh |

Thông số ban đầu để benchmark, không phải cam kết đã đạt:

- Buffer tối đa 1.000 log / 2 MiB nội dung, cap từng message/stack; policy ưu tiên lỗi và đếm dropped entry.
- Một capture, một encoder, một upload; cùng sự cố tối đa một report có ảnh mỗi 60 giây, cap report mới mỗi session.
- Outbox 10 report / 20 MiB / 7 ngày; giữ lỗi quan trọng trong quota, xóa ảnh trước khi bỏ report text. Cap bytes phải bao gồm file tạm; disk full phải tiếp tục gameplay.
- HUD steady state hướng tới không GC allocation từ phần code do package sở hữu sau warm-up; xác minh bằng profiler, không hứa toàn app zero-GC.
- Trên máy yếu đại diện, mục tiêu overhead collector + FPS HUD dưới 0,3 ms p95 main thread; capture scheduling dưới 1 ms p95 và không thêm spike trên 2 ms trong benchmark kiểm soát. Đây là mục tiêu ban đầu, phải điều chỉnh/giảm tính năng theo baseline.
- Ghi p50/p95/p99/max frame time, số frame vượt budget 30/60 FPS, CPU/GC/peak RAM và IO/network. Đo riêng pipeline capture và encode; async không đồng nghĩa miễn phí.

## 7. Các phase triển khai

Mỗi phase lưu receipt mới cùng Unity version, platform/device, commit, config, log/test/ảnh cần thiết. Không dùng receipt trước migration để xác nhận asset sau migration.

Cập nhật 2026-11-04: checkbox đánh dấu đầu việc phần mềm đã triển khai/kiểm tra trong phạm vi receipt; gate thiết bị/tài khoản thật vẫn giữ mở. Trạng thái chi tiết mới nhất ở checkpoint cuối file và `Documentation~/TRACKING.md`, bao gồm các mục kết hợp mới đạt một phần.

### Phase 0 — Baseline, phạm vi và thiết kế (đầu vào bắt buộc)

- [x] Snapshot inventory/hash nguồn SRDebugger/SRF, license/notice/icon/font; ghi quyền dùng nội bộ và phạm vi phân phối.
- [ ] Compile source hiện có; kiểm console, asmdef, Input System và baseline CPU/GC/RAM khi tắt/đóng/mở panel.
- [ ] Chốt platform đầu tiên, máy yếu đại diện, pipeline, target FPS; ghi những platform chưa hỗ trợ.
- [x] Chốt folder, assembly graph, contracts và config schema; lập bảng rename + serialized migration, quyết định phần giữ/thay/loại.
- **Đầu ra:** baseline receipt, dependency map, inventory/migration map, performance test scene.
- **Gate:** biết baseline có compile hay không; blocker phải được ghi và xử lý trước khi claim nâng cấp. Phần fork cần phạm vi license rõ trước khi phân phối.

### Phase 1 — Fork Dreamy và đóng gói độc lập

- [x] Tạo UPM layout/assemblies, thay brand/namespace/class theo inventory; giảm SRF xuống helper cần thiết hoặc thay owner.
- [x] Migrate settings/prefab/resource paths, bảo toàn reference; loại bootstrap/endpoint vendor khỏi bản ship.
- [x] Tách input backend tùy chọn; bootstrap explicit, một host sống qua scene, shutdown đối xứng.
- [x] Xây immutable config snapshot và extension contracts; runtime không tham chiếu sample/Dreamy package khác.
- **Đầu ra:** package Dreamy compile, migration tool + dry-run/report, QuickStart tối thiểu.
- **Gate:** consumer sạch không có StompyRobot/Core/UI/Graphy chạy được; consumer thiếu Input System vẫn compile qua backend hợp lệ; không Missing Script/reference; namespace/API/menu đúng Dreamy.

### Phase 2 — Access khó mở nhầm và debugger căn bản

- [x] Gesture 5 tap, timeout/gap/region config; PIN 4–32 mặc định `0000`, passphrase 8–64; session TTL/cooldown/relock.
- [x] Console/filter/export, system info, command registry và command permission theo preset.
- [ ] UI focus/safe area/mobile keyboard; kiểm `Keyboard.current == null`, multitouch, timeScale=0, app pause/resume.
- **Đầu ra:** sample gesture/password, project command sample, config validation và UI Dreamy.
- **Gate:** test timeout/reset/cooldown/credential boundaries; thao tác thật không che nút game; release không chạy command ngoài allowlist.

### Phase 3 — FPS HUD chọn lọc từ Graphy

- [x] Off/FPS Only/Compact/Detailed, corner/scale/color, toggle độc lập với panel.
- [x] Single sampler, fixed history, text/graph tick riêng; giải phóng collector khi hết consumer.
- [ ] Benchmark cùng scene khi HUD off/on và graph off/on; profile allocation/rebuild.
- **Đầu ra:** HUD dùng độc lập và sample bật/tắt, attribution nếu lấy upstream code.
- **Gate:** không block input; metric label đúng; bật/tắt nhiều lần không thêm collector; overhead đạt budget đã chốt hoặc hạ mặc định xuống FPS Only.

### Phase 4 — Report local và capture ưu tiên gameplay

- [x] Snapshot lỗi + context, dedupe/rate limit; local exporter/outbox schema và retention.
- [x] Thay screenshot sync bằng downscale + async readback + encoder đã kiểm; fallback text-only.
- [x] Low-memory/timeout/cancel/backpressure; report status và reason khi thiếu ảnh.
- [ ] Optional QA checkpoint screenshot; metadata timestamp và stale age; dirty-session marker chỉ ghi “abnormal previous session”.
- **Đầu ra:** report bundle local, capture benchmark, fault-injection scene.
- **Gate:** log burst, GPU readback lỗi, thiếu RAM/disk full/cancel không chặn game; overlay/UI trong ảnh đúng thiết kế; native crash test không bị báo nhầm đã capture được.

### Phase 5 — Relay, Telegram và Gmail/email

- [x] Chốt HTTP schema/version, upload auth giới hạn và abuse control; secrets chỉ server.
- [x] Reference relay ngoài UPM, local fake server; idempotency, durable ack, fan-out/destination status.
- [x] Telegram summary + attachment; email summary + attachment/link, auth/quota backend.
- [x] Offline queue/restart/retry/cancel, cap payload/bandwidth; trạng thái gửi không mở modal trong gameplay.
- **Đầu ra:** transport client, reference relay, config hướng dẫn deploy và vận hành token/quota.
- **Gate:** fake endpoint test 429/5xx/timeout/mất ack/duplicate; sau đó integration test kênh test khi triển khai được cấp credential. Không gửi tin thật trong giai đoạn nghiên cứu này.

### Phase 6 — Crash nặng và xác minh trên thiết bị

- [ ] Tích hợp native crash/ANR provider tùy chọn theo platform; tách khỏi foundation, không gọi Unity render API trong fatal handler.
- [ ] Correlate session/build/report ID, xử lý symbol/native evidence và gửi ở lần khởi động tiếp theo nếu có dữ liệu.
- [ ] Test managed exception, main-thread hang, native crash, low-memory và process kill như những ca riêng; chạy trên build thử nghiệm.
- [ ] Kiểm OS suspend, mất graphics context, restart có report dở; phân biệt crash đã xác nhận và session kết thúc bất thường.
- **Đầu ra:** support matrix platform × loại lỗi × dữ liệu có thể thu.
- **Gate:** mỗi claim có receipt tương ứng; không cam kết ảnh tức thời khi process chết; case không hỗ trợ có fallback rõ.

### Phase 7 — Tái sử dụng, release và tài liệu

- [x] Import từng sample mới vào consumer sạch, sửa sample/theme/command mà không sửa Runtime; upgrade package giữ customization.
- [ ] Build release off/diagnostics, kiểm bootstrap/command không bị rò và asset size thực tế; IL2CPP/stripping platform mục tiêu.
- [ ] Profile gameplay/menu/loading, screenshot/upload burst trên device yếu; regression scene/domain reload và lifecycle event.
- [x] README, migration, API/config reference, privacy policy hook, benchmark/support matrix, changelog và rollback version.
- **Đầu ra:** bản nội bộ có version, artifact và hướng dẫn upgrade.
- **Gate:** gates bắt buộc trên consumer/platform đã chọn đều đạt; phần chưa chạy ghi chưa xác minh. Chưa gọi package production-ready nếu chỉ compile trong sandbox.

## 8. Thứ tự ưu tiên và checkpoint

**MVP nội bộ:** phase 0–4 — package độc lập, đổi Dreamy, access mới, console/command, FPS HUD và report local có ảnh best effort.

**Mốc gửi tự động:** phase 5 — relay Telegram/email; consumer không có relay vẫn dùng đủ chức năng local.

**Mốc hard-crash diagnostics:** phase 6–7 — provider/platform receipt, consumer reuse, release validation. Phase 7 cần làm tối thiểu cho bất kỳ mốc nào được phát hành nội bộ.

Chưa đưa remote command từ Telegram, video replay, full object inspector hoặc screenshot mỗi frame vào scope.

Checkpoint hiện tại:

- Hoàn tất: nghiên cứu code gesture/PIN/capture hiện có; tham khảo Graphy và tài liệu Unity/Telegram/Gmail; viết lại plan theo yêu cầu mới.
- Chưa làm: sửa source, migrate asset, cài Graphy, tạo relay, cấu hình credential, gửi tin hoặc chạy benchmark/build.
- Bước tiếp theo: phase 0. Các API, folder, thông số và budget ghi là dự kiến; cần kiểm lại với source/Unity/device tại lúc triển khai.

## 9. Bổ sung triển khai theo yêu cầu mới

- Mặc định chính thức: **5 tap + PIN `0000`**, cả số tap và PIN đều tùy chỉnh được, giữ số 0 đầu. Passphrase là phần mở rộng sau.
- Cheat options chia category/container như package gốc: bool, số có range, enum, string, read-only, action và destructive action có xác nhận; ID ổn định, owner dispose, kiểm quyền khi thực thi. Logic riêng game ở sample.
- Mỗi phase có kiểm thử tự động và nghiệm thu UI/device phù hợp; fake success/offline/429/5xx/reject/timeout/cancel trước khi dùng endpoint thật. Test fake không chứng minh gửi thật hoặc không lag.
- Submodule: `LocalPackages/com.dreamy.debugger`, remote private `Dreamy-Game-Foundation/com.dreamy.debugger`. Tạo trước khi viết implementation.
- Xem `LocalPackages/com.dreamy.debugger/Documentation~/ACCEPTANCE.md` và `TRACKING.md` để theo dõi gate thực tế; chưa nghiệm thu không ghi pass.

### Checkpoint triển khai 2026-11-04

Đã tạo submodule private và đăng ký UPM. Phần nền access/cheat/fake-report đã có code, sample Editor có thể thử tại **Window > Dreamy Debugger > Acceptance Playground**. Sandbox và consumer sạch đều đạt **44 EditMode + 3 PlayMode**. Runtime foundation không phụ thuộc UnityEngine/Core/UI/Graphy/Input System.

Đây là nghiệm thu một lát cắt nền tảng, chưa hoàn tất toàn phase 0/1/2. UI/vendor migration, console, HUD, capture/outbox, relay và crash/platform gates vẫn chờ triển khai. Xem `docs/evidence/debugger/verification.md` và tracking trong submodule để tiếp tục đúng mốc.

## 10. Checkpoint hoàn thiện phần mềm — 2026-11-04

Đã triển khai bản `0.2.0-preview.1`: fork SR/SRF trong submodule, giữ GUID và migrate prefab/animation bằng Editor API; Control Center mới theo theme Dreamy; overlay SR được tái sử dụng với thương hiệu Dreamy; typed cheat category, PIN/passphrase và access guard; HUD chia mode dùng chung sampler; log queue có giới hạn; capture GPU bất đồng bộ/encode worker; outbox bền vững, retry/dead letter; relay Telegram/Gmail/SMTP và fake mode; Android OS exit evidence, native SDK provider contract; sample/config/variant riêng và release scene stripping.

Bằng chứng và trạng thái nghiệm thu cuối cùng: `docs/evidence/debugger/phase-completion/verification.md`, `LocalPackages/com.dreamy.debugger/Documentation~/TRACKING.md` và `PLATFORM_SUPPORT.md`. Không dùng checkpoint phần nền cũ để kết luận trạng thái hiện tại.

Phần mềm ở các phase đã có implementation; các gate cần thiết bị, native fault thật, iOS/Xcode và credential destination vẫn chưa được nghiệm thu. Chưa có thiết bị ADB hoặc credential kênh thật trong phiên này. Không bật recorder chụp liên tục trước crash khi chưa có benchmark thiết bị; luôn ưu tiên trải nghiệm.


### Delivery checkpoint — 2026-11-04

Submodule đã push commit `1f9a5e7`, tag `v0.2.0-preview.1`. Kiểm tra cuối khớp 263 fingerprint: 64 EditMode + 10 PlayMode + 13 relay tests đạt; Linux dev/release và Android dev IL2CPP ARM64 build thành công; Linux release có 0 asset debugger. Chưa nghiệm thu trên thiết bị/tài khoản đích thật.
