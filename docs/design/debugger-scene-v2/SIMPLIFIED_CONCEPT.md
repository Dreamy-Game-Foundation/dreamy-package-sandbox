# Runtime Hierarchy · Concept tinh gọn

[Mở prototype](concept-simplified.html) · [Mở thẳng Inspector](concept-simplified.html?view=inspector)

## Luồng chính

Tìm object trong cây, mở component và chỉnh đúng thuộc tính. Tên object là tiêu đề Inspector. Active, Tag và Layer nằm ngay bên dưới hoặc cạnh tên. Component dùng icon từ package và một hàng gập mở. Position, Rotation và Scale dùng các ô X/Y/Z riêng. Vùng Apply/Cancel chỉ xuất hiện khi đang sửa.

| Bố cục | Cách dùng |
| --- | --- |
| Desktop | Cây và Inspector đặt cạnh nhau. Gõ trực tiếp vào ô số, Enter hoặc Apply để xác nhận. |
| Dọc | Bắt đầu ở cây. Chọn object để mở Inspector. Nút Objects giữ lại tìm kiếm và vị trí cây. |
| Ngang thấp | Một pane tại một thời điểm. Nút Objects nằm trên thanh đầu. Nội dung Inspector cuộn độc lập. |
| Chỉnh số trên màn hình nhỏ | Chạm đúng trục hoặc số cần chỉnh. Bảng số có giá trị, −/+, Cancel và Apply. Cuộn danh sách không thay đổi số. |

Concept chọn một pane dưới 741 px hoặc khi chiều cao từ 460 px trở xuống để thử trên trình duyệt. Khi triển khai Unity, chốt ngưỡng từ kích thước workspace trong Canvas và vùng an toàn thực tế.

## Bề mặt mặc định

- Giữ một ô Search cho cây. Tra cứu theo tên là luồng chính.
- Gom Refresh, Undo và Redo vào menu góc phải. Undo/Redo hiện trạng thái disabled khi lịch sử rỗng.
- Bỏ toolbar riêng, filter component độc lập và nút Copy/Pin lặp theo dòng khỏi concept. Để dành quyết định về Compare, Pin, Pause, Copy path và bộ lọc nâng cao cho bước triển khai Unity.
- Boolean hiện checkbox, lựa chọn hiện trong chính ô giá trị. Color và reference có thể dùng swatch hoặc ô giá trị mở picker theo ngữ cảnh. Bản mẫu tập trung vào Active, Tag, Layer, số và vector.

## Tham khảo và chuyển sang Unity

[UnityRuntimeInspector](https://github.com/yasirkula/UnityRuntimeInspector/tree/2d461b975725f4713aee5eb3857661e361ab8d27) dùng Hierarchy kết nối Inspector qua selection. **HierarchyField** tách thao tác mở nhánh, **GameObjectField** trình bày dữ liệu object rồi đến component, và **TransformField** gom Local Position, Rotation và Scale. Concept lấy lại cấu trúc này, sử dụng palette Dreamy hiện tại cùng Barlow, JetBrains Mono và các icon component trong package.

UI Unity sẽ tiếp tục dùng shell và vùng an toàn của Dreamy. Bố cục mới dùng lại cơ chế kiểm tra quyền và xung đột khi ghi của Inspector. Dữ liệu trong prototype là fixture trình duyệt. Các chỉnh sửa chỉ tồn tại trong lần mở trang.

## Kiểm chứng concept

Kiểm tra trong Orca Browser ở 1280×720, 390×844 và 844×390. Chạy thử tìm kiếm và chọn object, sửa số, Cancel/Apply, Undo/Redo, và đổi hướng khi bảng số đang mở. Các kiểm tra này áp dụng cho prototype HTML. Unity Play Mode, CanvasScaler, safe area thiết bị, bàn phím ảo và prefab thuộc bước triển khai sau.
