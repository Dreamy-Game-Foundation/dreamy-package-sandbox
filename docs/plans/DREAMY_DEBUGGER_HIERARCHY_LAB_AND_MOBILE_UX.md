# Dreamy Debugger — Hierarchy Lab và UX chỉnh sửa

Plan tiếp theo cho Hierarchy V2. Điểm xuất phát là runtime đã có quyền chỉnh theo config, từng ô vector, kéo giá trị trên nhãn, Tag/Layer và icon cho các nhóm component phổ biến. [Checkpoint hiện tại](../design/debugger-scene-v2/CHECKPOINT.md)

## P0 · Chỉnh ngay khi mở Hierarchy

- Với mode Development/QA và `AllowSceneEditing` bật, mở Hierarchy sau khi unlock là sửa được ngay. Bỏ nút và hộp thoại bật Edit cho mỗi lần mở. Dùng badge nhỏ **Editing** hoặc **Read only** cùng lý do khi quyền bị khóa. Lab bật quyền sẵn.
- Lấy quyền ghi từ runtime thực tế, dùng chung `CanEditScene` và `InspectionMember.TryCommit`. Bỏ trạng thái bật Edit riêng của tab. Đóng tab, đổi scene hoặc đổi selection hủy draft. Sau khi mở lại, tính quyền từ phiên hiện hành. Mất quyền giữa lúc nhập phải khóa control và bỏ draft. Off/ReleaseDiagnostics và phiên đã relock không cho ghi.
- Kiểm tra config từ entry point thật. Bootstrap của sandbox hiện đọc `Assets/DebuggerConfiguration.asset`; lab dùng cấu hình riêng được bind rõ ràng. Chạy Play trực tiếp ở lab và từ Bootstrap để phát hiện trường hợp runtime tự khởi tạo đọc nhầm asset.

## P0 · Scene Hierarchy Lab

Tạo `Assets/_Project/Scenes/HierarchyInspectorLab.unity` để mở trực tiếp và bấm Play. Dùng primitive, màu phẳng và các prefab hiện có. Có camera, đèn và đúng một EventSystem. Host có cấu hình Development, tự mở Hierarchy sau khi unlock, chọn sẵn đối tượng mẫu và hiển thị cách reset ngắn gọn. Scene chạy độc lập với Bootstrap, gameplay, save, SDK và network. Khởi đầu khoảng 20–30 GameObject và một cây lồng 3 cấp.

| Nhóm trong scene | Thao tác thử và kết quả thấy ngay |
| --- | --- |
| Transform & 3D Physics | Đổi từng trục, Rigidbody Mass, Gravity, Collider Size/Trigger. Cube đổi vị trí hoặc hành vi khi bật mô phỏng. |
| Camera, Light & Rendering | Đổi FOV, cường độ đèn, màu và trạng thái renderer. Vật thể và ánh sáng cho phản hồi trực tiếp. |
| 2D Physics | Rigidbody2D, Box/Circle Collider2D. Đổi Gravity Scale, Offset và Radius. |
| UI & Audio | Canvas, Image, text, Toggle, Slider và AudioSource. Nhìn thấy thay đổi UI, âm thanh chỉ phát khi bấm thử. |
| Script Fields | Bool, enum, flags, số nguyên lớn, số âm, Vector4, màu, collection và reference. Có đối tượng trùng tên, inactive và một field đổi theo thời gian để thử conflict. |
| Scene Operations | Nút spawn, destroy và reparent trên dữ liệu mẫu. Kiểm tra selection, tìm kiếm, Reveal và thông báo khi object biến mất. |

**Reset samples** khôi phục dữ liệu fixture trong phiên chạy. Tách cụm physics/âm thanh khỏi trạng thái mặc định để demo đứng yên và dễ kiểm tra. Chỉ dùng tag/layer đã có trong project. Tạo thêm một display nhỏ chỉ hiện giá trị trước/sau của field đã chỉnh để phân biệt draft với giá trị runtime.

## P0 · Icon có độ phủ kiểm chứng được

- Lập bảng `type → sprite → nguồn → fallback` cho toàn bộ component trong lab và danh sách Unity/uGUI package công bố hỗ trợ. Bổ sung icon riêng cho các kiểu còn đang dùng chung như Button/Toggle/Slider/InputField, Canvas/CanvasScaler/LayoutGroup, AudioSource/Listener, MeshFilter/MeshRenderer. Có icon cảnh báo cho missing script. Phân biệt 2D/3D và AudioSource/Listener bằng hình dạng.
- Thứ tự chọn icon là override theo tên C# đầy đủ trong `HierarchyComponentIcons`, map đúng type, map nhóm gốc, rồi icon Script hoặc Component. Một component project có thể gán sprite của mình. Icon riêng cho mọi type bên ngoài được làm khi có mapping cụ thể. Mọi hàng luôn có ảnh và tên ngay cả khi type chưa biết.
- Dùng sprite Dreamy tự tạo hoặc nguồn có quyền phân phối rõ ràng. Icon foldout, dropdown, back, search và clear đều là sprite có hit target riêng. Giữ theme Material Compact, kiểm tra contrast và kích cỡ lúc hiển thị thực.
- Khi kiểm tra prefab, rà cả bảng icon và từng component có trong lab. Với các type công bố hỗ trợ, thiếu sprite hoặc rơi về icon generic là lỗi. Với type lạ, xác nhận fallback và custom override hoạt động sau khi đóng/mở nhóm, cuộn qua row tái sử dụng và chạy player.

## P0 · Chỉnh số theo thiết bị nhập

| Phương án | Quyết định |
| --- | --- |
| Kéo nhãn trực tiếp | Giữ với chuột/trackpad. Shift chỉnh tinh, Escape hủy, thả để ghi. Tắt với ngón tay để nhãn và trục không tranh thao tác cuộn. Phân biệt theo loại con trỏ để chuột gắn vào tablet vẫn dùng được. |
| Chạm giá trị mở bộ chỉnh số | Mặc định cho touch. Hiện tên field và trục, số chính xác, nút ± lớn, bước chỉnh, Apply và Cancel. Chạm giá trị mới mở bàn phím; người dùng có thể dùng ± mà không gọi bàn phím. |
| Slider | Dùng trong bộ chỉnh số với field có min/max có ý nghĩa, ví dụ Volume hoặc Alpha. Kèm giá trị chính xác và ô nhập. Position, Mass và các số tự do dùng stepper/nhập trực tiếp. |
| Nhấn giữ / tăng tốc | Để sau. Chỉ thêm nếu kiểm tra trên thiết bị cho thấy cần và việc nhấc ngón, hủy hoặc đổi focus dừng đúng. |

Trên phone dọc dùng sheet dưới, ngang thấp dùng vùng chỉnh cạnh hoặc dialog có thể cuộn vào chỗ vừa màn hình. Sheet giữ trục được chọn, đổi X/Y/Z/W nhanh và chỉ Apply phần đã chỉnh. Dùng cùng transaction hiện tại để giữ precision, kiểm tra overflow và phát hiện runtime đổi giá trị trong lúc nhập. Back, chạm ra ngoài hoặc mất quyền hủy draft. Khi xoay hoặc bàn phím bật, giữ draft hợp lệ và giữ Apply/Cancel trong vùng chạm được. Dùng ít nhất 48×48 dp làm mục tiêu vùng chạm trên Android và đo trên thiết bị với CanvasScaler thực tế. [Apple Gestures](https://developer.apple.com/design/human-interface-guidelines/gestures) · [Apple Sliders](https://developer.apple.com/design/human-interface-guidelines/sliders?changes=_1) · [Android touch targets](https://developer.android.com/guide/topics/ui/accessibility/views/apps-views)

## P0 · Bố cục và trạng thái

- Giữ theme Dreamy. Dọc dùng Hierarchy → Inspector và nút Back giữ vị trí cây. Ngang đủ rộng dùng hai pane. Ngang thấp ưu tiên viewport của field và điều khiển chọn object, thay cho các nút dài và metadata lặp. Dùng vùng khả dụng sau safe area/bàn phím, không dùng hướng màn hình để quyết định một mình.
- Header Inspector chỉ hiện tên, Active, Tag và Layer có thể thu vào **Object**. Component header gồm icon, tên và chevron. Đặt field quan trọng của từng loại lên trước, ví dụ Transform, Mass/Gravity, FOV, Volume. Ô trục có nhãn chữ đi kèm màu. Chi tiết ít dùng đưa vào foldout hoặc menu.
- Làm rõ trạng thái giá trị có thể sửa, draft, đã Apply và conflict bằng phản hồi cạnh field. Lỗi nhập giữ lại nội dung để sửa. Conflict hiển thị giá trị mới nhất và nút Reload. Filter, selection, foldout và vị trí cuộn sống tách khỏi row vật lý để resize không mất ngữ cảnh.
- Borrow cách nhóm thuộc tính và thao tác bàn phím từ Unity Inspector. Học cách nối selection giữa hai pane và drawer theo kiểu từ UnityRuntimeInspector. Dùng lại các pattern phù hợp với runtime và theme Dreamy. [Unity Inspector](https://docs.unity.com/en-us/engine/6000.0/manual/unity-editor/editor-windows-views-reference/using-the-inspector) · [UnityRuntimeInspector](https://github.com/yasirkula/UnityRuntimeInspector)

## P1 · Mở rộng sau khi P0 ổn định

1. **Undo/Redo theo phiên**. Một lần Apply hoặc thả kéo là một mục lịch sử. Nếu game đã đổi giá trị sau đó, Undo báo conflict. Lịch sử phản ánh giá trị chỉnh trong Inspector và không hoàn tác side effect gameplay.
2. **Drawer theo kiểu**. Color picker, flags, Tag picker từ catalog project, reference picker giới hạn theo type và object đang load, pin field thường dùng. Ưu tiên Tag, Color và flags trước reference picker.
3. **Tìm và đối chiếu**. Search theo component/type/tag/layer, Reveal object được tham chiếu, copy giá trị chuẩn xác. Thử Inspector pin và so sánh hai object khi làm QA.
4. **Mở rộng dự án**. Registry cho icon, nhóm thuộc tính và drawer riêng. Xem xét Add/Remove Component, multi-edit và reparent sau khi có Undo, kiểm tra dependency và semantics cho từng thao tác.

## Trình tự triển khai và tiêu chí bàn giao

1. **Quyền chỉnh và lab**. Play từ scene mới và từ Bootstrap. Unlock xong chỉnh Mass, Position X, Tag và Layer ngay. Thử relock giữa lúc nhập, mở lại và xác nhận runtime đọc đúng config. Chạy lab từ scene chưa từng mở trước đó.
2. **Bảng icon và bố cục**. Kiểm tra từng type trong lab, custom override, missing script và prefab bindings. Xem ảnh runtime ở 390×844, 844×390, tablet hai hướng và desktop. Xác nhận tên field dài, số lớn, picker và trạng thái sửa vẫn đọc được.
3. **Touch editor**. Trên máy Android/iOS thực nếu có, thực hiện bằng một tay các việc cuộn nhanh, đổi Position X bằng ±, nhập số âm/chính xác, sửa slider, xoay khi còn draft và đóng keyboard. Tiêu chí là cuộn không làm đổi số, một Apply chỉ ghi một lần, trục khác giữ nguyên và nút Apply luôn tiếp cận được. Editor/Simulator chỉ ghi nhận bố cục và luồng mô phỏng.
4. **Kiểm thử hồi quy**. Dùng bộ SceneInspection hiện có và bổ sung coverage tại đúng rủi ro của auto-edit, icon mapping, draft/cancel và định tuyến touch. Ghi kết quả theo lớp kiểm chứng. Cần receipt thiết bị cho kết luận về chạm thật.

P0 và các mục P1 về chỉnh giá trị, picker, tìm, ghim, đối chiếu và cấu hình trình bày đã được triển khai trong workspace. [Checkpoint và kiểm chứng](../design/debugger-scene-v2/CHECKPOINT.md)

### Kết quả đánh giá các thao tác cấu trúc

- **Add/Remove Component** cần tính dependency từ `RequireComponent`, khôi phục thứ tự và tham chiếu khi Undo, và xử lý side effect của lifecycle khi thêm hoặc xoá component.
- **Multi-edit** cần mixed values và quy tắc khi một trong nhiều object bị gameplay thay đổi giữa lúc mở draft và Apply.
- **Reparent** cần chọn rõ giữ local hay world transform, lưu sibling index và xác thực parent vẫn tồn tại khi Undo. Trường hợp khác scene cần policy riêng.
- Các thao tác này được để lại cho P2. Project có thể cấu hình icon, nhãn, nhóm và chọn drawer dựng sẵn theo component/field. Drawer UGUI tùy biến qua code cần hợp đồng riêng cho draft, focus và xoay màn hình.

## Cập nhật sau duyệt concept

Đã áp dụng bố cục tinh gọn trong Unity. Active, Tag và Layer nằm trong phần tóm tắt object. Lệnh phụ tập trung trong menu và thao tác chạm mở trình chỉnh cho từng giá trị. Kết quả cuối gồm 39/39 EditMode, 9/9 PlayMode và kiểm tra trực tiếp trên Lab trong Unity Editor. Các mục thiết bị thật và P2 vẫn mở. [Checkpoint](../design/debugger-scene-v2/CHECKPOINT.md) · [Receipt](../evidence/debugger/hierarchy-v2/simplified-ui.json)
