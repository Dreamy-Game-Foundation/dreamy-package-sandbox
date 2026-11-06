# Cheat của game — partial class + attribute

Thêm cheat bằng cách tạo/sửa file `Runtime/GameCheatOptions.TenTinhNang.cs`. Không phải sửa host hay thêm `yield return`.

## Cấu trúc

| File | Trách nhiệm |
|---|---|
| GameCheatOptions.cs | Constructor nhận wallet/balance service, đăng ký reflection một lần |
| GameCheatOptions.Economy.cs | Coins, Gems |
| GameCheatOptions.Inventory.cs | Cards, chests, reward items, skin |
| GameCheatOptions.Entitlements.cs | No Ads, VIP, Starter Fund, Premium Pass |
| GameCheatOptions.Diagnostics.cs | Kết quả thao tác và trạng thái integration |
| GameCheatOptions.Time.cs | Time Scale, reset |
| GameCheatOptions.Application.cs | Target FPS, VSync, scene |
| GameCheatOptions.Wallet.cs | Helpers đọc/grant/save và xử lý lỗi, không hiện lên panel |
| SandboxCheatHost.cs | Đợi game ready, đăng ký đúng một container, Dispose khi disable |
| link.xml | Giữ reflected members khi stripping/IL2CPP |

Đây là code game-owned. Runtime package không phụ thuộc Core/Economy/Shop của game. Host dùng wallet thật; không đăng ký model Coin/God Mode giả vào game.

## Cách sử dụng

1. Play BootstrapScene.
2. `Window → Dreamy Debugger → Cheat Panel`, nhập PIN (mặc định 0000); hoặc tap góc trên trái 5 lần.
3. Tab Cheats: Economy/Inventory/Entitlements có card balance/amount/action hoặc ownership/grant.
4. Chọn amount, bấm Add rồi Confirm. Wallet lưu và phát BalanceChanged như trước.
5. Gameplay/Application có Time Scale, Reset, Target FPS và giá trị chỉ đọc.

No Ads cấp entitlement thật cho sandbox; callback hiện là ApplyNoAdsDemo, chưa có ads SDK. Không tạo IAP receipt giả hoặc thu hồi ownership mua thật. Mỗi grant resource tạo transaction ID riêng; ownership đã có sẽ không grant lại.

## Thêm cheat thông thường

```csharp
using System.ComponentModel;
using Dreamy.Debugger.Presentation;

namespace Dreamy.Template.Debugging
{
    public sealed partial class GameCheatOptions
    {
        [Category("My feature"), DisplayName("Test amount")]
        [NumberRange(1, 1000), Increment(10), Sort(0)]
        public int TestAmount { get; set; } = 100;

        [Category("My feature"), DisplayName("Apply"), Sort(1)]
        public void ApplyTestAmount()
        {
            // Gọi service thật của game bằng TestAmount.
        }
    }
}
```

Property có public setter → editable; getter-only/private setter → read-only; enum → dropdown; public void không tham số → button chạy ngay; gắn CheatConfirmation cho thao tác nguy hiểm. Resource/ownership card grants luôn confirm. Helper giữ private hoặc thêm Browsable(false). Field/static/method có tham số không tạo control. Chỉ partial cùng namespace/class/assembly mới ghép được.

## Thêm thẻ tài nguyên

Copy nhóm 3 member trong Economy.cs: read-only balance, amount có NumberRange, method grant. Mỗi member dùng ResourceCheat với cùng card ID/title và role tương ứng. Ví dụ card ID `sandbox.item.energy` tự sinh option ID `.balance`, `.amount`, `.grant`; ResourceId phải được catalog/wallet game hỗ trợ. Không sửa trực tiếp JSON/PlayerPrefs hoặc model UI.

## Thêm ownership

Copy cặp member trong Entitlements.cs: bool owned read-only + method grant có kiểm tra chưa owned. OwnershipCheat cùng card ID/title tự sinh `.owned`, `.unlock`. UI ẩn action khi đã owned; game vẫn giữ guard để automation không grant lặp.

## ID và lifecycle

32 option và 10 card hiện có giữ nguyên ID trước migration. CheatId chỉ định ID độc lập với tên C#; resource/ownership ID lấy từ card nên đổi tên member không làm mất pin/automation. Với option không có CheatId/card, ID mặc định là `sandbox.game.<MemberName>`: đổi tên sẽ đổi ID.

Host đăng ký một lần sau GameInstaller Ready, dùng runtime.Cheats và giải phóng lease khi disable hoặc runtime đổi. Mỗi game có container/service riêng. Không extend partial trong assembly package từ code game. Các API typed vẫn hỗ trợ integration khác, nhưng không còn được dùng để liệt kê cheat trong project này.

Time Scale/Target FPS thay đổi Unity trong session, không ghi wallet và không tự reset khi host disable. Có nút Reset Time Scale về 1. Target FPS không bảo đảm FPS thực tế; VSync/platform có thể ưu tiên khác. Không dùng nút reset của demo để xóa save thật.

Hướng dẫn attribute đầy đủ: LocalPackages/com.dreamy.debugger/Documentation~/CHEAT_OPTIONS.md. Evidence migration: docs/evidence/debugger/attribute-migration. IL2CPP/link.xml cần được xác minh lại với build của chính game khi thêm/đổi reflected type.


## Workflow preview.3

Host dùng `runtime.RegisterCheats(Options, "sandbox.game")`, giữ lease để Dispose. Nếu container implement INotifyPropertyChanged, notification tự subscribe, coalesce ở main-thread tick và unsubscribe khi owner/runtime đóng. Typed CreateOptions vẫn có cho test/integration thấp, không cần dùng ở host mới. Các action card grant luôn confirm; Reset Time Scale là action thường chạy ngay. Dynamic changes giữ control/card/pin không đổi và chỉ dựng lại group bị ảnh hưởng.
