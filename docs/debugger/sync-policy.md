# Đồng bộ Dreamy Debugger và ThunderPunch Debugger

Kế hoạch triển khai và các điều kiện nghiệm thu nằm trong [kế hoạch Asset Store](../plans/THUNDERPUNCH_DEBUGGER_ASSET_STORE_2026-11-27.md).

| Phần | Owner | Cách chuyển thay đổi |
| --- | --- | --- |
| Logic chung đã có quyền phân phối và public contract | Dreamy | Fix kèm test ở Dreamy, pin commit, lập diff review cho ThunderPunch. |
| Logic được viết riêng do nguồn vendor chưa được duyệt | Repo triển khai tương ứng | Chia sẻ contract và ca kiểm thử đã được phép. Ghi `adapted` trong ledger. |
| UI, logo, prefab trình bày, menu, setup và listing | Từng repo | Giữ thiết kế và định danh của từng sản phẩm. Chuyển cải tiến hành vi bằng PR riêng khi cần. |
| Namespace, GUID, package ID, lưu trữ và cài đặt | Từng repo | Giữ định danh ổn định sau phát hành. Ghi migration nếu cần đổi. |
| Direct QA, Relay và công cụ xuất dữ liệu | Dreamy cho phần hiện có | ThunderPunch ghi trạng thái tính năng riêng. Thẩm định phần xử lý và truyền dữ liệu trước khi đưa vào bản Store. |

## Quy trình agent

1. Đọc [inventory](asset-provenance.md), [allowlist](sync-allowlist.json) và [ledger](sync-ledger.jsonl). Xác nhận đang ở đúng package, nhánh và commit. Ghi một ID kèm repro, loại thay đổi, đường dẫn, ảnh hưởng API, GUID, dữ liệu, IL2CPP và người review.
2. Với fix dùng chung phát hiện ở ThunderPunch, đưa fix và regression về Dreamy trước. Port từ commit Dreamy đã review. Mỗi hướng tạo PR riêng. Với module triển khai riêng, đưa ca kiểm thử hành vi về repo còn lại, rồi sửa implementation tương ứng.
3. Chạy trình lập danh sách dry-run từ repo ThunderPunch. Trình này đọc hai commit bất biến và chỉ báo đường dẫn cùng trạng thái review.

   ```powershell
   python <ThunderPunchRepo>/Tools~/sync/plan.py LocalPackages/com.dreamy.debugger <base-sha> <head-sha> docs/debugger/sync-allowlist.json
   ```

4. Mọi dòng `blocked-unapproved` cần bằng chứng hoặc ghi loại trừ trước khi port. `review-port` đưa file vào phạm vi review. Khi mapping được triển khai, review cả namespace, đường dẫn, GUID và scene/prefab references trước commit. Một lần chạy lặp phải cho kết quả giống nhau.
5. Ghi SHA commit, version, test receipts và ghi chú migration của từng repo trong ledger. Cập nhật changelog khách hàng tương ứng. Khi rollback, revert commit port hoặc pin lại gitlink và ghi revision đã quay về. Xử lý rollback dữ liệu theo migration của phiên bản.

## Áp dụng thay đổi đã review

`port.py` gom các thay đổi C# hợp lệ thành một bản vá ba chiều, đổi namespace/định danh được hỗ trợ và giữ line endings của đích. Chạy mặc định để review. Chỉ thêm `--apply` khi phạm vi đã đúng. Tool dừng nếu có xung đột và giữ nguyên working tree trước khi áp dụng. Tệp mới, xóa tệp, prefab, GUID và override của theme tiếp tục xử lý riêng.

```powershell
python -B <ThunderPunchRepo>/Tools~/sync/port.py <source-repo> <base-sha> <head-sha> docs/debugger/sync-private-allowlist.json <destination-repo> dreamy thunderpunch
```

Để chạy hướng ngược, đổi thứ tự repo và đặt hai tham số cuối là `thunderpunch dreamy`. Allowlist private áp dụng cho nhánh nội bộ. [Allowlist thương mại](sync-allowlist.json) được kiểm riêng theo [kiểm kê nguồn](asset-provenance.md).

Khi cài hai package trong một project, chọn một debugger để hoạt động và đặt **Mode Off** ở package còn lại. Hai debugger bật cùng lúc chưa được xác nhận là cấu hình hỗ trợ. Khi kiểm thử port, ghi rõ package nào active.

## Sổ theo dõi

Mỗi dòng JSON trong ledger có `id`, `scope`, `summary`, `paths`, `dreamy`, `thunderpunch`, `compatibility`, `verification` và `status`. SHA hoặc version chưa có dùng `null`. Pending hoặc deferred phải ghi điều kiện còn thiếu.
