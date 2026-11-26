# Kiểm kê nguồn cho ThunderPunch Debugger

Baseline Dreamy đã commit ở `9027182`. Checkout Dreamy hiện có thay đổi local. Kiểm kê này đánh dấu phạm vi cần thẩm định, chưa cấp quyền phân phối cho tệp upstream nào.

| Đường dẫn Dreamy | Nguồn quan sát được | Trạng thái thương mại và việc cần làm |
| --- | --- | --- |
| `Runtime/Imported/Foundation/**` | Có license gắn với Stompy Robot LTD trong thư mục. | Kiểm tra từng tệp với bản upstream và license tương ứng. Một license trong thư mục chưa xác định được phạm vi của mọi tệp dẫn xuất. |
| `Runtime/Imported/Presentation/**`, prefab và catalog tham chiếu từ đó | Lịch sử migration ghi nguồn SRDebugger và thay đổi GUID, script, prefab. | Chờ xác nhận quyền phân phối hoặc thay bằng implementation và asset gốc. |
| `Runtime/Engine/ThirdParty/Graphy/**` | Package có notice Graphy và license Roboto. | Đối chiếu từng file với notice và yêu cầu Asset Store. |
| `Runtime/Branding/**` và font trong Imported | Có các notice riêng cho icon, font và asset; tài liệu Material UI chỉ nguồn của một phần. | Rà tài sản theo file, bao gồm logo và ảnh. Tài sản ThunderPunch hiện dùng vector mark mới. |
| `Runtime/Engine`, `Runtime/DirectQa`, `Runtime/Reporting` | Mã chức năng Dreamy với các dependency trong asmdef. | Xác minh tác giả và truy vết dependency vào Imported. Rà quyền trước khi thêm đường dẫn vào allowlist. |
| `Tools~/relay/**`, `Samples~/**`, docs và screenshots | Công cụ nội bộ, sample và tài sản mang thương hiệu Dreamy. | Rà độc lập quyền, dependency và dữ liệu chứa trong artifact. Mặc định loại khỏi bản ThunderPunch đầu. |

Mỗi file chuẩn bị chuyển cần ghi upstream SHA, chủ sở hữu, license, bằng chứng cấp quyền, notice đi kèm và người review. Bản preview ban đầu của ThunderPunch dùng mã C# và giao diện Editor mới.
