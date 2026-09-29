# Dreamy Package Sandbox

Đây là Unity project dùng các Dreamy package dưới dạng Git submodule trong
`LocalPackages/`. Unity Package Manager tham chiếu chúng bằng đường dẫn local
trong `Packages/manifest.json`, nên cần khởi tạo submodule trước khi mở project.

## Clone project kèm submodule

Khi clone project lần đầu, dùng:

```powershell
git clone --recurse-submodules <repository-url>
cd dreamy-package-sandbox
```

Lệnh này clone repository chính và checkout đúng revision của mọi submodule.

## Khởi tạo submodule sau khi đã clone project

Nếu project đã được clone nhưng thư mục `LocalPackages/` chưa có nội dung, chạy
tại thư mục gốc project:

```powershell
git submodule sync --recursive
git submodule update --init --recursive
```

Hai lệnh trên:

1. Đồng bộ URL trong cấu hình Git local với `.gitmodules`.
2. Clone các submodule còn thiếu.
3. Checkout mỗi submodule về commit được repository chính ghim lại.
4. Khởi tạo cả submodule lồng nhau, nếu có.

## Các package được clone

| Package | Đường dẫn local | Repository |
| --- | --- | --- |
| `com.dreamy.assets` | `LocalPackages/com.dreamy.assets` | [GitHub](https://github.com/Dreamy-Game-Foundation/com.dreamy.assets) |
| `com.dreamy.audio` | `LocalPackages/com.dreamy.audio` | [GitHub](https://github.com/Dreamy-Game-Foundation/com.dreamy.audio) |
| `com.dreamy.core` | `LocalPackages/com.dreamy.core` | [GitHub](https://github.com/Dreamy-Game-Foundation/com.dreamy.core) |
| `com.dreamy.dataconfig` | `LocalPackages/com.dreamy.dataconfig` | [GitHub](https://github.com/Dreamy-Game-Foundation/com.dreamy.dataconfig) |
| `com.dreamy.datasave` | `LocalPackages/com.dreamy.datasave` | [GitHub](https://github.com/Dreamy-Game-Foundation/com.dreamy.datasave) |
| `com.dreamy.editor-tools` | `LocalPackages/com.dreamy.editor-tools` | [GitHub](https://github.com/Dreamy-Game-Foundation/com.dreamy.editor-tools) |
| `com.dreamy.ui` | `LocalPackages/com.dreamy.ui` | [GitHub](https://github.com/Dreamy-Game-Foundation/com.dreamy.ui) |

Kiểm tra trạng thái sau khi khởi tạo:

```powershell
git submodule status --recursive
```

Mỗi dòng bắt đầu bằng khoảng trắng nghĩa là submodule đã được checkout đúng
commit. Dấu `-` ở đầu dòng nghĩa là submodule chưa được khởi tạo; chạy lại lệnh
`git submodule update --init --recursive`.

## Mở project bằng Unity

Sau khi submodule đã được checkout, mở thư mục project bằng Unity. Package
Manager sẽ đọc các dependency `file:../LocalPackages/...` trong
`Packages/manifest.json` và dùng các package local tương ứng.

## Cập nhật submodule

Không dùng `git pull` bên trong một submodule trừ khi bạn chủ ý thay đổi package
đó. Repository chính ghim commit tương thích cho từng package. Để trở về các
commit được ghim sau khi có thay đổi local trong submodule, xem xét và xử lý các
thay đổi đó trước, rồi chạy:

```powershell
git submodule update --init --recursive
```
