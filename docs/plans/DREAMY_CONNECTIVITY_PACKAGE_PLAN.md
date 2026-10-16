# Plan triển khai Dreamy Connectivity

Ngày: 2026-10-15. Trạng thái: **đã triển khai local v0.1.0; còn gate native/device và clean consumer**.

Mục tiêu: dùng lại logic kiểm tra kết nối, phục hồi hành động và panel Retry trong nhiều game; thay được SDK/backend/UI; có adapter Android/iOS, prefab và sample import được thật.

## 0. Kết quả triển khai local

Cập nhật MVP 2026-10-16: runtime thêm `ConnectionRetryModel/State`; sample dùng `ConnectionRetryPresenter : IPanelPresenter` và `IConnectionRetryView`. `ConnectionRetryPanelGateway` mở/đóng qua PanelManager; factory tạo presenter theo từng lần mở. Root giữ model/service và prefab reference, không giữ presenter hoặc live panel. Tên cũ `ConnectionRecoveryPresenter` / `IConnectionRecoveryView` được thay thế, có migration asset bằng Unity APIs. 32 tests, managed mobile branches, runtime-only consumer và Play Mode manager/cached reopen/Back đã pass; xem VALIDATION.md của package.


Cập nhật quyết định 2026-10-16: chỉ sample panel phụ thuộc Dreamy UI/Feature. `Integration/UI`, Editor authoring và UI tests đã chuyển vào `Samples~/Connectivity Feature`; package manifest chỉ còn UniTask và Unity modules. Các đoạn thiết kế cũ về UI dependency bắt buộc đã được thay thế bằng quyết định này. Core và sample giữ assembly names/GUID; consumer không UI đã compile/chạy trong Unity batchmode, 28 runtime/UI tests pass.


- Package: `LocalPackages/com.dreamy.connectivity`; xem [README](../../LocalPackages/com.dreamy.connectivity/README.md) và [validation](../../LocalPackages/com.dreamy.connectivity/VALIDATION.md).
- 27 EditMode tests pass; C# Android/iOS branches compile. Prefab/scene tạo bằng Unity, sample import lại qua Package Manager và luồng loss/recovery/timeout/cancel đã chạy trong Play Mode.
- Sample được chốt theo yêu cầu production: **Connectivity Feature** chứa installer/root/prefab dùng gateway thật; **Connectivity Diagnostics** riêng chứa nút giả lập loss, recovery, timeout, server error và unknown outcome.
- Điều chỉnh so với thiết kế ban đầu: concrete Retry view là Canvas modal trung lập, không kế thừa UIPanel, để dùng cả trước khi PanelManager sẵn sàng. Host có thể thay `IConnectionRecoveryUI` hoặc view để tích hợp navigation riêng. Một target cấu hình trên root; multi-target gateway do host route.
- Nội dung các phần sau giữ vai trò thiết kế tham chiếu, không thay cho API source hoặc bằng chứng validation. Những gate native mobile, clean consumer, landscape/device và publish chưa hoàn thành được ghi rõ trong validation.

## 1. Nguồn thiết kế và hiện trạng

- Bắt buộc tuân thủ [quy tắc package](../rules/DREAMY_PACKAGE_DESIGN_RULES.md), đặc biệt dependency một chiều, MVP, lifecycle, release submodule và sample copy-ready.
- Đã đọc `Packages/manifest.json`, `Packages/packages-lock.json`, `.gitmodules`, package/asmdef Economy, Shop, UI và source presenter. Sandbox dùng Unity `6000.4.12f1`, resolve Dreamy qua local packages.
- UI có `Dreamy.UI.Presentation` với `IPanelPresenter.Show/Dispose`; concrete Unity panel phải nằm ngoài domain.
- Chưa có package Connectivity trong manifest. Không tìm thấy `toolkit.json`, compatibility registry hoặc skill `omg-*` trong checkout và các thư mục skill đã kiểm tra; đường dẫn toolkit sibling trong rules cũng không tồn tại. Chưa xác minh maturity từ toolkit. Khi triển khai, kiểm tra lại trước khi ghi compatibility.
- Các interface, type, file, version và lệnh mới bên dưới là **đề xuất**, không phải API/lệnh đã tồn tại. Không sửa package, prefab hoặc manifest trong đợt viết plan này.

## 2. Quyết định chính: inject gateway, cung cấp adapter mặc định

**Chốt contract kiểm tra; không cố định một thuật toán kiểm tra cho mọi game.** Root truyền gateway; package cung cấp adapter Unity và delegate để game dùng ngay. Không phụ thuộc trực tiếp SDK nội bộ.

Hàm SDK của team:

```csharp
public static bool Internet_IsConnected()
{
    return Application.internetReachability != NetworkReachability.NotReachable;
}
```

Hàm này cho biết tín hiệu reachability của thiết bị. Không đủ để khẳng định Internet hoặc backend đang hoạt động, ví dụ Wi-Fi có kết nối nhưng chưa đăng nhập captive portal. Unity cũng ghi rõ giới hạn này trong [internetReachability](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-internetReachability.html).

| Game | Adapter nên dùng | Ý nghĩa |
| --- | --- | --- |
| Casual, chỉ cần chặn khi thiết bị báo offline | `UnityReachabilityGateway` hoặc delegate SDK | Có interface mạng khả dụng; request thật vẫn có thể lỗi |
| Có backend sở hữu bởi team | Reachability + `HttpEndpointConnectivityGateway` | Kiểm tra endpoint cụ thể theo contract backend |
| SDK tự quản lý trạng thái kết nối | Host implement `IConnectivityGateway` | Chuyển trạng thái SDK sang contract package |
| Ads/IAP/SDK bên thứ ba | Adapter SDK và kết quả thao tác thực | Không lấy health của backend game để suy ra SDK khác hoạt động |
| Test/Editor demo | `ScriptedConnectivityGateway` | Điều khiển kết quả và độ trễ, chỉ dành cho sample/test |

Delegate SDK kiểu `Func<bool>` chỉ được map thành `NetworkAvailable`/`NoNetwork`, không được map `true` thành `EndpointReachable`. Cách gọi static SDK nằm tại composition root của game. Đổi SDK không làm domain đổi code.

## 3. Phạm vi bản đầu

Bao gồm:

- Check theo hành động, không chặn toàn game mặc định.
- Gateway injectable; Unity reachability, delegate SDK, HTTP endpoint và fake có kịch bản.
- Chính sách `NetworkHintOnly`, `EndpointRequired`, `RequestFirst`.
- Kết quả tường minh; check cache ngắn hạn theo target và chống probe trùng.
- Retry thủ công; retry tự động có giới hạn chỉ khi host khai báo thao tác an toàn.
- Panel Retry / Open Settings / Cancel, loading, timeout, lỗi platform và giới hạn retry.
- Mở settings Android/iOS, recheck khi resume và hỗ trợ host custom UI.
- Samples, prefab, demo scene, domain/EditMode/PlayMode tests, build/device checklist.

Ngoài phạm vi bản đầu: background network monitor chạy liên tục, tự bật Wi-Fi/mobile data, tự đăng nhập captive portal, offline command queue bền vững, đồng bộ multiplayer, SDK store/ads cụ thể, tự khôi phục giao dịch của backend. Domain không giữ receipt IAP hoặc balance.

## 4. Package, assembly và dependency

Tên đề xuất `com.dreamy.connectivity`, namespace `Dreamy.Connectivity`, version đầu dự kiến `0.1.0`; chọn tên/version cuối theo catalog lúc triển khai.

```text
com.dreamy.connectivity/
  Runtime/                      Dreamy.Connectivity.Runtime
    Contracts/ Domain/ Policies/ Execution/
  Integration/Unity/            Dreamy.Connectivity.Unity
    Gateways/ Lifecycle/ Platform/
  Integration/UI/               Dreamy.Connectivity.UI
    Views/ Presentation/ Installation/
  Plugins/iOS/                  Native bridge mở app settings
  Editor/                       Dreamy.Connectivity.Editor
  Tests/EditMode/ Tests/PlayMode/
  Samples~/Connectivity Demo/
    Runtime/ Prefabs/ Scenes/ Config/ README.md
  Samples~/Host Gateway Example/
    Runtime/ README.md
  Documentation~/ README.md CHANGELOG.md VALIDATION.md
```

| Assembly | Reference dự kiến | Ghi chú |
| --- | --- | --- |
| Runtime | UniTask | `noEngineReferences: true`; không Core/UI/SDK/HTTP Unity |
| Unity | Runtime, UniTask, UnityEngine modules cần dùng | Reachability, UnityWebRequest, JNI và app lifecycle |
| UI | Runtime, Unity, Dreamy.UI.Runtime, Dreamy.UI.Presentation, UniTask, UGUI/TMP | Concrete panel, presenter, bridge |
| Editor | Các assembly cần inspect | `includePlatforms: [Editor]`; validator/builder |
| Tests | Assembly được test, Test Framework | Không xuất hiện trong player runtime |
| Sample | Runtime, Unity, UI và dependency thực dùng | Tên assembly riêng, không trùng khi import hai sample |

Manifest package đầu khai báo dependency thực dùng của cả integration được ship: UI, UniTask, UGUI và Unity modules; chốt version từ package source/lock, không copy version cũ trong README. Đây là một UPM package có nhiều assembly, nên UI vẫn là dependency cài đặt dù headless consumer chỉ reference Runtime. Chỉ tách UPM UI riêng khi có consumer cần cài core không kéo UI; không tuyên bố UI dependency là tùy chọn khi manifest còn bắt buộc.

Không dependency Economy, Shop hoặc `_Project`. Native iOS plugin chỉ enable iOS; Android JNI được isolate bằng platform compile guards, Editor gọi adapter unsupported/fake. Không thêm Java/Gradle dependency nếu JNI tới system API là đủ.

## 5. Contract đề xuất và ownership

### 5.1 Check và dữ liệu

```csharp
public interface IConnectivityGateway
{
    UniTask<ConnectivityCheckResult> CheckAsync(
        ConnectivityCheckRequest request,
        CancellationToken cancellationToken);
}

public interface IConnectivityService
{
    UniTask<ConnectivityCheckResult> CheckAsync(
        ConnectivityCheckRequest request,
        CancellationToken cancellationToken);
}
```

- `ConnectivityCheckRequest`: `TargetId`, mức kiểm tra, `ForceRefresh`. Target ID ổn định, không nhận URL tùy ý từ button.
- `ConnectivityCheckResult`: `Status`, `TargetId`, `Evidence`, thời điểm kiểm tra, nguồn cache, mã lỗi ổn định. Status: `Unknown`, `NoNetwork`, `NetworkAvailable`, `EndpointReachable`, `EndpointUnavailable`, `TimedOut`, `Misconfigured`. Cancellation theo token, không đổi thành `NoNetwork`.
- `NetworkAvailable` chỉ đủ cho `NetworkHintOnly`; `EndpointRequired` cần `EndpointReachable`. `RequestFirst` bỏ qua probe trước, phân loại lỗi từ thao tác thật.
- `ConnectivityOptions`: timeout, cache TTL, target definitions, giới hạn retries, auto-retry policy. Immutable snapshot tại root; không persist vào save người chơi.
- `ConnectivityService`: owner cache và probe dedupe. Chỉ dùng evidence đúng target/mức kiểm tra; không suy ra backend B từ backend A. Không dùng cached positive để bỏ qua lỗi request mới.
- HTTP gateway chỉ xác nhận khi response đúng status/body contract; redirect HTML/captive portal không được coi là pass. HTTPS, URL do host cung cấp; không mặc định ping Google hoặc dịch vụ bên ngoài.

### 5.2 Settings và app lifecycle

```csharp
public interface IConnectionSettingsGateway
{
    ConnectionSettingsCapabilities GetCapabilities();
    UniTask<OpenConnectionSettingsResult> OpenAsync(
        ConnectionSettingsTarget target,
        CancellationToken cancellationToken);
}
```

Target: `Connection`, `App`. Result: `Opened`, `Unsupported`, `Failed`, kèm destination thực tế. `Opened` chỉ nghĩa OS chấp nhận yêu cầu; không có nghĩa kết nối đã được sửa.

Một Unity lifecycle bridge do root sở hữu phát resume/focus được debounce. Domain không implement `MonoBehaviour`. Presenter/controller chỉ subscribe khi session còn sống.

### 5.3 Recovery và thực thi hành động

- `ConnectionRecoveryCoordinator`: sở hữu một recovery session UI đang active. Request khác trả `Busy`; không tự queue và không chạy ngầm nhiều hành động sau một lần Retry.
- `ConnectivityActionRunner`: orchestration tùy chọn, nhận operation callback, target, retry safety, policy và owner token. Không ép mọi HTTP client kế thừa base class của package.
- Operation trả `ConnectedOperationResult<T>` với `Succeeded`, `ConnectivityFailure`, `ServiceUnavailable`, `Rejected`, `OutcomeUnknown`; phân loại từ host. Không bắt mọi exception rồi gọi đó là mất mạng.
- `RetrySafety`: `Never`, `ReadOnly`, `IdempotentWithKey`. Mặc định `Never`; có nút Retry hành động chỉ khi host khai báo an toàn. Với `Never`, chỉ recheck và trả quyền điều khiển cho host.
- `ConnectivityActionResult<T>`: terminal `Succeeded`, `Cancelled`, `RetryExhausted`, `Failed`, `OutcomeUnknown`, `Busy`; mang giá trị/mã lỗi gốc, số lần thử. Callback exception không được nuốt: log boundary có operation ID, trả lỗi hoặc propagate theo một quy ước đã document.
- ID operation dùng chống click trùng trong session; idempotency key dùng bởi backend phải do owner giao dịch cấp, giữ nguyên payload và key qua mọi retry. Package không tự bảo đảm idempotency server.
- `IConnectionRecoveryUI` là boundary async mở/đợi quyết định; view phát Retry/Settings/Cancel; presenter render state. Domain không gọi PanelManager trực tiếp.

SOLID: check, platform settings, recovery và view có lý do thay đổi riêng; thêm gateway không đổi service; fake/Unity/SDK phải giữ cùng ý nghĩa result; interface nhỏ theo boundary thay thế; constructor injection tại leaf. Không thêm interface cho mọi DTO hoặc helper.

## 6. State machine và thuật toán retry

```text
Created -> Checking -> Executing -> Succeeded
                 |         |
                 v         v
            WaitingForUser <--- ConnectivityFailure
                 |
      Retry -----+-> Checking -> Executing (nếu được phép)
      Settings ----> OpeningSettings -> AwaitingResume -> Checking
      Cancel ------> Cancelled

Rejected/UnknownOutcome/Exhausted/OwnerCancelled -> terminal phù hợp
```

Trình tự:

1. Validate options, operation ID, retry safety và target trước khi mở session. Owner đã hủy: không probe, không mở UI, không gọi action.
2. `NetworkHintOnly`/`EndpointRequired` check theo policy; `RequestFirst` chạy action trực tiếp. Chỉ check fail có nghĩa liên quan mới mở recovery.
3. Sau khi pass, thực thi action một lần; vẫn xử lý lỗi request thật. Phân biệt HTTP 401/403, business rejection, 5xx và transport failure; host chọn thông điệp/policy riêng cho 5xx.
4. Lỗi kết nối: chuyển `WaitingForUser`; chỉ một panel. Retry bị disable khi checking/executing; callback gắn generation/session ID để bỏ kết quả cũ.
5. Retry thủ công luôn `ForceRefresh`; nếu đã có evidence mới từ request thật thì dùng nó thay vì gọi health thừa. Tăng attempt khi action thực sự được gọi; probe có counter/giới hạn riêng, không đốt action attempt khi chỉ check fail.
6. Auto retry mặc định tắt. Nếu bật và thao tác an toàn: tối đa 2 lần bổ sung, backoff 1s/2s có jitter; delay dùng thời gian thực và cancellation, không phụ thuộc `timeScale`. Manual retry tối đa 3 lần bổ sung cho một session; config cho phép đổi, UI còn Cancel khi hết lượt.
7. Timeout mỗi probe mặc định 5s. Action timeout do host/HTTP client quản lý và báo về; không giả định callback đã dừng chỉ vì wrapper hết giờ. Tuyệt đối không bắt đầu action tiếp theo khi lần cũ còn có thể đang mutate.
8. Timeout sau khi gửi giao dịch mà chưa biết đã commit: `OutcomeUnknown`. Host reconcile bằng cùng key hoặc status endpoint trước khi cho chạy lại. Với IAP, SDK/backend transaction owner điều khiển tiếp; package không retry purchase mù.
9. Settings chỉ mở từ thao tác người dùng. Khi quay lại: invalidate cache target, recheck một lần. Mặc định không tự thực thi action sau resume; nếu pass, hiện nút tiếp tục/Retry, host có thể opt-in auto-resume với action an toàn.
10. Close, Back, scene unload hoặc owner token bị hủy: hoàn tất session một lần, unsubscribe/cancel waiter và không đụng view đã dispose. UI đóng không đồng nghĩa backend hủy được giao dịch; host transaction owner vẫn reconcile nếu cần.

Mặc định không pause game qua `Time.timeScale`, không chặn input toàn app và không theo dõi mạng mỗi frame. Một session có một `CancellationTokenSource` liên kết owner; không dùng `async void` trừ Unity event wrapper có error boundary.

### Đồng thời và cache

- Dedupe chỉ probe cùng target/options, không dedupe hoặc chia sẻ kết quả mutation giữa các caller.
- Caller hủy chỉ ngừng chờ của caller đó. Shared probe do service quản lý token; kết thúc khi không còn waiter hoặc root dispose. Không dùng token của caller đầu làm token của tất cả caller.
- TTL positive dự kiến 3s, negative 1s; force refresh và app resume bỏ cache. Test với monotonic clock/delay injectable thay vì chờ wall clock thật.
- `Busy` không mở thêm popup; caller chọn thông báo hoặc thử lại sau. Không có singleton callback global bị request sau ghi đè.

## 7. Android/iOS và fallback

### Android

- Android API 29+: ưu tiên Internet Settings Panel qua `Settings.Panel.ACTION_INTERNET_CONNECTIVITY`; kiểm API level và xử lý khi thiết bị không hỗ trợ activity. Tham khảo [Settings.Panel](https://developer.android.com/reference/android/provider/Settings.Panel).
- Fallback lần lượt `Settings.ACTION_WIRELESS_SETTINGS`, `Settings.ACTION_WIFI_SETTINGS`, cuối cùng `Settings.ACTION_SETTINGS` theo capability. Dùng API công khai; không tự bật/tắt kết nối.
- JNI chạy trên UI thread, dispose `AndroidJavaObject`; resolve intent khi khả thi và vẫn catch lỗi `startActivity`. Không để thiếu package visibility query tạo kết luận sai rằng mọi intent unsupported; kiểm manifest tối thiểu theo implementation thực.
- Nếu host yêu cầu App settings riêng: action app details với package URI; phân biệt với màn hình kết nối.
- Settings panel có thể không tạo đủ cặp pause/resume: cung cấp nút Retry luôn hoạt động khi app focus; không chờ resume vô hạn. Test OEM khác nhau.
- HTTP adapter cần quyền Internet theo Android manifest thực; không thêm quyền Wi-Fi/location nhạy cảm chỉ để mở Settings.

### iOS

- Dùng native bridge tới `UIApplicationOpenSettingsURLString` và completion của `openURL`. API công khai mở **app settings**, không cam kết mở thẳng Wi-Fi/mobile settings. Nguồn: [Apple openSettingsURLString](https://developer.apple.com/documentation/uikit/uiapplication/opensettingsurlstring).
- Không dùng URL scheme private `prefs:`/`App-Prefs:` để cố mở Wi-Fi. Nút hiển thị đúng là “Mở cài đặt ứng dụng”; phần hướng dẫn cho phép người dùng mở Wi-Fi/di động thủ công.
- Bridge `.mm`, C ABI, callback marshaling về Unity main thread, registry callback có cleanup/generation; IL2CPP/AOT callback giữ delegate sống theo lifecycle. Không callback vào session đã đóng.
- Không có UI giả để báo settings mở thành công khi unsupported. iOS Simulator chỉ smoke; hành vi settings/resume phải kiểm trên iPhone thật.

### Desktop/Editor/WebGL

- Settings trả `Unsupported`, nút ẩn/disable; vẫn Retry/Cancel. Editor sample có simulation rõ ràng.
- Reachability Unity trên desktop không đại diện cho Internet thực. WebGL/console cần gateway host riêng; không ghi supported cho đến khi có build/test. Target chính bản đầu là Android/iOS.

## 8. Panel, prefab và integration

Concrete `ConnectionRetryPanel : UIPanel` implement `IConnectionRecoveryView`; presenter nhận dependency từ factory tại root. Có adapter host chỉ dùng Canvas thường để chứng minh contract không buộc PanelManager.

Hierarchy prefab đề xuất:

```text
ConnectionRetryPanel [CanvasGroup, UIPanel/view]
  Backdrop [Image, raycast blocker]
  SafeArea [component SafeArea hoặc nhận parent safe area]
    Dialog
      Icon
      Title [TMP]
      Message [TMP]
      Progress [spinner + status]
      RetryButton
      SettingsButton
      CancelButton
```

- Prefab đầy đủ reference, TMP font/material, anchor, layout và touch target. Text/icon thông qua presentation mapping, không hardcode tiếng Việt trong domain.
- State: offline, checking, retrying, server unavailable, retry exhausted, settings unsupported/open failed; Back tương đương Cancel trong demo.
- `PanelPresenterFactory`/host adapter theo API thực đã xác minh lúc code; một presenter mỗi lần mở, dispose khi đóng/disable/destroy hoặc show lỗi. Không giữ delegate action trong static field.
- Panel address do game cấu hình; sample dùng serialized prefab direct reference để chạy không bắt buộc build Addressables. Có hướng dẫn tùy chọn Addressables + release ownership, không thêm Assets dependency chỉ cho demo.
- Builder/validator nằm Editor. Tạo prefab/scene bằng Unity Editor/MCP hoặc Editor API; giữ `.meta`, tuyệt đối không ghép YAML. Kiểm Console, Missing Script, reference null và Game view sau import.

## 9. Sample phải giao kèm

| Sample | Nội dung | Tiêu chí chạy |
| --- | --- | --- |
| Connectivity Demo | Scene, Retry prefab, root installer, fake gateway có công tắc Offline/Online/Timeout/ServerError | Import, mở scene, Play, đủ Retry/Cancel/Settings states |
| Unity Gateway | Nút chạy read-only action, adapter reachability và endpoint host cấu hình | Không tự gọi endpoint public; thiếu URL báo Misconfigured |
| Host Gateway Example | Delegate `Func<bool>` và hướng dẫn bọc hàm SDK team, custom async gateway | Compile không cần SDK team; fake delegate được ghi demo |
| Custom View | Canvas thường và view riêng dùng cùng coordinator | Thay UI không sửa domain, không import scene khác |
| Transaction Recovery | Fake server idempotency + response bị mất sau commit | Retry/reconcile không tăng reward hai lần; ghi rõ không phải production backend |

Ưu tiên đặt các mode chạy chung trong Connectivity Demo, không tạo nhiều bản asmdef/type trùng. `package.json.samples` khai báo chính xác hai folder public ở cây thư mục; mode Unity/Custom/Transaction là scene hoặc lựa chọn trong sample đầu. Dependency sample ghi rõ; import riêng từng sample và cùng lúc phải compile.

README bắt buộc có: quick start root, thứ tự install/Ready, code SDK delegate, endpoint contract, gọi action an toàn, xử lý request lỗi, mở settings theo platform, cancellation, custom panel, Addressables tùy chọn, test trên device và gỡ sample. Source phát hành nằm `Samples~`, không lấy bản sandbox chỉnh riêng làm chuẩn.

## 10. Các phase triển khai

| Phase | Deliverable | Gate trước phase sau |
| --- | --- | --- |
| C0 — Baseline | Commit/version/dependency ledger, chọn path package, lập consumer fixture | Không còn API/assembly ownership chưa rõ |
| C1 — Domain | DTO, gateway/service, policies, target validation, cache/probe dedupe | Unit tests check/evidence/cancellation/cache |
| C2 — Recovery | State machine, action runner, budgets, duplicate protection, unknown outcome | Fake operation tests và fake clock; không callback muộn vào view |
| C3 — Platform | Unity/delegate/HTTP gateways, Android settings, iOS bridge, resume | Android build + iOS compile; test fallback platform |
| C4 — Presentation | Presenter, UI bridge, panel prefab, Editor validator | PlayMode open/retry/settings/cancel/reopen |
| C5 — Samples | Scenes, fake backend, SDK example, custom UI, README | Fresh import từng sample/cả hai, không dependency ẩn |
| C6 — Consumer/release | Sandbox adapter, migration/install docs, changelog, validation report | Clean consumer compile, player/device gates, pin commit/tag/lock |

Không commit/push/tag tự động theo plan này. Nếu tạo repo package mới cần xác nhận URL thật trước khi ghi `.gitmodules`; không giả định repo remote đã tồn tại.

## 11. Test matrix và nghiệm thu

- Domain: mapping SDK true/false không thổi phồng evidence; endpoint pass/fail/timeout/misconfigured; TTL/force refresh; shared probe cancellation; multi-target isolation; retry budget/backoff; `Busy`; duplicate click; stale callbacks.
- Execution: preflight pass nhưng request fail; cancellation trong check/delay/action; non-idempotent action không tự retry; response mất sau commit; unknown outcome reconcile; không chạy action thứ hai khi action đầu chưa kết thúc.
- UI: mở/đóng 20 lần không nhân listener; Back/Cancel; scene unload; timeScale=0; show/load lỗi; resume khi session đã hủy; hai caller cùng yêu cầu; UI chết sau action nhưng giao dịch vẫn được owner xử lý.
- Mobile: airplane mode, Wi-Fi không Internet, Wi-Fi/di động đổi qua lại, captive portal, timeout/DNS/TLS, backend maintenance, OS settings lỗi/unsupported, quay về app, portrait/landscape/safe area.
- Build: Android IL2CPP ARM64; iOS Xcode/IL2CPP compile/link native bridge và iPhone thật. Test Editor không thay thế device gate. Không tuyên bố production-ready khi thiếu iOS device evidence.
- Release: manifest/asmdef graph không vòng, Runtime không Editor, sample `.meta`/prefab refs sạch, API docs và changelog khớp; package commit/tag và consumer lock pin cố định.

Harness dự kiến `Tests~/validate-connectivity.py` cần được viết ở C1/C2, không phải lệnh hiện có. Harness chạy managed tests/contract/assembly checks; Unity EditMode/PlayMode và native device behavior có report riêng. `VALIDATION.md` ghi version, SHA, command, kết quả và gate chưa chạy.

**Done khi** một game dùng SDK bool, một game dùng backend gateway và một host custom UI đều dùng chung domain; sample import mới chạy thật; Retry/Settings/resume/cancel không lặp hành động hoặc rò lifecycle; có bằng chứng build/device theo platform đã công bố.

## 12. Quan hệ với Economy

Xem [plan Economy và resource acquisition](DREAMY_ECONOMY_RESOURCE_RECOVERY_PLAN.md). Connectivity chỉ bọc đường đi cần mạng như load catalog/purchase gateway. Spend local và panel thiếu resource không bắt buộc mạng. Nếu đang xử lý mua hàng, host điều phối modal theo thứ tự; không để panel Connection và panel thiếu resource mở lồng vô hạn.
