# Sandbox game cheats

Cheat của game nằm trong `Runtime/SandboxGameCheats.cs`, đăng ký bởi `SandboxCheatHost` sau khi GameInstaller Ready. Host chờ wallet thật, dùng registry/access của DebuggerRuntime và giải phóng registration khi disable. Không đăng ký các biến Coin/God Mode giả của scene mẫu vào game nữa.

## Sử dụng

1. Play `Assets/_Project/Scenes/BootstrapScene.unity`.
2. `Window → Dreamy Debugger → Cheat Panel`, nhập PIN cấu hình (mặc định `0000`). Hoặc tap góc trên trái 5 lần trong Game/Simulator.
3. Tab Options: chọn `Coins grant amount` / `Gems grant amount`, bấm `Add Coins` / `Add Gems`, rồi Confirm.
4. Inventory: Knight cards, Silver chests, Daily reward cards, Red hero skin.
5. Entitlements: `Grant No Ads`, Lifetime VIP, Starter Fund, Season 1 Premium Pass. Xem `owned` và `Last cheat result` để kiểm tra kết quả.

Mỗi thao tác grant tạo transaction ID mới, gọi IResourceWallet.TryGrant để lưu vào save economy và phát BalanceChanged cho UI. Balance là read-only; không sửa trực tiếp JSON, PlayerPrefs hay model UI. Grant amount giới hạn 1–1.000.000, có xác nhận trước khi lưu. Mở lại game vẫn giữ tài nguyên/entitlement đã cấp.

**No Ads:** project này đã có entitlement `entitlement.remove-ads` và EntitlementEffectBinding. Callback hiện là ApplyNoAdsDemo ghi log; chưa có SDK quảng cáo. Cheat cấp ownership thật vào wallet và kích hoạt hook đó, không tạo giao dịch IAP/receipt giả. Khi tích hợp SDK, nối callback game hiện có với API No Ads của SDK. Không cung cấp nút thu hồi entitlement mua thật.

## Mỗi game có bộ cheat riêng

- Dùng `BoundCheatExamples.cs` trong sample Quick Start làm mẫu: ResourceCheatOptions nhận delegate đọc balance/grant; OwnershipCheatOptions nhận delegate owned/unlock. Không phụ thuộc Core/Economy/Shop.
- Game thay catalog ID/nhãn/delegate trong class riêng, ví dụ SandboxGameCheats. Có thể bỏ/tạo nhóm và option tùy game.
- Chỉ host game phụ thuộc các service của game. Package debugger Runtime không tham chiếu game hoặc Dreamy Core/UI.
- Prefab/config riêng: `Prefabs/SandboxDebugger.prefab`, `SandboxDebuggerConfiguration.asset`. Đây là variant từ package base; chỉnh ở project, không sửa package Runtime.
- Editor setup: mở BootstrapScene, chạy `Window → Dreamy Debugger → Set up sandbox game cheats`; installer giữ config/prefab đã tùy chỉnh, không thêm host trùng. Lưu scene sau khi setup.
- Standalone DebuggerDemo vẫn dùng ProjectCheatHost với dữ liệu giả để thử UI mà không cần bootstrap game.

Entitlement và resource được wallet lưu lâu dài; không dùng nút reset mẫu để xóa save game. Build development/Editor mới đăng ký cheat; policy release của DebuggerRuntime/scene processor vẫn áp dụng.

Kiểm chứng: các test ở Tests/Editor dùng thư mục save tạm riêng; không ghi vào save đang chơi. Evidence ở `docs/evidence/debugger/project-cheats`.
