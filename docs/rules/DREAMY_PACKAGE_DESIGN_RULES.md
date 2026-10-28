# Dreamy: quy tắc thiết kế package, sample và tích hợp Unity

Trạng thái: quy tắc thiết kế cho các phase trong [`plan`](../plans/DREAMY_PACKAGES_TEMPLATE_READINESS.md). Áp dụng cho `LocalPackages/com.dreamy.*`, consumer template, sample được chuyển sang `Assets/_Project`, và thao tác Unity qua MCP. Quy tắc này không thay cho kiểm chứng theo commit. Với toolkit Codex, `../dreamy-codex-toolkit/toolkit.json` là nguồn canonical về trạng thái, maturity, module/preset/skill/harness; đối chiếu compatibility registry và source package trước khi khẳng định API. Nếu toolkit không có trong checkout consumer, ghi “chưa xác minh” và kiểm tra source/manifest trực tiếp.

## 1. Nguồn sự thật và release

1. Trước khi dùng hoặc sửa package, đọc `Packages/manifest.json`, `Packages/packages-lock.json`, `.gitmodules`, `package.json`, asmdef và commit SHA submodule. Version trong README không thay thế lock/commit.
2. URL trong `.gitmodules` là URL repo hợp lệ; `.gitmodules` của consumer, manifest Git URL và tài liệu install phải thống nhất. Ghi rõ tag hoặc commit, không dựa vào `main` cho release có thể lặp lại.
3. `package.json` version, tag Git, changelog, dependency version và compatibility registry phải khớp tại commit phát hành. Khi đổi public API, serialized data hoặc asmdef, ghi migration và tăng version theo mức thay đổi; không đổi tag đã phát hành.
4. Mỗi package khai báo **mọi dependency dùng bởi Runtime/Editor asmdef và source của nó**. Tách Editor assembly khỏi Runtime; Runtime không tham chiếu `UnityEditor`, assembly `.Editor` hoặc `#if UNITY_EDITOR` để che một dependency Editor trong asmdef.
5. Submodule phải ở commit cố định trong repo consumer. Kiểm tra trạng thái dirty trong từng submodule trước khi cập nhật gitlink; không dùng thay đổi chưa commit của sandbox làm bằng chứng cho bản tag của template.
   Luồng phát hành: sửa và kiểm chứng trong repo package -> commit/push package -> tạo tag theo version -> cập nhật gitlink hoặc Git URL pin ở consumer -> resolve lock -> chạy consumer gate. Commit ở repo cha không tự mang theo thay đổi chưa commit trong submodule.
6. Package công bố sample trong `package.json` bằng `samples` trỏ đến folder có thật trong `Samples~`. Nếu sample chỉ là nội bộ, ghi rõ và không gọi là importable.

## 2. Ranh giới package và SOLID thực dụng

1. Code dùng chung qua nhiều game, có hợp đồng rõ và consumer test thuộc package. Code theo art, flow, SDK, pricing và luật riêng game thuộc `Assets/_Project`. Core chỉ chứa primitive nền tảng; không đẩy feature vào Core.
2. Dependency một chiều: Core/DataConfig/Datasave và các contract nền -> domain feature -> host integration/UI. Package không tham chiếu code ở `_Project`; domain không tham chiếu prefab, concrete panel, SDK store/ads hoặc Editor.
3. Model/domain xử lý quy tắc và giao dịch; presenter phối hợp model/service với view; view chỉ nhận input, render state và phát UI events. Không đặt save, pricing, wallet mutation hoặc SDK call trong panel.
4. Dùng interface khi có ranh giới thay thế thật: clock, wallet, config/source, save, purchase gateway, platform gateway, view. Không tạo interface/manager/factory cho từng class chỉ để “đủ SOLID”. Một class có một lý do thay đổi có thể kiểm tra được.
5. Mở rộng qua adapter ở host cho SDK và presentation, không sửa domain cho từng game. Không thêm package dependency chỉ để sample compile nếu dependency đó không thuộc Runtime contract; ghi rõ dependency sample và thử import.
6. Chọn owner cho từng dữ liệu: DataConfig = dữ liệu design tĩnh; Datasave = trạng thái người chơi bền vững và migration; runtime state = session; view = biểu diễn. Một giao dịch reward chỉ có một owner ghi save và một idempotency key khi có thể retry.
7. Không tối ưu CPU/GC/bộ nhớ bằng suy đoán. Có baseline và thiết bị mục tiêu trước khi thêm pool/cache/phức tạp hóa code.

## 3. Composition, MVP và lifecycle

1. ServiceLocator chỉ được resolve ở `GameInstaller`, feature installer/root, presenter hoặc controller cấp cao đã có quy ước. Model, item, leaf UI, pooled object, VFX và projectile nhận dependency tường minh. Cho phép overload installer nhận dependency trực tiếp để sau này đổi DI.
2. `GameInstaller` quy định thứ tự: tạo save và service nền -> đăng ký config types -> `InitializeAsync` -> đăng ký wallet/gateway -> cài feature services -> báo Ready -> tạo/mở panel. Controller gọi `Get<T>` trong `Awake` chỉ an toàn nếu parent bảo đảm đã Ready.
3. Mỗi subscription, UniTask/coroutine, DOTween, Addressables handle, pool lease và service registration có owner và cleanup path. Presenter `Dispose` khi view bị hủy; hủy async theo owner; scene unload không để callback chạy vào object đã chết.
4. Prefab UI không tự lén tạo thêm source of truth cho balance/config. Item nhận state từ panel/presenter. Một panel có contract bind, show, refresh, close rõ; mở lại không nhân đôi event listener.
5. Không bắt buộc VContainer trong package domain. Nếu consumer dùng VContainer, đặt `LifetimeScope` và registration trong `_Project` hoặc adapter riêng, truyền dependencies vào API hiện có. Không để VContainer và ServiceLocator cùng làm owner của một service. Chỉ quyết định migration sau prototype và test vòng đời.

## 4. Sample và prefab dùng được thật

1. Phân loại rõ: **copy-ready** = import mới, compile, prefab đủ component/serialized refs, mở và tương tác được khi host cài dependency đã ghi; **skeleton** = code/vỏ ví dụ cần tự ráp. README, `package.json` và asset phải cùng lời hứa.
2. Source of truth của sample phát hành là `Samples~` trong package ở commit/tag, không phải bản đã sửa trong `Assets/Samples` của sandbox. Sau mỗi sửa prefab ở sandbox, đưa thay đổi được chấp nhận trở lại package bằng Unity rồi thử import trên consumer mới.
3. Prefab/variant/nested prefab phải giữ `.meta` GUID của mọi script, sprite, font, source prefab và asset được serialize. Tạo/sửa prefab bằng Unity Editor/MCP; không ghép YAML thủ công. Kiểm tra `Missing Script`, `Missing Prefab`, override và field null trong Inspector.
4. Khi chuyển sample vào `_Project`, dùng Move trong Unity Project window hoặc `AssetDatabase.MoveAsset`; chuyển cùng `.meta`, cả folder cần thiết và dependency nội bộ. Chỉ copy khi cần bản độc lập có GUID khác; sau copy kiểm tra reference và asmdef. Không để 2 bản asmdef cùng tên hoặc 2 class trùng cùng assembly.
5. Prefab variant có thể phụ thuộc base prefab ở package nếu host chấp nhận giữ package này lâu dài. Nếu mục tiêu là asset độc lập tùy chỉnh sâu, tạo prefab owned by project, document dependency và kiểm tra khi nâng package.
6. `Resources/DataConfig/<key>.json` chỉ có một bản được load trong consumer. Đổi vị trí vẫn phải giữ resource path/key hoặc cập nhật registration; không coi việc file tồn tại là config đã được register trước initialize.
7. Sample wallet trong memory, store gateway giả, consent/rate-us giả phải được đánh dấu demo và không xuất hiện trong đường production. Consumer thay bằng adapter có persistence và giao dịch xác nhận.
8. Sample UI phải xét portrait/landscape theo game, safe area, nhiều aspect ratio, TMP/font, touch target, back/close và trạng thái loading/error/empty; chỉ yêu cầu các state thuộc contract của feature. Chụp bằng chứng ở Game view sau import.

### 4.1. Kế thừa BaseFeaturePanel khi tạo UI feature

1. **Panel/popup feature chuẩn phải được tạo dưới dạng Prefab Variant của** `Packages/com.dreamy.feature/Runtime/Prefabs/BaseFeaturePanel.prefab` (source local: `LocalPackages/com.dreamy.feature/Runtime/Prefabs/BaseFeaturePanel.prefab`). Áp dụng cho sample và UI production dùng nền Dreamy, bao gồm Battle Pass, Premium popup và RewardPanel. Không dựng lại một root tương đương chỉ để có CanvasGroup, Backdrop, tween và SafeArea riêng.
2. Đây là **kế thừa prefab**, không phải một class C# `FeaturePanelBase`. Script concrete view vẫn kế thừa `UIPanel`, implement view contract; presenter dùng `FeaturePresenter` và adapter `IPanelPresenter` theo assembly boundary hiện có. Không đưa UnityEngine/UI vào `Dreamy.Feature.Runtime` để tạo base MonoBehaviour cho đủ tên.
3. Giữ GUID và liên kết source của base. Thêm concrete panel component và nội dung feature dưới container phù hợp sau khi inspect hierarchy thực tế; bind `canvasGroup`/`tweenPlayer` và các field view bằng Editor API. Tái sử dụng SafeArea/Backdrop/transition kế thừa, tránh hai owner SafeArea trên cùng content hoặc hai tween điều khiển cùng target. Override art/layout/transition có chủ đích ở variant; không unpack hoặc copy hierarchy để giả làm kế thừa.
4. Base chỉ sở hữu cấu trúc hiển thị và cấu hình UI chung. Wallet, claim, save, pricing, service resolution và feature state tiếp tục ở model/presenter/composition root; không thêm logic Battle Pass/reward vào base prefab hoặc Core. Khi nhiều feature dùng RewardPanel, host sở hữu và inject panel dùng chung; không mặc định tạo thêm một panel thưởng production cho mỗi feature.
5. **Ngoại lệ có lý do cụ thể:** HUD entry, badge, inline widget, list item và overlay chuyên dụng không có cấu trúc panel chuẩn không buộc kế thừa BaseFeaturePanel. Nếu host đã có base panel riêng hoặc cần asset độc lập với package base, ghi base thay thế/lý do trong README hoặc plan, cùng owner SafeArea/transition và dependency thực tế. Không mặc định dùng ngoại lệ cho toàn bộ sample.
6. Sample builder cho panel và item phải instantiate prefab base và lưu thành variant bằng Unity Editor/PrefabUtility. Nếu base không tồn tại hoặc thiếu dependency/component cần thiết, báo lỗi rõ; không âm thầm chuyển sang dựng panel mới. Khi regenerate variant đã có, giữ `.meta` GUID, source link và các override art được bảo vệ theo contract của builder.
7. **Gate trước khi bàn giao:** kiểm tra `PrefabUtility.GetPrefabAssetType(...) == PrefabAssetType.Variant`, kiểm tra source prefab chain dẫn về đúng BaseFeaturePanel/BaseFeatureItem theo loại asset (hoặc base thay thế đã document), không Missing Script/Prefab, required serialized references đầy đủ; inspect SafeArea/Backdrop/tween và chạy Show/Hide/Back/reopen. Kiểm source `Samples~` lẫn sample import mới/consumer. C# kế thừa `UIPanel` hoặc test claim pass chưa đủ chứng minh prefab tuân thủ rule này.
8. Khi phát hiện asset cũ chưa tuân thủ, ghi thành migration task cụ thể; chuyển qua Unity Editor/MCP, bảo toàn GUID, references, prefab overrides và source sample. Evidence behavior/build trước migration vẫn thuộc asset cũ; sau migration phải chạy lại prefab inspection và UI/consumer checks liên quan trước khi nhận là đã đạt gate kế thừa.

### 4.2. Kế thừa BaseFeatureItem cho item view

1. **Item view feature chuẩn phải là Prefab Variant từ** `Packages/com.dreamy.feature/Runtime/Prefabs/BaseFeatureItem.prefab` (source local tương ứng trong `LocalPackages`). Áp dụng cho tier/row, reward slot, reward display item và feature card. Item view không kế thừa panel base.
2. Tái sử dụng Image, Button, LayoutElement, CanvasGroup, tween và container Icon/Content kế thừa; thêm script view và layout riêng trên variant. Không dựng root tương đương rồi copy hierarchy. Item chỉ hiển thị có thể disable root Button/raycast kế thừa để child button tiếp tục nhận input; thay đổi này phải là override có chủ đích.
3. HUD/inline widget có cấu trúc khác hoặc host có base item riêng chỉ dùng ngoại lệ khi ghi rõ lý do và base thay thế trong README/plan. Khi HUD entry dùng hình thái feature card, ưu tiên variant từ BaseFeatureItem như sample BattlePassEntry.
4. Builder, gate source chain/GUID/bindings và migration qua Unity Editor/MCP ở mục4.1 áp dụng tương tự cho item. Kiểm tra cả nested item prefab bên trong tier, item được instantiate lúc runtime và asset trong Samples~ sau export/import mới.

## 5. Coding convention và Unity serialization

1. Theo style gần file đang sửa: C# PascalCase cho type/method/property, camelCase cho field private, `I` cho interface; một public type chính một file; namespace theo package/feature. Không đổi hàng loạt style khi sửa một bug.
2. Serialized fields dùng `[SerializeField] private`, tránh đổi tên/type hoặc chuyển asmdef khi chưa có kế hoạch migration. Nếu phải đổi, dùng `FormerlySerializedAs` khi phù hợp và kiểm prefab/scene thực.
3. Ưu tiên constructor cho pure C# model/presenter; MonoBehaviour chỉ giữ reference Unity và lifecycle. Không dùng static mutable state thay cho save hoặc scene owner. Không nuốt exception; log tại boundary có context, để domain trả kết quả có nghĩa.
4. Async phải có cancellation theo owner, quan sát lỗi, không fire-and-forget không kiểm soát. Các event UI được gắn/tháo đối xứng; tween bị kill khi owner bị disable/destroy.
5. ID của config, resource, audio, localization, IAP và save phải ổn định, được validate; đừng dùng display text hoặc tên GameObject làm key bền vững.
6. Tránh Editor API trong Runtime, API test trong player assembly và reference vòng giữa asmdefs. Chạy kiểm tra asmdef graph khi thêm dependency.

## 6. MCP và tự động hóa Editor

1. Trước thao tác có state, đọc `mcpforunity://instances`, chọn đúng Unity instance, đọc `mcpforunity://custom-tools` và editor state; xác nhận đúng project/path. Nếu MCP không kết nối, dùng Unity batchmode/harness hoặc ghi rõ chỉ kiểm tra tĩnh.
2. Luồng bắt buộc: inspect asset/scene/prefab và `git status` -> thao tác nhỏ -> save -> refresh/đợi compile -> Console -> prefab/scene inspection -> targeted tests -> diff/status. Không gọi công cụ tạo hàng loạt theo giả định hierarchy hoặc GUID.
3. Ưu tiên API Unity/MCP cho scene, prefab, importer, `.meta` và serialized reference; text edit chỉ cho source code và JSON/Markdown đơn giản. Không raw-edit prefab/scene YAML để nối object.
4. Thao tác package/scene phải có rollback rõ và không sửa asset người dùng ngoài phạm vi. Khi asset đang dirty ở Editor hoặc worktree, lưu trạng thái/bằng chứng trước khi thay đổi; không xóa/reimport toàn project để “thử sửa”.
5. MCP không phải chứng cứ nếu chỉ trả về lệnh đã gửi. Ghi output compile/Console, test results, ảnh hoặc inspection của prefab, cùng Unity version và commit. Editor Tools không được coi là API headless/dry-run cho tới khi contract công khai đã được xác minh.

## 7. Gate tối thiểu trước khi gọi package hoàn thiện

- **Source:** manifest, dependency, asmdef Runtime/Editor, sample declaration, `.meta`, README và changelog thống nhất; commit/tag/lock khớp.
- **Compile:** Unity Editor compile sạch ở package sandbox và ít nhất một template consumer mới; player build đúng platform mục tiêu.
- **Prefab inheritance:** panel/popup chuẩn là variant từ BaseFeaturePanel, item view chuẩn là variant từ BaseFeatureItem; base thay thế phải có lý do đã document; kiểm source chain, required bindings, SafeArea và transition theo mục 4.1.
- **Behavior:** targeted domain tests cho boundary có rủi ro; import sample mới, mở prefab, Play Mode đi qua user flow thật, kiểm save/restart/scene unload/duplicate action; Console không có error.
- **Release:** có migration nếu public/serialized contract đổi, CI/fixture consumer, artifact log và owner phê duyệt trạng thái maturity. Nếu gate chưa chạy, ghi `chưa xác minh`, không ghi `pass`.

Đối chiếu Unity về [package layout](https://docs.unity3d.com/6000.0/Documentation/Manual/cus-layout.html) và [package samples](https://docs.unity3d.com/6000.0/Documentation/Manual/cus-samples.html) khi thay cấu trúc package hoặc sample.
