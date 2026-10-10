# Basic Feedback — starter cho game

Sample gồm prefab dùng trong game, preset reward và một scene preview. Không cần Economy, Dreamy UI, Dreamy Audio hoặc Dreamy Core.

## Chọn đúng asset

| Asset trong Generated | Dùng để làm gì |
| --- | --- |
| GameFeedbackRig.prefab | Kéo vào scene game; đã có host, effects Canvas, pool roots và databases |
| CoinReward / StarReward / EnergyReward / GemReward | Icon fly rồi punch HUD; chỉnh trực tiếp trong Inspector |
| ButtonClick.asset | Punch + haptic cho button |
| Hit.asset | VFX, camera shake và haptic song song |
| FeedbackDemo.unity | Xem preview reward và thử từng primitive/trajectory |
| FeedbackRig.prefab | Rig của scene preview, gồm camera và các anchor demo |

Các preset reward không cấp tài nguyên và không thay đổi text số dư trong HUD. Game tự cập nhật state/UI; feedback chỉ animate.

## Setup vào HUD có sẵn

1. Import sample và TMP Essential Resources. Kéo **GameFeedbackRig** vào scene; giữ một host cho scene hoặc một host thuộc bootstrap.
2. Tạo một RectTransform làm điểm xuất hiện reward trong Canvas của bạn. Đây là **Icon Source**. **Target** là RectTransform của icon trên HUD: coin, star, energy, gem, booster…
3. Thêm component **Dreamy / Feedback / Feedback Player** vào object thuộc panel đang dùng. Gán Host, Definition, Icon Source và Target. Icon override có thể để trống để dùng sprite preset; Amount là dữ liệu runtime.
4. Dùng `Button.onClick → FeedbackPlayer.Play()` để kiểm ngay. `StopLast` và `CompleteLast` điều khiển lượt phát gần nhất. Khi panel/owner bị disable hoặc destroy, mọi request của owner đó sẽ dừng.

Prefab effect có Canvas overlay với sorting order 300 và resolution tham chiếu 1080×1920. Điều chỉnh order/CanvasScaler theo UI game. Các Image effect không chặn raycast; không cần một EventSystem mới để chạy rig. Chỉ scene preview có EventSystem.

Nếu project dùng asmdef, code sử dụng host cần reference `Dreamy.Feedback.Runtime` và `Dreamy.Feedback.Contracts`; code chỉ inject interface/context có thể reference Contracts.

## Gọi từ presenter

```csharp
[SerializeField] private FeedbackHost host;
[SerializeField] private FeedbackDefinition starReward;
[SerializeField] private RectTransform rewardSource;
[SerializeField] private RectTransform starHud;
[SerializeField] private Sprite starIcon;
private FeedbackHandle activeReward;

public void ShowConfirmedReward(long amount)
{
    host.Initialize();
    activeReward = host.Feedback.Play(FeedbackRequestBuilder.For(starReward)
        .From(this).IconsFrom(rewardSource).To(starHud)
        .WithAmount(amount).WithIcon(starIcon).Build());
}

// Chỉ dừng lượt bạn giữ, không dừng host dùng chung.
private void OnDisable() => activeReward.Stop();
```

Gọi sau khi game xác nhận reward; không cộng tài nguyên trong callback đến đích. Amount không quyết định số icon. Ví dụ thưởng 1000 coin vẫn có thể dùng 12 icon trong CoinReward.

## Thay tài nguyên và animation

| Reward | Style | Count | Gợi ý |
| --- | --- | --- | --- |
| Coin | ScatterMagnet | 12 | Bắn tỏa rồi hút về HUD |
| Star | Arc | 4 | Ít icon, giữ hướng ngôi sao |
| Energy | Fountain | 8 | Bật lên rồi gom về HUD |
| Gem | Spiral | 6 | Quỹ đạo xoắn, xoay nhẹ |

Duplicate một definition, đổi ID, sprite và các trường IconFly; giữ UI Punch nếu muốn HUD nhấn khi nhóm icon kết thúc. Straight phù hợp lượt gom nhanh. Duration là thời gian mỗi icon; Stagger làm icon xuất hiện lệch nhau. Size, Spread và ArcHeight dùng canvas units. Target được cập nhật khi HUD di chuyển.

Dùng `.WithIcon(sprite)` nếu nhiều resource dùng cùng graph. Definition giữ animation/preset; request chỉ giữ dữ liệu. Chọn Count theo mức độ nổi bật thay vì tạo hàng trăm icon cho reward lớn.

## VFX, floating text và camera

- Trong VfxDatabase, đổi prefab của entry `reward` hoặc thêm ID mới. Prefab cần `VfxInstance`; với ParticleSystem dùng `ParticleVfxInstance`. Cấu hình thời gian despawn đủ dài cho effect. Composite VFX còn cần Duration tối đa để tránh chờ mãi với effect loop.
- Thay font/material trên RewardText.prefab và text UI của preview. Đảm bảo font có glyph cần dùng và đầy đủ atlas/material dependencies. Font mới trong sandbox không tự trở thành font mặc định của package.
- Gán world camera trong Host nếu dùng floating text/camera shake. Có thể gọi `host.Initialize(gameCamera)` lúc setup; việc đổi camera dừng các lượt đang chạy. `.At(worldPosition)` là tọa độ world; floating text sẽ project bằng camera đó.
- Với camera follow, dùng transform offset riêng làm Camera Root. Overlay UI không rung khi camera world rung. Sample không chứa puzzle board để minh họa shake.

Icon fly giữa hai UI Canvas overlay dùng vị trí RectTransform. Nếu reward bắt đầu ở world object, project world position sang UI anchor bằng camera game trước, rồi truyền anchor đó vào IconsFrom; không truyền thẳng world Transform vào preset HUD.

## DOTween

Cài DOTween trước, rồi import sample **DOTween Feedback** trong Package Manager → Dreamy Feedback → Samples. Thêm `DotweenFeedbackProvider` lên rig rồi gán vào Host → Animation Provider **trước khi bật rig/chạy game**. Bản sample dùng chung chạy bằng backend Unity khi không có adapter; tất cả 5 trajectory vẫn có sẵn. Backend DOTween dùng tween/sequence nội bộ, gameplay vẫn nhận FeedbackHandle.

## Preview và troubleshooting

Mở FeedbackDemo và Play. Bốn nút resource phát các reward preset; các nút bên dưới cho phép thử từng channel và so sánh 5 trajectory. Stop/Complete chỉ dùng để kiểm behavior.

- Không thấy icon: kiểm Icon Source/Target là UI anchors đang active; definition có sprite; Canvas effect nằm trên HUD.
- Không rung camera: Host cần camera; UI overlay không chịu ảnh hưởng camera shake.
- Font vuông/trắng: import TMP Essential Resources và kiểm font atlas/material/glyph.
- Button preview không nhận input: scene dùng StandaloneInputModule; chọn Old Input Manager/Both hoặc thay bằng InputSystemUIInputModule. Rig game không thay input module của project.
- Effect cũ dừng khi play mới: Punch cùng target, Camera Shake và Screen Flash có chính sách replace. VFX/text/icon có lượt độc lập và pooling.

Sandbox giữ font/confetti bạn chọn cho preview nội bộ. Bản `Samples~` có Liberation Sans (OFL, xem FontLicense.txt), VFX mặc định và icon resource riêng để import độc lập; không cần bộ Hyper Casual FX.

## Tạo và chỉnh data nhanh

Cách ít setup nhất: dùng GameFeedbackRig + duplicate preset có sẵn + FeedbackPlayer. Rig đã có databases; bạn chỉ gán Definition, Icon Source/Target khi cần. Không cần tạo database cho từng definition, cũng không cần database cho UI Punch, Icon Fly, Screen Flash hay Camera Shake node (shake node lưu options trực tiếp).

`Generated/CommonPresets/` có thêm các asset:

| Preset | Effect | Dữ liệu khi gọi |
| --- | --- | --- |
| ButtonConfirm | Punch + Success haptic | `To(button)` |
| ButtonReject | Punch nhẹ + Warning haptic | `To(button)` |
| RewardPop | VFX reward + text `+{amount}` + haptic | `At(worldPosition)`, `WithAmount(amount)`; camera cho text |
| ComboPopup | Text `Combo x{amount}` + haptic | `At(worldPosition)`, `WithAmount(combo)`; camera cho text |
| LevelComplete | VFX + flash + Success haptic, rồi Delay + punch | `At(worldPosition)`, `To(continueButton)` |
| SoftShake | Rung camera nhẹ | Gán camera game vào Host |
| SuccessFlash | Flash xanh nhẹ + Success haptic | Không cần anchor |

Các preset reuse VFX ID `reward` và text ID `default` trong rig. Các optional node sẽ được bỏ qua nếu thiếu service/target; bật Required nếu effect đó bắt buộc. VFX và floating text cần world position, không phải vị trí RectTransform. Đây là preset animation thông dụng, không phải bộ VFX/art mới.

Tạo asset riêng bằng **Assets → Create → Dreamy → Feedback → Common Presets**. Menu tạo asset mới trong thư mục đang chọn; chỉnh ID riêng nếu dùng nhiều biến thể. Graph được lưu trong asset; factory chỉ chạy ở Editor.

Trong Definition Inspector:

- **Add effect…** chọn node và nhận các giá trị mặc định. Đổi Type sẽ reset options của node đó về mặc định; Undo để khôi phục nếu đổi nhầm.
- IconFly chỉ hiện sprite/animation; source và target được truyền lúc Play. Unscaled Time đặt ở Definition, không cần lặp lại trên từng node.
- **Lookup IDs from rig** là tùy chọn: kéo Host vào để chọn VFX/text ID từ đúng database và thấy cảnh báo ID thiếu. Reference này chỉ dùng trong Editor, không lưu vào Definition.
- Dùng Up/Down/Remove để sắp xếp graph. Sequence chờ từng bước; Parallel chạy đồng thời. Duration của VFX là thời gian tối đa chờ effect.

Database Inspector hiển thị entry theo ID, thêm/xóa/đổi thứ tự và validation ngay trong Inspector. Khi thêm VFX entry, chỉ cần ID + prefab; Prewarm Count có thể để 0. Fixed Lifetime chỉ hiện khi chọn chế độ FixedLifetime. Floating text entry cần prefab có FloatingTextInstance/TMP; duplicate database sample nếu muốn giữ font và style mặc định.

Ví dụ phát combo sau khi gameplay xác nhận:

```csharp
host.Initialize(gameCamera); // bind camera ở bước setup
host.Feedback.Play(FeedbackRequestBuilder.For(comboPopup)
    .From(this).At(matchPosition).WithAmount(comboCount).Build());
```

Ví dụ phản hồi button bằng Inspector: thêm FeedbackPlayer vào button, gán Host + ButtonConfirm + Target là RectTransform của button; nối Button.onClick → FeedbackPlayer.Play(). Không cần script mới.
