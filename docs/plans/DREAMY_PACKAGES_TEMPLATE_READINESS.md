# Rà soát Dreamy packages trước khi đưa vào template

Ngày khảo sát: 2026-10-01. Phạm vi: 12 submodule trong `LocalPackages/`, sandbox hiện tại và `../dreamy-template-project`. Đây là **đánh giá tĩnh và kế hoạch kiểm chứng**, chưa phải kết luận một lần import sạch hoặc bản build player đã chạy. Sandbox đang có thay đổi chưa commit ở samples, `_Project`, manifest và submodule Settings; template cũng có thay đổi chưa commit. Giữ nguyên chúng khi thực hiện các phase.

## Kết luận hiện tại

**Chưa nên coi bộ package là codebase ổn định cho nhiều game.** Nền tảng có hợp đồng và phân lớp khá rõ; các feature mới vẫn ở `0.1.0`, sample phát hành chưa đồng bộ với prefab đã hoàn thiện trong sandbox, còn sai lệch package/tag/lock và thiếu kiểm thử chuyển vào consumer. Không thể hứa “import vào template sẽ không lỗi” trước khi chạy một lần import mới, compile, mở prefab, Play Mode và build player ở chính template.

| Mức | Phát hiện có bằng chứng | Tác động / việc cần làm |
| --- | --- | --- |
| P0 | Template `Packages/manifest.json` trỏ `com.dreamy.editor-tools` đến `Dreamy-Game-Foundation/-com.dreamy.editor-tools.git`, khác URL canonical trong `.gitmodules`. | Sửa URL, resolve lại lock; kiểm tra UPM không báo lỗi. |
| P0 | Template `Assets/_Project/Scripts/Dreamy.Template.Runtime.asmdef` tham chiếu `Dreamy.Audio.Editor` trong assembly Runtime không giới hạn platform. | Bỏ tham chiếu Editor; compile Android/iOS hoặc player target thích hợp. |
| P0 | Template pin Core `v1.1.1`, Assets `v1.0.0`, UI `v1.0.2`, DataConfig/Datasave `v1.0.0`; feature mới khai báo Core `1.1.2`, Assets `0.1.1` (qua UI), UI `0.2.0`, DataConfig/Datasave `0.2.0`. Lock template còn phản ánh bộ tag cũ. | Chốt một tập commit/tag tương thích rồi nâng template có kiểm soát; không trộn bản cũ với feature mới. |
| P0 | Source `Samples~/Settings Feature/Prefabs/SettingsPanel.prefab` chỉ 748 byte, trong khi prefab đã chỉnh ở `Assets/Samples/.../SettingsPanel.prefab` khoảng 70 KB. `RateUsPanel` tương tự 746 byte so với khoảng 57 KB. Source sample chưa đúng lời hứa “dùng ngay”. | Hoàn thiện prefab **trong package** từ bản đã kiểm chứng, giữ GUID/reference; thử import mới. |
| P0 | Source Settings sample có 9 file nhưng chỉ 2 `.meta`, Daily Reward 10 file nhưng chỉ 2 `.meta`; prefab tham chiếu GUID script. | Kiểm tra GUID trên import sạch; bổ sung `.meta` cho asset cần tham chiếu nếu cần. Không nhận prefab chỉ vì file tồn tại. |
| P1 | `Samples~` có ở Assets, Core, DataConfig, Datasave, UI nhưng `package.json` không khai báo `samples`; Package Manager không hiển thị các sample này để import. | Khai báo samples hoặc xác định chúng chỉ là ví dụ nội bộ. |
| P1 | Compatibility registry ở `../dreamy-codex-toolkit/compatibility/dreamy-packages.json` ghi UI và DataConfig là `drift`. Kiểm tra lại package cache Unity 6 cho thấy `Unity.TextMeshPro.asmdef` nằm trong `com.unity.ugui` 2.0.0 đã khai báo bởi UI; nhận định UI thiếu dependency TMP trong registry là lỗi thời. DataConfig Runtime dùng UniTask nhưng manifest ban đầu không khai báo trực tiếp. | Đã thêm UniTask vào manifest DataConfig; cập nhật registry theo commit thực khi phát hành và xóa nhận định UI sai sau consumer gate. |
| P1 | Settings/Shop/Daily Reward sample dùng `ServiceLocator.Get` trong `Awake`; template khởi tạo services bất đồng bộ trong `GameInstaller.InitializeAsync`. | Chỉ spawn/mở panel sau `BootstrapState.Ready` và sau feature installer, hoặc truyền dependency vào controller khi tạo. |
| P1 | Template và sandbox đều dùng Unity `6000.4.12f1`, nhưng template hiện không cài 5 package feature. | Kiểm tra import theo đúng thứ tự, manifest/lock, asmdef, Console và player build; phiên bản Unity giống nhau không đủ để chứng minh tương thích. |

Nguồn chính: `.gitmodules`, hai `Packages/manifest.json` và `packages-lock.json`, `ProjectSettings/ProjectVersion.txt`, các `LocalPackages/*/package.json`, `*.asmdef`, `Samples~`, `Assets/Samples`, template `GameInstaller.cs`, `../dreamy-codex-toolkit/toolkit.json` và compatibility registry. `toolkit.json` nằm ở repo toolkit bên cạnh, **không** nằm trong root sandbox; dữ liệu registry chỉ là ảnh chụp ở commit đã ghi, không tự động xác nhận các feature mới. Theo [Unity Manual về package samples](https://docs.unity3d.com/6000.0/Documentation/Manual/cus-samples.html), chỉ sample được khai báo trong `package.json` mới có luồng Import của Package Manager; thao tác Import sao chép nội dung sang `Assets`.

MCP tại thời điểm khảo sát chỉ kết nối `dreamy-package-sandbox` (Unity 6000.4.12f1), không có instance template. Sandbox Editor đang idle, không compile, Console truy vấn 20 warning/error gần nhất trả 0 mục; editor báo `external_changes_dirty=true`. Đây là baseline của sandbox, **không** là bằng chứng import/compile/build template.

## Đánh giá từng package

Mức dưới đây là **mức sẵn sàng để đưa vào template**, không phải điểm chất lượng tuyệt đối. `Cần kiểm chứng` nghĩa là chưa có bằng chứng ở consumer mới.

| Package (checkout hiện tại) | Bằng chứng / điểm mạnh | Khoảng trống ưu tiên | Mức |
| --- | --- | --- | --- |
| `com.dreamy.core` 1.1.2 | Runtime nền tảng, ServiceLocator/EventBus/lifecycle; có Runtime tests. | Template còn pin 1.1.1; kiểm tra reset domain, đăng ký trùng và vòng đời khi đổi scene trong consumer. | Có điều kiện |
| `com.dreamy.assets` 0.1.1 | Loader Addressables/Resources và ownership API; có sample BasicAddressables trên disk. | Chưa thấy test assembly; sample chưa khai báo; kiểm tra release handle, request trùng, cancel và lỗi load. | Có điều kiện |
| `com.dreamy.audio` 0.1.0 | Runtime/Components/Editor tách riêng; EditMode và PlayMode tests, 2 sample được khai báo. | Chốt profile/mixer asset trong template, kiểm tra stop/scene unload và player build; tag template 1.0.2 không phản ánh `package.json` checkout. | Có điều kiện |
| `com.dreamy.dataconfig` 0.2.0 | Typed JSON, validation, source abstraction; editor validation. | Đã bổ sung khai báo UniTask trực tiếp trong worktree; còn thiếu regression test cho config sai/thiếu và init nhiều lần; template ở tag 1.0.0. | Chưa sẵn sàng làm chuẩn |
| `com.dreamy.datasave` 0.2.0 | Versioned save, backup/migration/codec; có Runtime tests. | Template ở tag 1.0.0; cần consumer test migration, dữ liệu lỗi, resume/pause và codec trên player; sample chưa khai báo. | Có điều kiện |
| `com.dreamy.editor-tools` 0.2.1 | Editor-only assembly; công cụ nội bộ. | URL template sai; registry chưa xác nhận headless/dry-run/JSON contract, không dùng làm nền cho tự động hóa MCP khi chưa có API công khai. | Nội bộ, chưa là contract |
| `com.dreamy.ui` 0.2.0 | Panel/layer/tween/safe area, có Editor tween test. | TMP đến từ `com.unity.ugui` 2.0.0; đã khai báo sample BasicUI trong worktree. Còn phải kiểm tra canvas, back, safe area, panel close/reopen, scene unload trên consumer. | Có điều kiện |
| `com.dreamy.feature` 0.1.0 | Contracts nhỏ cho feature. | Chưa có sample/test; rà lại liệu các contracts có người dùng thực và tránh biến thành “kho chung”. | Thử nghiệm |
| `com.dreamy.feature.economy` 0.1.0 | Wallet Datasave, idempotent grant và ResourceUIHolder tách assembly. | Chưa thấy test assembly; test atomic grant/exchange, tràn số, retry, save/load; xem lại dependency UI ở manifest cho consumer chỉ dùng domain. | Chưa sẵn sàng làm chuẩn |
| `com.dreamy.feature.daily-reward` 0.1.0 | Model/presenter/installer và model test; sample được khai báo. | Kiểm tra ngày đổi múi giờ, clock rollback, claim lại, persistence; sample GUID/prefab và ví demo phải qua import sạch. | Cần kiểm chứng |
| `com.dreamy.feature.shop` 0.1.0 | Catalog/model/presenter, ShopModel tests; có gateway host cho IAP. | Xác nhận giao dịch thực, retry/idempotency, giá và inventory; demo gateway chỉ mô phỏng, không dùng production; prefab sample đã chỉnh khác source package. | Cần kiểm chứng |
| `com.dreamy.feature.settings` 0.1.0 | Runtime không có concrete view; gateway platform do host sở hữu. | Chưa thấy tests; source prefabs chỉ là khung; kiểm tra save setting, consent/restore/rate-us, unregister và gateway thật. Submodule hiện dirty. | Chưa sẵn sàng import dùng ngay |

`Tests` ở đây được ghi từ file test/asmdef tìm thấy, **không phải** kết quả test chạy thành công. Một package có tests vẫn cần compile và chạy đúng commit trong Unity.

## Import sample rồi chuyển sang `_Project`

Có thể dùng sample làm **điểm khởi đầu sở hữu bởi game**. Chưa thể bảo đảm “move là chạy” với trạng thái hiện tại. Quy trình đề xuất:

1. Chốt commit/tag của 12 package và backup/branch template với trạng thái hiện có. Sửa P0 manifest và Runtime asmdef; resolve lock, xác nhận mọi package cần thiết đều hiện trong Package Manager.
2. Import từng sample đã khai báo qua Package Manager. Nếu package thiếu `samples`, bổ sung metadata trong package trước; không copy trực tiếp từ `Samples~` rồi coi như đã kiểm chứng.
3. Trong Unity Editor, **Move** cả folder sample cùng `.meta` sang `Assets/_Project/Features/<Feature>/` bằng Project window/AssetDatabase. Nếu muốn sửa visual theo game, ưu tiên prefab variant dưới `_Project` khi source prefab package là bản chuẩn; nếu sample được thiết kế copy-ready thì game sở hữu bản sao. Không để cả bản `Assets/Samples` và `_Project` có cùng asmdef/class.
4. Kiểm tra tất cả prefab: `Missing Script`, `Missing Prefab`, serialized fields, TMP/font/sprite, nested prefab/variant source, Addressables key, EventSystem và safe area. Prefab nguồn Settings hiện **không đạt** bước này.
5. Chuyển JSON config vào một `Resources/DataConfig` hoặc provider do host quy định; giữ đúng key `shopCatalog`, `dailyRewardSchedule` và thứ tự `RegisterConfig` **trước** `InitializeAsync`. Không để cùng resource path ở hai folder `Resources`.
6. Ở `GameInstaller`, đăng ký Datasave/Audio/wallet/platform gateway, đăng ký config, chờ config init xong, cài feature services, rồi mới cho UI mở. Chọn **một** `IResourceWallet` có persistence cho Shop và Daily Reward; ví `InMemoryResourceWallet` và gateway giả chỉ dùng demo.
7. Compile, đọc Console, mở từng panel trong Play Mode, đóng/mở lại, chuyển scene, reload save; sau đó build player. Chỉ lúc này mới kết luận sample di chuyển an toàn.

Lưu ý: tài liệu source sample Settings hiện mô tả panel/variant đầy đủ, nhưng prefab trong package mới là khung; README của Daily Reward cũng nói “copy entire folder” đồng thời bảo tự tạo host prefab. Chuẩn hóa lời hứa “copy-ready” theo đúng nội dung được phát hành.

## Kiến trúc ServiceLocator và khả năng đổi sang VContainer

**Nên giữ ServiceLocator trong Core hiện tại và chuẩn hóa constructor/explicit injection ở model, presenter, leaf UI.** Hiện Shop/Daily Reward/Settings đã có interface và overload installer nhận dependency tường minh ở vài chỗ, nên chuyển composition root về sau là khả thi. Công việc sẽ **không nhanh theo kiểu thay một dòng ở Core**: `GameInstaller`, các feature installer, controller sample `Awake`, ResourceUIHolder, vòng đời scene/panel và test fixtures còn gọi locator. Tốc độ phụ thuộc vào mức giảm các call site này trước khi migrate; chưa có số đo để ước lượng ngày công đáng tin.

Nếu cần scope theo scene/feature, xác thực dependency lúc khởi động hoặc dự án lớn có nhiều service, làm thử một vertical slice Shop hoặc Settings bằng `LifetimeScope` ở `_Project`, không đặt VContainer trong Core. Giữ các package domain/presenter không phụ thuộc framework; chuyển phần đăng ký từ installer sang host adapter và truyền dependency rõ ràng. Không vận hành đồng thời hai container làm chủ cùng một service. [VContainer](https://github.com/hadashiA/VContainer) hỗ trợ `LifetimeScope` và child scope; đây là khả năng của thư viện, còn lợi ích cho Dreamy cần được đo bằng prototype.

Tiêu chí quyết định: một feature chuyển được mà không đổi domain/presenter; thứ tự khởi tạo và dispose rõ; scene unload không rò; các sample vẫn có đường cài đơn giản; build/IL2CPP và test pass; công viết/wiring giảm trong 2 feature tiếp theo. Nếu không đạt, ServiceLocator ở composition root cùng explicit dependencies là đủ cho casual nhỏ.

## Phases thực hiện

| Phase | Việc làm cụ thể | Đầu ra và gate bắt buộc |
| --- | --- | --- |
| 0. Baseline có thể lặp lại | Ghi commit SHA từng submodule, dirty state, `package.json`, manifest/lock hai consumer; so sánh registry với checkout; tạo matrix dependencies/asmdefs/samples. | Một bảng versions/commit duy nhất; mọi drift được gắn owner; không sửa đè thay đổi đang có. |
| 1. Gỡ blocker import | Sửa URL Editor Tools trong template; loại Editor ref khỏi `Dreamy.Template.Runtime`; chốt/pin bộ tag/commit tương thích, cập nhật lock; sửa dependency khai báo thiếu ở UI/DataConfig. | UPM resolve sạch; Editor compile sạch; Runtime asmdef không tham chiếu Editor; build player tối thiểu thành công. |
| 2. Hoàn thiện package source/sample | Đồng bộ prefab Settings/Shop/Daily Reward đã kiểm chứng về `Samples~`; giữ GUID/script `.meta` cần thiết; thêm khai báo sample thiếu; sửa README đúng nội dung; kiểm tra duplicate resource path và simulated gateway. | Import **mới** vào consumer sạch: không Missing Script/Prefab, fields đã nối, panel mở/đóng được. Không dựa vào sample đã sửa ở sandbox. |
| 3. Consumer integration | Trong template, đăng ký wallet bền vững, Audio/gateway/config/feature theo thứ tự; chuyển sample sang `_Project`; giữ một owner cho config và save; tạo scene demo tích hợp Settings, Daily Reward, Shop, holders. | Play Mode từ bootstrap tới từng panel; claim, mua bằng currency, restart rồi kiểm tra balance/claim/settings; Console 0 errors. |
| 4. Hardening từng package | Thêm test tập trung cho economy transaction/idempotency, clock/day boundary, shop retry, settings persistence; kiểm tra AssetLoader lifecycle, UI panel lifecycle, Datasave migration; cập nhật API docs/changelog/semver. | Test liên quan pass tại SHA phát hành; EditMode/PlayMode và Android/iOS build tối thiểu theo mục tiêu; registry cập nhật từ SHA thật. |
| 5. Chuẩn hóa phát hành | CI cho mỗi package và một consumer fixture; pin tags, lock, sample smoke, README install/migration; release checklist và rollback. | Không có tag/package/lock drift, build consumer lặp lại được trên máy sạch; package chỉ được gắn `stable` sau gate này. |
| 6. Mở rộng tính năng | Ưu tiên backlog bên dưới dựa trên game thật, mỗi package có contract, sample, test và consumer slice riêng. | Không tạo package chỉ vì có tên trong roadmap; ít nhất hai consumer hoặc nhu cầu reuse đã rõ. |

### Kịch bản kiểm chứng nhỏ nhất cho phase 1–3

- `git submodule status`, `git status --short`, parse tất cả manifest/asmdef JSON và so SHA trước/sau.
- Unity 6000.4.12f1 mở template, Package Manager resolve xong, Console 0 compile errors; kiểm tra `packages-lock.json` phản ánh commit/tag đã chốt.
- Import sample ở **một checkout sạch**; dùng Unity kiểm tra tất cả prefab và references; chỉ Move qua Editor, rồi kiểm lại references. Import trong sandbox hiện tại không chứng minh case này.
- Play Mode: bootstrap Ready; Settings kéo Music/SFX rồi restart; Daily Reward claim một lần, restart, thử claim lại; Shop exchange currency, retry cùng transaction ID và kiểm tra balance; mở/đóng panel lặp, chuyển scene.
- Chạy targeted package tests có sẵn và một player build. Ghi chính xác command, Unity log, commit, pass/fail cho từng gate.

## Backlog package cho casual và puzzle

Thứ tự là đề xuất; chỉ tách thành package khi có nhu cầu reuse thực. Các mục `feedback`/`localization` đã có record trong toolkit registry nhưng **chưa được import trong sandbox/template hiện tại**, nên là việc tích hợp/kiểm chứng trước khi tự viết mới.

| Ưu tiên | Package hoặc module | Ranh giới hợp lý / điều kiện |
| --- | --- | --- |
| Sau phase 3 | Feedback, Localization | Kiểm tra repo/commit sẵn có, tích hợp audio/haptic/VFX và key/locale/TMP; không ép feature Runtime phụ thuộc visual. |
| Sau phase 3 | Scene loading, pooling, tutorial | Khởi tạo/load/cancel và transition có owner; pool reset state; tutorial theo milestone game. Có thể bắt đầu trong `_Project` nếu chưa có consumer thứ hai. |
| Theo sản phẩm | Progression/level, energy/lives, objectives/quests, achievements | Domain state và versioned save; UI + reward delivery do host. Dùng wallet hiện có, không tạo nhiều nơi cộng tiền. |
| Theo monetization | IAP adapter, ads/rewarded ads, remote config, analytics | SDK nằm ở adapter `_Project` hoặc package integration riêng; xác nhận giao dịch, idempotency, consent/privacy, offline/retry trước khi grant reward. |
| Puzzle vertical slice | Board/grid, rules/move validation, level data, win/lose, undo/replay, hint, boosters | Giữ engine puzzle pure C# và deterministic; view/animation/Input thuộc host. Test replay cùng seed, undo và save giữa level trước khi tách package. |
| Khi có nhu cầu thực | Live events, battle pass, gacha, cloud save, content delivery | Chỉ mở khi backend, economy, gian lận, vận hành và migration đã có yêu cầu rõ; không đưa vào Core. |

## Quyết định cần chốt khi bắt đầu thực thi

1. Commit/tag cụ thể cho mỗi package trong template: dùng các checkout feature hiện tại hay phát hành tag mới sau hardening.
2. Sample là bản “copy-ready đầy đủ” hay “skeleton để tự ráp”. Hiện README và prefab nguồn chưa thống nhất.
3. Game đầu tiên dùng wallet Datasave nào và gateway IAP/consent nào; không phát hành gateway mô phỏng làm mặc định.
4. Platform gate: Android, iOS hay cả hai; hiện chưa chạy build trên template nên không gán trạng thái pass.

Quy tắc áp dụng trong quá trình thực hiện nằm ở [`docs/rules/DREAMY_PACKAGE_DESIGN_RULES.md`](../rules/DREAMY_PACKAGE_DESIGN_RULES.md).
