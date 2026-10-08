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

Package lucky-wheel đã được checkout và khai báo trong Unity manifest; sample Lucky Wheel Feature được dùng trong Foundation demo.

## Foundation feature demos

Chạy `Assets/_Project/Scenes/BootstrapScene.unity`, hoàn thành hoặc Skip tutorial tự động, rồi mở demo từ Foundation panel:

| Button | Demo |
| --- | --- |
| Shop / Daily Reward / Settings / Rate Us | Sample có sẵn của package |
| Missions | Sample MissionDemoPanel: giả lập stage win/collect coin, claim thưởng |
| Progression | Sample ProgressionDemoPanel: Complete Stage ho?c Grant XP theo catalog |
| Lucky Wheel | Sample LuckyWheelPanel: spin/reveal, grant vào wallet dùng chung |
| Economy | Sample Wallet Demo: xem coin/gem/gold/chest, Grant 100 Coins và Retry Last Grant |
| Tutorial Demo | Chạy lại walkthrough Foundation với save trong bộ nhớ riêng |

Các feature dùng DataConfig, Datasave và wallet của GameInstaller. Tutorial Demo không xóa save người chơi. Progression sample hiện dùng Sequential và thưởng `currency.gold`; xem số dư trong Economy. Demo platform gateway của Settings/Shop vẫn là giả lập.

UI/logic mới nằm trong `Samples~` của package và bản import tương ứng trong `Assets/Samples`; FoundationFeatureDemos chỉ nối service và navigation. Khi cần dựng lại các prefab demo, dùng `Dreamy > Sandbox > Build Foundation Feature Demos` ngoài Play Mode và đóng Prefab Stage trước; lệnh này cập nhật Foundation cùng các prefab demo được chỉ định.

## Feedback samples

`com.dreamy.feedback` ?? ???c th?m d??i d?ng submodule trong `LocalPackages`, v?i UPM dependency local. Import **Basic Feedback** v? **Feedback Economy** t? Package Manager; m? scene trong `Generated` c?a t?ng sample. Economy sample c?n Basic Feedback v? Economy/UI/Audio.

Trong sandbox, ch?y `Assets/_Project/Scenes/BootstrapScene.unity`, r?i ch?n **Feedback** trong Foundation. Grant 100 Coins ph?t text/icon/VFX/audio sau khi wallet thay ??i; Retry kh?ng ph?t th??ng/feedback l?n n?a. Feedback d?ng wallet Datasave v? audio c?a bootstrap; coin reward t? c?c feature kh?c c?ng ???c host quan s?t.

D?ng `Dreamy/Sandbox/Build Feedback Samples and Integration` ?? n?i l?i asset demo ???c ch? ??nh. Xem [README Feedback](LocalPackages/com.dreamy.feedback/README.md), [plan](docs/plans/DREAMY_FEEDBACK_PACKAGE_PLAN.md) v? [validation](LocalPackages/com.dreamy.feedback/VALIDATION.md). Feedback 0.2.0 hi?n l? local, ch?a commit/push/tag.
