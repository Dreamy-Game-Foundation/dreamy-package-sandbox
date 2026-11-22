# Dreamy Debugger — Runtime Hierarchy + Inspector

Ngày: 2026-11-23. Trạng thái: **candidate nội bộ đã triển khai và push source package lên main** (`8593c75`). Kiểm chứng: 102/102 EditMode, 5/5 PlayMode Scene/Inspector. Tiến độ từng hạng mục, source/test và các gate còn mở ở [checkpoint](../evidence/debugger/runtime-inspection/CHECKPOINT.md). Người dùng không yêu cầu chạy thêm build; chưa chứng nhận khả năng phát hành Asset Store.

Baseline khi lập plan: sandbox `f7933dd`; submodule `LocalPackages/com.dreamy.debugger` tại `1516f9fff4f62e7b3acb5f38b566509f5e47291b`, package `0.1.8`. Các phần đánh dấu **đề xuất** bên dưới giữ lại thiết kế gốc; đối chiếu checkpoint để phân biệt phần đã triển khai và gate còn mở.

## 1. Có cần thiết không?

**Nên có, ưu tiên cao cho sản phẩm debugger thương mại toàn diện.** Console trả lời “lỗi gì”; Hierarchy + Inspector giúp trả lời “object nào, còn tồn tại không, đang active không, component và giá trị hiện tại là gì”. Lợi ích rõ nhất khi lỗi chỉ xuất hiện trên Android/iOS hoặc player build, nơi không có Unity Editor Inspector.

| Tình huống của khách hàng | Giá trị thực tế |
| --- | --- |
| Popup không hiện trên device | Tìm object inactive, kiểm tra parent, activeSelf/activeInHierarchy và RectTransform |
| Nhân vật đứng sai vị trí | Kiểm tra local/world transform và sửa giá trị để thử giả thuyết |
| Sau chuyển scene còn object cũ | Duyệt scene và DontDestroyOnLoad, nhận biết object đã hủy |
| Gameplay có trạng thái sai | Xem field của MonoBehaviour, không phải thêm cheat riêng cho mỗi biến |
| QA gửi bug khó tái hiện | Copy object path/component/member để đính kèm báo cáo thủ công |

Đây là đánh giá sản phẩm từ workflow, **chưa phải bằng chứng tăng doanh số hoặc nghiên cứu khách hàng**. Không bắt buộc để mọi debugger được bán, và không phải yêu cầu submission phải có Hierarchy. Nếu chỉ định vị “console + screenshot”, có thể phát hành trước; nếu quảng bá “runtime debugging toolkit”, nên hoàn tất phạm vi P0 dưới đây trước khi marketing tính năng này.

Điểm khác biệt nên là trải nghiệm liền mạch với Console/Cheats/Screenshot, UI mobile dễ dùng, IL2CPP được kiểm chứng và package độc lập. Bản miễn phí tham khảo đã cung cấp nhiều chức năng tương tự; chỉ thêm một cây object sẽ chưa đủ tạo khác biệt.

## 2. Source hiện tại và ràng buộc

Các đường dẫn trong bảng tính từ `LocalPackages/com.dreamy.debugger/`, trừ nơi ghi rõ sandbox.

| Bằng chứng đã đọc | Kết luận và tác động |
| --- | --- |
| Sandbox `ProjectSettings/ProjectVersion.txt` | Unity `6000.4.12f1`; đây là môi trường hiện tại, không chứng minh support mọi Unity 6 |
| Sandbox `Packages/manifest.json`, `Packages/packages-lock.json`; package `package.json` | Debugger local, uGUI `2.0.0`; manifest debugger yêu cầu Unity `6000.0`, không phụ thuộc Dreamy Core/UI/DataConfig/Datasave |
| `Runtime/Dreamy.Debugger.Runtime.asmdef` | Pure .NET, `noEngineReferences: true`; không đưa Transform/GameObject/uGUI vào assembly này |
| `Runtime/Engine/Dreamy.Debugger.Engine.asmdef` | Engine phụ thuộc Runtime/Foundation/Presentation/uGUI; bị loại khi `DISABLE_DREAMY_DEBUGGER` |
| `Runtime/Imported/Presentation/Dreamy.Debugger.Presentation.asmdef` | Presentation không phụ thuộc Engine; không được thêm chiều phụ thuộc ngược tạo vòng |
| `Runtime/Engine/DebuggerRuntime.cs`, `DebuggerConfiguration.cs` | Có owner khởi tạo, access session, mode Off/Development/Qa/ReleaseDiagnostics, teardown; tái sử dụng thay vì singleton mới |
| `Runtime/Imported/Presentation/Scripts/UI/Other/DebugTabController.cs` | Có AddTab, sort, ActiveTabChanged và activation; dùng đường tích hợp này |
| `Runtime/Imported/Presentation/Scripts/UI/DebuggerTabController.cs` | Getter ActiveTab dùng Enum.Parse; tab key mới cần xử lý tương thích, không dùng key tùy ý rồi bỏ qua getter này |
| `Runtime/Imported/Presentation/Scripts/Settings.cs` | DefaultTabs hiện có giá trị 0–5, Screenshot=5; không đổi số cũ |
| `Runtime/Imported/Presentation/Scripts/PresentationAssets.cs`, `DebuggerAssetCatalog.cs`; `Runtime/Prefabs/DebuggerAssets.asset` | Tab/prefab/font/icon được đưa qua catalog; phải ship binding đầy đủ cho consumer |
| `Runtime/Engine/ScreenshotTabController.cs`, `Editor/ScreenshotTabAssets.cs` | Có tiền lệ controller ở Engine, authoring chỉ trong Editor và prefab được lưu sẵn |
| `Runtime/Imported/Foundation/Scripts/UI/DebuggerPalette.cs`; `Documentation~/MATERIAL_UI.md` | Material Compact: Barlow + JetBrains Mono, sprite nhỏ, uGUI Text; dùng cùng palette/font, không thêm TMP chỉ cho feature này |
| `Tests/EditMode`, `Tests/PlayMode`, `Tests/EditorIntegration`, `Tests/StartupEditor` | Có các tầng test để mở rộng; kết quả cũ không thay thế test tính năng mới |
| `Documentation~/RELEASE.md`, `README.md`, `package.json` | Package hiện mô tả dùng nội bộ; license `UNLICENSED`; chưa phải artifact có quyền phân phối công khai được xác nhận |

Sandbox không có `toolkit.json` hoặc `compatibility/dreamy-packages.json` tại thời điểm khảo sát. Plan dựa trên manifest/lock/asmdef/source thực tế, không khẳng định API từ toolkit chưa được xác minh.

### Gate thương mại cần giải quyết sớm

`Documentation~/RELEASE.md` ghi rõ vendor-derived content giữ điều kiện upstream và version change không cấp quyền phân phối công khai. Phải lập inventory source/font/icon/shader, đối chiếu quyền phân phối từng phần trong `Runtime/Imported`, đặc biệt Presentation và mọi nội dung xuất phát từ SRDebugger. Foundation có file MIT riêng; **không suy rộng license đó cho toàn bộ Presentation**.

Nếu không có quyền phù hợp: thay thế phần bị hạn chế bằng implementation/asset có quyền phân phối rõ ràng trước khi bán. Đổi namespace hoặc reskin không giải quyết quyền source. Đây là blocker của **commercial release**, không ngăn viết plan hay prototype nội bộ. Lập danh sách file ảnh hưởng và rebaseline kiến trúc nếu phải thay shell trước khi triển khai tích hợp sâu.

## 3. Tham khảo UnityRuntimeInspector và quyết định tái sử dụng

Repo được yêu cầu: [yasirkula/UnityRuntimeInspector](https://github.com/yasirkula/UnityRuntimeInspector/tree/master). Snapshot khảo sát qua GitHub API: `2d461b975725f4713aee5eb3857661e361ab8d27`.

[README tại snapshot](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/.github/README.md) mô tả hai panel kết nối selection, configurable refresh, skin/drawer, collection, tìm kiếm scene và DontDestroyOnLoad. Dùng làm checklist UX và hành vi. [RecycledListView](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Helpers/RecycledListView/RecycledListView.cs) là tham khảo cụ thể cho viewport recycling và pooling; cần tự benchmark trong Dreamy.

| Hướng | Đánh giá |
| --- | --- |
| Bắt khách cài nguyên upstream | Nhanh để thử nhưng thêm setup, khả năng trùng dependency và skin khác; không chọn cho trải nghiệm mặc định |
| Vendor nguyên repo rồi reskin | Có thể giảm công backend nhưng kéo theo phạm vi bảo trì/license; cần audit đầy đủ, không mặc định chọn |
| **Dreamy-native module, tham khảo hành vi; port có chọn lọc nếu có lợi** | **Hướng đề xuất**: một owner access/input/catalog, UI đồng nhất, phạm vi có thể kiểm thử; không bắt buộc port source để hoàn thành P0 |

Nếu port: ghim SHA, ghi rõ file nào copied/modified, giữ copyright và license, chọn namespace tránh trùng upstream, test cùng project đã cài upstream. [LICENSE MIT](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/LICENSE.txt) có điều kiện giữ thông báo. [ColorWheel license](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/ExternalResources/ColorWheel/ColorWheelLICENSE.txt) là BSD 3-Clause riêng; nếu không mang ColorWheel thì không nhập asset đó. Phân biệt source GitHub với bản tải từ Asset Store và điều kiện phân phối tương ứng. Kết quả audit license phải được ghi nhận trước khi đưa code bên thứ ba vào artifact thương mại.

## 4. Phạm vi phát hành

### P0 — bản đầu đủ hữu ích và được bán như một feature hoàn chỉnh

- Một tab **Scene** trong debugger; desktop có Hierarchy + Inspector song song, mobile dùng hai trang trong cùng tab.
- Hierarchy theo loaded scene, bao gồm inactive object; hỗ trợ DontDestroyOnLoad bằng public runtime API, ẩn cây debugger mặc định.
- Expand/collapse lazy, single selection, reveal selected, search tên object không phân biệt hoa thường; filter scene/active. Kết quả tìm kiếm kèm scene/path để phân biệt tên trùng.
- Cập nhật spawn/destroy/reparent/rename/scene unload mà không mất selection/scroll không cần thiết. Không dùng tên/path làm identity.
- Inspector header: tên, scene, path, instance ID trong session, tag/layer read-only; activeSelf có thể sửa, activeInHierarchy read-only.
- Transform: local position/rotation Euler/scale có thể sửa; world position read-only. RectTransform có drawer riêng cho anchoredPosition, sizeDelta, anchors, pivot; cảnh báo giá trị có thể bị LayoutGroup/Animator ghi đè.
- Component foldout, trạng thái enabled nếu component hỗ trợ, missing script hiển thị rõ, filter theo tên component/member.
- Field public và `[SerializeField]` thuộc kiểu được hỗ trợ; chính sách chi tiết ở mục 6. Properties tắt mặc định, chỉ dùng adapter/allowlist rõ ràng.
- Drawer bool, numeric chuẩn, string, enum/flags, Vector2/3/4, Vector2Int/Vector3Int, Color, UnityEngine.Object read-only + inspect/reveal reference. Không hứa object picker trong P0.
- Array một chiều/List<T> có paging và sửa phần tử kiểu được hỗ trợ; kích thước read-only. Serializable class/struct có giới hạn độ sâu và phát hiện cycle.
- Chỉnh sửa chỉ trong session đã unlock và mode cho phép. Nút Refresh/Pause view, Read only/Edit rõ ràng. Không ghi các giá trị này vào asset/save của game.
- Prefab/font/icon/catalog hoàn chỉnh, QuickStart demo, tài liệu giới hạn, test player IL2CPP.

### P1 — sau khi P0 đạt gate

Picker chọn object trong scene (loại trừ UI debugger), reference picker có type filter, pin/watch một số member có giới hạn, custom drawer API public ổn định, copy snapshot có lọc dữ liệu, Undo phiên runtime theo tập member đã kiểm thử. Properties nâng cao và adapters Renderer/Camera/Light chỉ sau khi kiểm chứng side effect.

### P2 — chỉ làm khi có nhu cầu xác nhận

Multi-select, add/remove component, delete/duplicate object, drag reparent, invoke method có whitelist, material editor, lưu/replay snapshot, remote inspector. Những phần này tăng mạnh rủi ro và phạm vi support; không nằm trong quảng cáo P0.

Không hứa: thay thế Unity Editor, SerializedObject/PropertyDrawer/CustomEditor chạy trong player, ECS hierarchy, inspection mọi managed object, debug từ máy khác hoặc đồng bộ network authority.

## 5. Kiến trúc và điểm tích hợp

### Owner và chiều phụ thuộc

Đề xuất đặt implementation Unity-facing tại `Runtime/Engine/SceneInspection/`, trong Engine assembly hiện có. Tránh thêm assembly cho đến khi có nhu cầu tách thực tế; feature flag và lifecycle vẫn do root hiện tại quản lý.

```text
DebuggerRuntime + DebuggerConfiguration + access policy
  -> SceneInspectionSession
       -> HierarchyIndex / SelectionState
       -> MemberDescriptorCache / InspectorBinding
       -> InspectionWritePolicy
  -> SceneTabController -> HierarchyView / InspectorView -> saved prefabs

Engine -> Presentation -> Foundation
Engine -> Runtime (pure .NET contracts hiện có)
Editor -> Engine/Presentation (authoring + catalog + tests)
```

Tên trên là thiết kế đề xuất. Leaf views nhận session/binding tường minh; không gọi service locator ở mỗi row. Không có dependency sang project `Assets/`, Dreamy Core/UI hoặc upstream plugin để dùng P0.

### Hợp đồng dữ liệu và lifecycle

- Node identity dùng instance ID kèm kiểm tra Unity object còn sống và generation của session/index. Tránh ID reuse sau destroy; scene identity dùng handle trong session. Path chỉ phục vụ hiển thị/copy, có thể đổi sau reparent.
- Cache descriptor theo Type; không giữ strong reference mọi object trong static cache. Selection và bindings được clear khi unload/destroy/stop. Pool có capacity và clear bindings/listeners khi trả row.
- Scene index lấy roots theo scene, duyệt theo budget; sceneLoaded/sceneUnloaded đánh dấu dirty. Thay đổi bên trong scene phát hiện qua bounded reconciliation vì không có một event public tổng quát cho mọi thay đổi GameObject.
- DontDestroyOnLoad: spike kiểm chứng `gameObject.scene` của một helper do debugger sở hữu sau DontDestroyOnLoad và lấy roots từ scene đó bằng API public; helper có HideInHierarchy và bị filter khỏi kết quả. Không reflection UnityEditor/internal API; nếu spike không đạt player gate thì không đánh dấu P0 hoàn tất.
- Filtering loại debugger bootstrap/presentation/HUD/overlay theo root owner/reference, không chỉ theo tên. Có tùy chọn nội bộ xem debugger roots; user game không bị ẩn nhầm vì đặt tên trùng.
- `OnEnable`: bind owner, reconcile dirty data. `OnDisable`/đổi tab/đóng panel: dừng refresh, cancel scan, hủy draft chưa commit, bỏ input focus. Giữ selection/expanded IDs còn hợp lệ trong RAM để reopen.
- Access hết hạn/lock/mode đổi: hủy draft, đóng quyền edit ngay; mọi write kiểm tra lại policy. Teardown không tạo service mới. Reset static state cho Enter Play Mode không Domain Reload.
- Search chạy incremental với generation token; query mới hủy kết quả query cũ. Hiển thị “Scanning…” và số lượng kết quả tạm thời; không giả vờ đã tìm hết scene nếu scan chưa xong.

### Tích hợp navigation/configuration

1. Đề xuất thêm `DefaultTabs.Scene = 6`, giữ nguyên 0–5 và key `Scene`; đối chiếu Enum.Parse/OpenTab/disabled tabs/default tab/persistence. Getter dùng TryParse để custom key không làm crash nếu cần mở rộng.
2. Thêm `SceneTab.prefab` vào catalog và builder theo tiền lệ Screenshot. Test catalog cũ thiếu entry phải fallback/hide tab có kiểm soát, không NullReference. Không thêm cả catalog và AddTab thủ công khiến tab nhân đôi.
3. Cấu hình đề xuất: EnableSceneInspection=true cho Development/Qa; AllowSceneEditing=false mặc định; hierarchy/member refresh interval; scan budget; depth/page/cache limits. Inspector bắt đầu Read only; chuyển Edit chỉ khi cấu hình cho phép và session unlock.
4. `Off`, `ReleaseDiagnostics` và build không được policy cho phép: không tạo scene index/tab/listener. `DISABLE_DREAMY_DEBUGGER` phải compile sạch; audit thêm dependency/resource để không quảng cáo “zero bytes” chỉ dựa vào define.
5. Có thể nhớ split ratio/filter/layout trong preference namespace của debugger; không nhớ credential/draft/object ID qua app restart. Giá trị field đã edit không được serialize tự động.

## 6. Inspector: quy tắc phản chiếu và ghi giá trị

| Chủ đề | Contract P0 |
| --- | --- |
| Field eligibility | Instance fields public hoặc private/protected có `[SerializeField]`, kiểu hỗ trợ; loại static, const, `[NonSerialized]`, `[HideInInspector]`, obsolete. readonly chỉ xem. Kế thừa được duyệt có kiểm soát, tránh member trùng tên |
| Unity components | Transform/RectTransform/Behaviour dùng adapter tường minh. Không reflect bừa toàn bộ properties của Renderer/Material; tránh getter tạo instance hoặc gọi API gây side effect |
| Properties | Chỉ opt-in/allowlist, không indexer, có getter phù hợp; setter cần policy riêng. Getter ném lỗi chỉ đánh dấu member, không phá cả inspector; ngắt auto retry gây log spam |
| Custom struct | Write-back từ leaf qua từng struct cha đến field owner; list/array element struct phải ghi trả đúng index. Test nested struct là release gate |
| Reference và cycle | Unity fake-null/destroyed, null managed value, cycle và max depth có trạng thái riêng. Không tự tạo object mới khi gặp null |
| Unsupported | Dictionary, multidimensional arrays, SerializeReference polymorphic editing, delegate/event, generic open type, pointer: nhãn Unsupported/read-only, không crash hoặc biến mất im lặng |
| Validation | Parse invariant cho số, không mất precision long/ulong/double/decimal do ép qua float. Không nhận overflow/NaN/Infinity. Enum có giá trị lạ vẫn render; flags giữ bits hợp lệ theo policy |
| Commit | Numeric/string/vector có draft, Enter/Apply mới write; Escape/Cancel bỏ draft; blur không tự ghi. Mobile có nút Apply rõ ràng. Toggle/enum ghi một lần từ thao tác đã xác nhận khi đang Edit |
| Concurrent changes | Trong lúc focus không ghi đè draft. Trước commit đọc lại giá trị; nếu khác baseline thì báo conflict và yêu cầu Reload hoặc Apply lại có chủ đích |
| Failure | Setter throw hoặc game từ chối: hiển thị lỗi và đọc lại giá trị thật. Không hứa rollback side effect tùy ý; không dùng UnityEditor.Undo trong runtime |
| Session effect | Đây là sửa trạng thái đang chạy. Lifecycle OnEnable/OnDisable hoặc setter của game có thể tự lưu/đồng bộ dữ liệu; UI nhắc rõ khi bật Edit. Debugger không thể bảo đảm game không có side effect |

Giới hạn mặc định đề xuất: depth 4, collection page 32, string preview 256 ký tự, string edit tối đa 4096 ký tự có thông báo; không dùng giới hạn kỹ thuật làm khóa upsell. Có cấu hình phù hợp và trạng thái “truncated/next page” rõ ràng.

**IL2CPP/AOT:** không Reflection.Emit hay runtime expression compilation làm điều kiện hoạt động. Cache metadata; built-in drawers/adapters có code tĩnh. Với type của khách chỉ được dùng qua reflection, cung cấp mẫu preserve cụ thể và công cụ Editor opt-in tạo file trong project của khách; không preserve mọi assembly mặc định. Unity mô tả `[Preserve]` và `link.xml` cho code bị stripping; `link.xml` của consumer nên nằm dưới Assets, không coi file trong package là đủ ([Unity manual](https://docs.unity.com/en-us/engine/6000.0/manual/programming-environment/script-compilation/managed-code-stripping/preserving)). Type/member thiếu metadata hiển thị Unsupported/unavailable với hướng dẫn, không tuyên bố inspection toàn bộ IL2CPP tự động.

## 7. UI/UX — Scene workspace đồng bộ Material Compact

### Layout desktop/landscape

```text
+-----------------------------------------------------------------------+
| Dreamy Debugger                 Scene                       [Close]   |
+------------+----------------------------------------------------------+
| Console    | [Search objects...] [All scenes v] [Inactive] [Refresh]   |
| Cheats     +-----------------------+----------------------------------+
| Performance| HIERARCHY             | INSPECTOR                        |
| Screenshot | v Gameplay            | Player       [Read only / Edit]  |
| > Scene    |   v Characters        | Gameplay / Characters / Player   |
| System     |     > Player  SELECTED| Active [on]  In hierarchy: Yes   |
|            |     > Enemy           +----------------------------------+
|            |   > UI                | v Transform                      |
|            | > DontDestroyOnLoad   | Local position  X 0  Y 1  Z 0    |
|            |                       | Rotation        X 0  Y 0  Z 0    |
|            |                       | Scale           X 1  Y 1  Z 1    |
|            |                       | v PlayerController [enabled]     |
|            |                       | Speed                    [ 5.0 ] |
+------------+-----------------------+----------------------------------+
| Live view | 1 selected | 128 objects | [Pause view] | [Copy path]      |
+-----------------------------------------------------------------------+
```

Đây là wireframe cấu trúc, không phải screenshot Unity hoặc bằng chứng UI đã hoàn thành. Tái sử dụng shell/navigation hiện có; tên tab cũ trong wireframe chỉ mô tả vai trò.

- Breakpoint theo **content width sau rail và safe area**: từ 900 logical units dùng split, 720–899 cho phép split nếu hai pane đạt minimum 280/360; nhỏ hơn chuyển stacked navigation. Split mặc định 38/62, kéo được và clamp minimum.
- Mobile portrait: toolbar gọn, Hierarchy toàn chiều rộng; chọn node mở Inspector với Back giữ scroll/query/expanded state. Breadcrumb tối đa 2 dòng, có Copy full path. Không ép hai cột chữ nhỏ trên màn 390px.
- Status Pause view chỉ dừng refresh debugger, **không đổi Time.timeScale**. Khi pause vẫn kiểm tra target còn sống trước thao tác; stale value có nhãn.

### Token và asset

| Token | Giá trị / sử dụng |
| --- | --- |
| Background / Surface / Control | `#101216` / `#1C1F25` / `#272C34` theo DebuggerPalette hiện tại |
| Border / Text / Muted | `#363E49` / `#EEF0F4` / `#99A3B2` |
| Accent / Selected / Warning | `#CB465D` / `#39212C` / `#E7B65A`; selection còn có indicator, lỗi có text/icon |
| Spacing / Radius | 4–8–12–16 logical units / radius 4–6 |
| Row | Desktop 30–32; touch 44–48. Tree indent 16, vùng chevron có hit surface riêng |
| Typography | Barlow Regular/SemiBold cho labels, JetBrains Mono cho numeric/path; body 14–16, caption không dưới 12 ở scale chuẩn |
| Icons | Cùng hệ Material hiện tại: tree, search, tune, refresh, chevron, lock, copy, back; icon vuông, preserveAspect; thêm notice nếu nguồn mới |

Không dùng nền đỏ toàn pane, blur hoặc font mới làm dependency. Flat background phủ đủ rect; sliced sprite chỉ cho control/card. Không auto-size đến mức chữ khó đọc.

### Components/prefab đề xuất

`Runtime/Prefabs/SceneInspection/`: `SceneTab.prefab`, `HierarchyRow.prefab`, `ComponentHeader.prefab`, `InspectorValueRow.prefab`, numeric/vector/enum/color drawer prefabs. Drawer động instantiate từ template và pool; layout lớn author sẵn trong prefab, không dựng toàn UI bằng tọa độ trong Update.

Tree flatten chỉ những branch mở; virtualization theo row height cố định. Inspector dùng flattened rows có row kind/height rõ ràng, prefix offsets cho viewport, expanded complex section rebuild model khi cần. Tránh lồng ContentSizeFitter/LayoutGroup sâu trên hàng nghìn entry. Pool tách theo drawer type và reset state trước bind lại.

### Interaction và trạng thái bắt buộc

- Chevron chỉ expand, row chọn; click active toggle không đồng thời chọn/expand sai. Desktop có hover/focus, arrow keys; Ctrl+F chỉ bắt khi Scene workspace có focus. Escape đóng editor/popup rồi Back trước khi đóng debugger.
- ActiveSelf và Behaviour.enabled là write action: validate access ngay lúc commit. Vô hiệu hóa parent có thể làm nhiều child inactive; UI vẫn phân biệt checkbox activeSelf với badge inactiveInHierarchy.
- Empty selection, scene rỗng, no results, scanning, scene unloaded, target destroyed, missing script, member error, read-only và access expired đều có text ngắn + hành động phù hợp.
- Form không mất caret khi refresh/pool; popup enum/color ở top overlay của debugger và có Cancel. Không dùng hover làm cách duy nhất đọc giá trị dài trên touch.
- IME/soft keyboard: field đang focus luôn nhìn thấy trong safe viewport; Apply/Cancel không bị bàn phím che. Rotation giữ selection và draft hợp lệ hoặc cancel có thông báo.
- Decorative text/icon tắt raycast; nút có Image hit surface riêng. Respect EventSystem hiện có, không tạo EventSystem trùng. Chỉ block input trong panel/modal; tích hợp cơ chế input gating hiện tại, không tự disable action map của game rồi quên restore.
- UI test bằng pointer/raycast/touch thực; `onClick.Invoke()` đơn thuần không chứng minh nút bấm được.
- Chụp ảnh review ở 1920×1080, 1280×720, portrait 390×844 và tablet 1024×768 với tên dài, Unicode, inactive branches, member errors. Dùng Game View thật sau prefab authoring; screenshot concept không phải release evidence.

## 8. Hiệu năng và nền tảng

Các con số dưới đây là **mục tiêu khởi đầu cần đo**, chưa phải claim marketing. Chốt model thiết bị Android đại diện tại Phase 0 và ghi OS/backend/thermal state cùng kết quả.

- Refresh hierarchy/member mặc định 0.25 giây, name/search reconciliation chậm hơn khi cần; chạy theo unscaled time. Không quét toàn scene mỗi frame. Tắt tab/lock không còn scan/member polling.
- Chia scan theo frame, budget đề xuất 1 ms desktop/2 ms mobile; nếu không đủ thì tăng latency có hiển thị, không block một frame để “đúng refresh interval”. Startup chỉ tạo index khi mở Scene lần đầu.
- Fixture: 1k và 10k objects, sâu 20 tầng, nhiều tên trùng, 100 spawn/destroy mỗi giây; selected component 100 members, collection 1k entries. Không tạo 10k GameObject row UI.
- Mục tiêu sau warm-up: selection cập nhật dưới 100 ms khi index sẵn; search 10k trả kết quả cuối dưới 500 ms trên thiết bị đã chốt; steady open CPU p95 phần feature dưới 2 ms/frame trên Android đại diện. Nếu không đạt thì tối ưu/điều chỉnh phạm vi và công bố latency thực.
- Không hứa zero-GC với reflection value types. Đo allocations/frame và bytes/refresh riêng; mục tiêu ban đầu trung bình dưới 1 KB/frame cho fixture inspector 50 visible scalar members, không full-panel rebuild mỗi refresh. Báo cả spike thay selection/search, không che bằng trung bình.
- Loop mở/đóng 100 lần và load/unload 20 lần: object/listener/pool không tăng vô hạn; capture heap sau warm-up và sau cleanup. Pool capacity cấu hình; không cache scene object qua session.
- Windows player và Android ARM64 IL2CPP là gate P0. iOS IL2CPP là gate trước khi quảng bá iOS; chưa có Mac/device thì giữ unsupported/unverified rõ ràng. WebGL, consoles, XR không nằm trong support ban đầu.
- Kiểm tra Built-in/URP cho overlay và demo trong fresh consumer; không kéo URP vào debugger để vẽ UI. HDRP chỉ ghi support sau smoke test. Unity 2022.3 không mặc định hỗ trợ vì package hiện khai báo minimum 6000.0.

## 9. Kế hoạch thực hiện và điều kiện qua từng phase

Ước lượng cho một lập trình viên Unity có kinh nghiệm, QA/UI tham gia khi cần: **22–34 ngày công**, không tính thay thế phần source thiếu quyền phân phối, thời gian chờ Asset Store hoặc thiết bị chưa có. Đây là ước lượng lập kế hoạch, cập nhật sau spike.

| Phase | Công việc / file chính | Kết quả và gate | Ngày công |
| --- | --- | --- | --- |
| 0. Baseline và spike | Inventory license; đọc startup/input/catalog/tests; spike DDOL, IL2CPP field reflection, struct write-back; xác định device/support matrix | Quyết định giữ/thay shell, dataset benchmark, proof-of-concept hoạt động trong player; danh sách blocker rõ ràng | 2–3 |
| 1. Domain/session | Thêm Engine/SceneInspection session, selection, index, lifecycle, access/write policy, config | Test scene/object identity, lock và teardown; không tạo phụ thuộc vòng hoặc scan khi đóng | 3–4 |
| 2. Hierarchy | Incremental index/search/filter, flattened tree, recycling rows, scene headers/DDOL | 10k fixture scroll/search được; spawn/destroy/reparent/unload không stale selection | 3–4 |
| 3. Inspector | Descriptor cache, adapters, scalar/enum/vector/color/reference, nested struct, paged collection, draft/error handling | Test getter/setter lỗi, precision, cycle, struct write-back, focus conflict; IL2CPP smoke chạy lại | 5–7 |
| 4. UI integration | SceneTab/controller/prefabs, catalog, enum, responsive split/mobile, keyboard/touch, visual polish | Screenshot Unity thật ở 4 viewport; pointer/raycast và IME pass; cũ vẫn hoạt động | 4–6 |
| 5. Consumer và performance | Fresh project, packaged artifact, input matrix, Mono/IL2CPP, device profiler, regressions | Source và artifact cùng fingerprint; build/runtime/CPU/GC evidence, không claim platform chưa test | 3–6 |
| 6. Docs và release candidate | QuickStart, extension/stripping guide, limitations, notices, changelog, Asset Store material | Release checklist đạt, clean import, không thiếu dependencies; commercial license gate đóng | 2–4 |

Checkpoint sau mỗi phase: commit SHA source, phạm vi đã xong/còn thiếu, test XML/log/screenshot/build hashes và blocker trong `docs/evidence/debugger/runtime-inspection/`. Không dùng receipt từ feature UI cũ để đóng gate mới. Khi tiếp tục ở turn sau, đọc checkpoint rồi kiểm tra lại baseline thay vì làm lại từ đầu.

## 10. File map triển khai dự kiến

**Package submodule sở hữu reusable implementation:**

- Mới: `Runtime/Engine/SceneInspection/` gồm session/index/selection/descriptor/binding/write policy/controllers/views/drawers, cùng `.meta`.
- Mới: `Runtime/Prefabs/SceneInspection/` và icon chọn lọc dưới `Runtime/Branding/Material/`; meta GUID cố định.
- Sửa nhỏ: `Runtime/Engine/DebuggerRuntime.cs`, `DebuggerConfiguration.cs`, teardown/preferences nếu cần.
- Sửa nhỏ có regression: `Runtime/Imported/Presentation/Scripts/Settings.cs`, `UI/DebuggerTabController.cs`, catalog asset và skin icon entry. Không sửa hàng loạt Imported để tiện feature.
- Mới: `Editor/SceneInspectionAssets.cs` theo tiền lệ `ScreenshotTabAssets.cs`; sửa `Editor/DebuggerAssetBuilder.cs` đúng catalog registration. Tool chỉ chạy explicit trong maintainer checkout, không tự regenerate/write PackageCache khi khách import.
- Tests: test classes mới đúng các thư mục/asmdef hiện có; không để Runtime assembly chứa UnityEditor reference.
- Docs/sample: `Documentation~/RUNTIME_INSPECTION.md`, sample scene/component trong `Samples~/QuickStart`, cập nhật README/CHANGELOG/release/third-party notices; nếu thêm sample riêng thì khai báo package manifest.

**Sandbox sở hữu demo integration/evidence/plan**, không chứa implementation debugger mà khách phải copy để package hoạt động. Thay đổi package cần commit trong repo submodule, sau đó cập nhật gitlink ở sandbox; không coi commit sandbox là đã publish package.

## 11. Kiểm thử và acceptance

| Tầng | Các tình huống cần chứng minh |
| --- | --- |
| EditMode/model | Flatten/search ancestor context; identity/generation; filtering; eligibility kế thừa; invariant parse/overflow; long precision; enum flags; cache limits; nested struct và list element write-back |
| PlayMode/runtime | Multi-scene, DDOL, inactive hierarchy, duplicate names, spawn/destroy/reparent/rename; selected object/component bị destroy; scene unload khi draft mở; pause với timeScale=0; access expiry giữa edit và commit; mode Off/ReleaseDiagnostics |
| PlayMode/UI | Recycling không giữ stale listener; refresh không phát write event; caret/draft không bị reset; missing script/unsupported/cycle/errors; back stack; split clamps; no duplicate tab |
| EditorIntegration/startup | Catalog mới/cũ, GUID/bindings, automatic/manual bootstrap; enable/disable/restart, Domain Reload off; no auto scene mutations; define disable compile; missing optional input assembly |
| Input/device | Legacy Input, Input System và Both trong consumer riêng; desktop pointer + keyboard, touch scroll/tap, safe area, rotation, soft keyboard; không chạm xuyên panel hoặc khóa gameplay sau close |
| Build/AOT | Fresh Unity 6000.0 supported baseline và sandbox 6000.4; Windows player; Android ARM64 IL2CPP mức stripping hỗ trợ kể cả High nếu listing hứa; validate values và setters trong player, không chỉ build thành công |
| Regression | Cheats, console/dock, PIN/session timeout, HUD, Screenshot hide/restore/capture, report local queue; screenshot mode không để Scene input blocker treo và không tự attach dữ liệu inspector vào report |
| Artifact | UPM/tarball và Asset Store import thực tế có đủ prefab/font/icon/notice/sample; không dependency file:../LocalPackages, không cần sandbox source hoặc upstream cài sẵn |

Release acceptance P0: người dùng cài package vào project sạch, import sample, mở Scene, tìm inactive object, chọn nó, xem components, bật Edit bằng config/session, sửa supported value và thấy game nhận thay đổi; unload scene rồi reopen không lỗi, đóng tab không scan tiếp. Thực hiện flow trên player IL2CPP và quay/chụp evidence từ artifact cuối.

Các lệnh có sẵn để dùng đúng mục đích:

```powershell
# Xác nhận phạm vi thay đổi, gồm submodule.
git diff --check
git -C LocalPackages/com.dreamy.debugger diff --check

# Sau khi đã đóng gói candidate .tgz thật; script cần đường dẫn archive.
python LocalPackages/com.dreamy.debugger/Tools~/verify_package.py <candidate.tgz>

# Chỉ theo dõi một test job Unity MCP đã khởi chạy; không tự tạo test job.
node tools/unity-test-watch.mjs <existing-job-id> --target=<verified-unity-instance>
```

Mẫu batchmode khi chạy trên consumer kiểm thử riêng, thay đường dẫn thực tế và không chạy project đang có Editor lock:

```powershell
& '<Unity.exe>' -batchmode -projectPath '<isolated-consumer>' -runTests -testPlatform EditMode -testResults '<editmode.xml>' -logFile '<editmode.log>'
& '<Unity.exe>' -batchmode -projectPath '<isolated-consumer>' -runTests -testPlatform PlayMode -testResults '<playmode.xml>' -logFile '<playmode.log>'
```

Exit code, XML results và runtime evidence đều phải kiểm tra; lệnh mẫu không phải kết quả đã chạy. Hôm nay chỉ tạo plan: không cần build/test runtime chưa thay đổi.

## 12. Asset Store và định nghĩa hoàn tất

Đối chiếu lại [Submission Guidelines chính thức](https://assetstore.unity.com/publishing/submission-guidelines) tại ngày nộp. Bản đọc khi lập plan yêu cầu mô tả chính xác dependency/giới hạn, tài liệu cho code/configuration, tổ chức package và API phù hợp; không tự suy ra được chấp thuận submission.

- [ ] Inventory quyền phân phối hoàn tất, đặc biệt Imported Presentation; root license/EULA/third-party notices nhất quán. Có bằng chứng thay thế hoặc quyền phù hợp cho phần không được phân phối.
- [ ] Sản phẩm hoạt động độc lập trong project sạch; không yêu cầu Dreamy sandbox hoặc tài khoản/relay để dùng Hierarchy/Inspector.
- [ ] Offline docs gồm setup, scope supported members, chỉnh giá trị có side effect, custom types/stripping, input, performance limits, cách tắt feature cho release.
- [ ] Asset Store export một root folder phù hợp, namespace riêng, meta/reference bền vững, không duplicate vendor assets; thử import artifact này riêng với UPM.
- [ ] Chốt Unity minimum theo test thực, không hạ từ 6000.0 xuống 2022.3 chỉ để rộng thị trường. Chỉ liệt kê platform/pipeline đã qua test; kiểm tra lại quy định theo phiên bản Editor dùng nộp.
- [ ] Demo scene và marketing screenshots/video từ feature thật; nói rõ “in-player local inspection”, không gợi ý remote debugger hoặc Unity Editor parity.
- [ ] Không tự thu thập/upload object names/values/snapshot. Copy path cục bộ; report attachment nếu có ở P1 phải do người dùng chủ động và có redaction.
- [ ] Build có feature disabled được kiểm tra compile và resources; default ReleaseDiagnostics không expose editing. Access gate là cơ chế vận hành QA, không quảng cáo PIN là bảo vệ tuyệt đối trước reverse engineering.
- [ ] Candidate version đề xuất `0.2.0-preview.1` rồi `0.2.0` nếu thay đổi vẫn additive; quyết định `1.0.0` thương mại sau khi đóng toàn bộ gate, không chỉ vì thêm tab.
- [ ] Public API/migration giữ enum số cũ, GUID cũ, default config an toàn; client cũ không bật edit bất ngờ sau upgrade.
- [ ] Changelog, test/build receipts và artifact hash khớp candidate cuối; support policy và known limitations đi cùng listing.
- [ ] Khi nộp, khai báo AI assistance nếu thuộc nội dung mà guidelines hiện hành yêu cầu; review lại yêu cầu này trên portal.

**Thứ tự bắt đầu được đề xuất:** Phase 0 inventory + spike → Hierarchy/index + UI shell → Inspector read-only → edit có policy → mobile/IL2CPP/performance → artifact và release docs. Chỉ xem feature hoàn tất khi cả flow người dùng, regression và quyền phân phối đều được chứng minh; chưa đạt gate thì giữ candidate nội bộ và ghi rõ phần còn mở.
