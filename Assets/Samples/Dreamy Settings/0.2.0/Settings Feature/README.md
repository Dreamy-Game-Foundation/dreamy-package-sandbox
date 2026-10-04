# Settings Feature

Sample của Dreamy Settings. Import từ Window > Package Manager > Dreamy Settings > Samples > Import. Unity chép nội dung vào Assets/Samples/Dreamy Settings/0.2.0/Settings Feature/.

## Cấu trúc và tích hợp

Giữ nguyên folder, .meta, asmdef và reference prefab khi chuyển vào project. Chỉ giữ một bản script/asmdef và một JSON cho mỗi key Resources/DataConfig. Bootstrap config/save/wallet/audio tại GameInstaller trước khi bật UI, theo [README package](../../README.md). Link tương đối này dùng trong source package; sau import, mở README package từ Package Manager.

Gán toggle/button của SettingsPanel, reference RateUsPanel prefab trên launcher. Cài Audio và ISettingsService thật trước tạo panel. Launcher mô phỏng chỉ phục vụ sample và tự init/show ở Awake; khi dùng manager-owned lifecycle phải chỉnh integration cho phù hợp.

Assembly Dreamy.Feature.Settings.Integration.Runtime reference Dreamy.Settings.Runtime, Dreamy.Core.Runtime, Dreamy.Economy.Runtime, Dreamy.UI.Runtime, Unity.TextMeshPro, UnityEngine.UI, UniTask.

## Addressables Group và class address

1. Lưu prefab/variant của game tại Assets/_Project/Prefabs/Panel/SettingsPanel.prefab. Với UIPanel, root phải có subclass tương ứng.
2. Mở Window > Asset Management > Addressables > Groups; tạo settings nếu chưa có.
3. Tạo group UI Panels và kéo prefab vào group.
4. Đặt cột Address thành Panel/SettingsPanel.prefab.
5. Tạo class dùng chung trong game:

```csharp
public static class PanelAddress
{
    public const string Home = "Panel/HomePanel.prefab";
    public const string Current = "Panel/SettingsPanel.prefab";
}
```

Đường dẫn asset trên disk và address là hai giá trị riêng. Address do bạn đặt, constant phải khớp chính xác cột Address. Tên group không phải key tải. HomePanel là ví dụ subclass do game tự tạo.

Launcher sample tự init/show trong Awake. Nếu giữ launcher, load prefab bằng AssetLoader rồi instantiate dưới Canvas; nếu chuyển sang PanelManager.Create, chỉnh launcher để manager quản lý lifecycle. Cài service thật trước cả hai luồng. RateUs launcher reference prefab trực tiếp; muốn tra address phải sửa integration của game.

Build Addressables content cho target trước khi thử player. AssetLoader cache prefab; đóng panel không tự unload cache. Chỉ unload sau khi mọi instance/consumer đã kết thúc.
