# Dreamy Debugger — Scene Workspace V2

Ngày cập nhật: **2026-11-23**. Trạng thái: **đã triển khai lát cắt Hierarchy UI + numeric/vector quick edit + Tag/Layer trong Unity**. 18/18 targeted EditMode và 8/8 PlayMode đạt. Phạm vi hoàn tất và các phase V2 còn mở ghi trong [checkpoint](../design/debugger-scene-v2/CHECKPOINT.md).

Thiết kế lại Runtime Hierarchy + Inspector để có chất lượng hình ảnh phù hợp sản phẩm thương mại, dễ hiểu như một workspace Editor và chỉnh nhanh theo từng kiểu dữ liệu. **Hỗ trợ cả màn hình dọc, ngang, resize và xoay trong lúc sử dụng** là acceptance bắt buộc theo yêu cầu mới nhất.

Plan này thay thế thiết kế UI/lộ trình cũ. Source candidate V1 được giữ làm nền tảng, không triển khai lại toàn bộ backend từ đầu.

## 1. Baseline và vấn đề thực tế

- Sandbox `b79b485`; debugger `8593c75b4f17110fdbad09770bcd852e5b2785df`. Manifest debugger vẫn `0.1.8`, minimum Unity `6000.0`; sandbox dùng Unity `6000.4.12f1`, uGUI `2.0.0`. Không thêm TMP hay Dreamy UI dependency chỉ cho feature này.
- Đã đọc manifest/lock, asmdefs, `SceneTabController`, `HierarchyRowView`, `InspectorRowView`, `InspectionMember`, `SceneInspectionAssets`, docs và [checkpoint V1](../evidence/debugger/runtime-inspection/CHECKPOINT.md). Đã xem ảnh runtime V1 desktop 1920×1080 và portrait 390×844.
- Runtime assembly thuần .NET (`noEngineReferences: true`); Engine tham chiếu Runtime/Foundation/Presentation/uGUI; không thêm chiều Presentation → Engine. Giữ access/session/root owner, enum `DefaultTabs.Scene=6`, catalog và define disable.
- Workflow `docs/agents/dreamy-workflow.md` và compatibility catalog được nhắc trong hướng dẫn không có ở checkout này. Nhận định API dựa trên manifest/lock/source đã đọc. Lessons screenshot/input cục bộ nhắc giữ owner hide/restore và không để blocker input tồn tại sau close; receipt đó không chứng nhận Scene V2.

| Quan sát V1 | Hệ quả | Quyết định V2 |
| --- | --- | --- |
| Toolbar có ba nút lớn Refresh/Pause/Read only | Chiếm chỗ nhưng thiếu phân cấp | Toolbar gọn; icon + tooltip; Edit có trạng thái rõ |
| Search lặp path dài dưới từng object | Cây giống log, khó nhận cấp bậc | Scene header, tên một dòng; context chỉ khi cần |
| TreeHeight 52; Inspector entry 76/108 cho nhiều loại | Mật độ thấp, cuộn nhiều | Row kind/density, drawer riêng theo kiểu |
| Vector là chuỗi `0, 0, 0` | Khó sửa một trục, dễ nhập sai | Ba ô X/Y/Z, scrub, stepper |
| Type/ID/hint xuất hiện thường trực | Metadata lấn át giá trị | Tooltip/details; chỉ hiện lỗi khi có |
| Portrait nhiều header/nút trước field | Tốn chiều cao, ít dữ liệu nhìn thấy | Header gọn, navigation hai trang, editor sheet |

**Flow mục tiêu:** mở Scene → tìm object → hiểu object đang chọn → thấy Transform và field quan trọng → chỉnh riêng X mà không nhập lại Y/Z → biết draft/applied/conflict → Back về đúng vị trí cây.

## 2. Nghiên cứu UnityRuntimeInspector

Repo tham khảo đã được clone, đọc README, xem `DarkSkin.png` và đọc các source dưới đây. Snapshot HEAD ngày khảo sát: **`2d461b975725f4713aee5eb3857661e361ab8d27`**, trùng snapshot đã ghi trong plan gốc. Link được ghim SHA để không lẫn hành vi của revision khác.

| Source đã đọc | Quan sát | Áp dụng cho Dreamy |
| --- | --- | --- |
| [HierarchyField](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeHierarchy/HierarchyField.cs) | Indent theo depth; selected/inactive riêng; expand có vùng click riêng | Cây một dòng, selected/focus riêng, chevron đúng hit target |
| [Vector3Field](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Fields/Vector3Field.cs) | Ba input riêng, đổi từng axis, refresh từng ô | X/Y/Z độc lập về UX; compound member có conflict policy |
| [TransformField](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Fields/TransformField.cs) | Local Position/Rotation/Scale có nhãn dễ hiểu | Transform card ba dòng chính, advanced foldout |
| [NumberField](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Fields/NumberField.cs) | Input dùng handler theo kiểu | Giữ kiểu gốc, bổ sung numeric gesture của Dreamy |
| [NumberRangeField](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Fields/NumberRangeField.cs) | RangeAttribute chọn slider + ô số; slider đi qua float | Range mới dùng slider; số precision cao giữ canonical numeric |
| [BoundInputField](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Helpers/BoundInputField.cs) | Giữ text khi focus, cache, màu invalid | Refresh không reset caret/draft, lỗi inline |
| [ColorField](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Fields/ColorField.cs) | Swatch mở picker, Color/Color32 | Swatch/alpha/picker; bổ sung HDR policy rõ |
| [ObjectReferenceField](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Fields/ObjectReferenceField.cs) | Picker, inspect/drop reference, lookup qua Resources | P0 chip + Reveal readonly; assignment P1; tránh global scan mỗi click |
| [UISkin](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Skin/UISkin.cs) | Font, line height, indent, label ratio và màu theo skin | Theme tokens/density, cập nhật theo event/version |
| [RecycledListView](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/Scripts/RuntimeInspector/Helpers/RecycledListView/RecycledListView.cs) | Pool item theo viewport, resize invalidation | Giữ virtualization; Inspector dùng model row kind/height |

Scrub số, stepper, transaction và value history trong plan là **đề xuất bổ sung cho Dreamy**, không gán nhầm cho upstream. Một số drawer upstream ghi khi input hợp lệ; Dreamy phải giữ access/conflict policy của mình.

Chọn phát triển tiếp module Dreamy, tham khảo UX và port chọn lọc khi có ích. Không bắt khách cài cả upstream. Nếu port, ghi file gốc/SHA/diff/license; GitHub source dùng [MIT](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/LICENSE.txt), ColorWheel có [BSD 3-Clause riêng](https://github.com/yasirkula/UnityRuntimeInspector/blob/2d461b975725f4713aee5eb3857661e361ab8d27/Plugins/RuntimeInspector/ExternalResources/ColorWheel/ColorWheelLICENSE.txt). Chỉ nhập notice của phần thực sự sử dụng; không nhầm điều kiện GitHub với bản tải từ Asset Store.

## 3. Concept hình ảnh: Material Compact — cùng theme Dreamy hiện tại

Chỉ đạo mới nhất: đồng nhất theme hiện tại, đổi tên hiển thị tab thành **Hierarchy**, đổi icon tab thành cây phân cấp, ưu tiên icon object/component dễ nhận như gallery Unity Editor người dùng gửi và giảm thông tin thừa. Quyết định này thay thế hướng accent xanh lam trước đó. Dùng `DebuggerPalette` hiện tại làm nguồn token, Barlow/JetBrains Mono và Material Compact control assets hiện có. Độ hoàn thiện đến từ bố cục, mật độ và trạng thái, không thay toàn debugger sang skin mới.

### Đổi tên và icon tab

- UI title/long title/sidebar/tooltip/QuickStart hiển thị **Hierarchy** (đúng chính tả); hai pane bên trong vẫn là Hierarchy và Inspector. Thay `Icon_Options` đang dùng bằng style riêng dự kiến `Icon_Hierarchy`, glyph cây phân nhánh có node, đọc được ở cùng kích thước các tab khác.
- Giữ internal tab key `Scene`, catalog address, `DefaultTabs.Scene=6`, GUID và preference identity để consumer hiện có không mất binding. Đây là đổi nhãn hiển thị; nếu sau này đổi public identifier phải có migration riêng. Không chỉ đổi key tùy ý vì getter ActiveTab đang parse enum.
- Icon normal/selected/disabled lấy tint cùng hệ sidebar hiện tại. Không dùng icon camera/landscape cho Hierarchy hoặc thêm nền badge khác tab lân cận.

### Giảm thông tin trên màn hình chính

- Mặc định chỉ có search/filter, object name/selection, component header và value. Bỏ dòng “Runtime workspace”, nhãn SCRIPT, ID/type lặp lại, hint hướng dẫn thường trực và counter kỹ thuật trong footer.
- Breadcrumb một dòng chỉ khi hữu ích; tag/layer/instance ID/full path ở menu **Details**. Footer chỉ xuất hiện khi scan, pending edit, lỗi hoặc conflict; không chiếm chiều cao lúc idle.
- Readonly/Edit hiển thị một trạng thái rõ; hướng dẫn gesture ở tooltip/first-use help. Tooltip không thay accessibility/touch details. Lỗi vẫn hiện sát field, không giấu thông tin cần quyết định.
- Giới hạn màu: accent Dreamy cho selected/focus/edit; màu nhận biết object/component chỉ ở icon nhỏ; X/Y/Z có chữ đi kèm. Không biến từng card/field thành một màu khác nhau.

[Concept HTML tương tác](../design/debugger-scene-v2/concept.html): chọn object, search, foldout, ô X/Y/Z, ±, scrub desktop, slider và reflow hẹp. Dữ liệu giả; không phải ảnh runtime hay evidence touch. Font browser là minh họa; Unity tiếp tục Barlow + JetBrains Mono hiện có và notice OFL.

| Token | Giá trị ban đầu | Vai trò |
| --- | --- | --- |
| Canvas / Panel | `#101216` / `#1C1F25` | DebuggerPalette.Background / Surface |
| Raised / Input | `#272C34` / `#272C34` | DebuggerPalette.Control; flat/sliced theo shell |
| Border | `#363E49` | DebuggerPalette.Border |
| Text / secondary | `#EEF0F4` / `#99A3B2` | DebuggerPalette.Ivory / Muted |
| Accent / selection fill | `#CB465D` / `#39212C` | DebuggerPalette.Accent / Selected |
| Success / warning / error | `#88D6B4` / `#E8BC70` / `#F0929A` | Applied / draft / conflict |
| X / Y / Z | `#F09C9C` / `#91D4B2` / `#99BCFF` | Luôn kèm chữ axis |
| Spacing | 4, 8, 12, 16, 24 | Nhịp toàn module |
| Radius | 4 control, 6 card, 8 popup | Gọn, không biến mọi control thành pill |

Text thường 13–14 logical units desktop, 15–16 touch; title 16–18; số Mono. Mục tiêu contrast chữ thường ≥4.5:1, indicator/focus ≥3:1; cần đo trên render thật. Không coi palette dự kiến là đã kiểm chứng. Không auto-size tới mức khó đọc.

## 4. Desktop và landscape rộng

```text
 Dreamy / Hierarchy                           [Read only / Edit] [⋯]
 ┌ HIERARCHY ─────────────┬ INSPECTOR ──────────────────────────────┐
 │ Search objects…       │ ◇ Player               [✓ Active] [⋯]  │
 │ [Scene ▾] [Filter ▾]   │ Level / Characters / Player             │
 │ ▾ DemoLevel           │ Tag Player · Layer Default              │
 │   ▸ Environment       │ ▾ Transform                      Local  │
 │   ▾ Characters        │   Position [X 0.00][Y 1.20][Z 0.00]     │
 │     ◇ Player          │   Rotation [X 0.00][Y 0.00][Z 0.00]     │
 │       ◇ Visuals       │   Scale    [X 1.00][Y 1.00][Z 1.00]     │
 │     ◇ Dummy           │ ▾ Player Controller              [✓]   │
 │ ▸ DontDestroyOnLoad   │   Speed    [━━━━●────────] [5.50]       │
 │                       │   Health   [−] [100] [+]                │
 └ 124 objects · Live ───┴ Position X changed    [Reload] [Apply] ──┘
```

Hierarchy khởi đầu 28–32% width, tham khảo minimum tree 240 và Inspector 420 logical units. Splitter clamp theo rect thực, vùng hit ≥8 desktop/lớn hơn trên touch; không làm pane nhỏ dưới min để giữ split bằng mọi giá. Label column ~35–40%, có min/max; resize không làm axis biến mất.

Toolbar cao dự kiến 40 desktop/48 touch. Refresh nhỏ + tooltip; Pause ghi **Pause inspection**, không đổi `Time.timeScale`. Edit có lock/pencil + chữ. Menu chứa density, refresh interval, details. Live nghĩa đang quan sát, khác Live edit. Scan/status không làm toolbar nhảy chiều cao.

## 5. Portrait, landscape thấp và responsive

**Hai yếu tố độc lập:** orientation và vùng nội dung thực. Portrait tablet có thể đủ rộng để split; landscape phone có thể thiếu chiều cao hoặc width sau safe area/sidebar nên phải dùng compact. Không quyết định chỉ bằng `Screen.width > Screen.height`.

| Điều kiện vùng khả dụng sau shell/SafeArea | Layout | Quy tắc |
| --- | --- | --- |
| Rộng ≥~760 và đủ minimum hai pane | Split | Tree + Inspector đồng thời; breakpoint là số khởi đầu cần đo |
| Hẹp hơn hoặc không đủ min pane | Two-page | Hierarchy / Inspector; Back giữ state |
| Landscape thấp, keyboard mở hoặc popup lớn | Compact + editor sheet | Thu metadata/header, editor có scroll riêng, action luôn tới được |
| Portrait rộng | Split nếu đạt min; cho chọn one-pane | Không cố ép theo orientation name |

Các ngưỡng là logical Canvas units, cần kiểm tra live CanvasScaler/render mode/reference resolution/match/parent rect trước authoring. Một độ phân giải 1920 không có nghĩa workspace rộng 1920 units.

### Portrait phone

- Header gọn, navigation Hierarchy/Inspector. Chọn object mở Inspector; chưa chọn có empty state “Choose object”. Back trả đúng search/filter/scroll/expanded state.
- Path dài ở details mở khi tap; ID/tag/layer không chiếm thường trực nhiều hàng. Trên 390×844, mục tiêu thấy Transform và ít nhất một component header tiếp theo khi keyboard đóng, kiểm chứng bằng fixture đúng scaler.
- Vector label trên, ba ô X/Y/Z dưới nếu đủ min width; nếu không, xếp mỗi axis một dòng. Không cắt số âm/lớn chỉ để giữ ba cột.
- Touch hit target mục tiêu ≥44 logical units và kiểm tra sau scaling trên thiết bị. Không nhét sáu nút ± vào một vector row nhỏ; tap field mở numeric sheet có stepper đầy đủ.

### Landscape phone

- Toolbar một hàng gọn; metadata thu vào details; ưu tiên chiều cao danh sách. Split chỉ nếu hai pane thực sự đọc và bấm được.
- Numeric sheet dùng side sheet hoặc dialog giới hạn width và scroll nội dung; không lấy gần hết chiều cao bằng một bottom sheet rồi che Apply.
- Notch bên trái/phải: nền full bleed, vùng điều khiển giữ trong SafeArea owner hiện tại. Test cả hai hướng landscape nếu project cho phép.

### Rotation / resize khi đang dùng

1. Lưu state độc lập với view: selected object identity, expanded branches, query/filter, anchor object + scroll offset của mỗi pane, component foldout, draft và active member.
2. Khi dimensions/insets/keyboard thay đổi, chọn layout mới qua event và rebuild geometry có kiểm soát; không polling full layout mỗi frame.
3. Giữ selection/draft và focus nếu field vẫn hợp lệ; kết thúc pointer capture/repeat đang chạy trước reflow. Không commit ngầm vì input bị recreate.
4. Giữ popup trong safe rect mới; nếu phải đổi popup thành sheet, dùng cùng edit model. Nếu target biến mất, cancel có thông báo. Không tạo sheet thứ hai.
5. Keyboard/IME: field đang focus nhìn thấy, Apply/Cancel tới được; text composition không bị refresh cắt ngang. Escape/Back đóng tầng editor → popup → compact Inspector → debugger.
6. Font-size/user scaling: reflow hoặc scroll, không giảm font để fit. Scroll luôn tới item cuối, có clearance cho footer/sheet.

Chỉ một SafeArea owner; background bên ngoài inset subtree. Không thêm inset lần hai vào tab nếu shell đã xử lý. World-space mode phải có layout contract/available rect riêng; không tự lấy screen safe area áp lên world-space canvas.

## 6. Hierarchy UX

1. Scene header có icon/count; DontDestroyOnLoad riêng. Cây thường tên một dòng, icon, chevron; path ở breadcrumb/details. Search chỉ thêm path khi cần phân biệt kết quả trùng.
2. Indent 16 desktop/18 touch; guide line tùy chọn. Lá không có chevron nhưng giữ cột thẳng. Deep tree dùng horizontal scroll/reveal path, không indent tới mức tên chỉ còn vài ký tự.
3. Tap row chọn; chevron chỉ expand. Desktop Up/Down chọn, Right expand/child, Left collapse/parent, Home/End. Ctrl/Cmd+F chỉ khi workspace focus, không giành phím của input/IME/game.
4. Selection có fill + vạch trái; focus có outline khác; inactive text dịu và badge, selected inactive vẫn đọc được. Không dùng eye icon khiến nhầm visibility với `activeSelf`.
5. Menu chuột phải/long press: Copy name/path, Reveal selected, Expand/Collapse branch. Touch không phụ thuộc hover. Search có clear/count/highlight, cancel query cũ, ancestor context. Filter chọn rõ scene/active thay cycle khó đoán.
6. Giữ identity theo object/session khi spawn/destroy/reparent/rename; không dùng path/name làm key. Giữ scroll anchor; target destroyed/unloaded không tự chọn object mới cùng row index.
7. Row desktop 28–32, touch 44–48 units; scene/search row height riêng. Virtualization biết row kind/height, pool bounded. Không dựng 10k UI objects để có tree đẹp.

P0 single selection. Multi-object edit/reparent/delete/add/remove component là P2 có policy riêng; không thêm nút giả khiến hiểu đã hỗ trợ.

## 7. Inspector và typed drawers

Header object: icon, tên, activeSelf toggle, breadcrumb; activeInHierarchy tách badge khi parent inactive. Theo chỉ đạo triển khai mới, **Tag/Layer được sửa trong Edit mode**: Tag nhập tên đã tồn tại, Layer dropdown các layer có tên; mặc định chỉ áp dụng object đang chọn, không thay children. Các write đều kiểm tra access/conflict. Component card có chevron, type icon, tên và Behaviour.enabled checkbox khi supported; toggle không collapse. Raw type/ID ở details.

| Dữ liệu | Drawer P0 | Thao tác |
| --- | --- | --- |
| float/double/decimal | Numeric | Scrub, ±, nhập chính xác; giữ kiểu gốc |
| Integer gồm long/ulong | Integer | Step nguyên, checked overflow; không qua float/double để lưu integer |
| RangeAttribute/adapter range | Slider + số | Range rõ; numeric canonical khi slider không đủ precision |
| Vector2/3/4, VectorInt | Ô theo axis | X/Y/Z/W, màu + chữ, copy/paste có kiểm tra |
| Transform | Position/Rotation/Scale | Local mặc định; World theo allowlist adapter |
| RectTransform | UI transform card | Anchors/pivot/anchoredPosition/sizeDelta/offset đúng mode, driven field rõ |
| bool | Checkbox | Discrete commit qua policy |
| enum | Dropdown | Search khi dài; unknown value vẫn hiển thị |
| flags | Checklist popup | Giữ unknown bits, underlying signed/unsigned đúng |
| Color/Color32 | Swatch + checkerboard | RGBA/hex/HSV/alpha, HDR policy |
| string | Input, mở multiline khi cần | Copy đầy đủ; không parse rich text từ value |
| Unity Object | Chip tên/type + Reveal | Null/Destroyed rõ; assignment P1 |
| Class/struct | Foldout | Depth/cycle guard, write-back đúng |
| Array/List | Count + trang + typed element | P0 sửa phần tử; resize/reorder P1 |
| Unsupported/missing script | Info row | Lý do ngắn, không giả editable |

Label thân thiện “Move Speed”; raw member path trong details/tooltip. Parse metadata `[Range]`, `[Tooltip]`, `[Header]`, `[Space]` vào descriptor, precedence không vượt hide/readonly/access. Không gọi arbitrary property/method của Unity component: giữ eligible fields và allowlist adapter.

Height theo drawer/model; error hiện inline khi cần; offsets cập nhật khi foldout/error/reflow. Không hardcode 108 cho mọi value, không nested ContentSizeFitter sâu.

## 8. Numeric và Transform: chỉnh từng ô

### Scalar

- Click số nhập chính xác. Scrub trên label/grip riêng, cursor rõ; drag ngang có delta. Shift Fine ×0.1, Ctrl/Cmd Coarse ×10 khi không xung đột OS/input. Giá trị tính từ baseline + tổng drag, tránh cộng sai số mỗi frame.
- Step integer mặc định 1, floating 0.1, cấu hình theo member. Nhấn giữ delay ~350 ms rồi repeat giới hạn; pointer cancel/lock/hide dừng ngay. Một click không sinh hai write.
- Touch numeric sheet gồm object/member path, số chính xác, −/+, step 0.01/0.1/1/10, Fine/Normal/Coarse và Apply/Cancel. Vertical drag trong list ưu tiên scroll; không nhận nhầm scroll thành scrub.
- Wheel mặc định scroll; chỉ đổi số khi field active và modifier được chỉ dẫn. Không đổi game chỉ vì rê chuột qua field.
- Slider chỉ khi có range. Với double/decimal và số nguyên lớn, không round-trip canonical qua slider float; tắt slider hoặc dùng normalized projection có giới hạn rõ. Không tự đoán mọi float nên có min/max.
- Giữ incomplete draft `-`, `1e`; không reset 0. Reject overflow, âm unsigned, NaN/Infinity mặc định. Cho số mũ float/double; locale decimal policy công bố rõ, internal invariant. Display rút gọn khác giá trị lưu, full precision ở editor/details.

### Vector / Transform

- X/Y/Z có input riêng và Tab order; sửa X không yêu cầu gõ lại vector. Mỗi axis có gesture/stepper; commit model vẫn biết đây là composite member.
- Baseline toàn vector: gameplay đổi Y trong lúc user sửa X → conflict mặc định, không im lặng ghi đè Y/Z. Merge theo axis chỉ là tùy chọn adapter sau kiểm chứng riêng.
- Euler rotation hiển thị độ, giữ display draft để tránh jump 0/360 khi scrub. Quaternion không đưa bốn số vào giao diện phổ thông; advanced nếu có phải validate/normalize và mô tả representation.
- Uniform scale link mặc định off; nhân theo ratio, xử lý gốc 0 bằng fallback rõ. Không tự link vì ba trục tình cờ bằng nhau.
- Local/World đổi label lẫn adapter. Test parent rotated/nonuniform scale. World Scale readonly P0; không hứa ghi `lossyScale`. RectTransform driven field có lý do/cảnh báo, không giả là field tự do.
- Quick actions “Position Zero”, “Rotation Identity”, “Scale One” trong menu có nghĩa runtime cụ thể. Khác Reload current; không gọi là reset về prefab/default khi không có nguồn authoring đáng tin.

## 9. Commit, Live preview và value history

Tái sử dụng/extend `InspectionMember.TryRead/TryCommit`, nested struct/list write-back và policy. View không ghi trực tiếp gameplay fields. Thực hiện transaction foundation trước khi nối gesture write.

**Default Commit:** text tạo draft, Enter/Apply ghi, blur không ghi; Escape/Cancel bỏ draft. Scrub/slider preview UI rồi commit một lần khi thả gesture hợp lệ. Bool/enum là discrete command sau khi Edit session đã xác nhận; không confirm từng click.

**Live preview opt-in:** bật có chủ đích trong Edit settings, ghi qua cùng gateway khi gesture, cap tối đa một write/member/frame và có frequency cap. Mỗi write kiểm tra quyền/current expected value. Gameplay ghi chồng → dừng preview/báo Changed externally. Không đóng băng game để tránh conflict.

Transaction lifecycle đề xuất:

1. Begin giữ target/session generation/member path/type, baseline, before-value; edit model độc lập row pool.
2. Update parse/validate theo kiểu; default chỉ preview UI. Live write rồi read-back actual value và cập nhật expected-value.
3. End commit hoặc cancel. Default cancel bỏ draft. Live cancel chỉ thử restore before-value qua access/conflict; thất bại thì nói rõ giá trị đã giữ, không giả đã rollback.
4. Hide/tab switch/lock/destroy/unload/pointer cancel dừng capture/repeat và hủy draft. Live write đã xảy ra là thay đổi thực; không ép restore sau khi mất quyền.
5. Read-back sau setter: game clamp/normalize thì hiện actual value. Collection đổi index/identity invalidate draft/history phù hợp, không ghi nhầm item mới cùng index.

History P0 ví dụ 50 commands/session, một scrub một command. Undo/Redo kiểm tra current expected/access/target validity, không force overwrite conflict. Ctrl/Cmd+Z/Y chỉ khi workspace focus và input không đang dùng text undo; có menu thay thế. Clear object references khi session kết thúc; không lưu history vào player save.

History chỉ khôi phục giá trị đã ghi; không hoàn tác mọi event/network/save/spawn side effect. Edit/Live confirmation phải nói rõ, không quảng cáo tương đương `UnityEditor.Undo`.

## 10. States và input ownership

| State | Hiển thị/hành động |
| --- | --- |
| Readonly config/mode | Lock + lý do; copy/reveal vẫn dùng được |
| Edit | Pencil + chữ; member readonly vẫn có lý do riêng |
| Draft valid | Marker vàng/delta, Apply/Cancel theo active editor |
| Invalid | Giữ input, lỗi ngắn, không ghi partial |
| Applied | Highlight ~200 ms, status không nhảy layout |
| Conflict | Current/candidate/baseline ở details; Reload/Cancel |
| Destroyed/unloaded | Target unavailable; dừng edit, Choose object |
| Empty/no results/scanning | Lời nhắc, count/progress, clear filter |

Không dùng màu/hover làm nguồn thông tin duy nhất. Unicode/tên dài có ellipsis + details, không bật rich-text interpretation cho name/path/value. Decorative graphic tắt raycast. Modal/popup reuse shell input gating và EventSystem; không tự tắt action maps của game rồi quên restore.

Draft/focus không mất khi refresh/pool. Scroll field khỏi viewport giữ draft hợp lệ trong model; row unbind dừng gesture/listener/tooltip/repeat. Popup/editor model sống đến khi đóng hoặc invalid; không giữ target vô hạn qua pooled view.

## 11. Asset, icon, sprite và quyền thương mại

### Gallery icon Unity Editor theo yêu cầu

Đã xem [jasursadikov/unity-editor-icons](https://github.com/jasursadikov/unity-editor-icons) và README. Repo mô tả gallery icon xuất từ Unity Editor, có PNG/JSON và tên tra cứu. Dùng làm reference cho **ngữ nghĩa và nhận diện**: hierarchy tree, GameObject cube, Transform axes, Camera, Light, script, RectTransform/Canvas, Rigidbody/Collider. Không suy ra quyền redistribute artwork Unity từ việc repo public; chưa xác nhận license cho phép bundle các PNG này trong package thương mại.

Phương án thực hiện: ưu tiên glyph tương đương từ Material đã có hoặc tự author icon generic cùng ngôn ngữ nét/khối. Map đề xuất: Hierarchy → account_tree; GameObject → deployed_code/cube; Transform → open_with; Camera → videocam; Light → lightbulb; Script → code; Canvas/RectTransform → dashboard/crop; Collider → outline shape. Tên glyph cần xác nhận file nguồn khi import. Nếu có quyền rõ cho từng Unity-derived PNG mới được bundle nó; lưu notice/provenance. Không dùng `EditorGUIUtility.IconContent` trong runtime player.

Icon “sinh động” bằng ngữ nghĩa, tint có giới hạn và state feedback: chevron xoay ngắn khi mở, selected chuyển màu nhẹ; không chạy animation lặp trong cây. Mọi icon có fallback generic cho component lạ, cache theo type, không lookup/allocate mỗi frame.

Đã tra nguồn license ngày 2026-11-23. Ưu tiên **Material icons đã bundle** để cùng hệ shell. Nếu concept cần outline mới, thử trọn nhóm Scene bằng Tabler rồi chọn một bộ; không trộn nhiều stroke style trong toolbar.

| Nguồn | License nguồn ghi nhận | Quyết định |
| --- | --- | --- |
| [Material icons](https://github.com/google/material-design-icons/blob/master/LICENSE) | Apache 2.0, package đã có notice | Ưu tiên reuse, ghim revision/file mới, ghi sửa đổi |
| [Tabler Icons](https://github.com/tabler/tabler-icons/blob/main/LICENSE) | MIT, cho phép bán bản sao kèm notice | Subset outline dự phòng, giữ copyright/license |
| [Lucide](https://lucide.dev/license) | ISC; subset Feather có MIT riêng | Thay thế nếu phù hợp, giữ notice liên quan; không cần dùng cùng Tabler |

Asset ledger theo từng file thực nhập: source URL, revision, license/notice path, modifications, hash, export settings. “Free download” không đủ để kết luận có quyền redistribute trong package bán. License icon mới không đóng gate Imported Presentation hiện có.

| Nhóm cần có | Nội dung | Cách làm |
| --- | --- | --- |
| Navigation | tree/scene/search/filter/chevron/back/clear/more/refresh/pause/play | Vector cùng hệ, kiểm tra 16/20/24 logical units |
| Object/component | cube/transform/rect/camera/light/script/canvas/collider/warning | Generic icon có quyền rõ; không lấy Unity Editor built-in art để bán lại |
| Editing | lock/pencil/check/cancel/plus/minus/undo/redo/copy/link/locate | Tint theo state; hit surface tách artwork |
| Controls | rounded fill/outline/focus ring/selection stripe/checkmark/checkerboard | Tự author SVG/shape, PNG RGBA 9-slice, border/padding đúng |
| Empty states | Choose object/No results/Missing target | Có thể gen minh họa original nhỏ bằng AI |

Runtime ship PNG Sprite + meta; SVG/source và recipe ở authoring assets. Raster 1×/2× theo display thực, atlas/padding/filter/alpha kiểm tra trên render. Không thêm dependency SVG runtime chỉ để dùng vài icon. Các glyph/control cần sắc nét dùng vector-derived sprite, không gen AI thay cho số/chevron/checkbox.

AI optional brief: geometric object tree, graphite/rose, transparent, ít chi tiết, không chữ/logo/trademark/vendor artwork. Lưu prompt/model/tool/date/hash/reference và điều kiện sử dụng khi tạo; review quyền thương mại và yêu cầu disclosure lúc phát hành. Không hứa độc quyền/copyright chỉ vì AI tạo. Nhiệm vụ plan hiện tại chưa tạo/import sprite runtime; chỉ cần gen khi chọn minh họa bitmap phụ.

Upstream MIT/BSD và icon license không giải quyết quyền source `Runtime/Imported/Presentation` của debugger. Audit đó vẫn là release gate đã có trong `Documentation~/RELEASE.md`.

## 12. Kiến trúc và file map dự kiến

Tên mới bên dưới là đề xuất, không phải API đang tồn tại.

| Phần | Owner/thay đổi |
| --- | --- |
| Index/projection | Giữ SceneInspectionSession/HierarchyProjection; scene/search/object row kinds, icon cache |
| Write/metadata | InspectionMember/descriptors + typed numeric operations, edit transaction, bounded history trong Engine |
| View/controller | SceneTabController orchestration/navigation; tách drawer registry khỏi InspectorRowView dùng chung |
| Numeric | NumericFieldView/NumericScrubHandle/NumericEditSheet; normalized input qua bridge hiện tại |
| Typed views | Vector/Range/Color/Flags/Reference/ComponentHeader views và prefab riêng |
| Theme | SceneInspectionTheme + density/layout tokens, bridge palette/fonts, không đổi toàn shell palette |
| Overlay | Popup host theo session trong shell, reuse visibility/access/input owner |
| Editor | Sửa authoring explicit hiện có khớp saved prefab; không generator tạm chạy lúc consumer import |
| Catalog | Một Scene entry, thêm theme/drawer binding ổn định; fallback catalog cũ |

Giữ GUID cũ cho prefab còn dùng, `.meta` mới cho asset mới; kiểm tra nested overrides và consumers. Author bằng Editor API/MCP đúng instance, dirty/save/reopen/validate; không rewrite hàng loạt YAML để reskin. Runtime thuần .NET không nhận UnityEditor/Unity types.

Implementation/assets thuộc submodule package; sandbox giữ plan/demo/evidence. Khách không cần copy sandbox code. Không tự publish/tag package vì viết plan.

## 13. Hiệu năng và lifecycle

- Hierarchy theo height từng row kind/density; Inspector flattened model + prefix offsets. Pool theo drawer type, capacity giới hạn. Không rebuild toàn Inspector mỗi polling tick.
- Reflow theo dimension/density/theme event, bỏ layout polling không cần thiết; không ForceRebuild liên tục khi scrub/splitter.
- Hidden/disabled/locked dừng index/member polling, tooltip, repeat, gesture. Unscaled time; giữ scan budget/cancel search hiện có.
- Fixture: 1k/10k objects, depth 20, collection 1k, 50 visible scalar members, 100 spawn/destroy mỗi giây. Mục tiêu để đo: selection cached <100 ms, search 10k <500 ms, CPU p95 feature <2 ms/frame trên Android đại diện đã chọn. Không quảng cáo zero-GC reflection.
- Đo bytes/refresh, steady CPU và spike selection/search/drag riêng. Reopen 100 lần và load/unload 20 vòng: pool/listener/history không tăng vô hạn; history/session không giữ destroyed targets.

## 14. Scope và lộ trình V2

**P0:** concept/theme, tree trực quan, typed drawers, quick numeric/vector/range/color/flags, transaction/live/history policy, dọc/ngang/resize/rotation, docs và targeted regression.

**P1:** reference assignment picker theo scope/type, drag reference, collections resize/reorder, pinned members, Light theme, extension UX. **P2:** multi-object edit, reparent, add/remove component, arbitrary property/method, remote inspection/gizmos — spec riêng khi có nhu cầu.

Ước lượng phần còn lại V2 trên backend hiện có, một Unity developer với UI/QA hỗ trợ; không cộng lại toàn bộ 22–34 ngày của plan V1.

| Phase | Việc làm | Bàn giao/gate | Ngày công |
| --- | --- | --- | --- |
| A. Design baseline | Concept desktop/dọc/ngang thấp, theme, icon inventory, đo scaler/SafeArea | Layout/state spec, numeric UX; concept không thay Unity gate | 1–2 |
| B. Tree/shell | Toolbar, scene header, selection, icons, filter menu, adaptive navigation | Saved prefabs, actual renders dọc/ngang, state qua rotation | 2–3 |
| C. Edit foundation | Typed operations, transactions, read-back/conflict/access/history | Model checks precision/overflow/cancel meaningful | 2–3 |
| D. Numeric/Transform | Scalar/vector/range, scrub/±, touch sheet, allowlist adapters | Sửa X không đổi Y/Z; readonly/step/rotation/cancellation đúng | 3–4 |
| E. Inspector polish | Foldout, enum/flags/color/reference/collections, metadata, state visuals | Popup focus/back, alpha/HDR, nested write-back/pool đúng | 2–3 |
| F. Integration | Input/layout/rotation matrix, lifecycle/profiler/regression | Receipts theo source, pending player/device rõ | 2–4 |
| G. Candidate/docs | QuickStart, shortcuts, limitations, asset ledger/notices, checkpoint | Reviewable candidate, bindings đủ, migration rõ | 1–2 |

Tổng dự kiến **13–21 ngày công**, cập nhật sau spike; không gồm thay source thiếu quyền phân phối, chờ device/build/store. Prototype hôm nay là đầu vào Phase A.

## 15. Acceptance và verification matrix

### Layout bắt buộc

| Nhóm | Viewport đề xuất | Nội dung kiểm chứng |
| --- | --- | --- |
| Desktop landscape | 1920×1080, 1280×720 | Splitter min/max, keyboard, số/tên dài |
| Phone portrait | 390×844, 360×800 | Two-page, vector reflow, sheet/IME, item cuối |
| Phone landscape | 844×390, 800×360 | Chiều cao thấp, compact/split theo rect thực, keyboard |
| Wide phone landscape | 2340×1080, 2400×1080 | Scaler/aspect, notch hai bên, khoảng bấm |
| Tablet cả hai hướng | 1024×768, 768×1024 | Split/compact đổi theo min pane, touch |
| Resize/rotation | Portrait ↔ landscape khi search/draft/popup đang mở | Identity/scroll/focus/draft giữ đúng, không write ngầm |

No-inset và inset bất đối xứng thực/fixture được ghi rõ; kiểm tra cả landscape trái/phải khi được hỗ trợ. Đây là viewport matrix, không giả danh thiết bị thật. Đo rect sau CanvasScaler, không screen-pixel-only. Safe area, keyboard, font scaling và shell sidebar cùng tham gia vào available rect.

### Flow và kiểm tra kỹ thuật

1. Mở Scene, tìm inactive object cùng tên ở scene khác, chọn đúng, reveal reference, Back giữ search/scroll. Destroy/unload giữa selection không stale.
2. Edit Position X `0 → 1.25` bằng ô/gesture/±, giữ Y/Z. Long/ulong precision, overflow, nested struct/list, slider range và flags unknown bits đúng.
3. Enter/Apply commit; blur không ghi text; Escape/Cancel bỏ draft. Readonly không khởi gesture. Revocation/hide/pointer cancel/rotation dừng repeat/capture đúng.
4. Gameplay thay current giữa edit → conflict. Live read-back clamp, cancel restore thất bại báo rõ. Undo một gesture một command, không giữ reference qua session.
5. Touch vertical scroll không đổi số; popup không click-through; close không để input blocker treo. Focus/IME không bị refresh cướp.

| Tầng evidence | Phạm vi |
| --- | --- |
| EditMode/model | Parse/round-trip/precision/overflow, compound conflict, history bounds, metadata, nested writes |
| PlayMode/UI | Pointer thật cho select/chevron/scrub/stepper/splitter, no write on refresh, pooling/focus |
| Integration | Catalog cũ/mới, một Scene entry, GUID/font/icon, Domain Reload off, shell/access/screenshot/cheats regression |
| Input | Legacy/Input System/Both trong consumer phù hợp; keyboard/pointer/touch/IME |
| Visual | Actual Unity renders đủ dọc/ngang, recorded scaler/data/state; so concept, sửa và chụp lại |
| Device/performance | CPU/GC/thermal và touch thực trên thiết bị ghi nhận |
| Player/artifact | Exact-final-source AOT/stripping/define disabled, fresh consumer/export import khi được yêu cầu |

Chỉ chạy check cần thiết theo change/failure. `onClick.Invoke()` không chứng minh raycast, ảnh concept không chứng minh Unity layout, archive structure check không thay import consumer. Kết quả V1 102/102 EditMode và 5/5 focused PlayMode là lịch sử. Broad regression V1 còn một Int64 Cheats failure đã tái hiện baseline, theo dõi riêng.

User trước đã yêu cầu **không cần thêm build**; giữ giới hạn đó. Editor/static/docs có thể tiếp tục; player/AOT để pending cho tới chỉ đạo mới. Windows smoke cũ trước final V1 và Android hết ổ C: không đóng gate V2.

## 16. Release và checkpoint tiếp tục

Gate trước thương mại: audit Imported Presentation/font/icon/shader/source, notices file port, clean artifact import và support matrix theo test thực. Reskin không cấp quyền cho vendor source. Đây là release gate đã có, không ngăn thiết kế/triển khai candidate nội bộ.

Version vẫn `0.1.8`; chọn preview version sau khi có API/migration cụ thể, không ghi đè published artifact cùng version. Không tự release/tag/store submit trong nhiệm vụ này.

[Checkpoint V2](../design/debugger-scene-v2/CHECKPOINT.md) ghi tiến độ của plan mới. Giữ checkpoint V1 nguyên làm hồ sơ lịch sử; handoff `DEBUGGER_RESUME_2026-11-23.md` có trạng thái trước implementation, không dùng để suy ra hiện tại chưa có code.

Bước tiếp theo: đo live scaler/SafeArea/available rect trong đúng Unity instance; author tree/header và một lát cắt Numeric + Transform đẹp ở **dọc, ngang rộng, ngang thấp và tablet**. Xây transaction foundation trước khi nối gesture vào gameplay; chỉ mở rộng các drawer còn lại sau khi lát cắt đạt cả hình thức và thao tác.
