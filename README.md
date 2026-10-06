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
| `com.dreamy.feature.missions` | `LocalPackages/com.dreamy.feature.missions` | [GitHub](https://github.com/Dreamy-Game-Foundation/com.dreamy.feature.missions) |

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

## Hướng dẫn sử dụng package

Các package dùng author **Dreamy Game Studio**. Mở README của từng package để xem dependency, reference asmdef và cách bootstrap. Trong sandbox, các package đã được cài bằng đường dẫn local; khi mang sang project khác, cài cả dependency Dreamy và các dependency Git như UniTask/DOTween.

| Package | Hướng dẫn |
| --- | --- |
| `com.dreamy.assets` | [README](LocalPackages/com.dreamy.assets/README.md) |
| `com.dreamy.audio` | [README](LocalPackages/com.dreamy.audio/README.md) |
| `com.dreamy.core` | [README](LocalPackages/com.dreamy.core/README.md) |
| `com.dreamy.dataconfig` | [README](LocalPackages/com.dreamy.dataconfig/README.md) |
| `com.dreamy.datasave` | [README](LocalPackages/com.dreamy.datasave/README.md) |
| `com.dreamy.editor-tools` | [README](LocalPackages/com.dreamy.editor-tools/README.md) |
| `com.dreamy.feature` | [README](LocalPackages/com.dreamy.feature/README.md) |
| `com.dreamy.feature.daily-reward` | [README](LocalPackages/com.dreamy.feature.daily-reward/README.md) |
| `com.dreamy.feature.economy` | [README](LocalPackages/com.dreamy.feature.economy/README.md) |
| `com.dreamy.feature.missions` | [README](LocalPackages/com.dreamy.feature.missions/README.md) |
| `com.dreamy.feature.progression` | [README](LocalPackages/com.dreamy.feature.progression/README.md) |
| `com.dreamy.feature.settings` | [README](LocalPackages/com.dreamy.feature.settings/README.md) |
| `com.dreamy.feature.shop` | [README](LocalPackages/com.dreamy.feature.shop/README.md) |
| `com.dreamy.feature.tutorial` | [README](LocalPackages/com.dreamy.feature.tutorial/README.md) |
| `com.dreamy.ui` | [README](LocalPackages/com.dreamy.ui/README.md) |
### Feature và sample

1. Cài package và dependency theo README; thêm Runtime assembly vào asmdef của game.
2. Trong Package Manager, chọn package > Samples > Import. Giữ asmdef của sample; reference assembly sample nếu code game dùng các type của sample.
3. Khởi tạo config/save/audio/wallet theo từng feature trước khi mở UI. Chỉ giữ một bản script/asmdef và một JSON cho mỗi đường dẫn Resources/DataConfig.
4. Với panel kế thừa UIPanel, đặt prefab thành Addressable, khai báo address và dùng PanelManager sau khi bind presenter/service. HUD Economy/Progression và TutorialOverlay cần instantiate/bind riêng.
5. Thay wallet/gateway mô phỏng bằng implementation của game. Build Addressables trước khi kiểm tra player.

Package lucky-wheel đang chưa có nội dung trong checkout này nên chưa có README/package.json để cập nhật.
