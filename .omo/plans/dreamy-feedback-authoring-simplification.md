# Dreamy Feedback — đơn giản hóa authoring

Status: implemented and validated. Final evidence is recorded below and in package VALIDATION.md. Earlier decisions remain as the implementation specification.

## 1. Mục tiêu và quyết định

- SO effect chỉ chứa cấu hình của đúng loại effect. Bỏ Base Asset, Replace Settings và Overrides khỏi Inspector và schema authoring mới của effect SO.
- Definition là bộ phát: chọn Play Mode, kéo asset vào một Effects list. Không phải tự chọn loại node rồi gán thêm effect.
- Bỏ hoàn toàn override trong schema SO mới, gồm effect và Definition step. Chỉ code request hoặc component scene/prefab được tùy chỉnh lúc phát.
- Inspector mặc định chỉ hiện dữ liệu cần thiết. Timing và Required nằm sau nút tùy chọn của từng step; ID tự tạo nằm trong Advanced.
- Một effect có thể phát trực tiếp, không cần tạo Definition, Library hoặc database.
- Lookup bằng ID dùng database riêng theo loại, theo chốt mới nhất; không chuyển các service sang Library chung. Reference trực tiếp vẫn được hỗ trợ.
- Lookup IDs from rig chuyển khỏi Inspector thông thường; chỉ xuất hiện trong cửa sổ migration khi dữ liệu legacy thật sự cần tra ID.

## 2. Model dữ liệu đề xuất

### Effect SO

Bảy loại hiện tại giữ nguyên tên class và script GUID: VfxFeedbackAsset, FloatingTextFeedbackAsset, IconFlyFeedbackAsset, UiPunchFeedbackAsset, CameraShakeFeedbackAsset, ScreenFlashFeedbackAsset, HapticFeedbackAsset.

Mỗi asset có Settings đúng loại, ID tự tạo và validation. Không chứa scene anchor, runtime handle, base preset hay override mask. Muốn preset độc lập khác thì Duplicate rồi chỉnh settings; sample không tạo thêm preset mới.

### Definition

Thêm base chung `FeedbackAsset : ScriptableObject` cho Effect và Definition để một ô Asset dùng được cả hai loại. Đây là API đề xuất, chưa tồn tại trong source hiện tại.

Chốt theo trao đổi bổ sung: base chung giữ stable ID, tự tạo ID, metadata/validation dùng chung và contract tạo snapshot playback. FeedbackEffectAsset vẫn là base cho bảy effect cụ thể, chứa logic resolve settings dùng chung. Không đưa pool, service instance hoặc state playback vào SO. Việc bỏ Base Asset là bỏ field tham chiếu inheritance giữa preset, không bỏ base class C#.

~~~text
FeedbackDefinition
  PlayMode: Sequence | Parallel | Staggered
  Effects: List<FeedbackStep>
  UnscaledTime: true
  StaggerInterval: chỉ hiện khi chọn Staggered

FeedbackStep
  Asset: FeedbackAsset
  Advanced:
    DelayBefore: 0
    Required: true
~~~

Inspector một row bình thường chỉ có Asset, nút reorder/remove và nút Settings. Multi-drag tạo nhiều row và tự nhận effect type. Tạo row chưa có asset phải báo rõ là chưa hoàn chỉnh; không lấp bằng một default effect tùy ý.

List có thể nhận Definition con khi cần ghép nhóm. Đây là reference tới asset khác, không mở lại cây node nhiều field trong Inspector. Chặn chu trình và giới hạn graph; một asset được dùng lại ở nhiều step vẫn hợp lệ và resolve thành snapshot riêng.

### Timing rõ nghĩa

| Mode | Hành vi |
| --- | --- |
| Sequence | Chờ DelayBefore của step, phát step, chờ hoàn thành rồi chuyển step kế tiếp |
| Parallel | Step i bắt đầu tại thời điểm definition bắt đầu + DelayBefore[i]; chờ mọi step hoàn thành |
| Staggered | Step i bắt đầu tại thời điểm definition bắt đầu + i × StaggerInterval + DelayBefore[i]; không chờ step trước kết thúc |

“Song song” và “cùng lúc” là cùng một Parallel mode. Delay là timing, không phải một loại effect. Sequence muốn dừng giữa hai effect thì đặt DelayBefore trên step sau. Muốn trì hoãn cả nhóm thì đặt delay trên step chứa Definition con. StaggerInterval/DelayBefore phải hữu hạn và không âm; timing unscaled kế thừa từ request gốc cho cả nhóm.

Definition rỗng hoặc row thiếu asset bị validation từ chối. Stop, Complete, owner disable/destroy và cancellation giữ nghĩa hiện tại; Complete bỏ qua delay đang chờ và hoàn tất phần việc còn lại theo contract hiện có.

## 3. Settings tĩnh và dữ liệu runtime

- Effect SO không có cấu hình override hoặc inheritance.
- Definition step chỉ chứa Asset/DelayBefore/Required; không có override settings.
- FeedbackPlayer: Override Settings mặc định tắt; chỉ hiện các field thích hợp cho asset đơn. Context động như amount/icon/source/target được giữ riêng.
- Code: giữ `WithOverrides` và `WithIcon`. Với Definition nhiều loại, mask request chỉ áp dụng cho loại tương ứng; tài liệu phải nói rõ đây là override toàn request.
- Thứ tự: effect settings → component/request customization → context runtime. Không bao giờ ghi ngược vào asset dùng chung.
- Resolver chụp cả nhóm trước khi chạy, kể cả step đang chờ delay; hai lượt phát không chia sẻ state.

### IconFly và các giá trị chỉ biết lúc play

SO IconFly chỉ lưu default icon/count, duration, trajectory, size, easing và các thông số visual. Không serialize Source/Target/Parent scene hoặc positions trong SO. Tách DTO authoring settings khỏi IconFlyOptions playback hiện tại để field runtime không bị che nhưng vẫn nằm trong dữ liệu asset.

Runtime dùng IconFlyPlayArgs đề xuất: Source, Target, Icon tùy chọn, Count nullable, Owner và Cancellation. Count=null dùng preset; Count=0 hoàn thành không spawn; Count âm hoặc >64 bị từ chối. Icon không truyền dùng default icon; thiếu cả hai báo lỗi. Source/Target thiếu bị từ chối trước spawn. Snapshot sprite/count khi play; vị trí đích có thể cập nhật theo Transform theo policy backend hiện có. Destroy anchor hoặc owner dọn toàn lượt phát.

Amount là số thưởng thực tế dùng cho text, Count là số icon visual; không coi 100 tiền là 100 icon. Các giá trị này độc lập. WorldPosition/FollowTarget cho VFX, amount/custom text cho FloatingText và camera binding cho shake cũng được truyền runtime theo loại.

API đề xuất: iconFlyService.Play("icon.reward", new IconFlyPlayArgs { Source = sourceAnchor, Target = hudAnchor, Icon = coinSprite, Count = 8 }); ID chỉ lấy preset từ IconFlyDatabase, PlayArgs cung cấp dữ liệu của lượt gọi. Không cần lấy SO trước hay sửa SO.

Facade giữ FeedbackContext và thêm typed IconFly args/count khi cần; WithAmount hiện tại không tự chuyển thành Count. Context của một Definition được truyền tới các effect phù hợp. Nếu hai step cùng loại cần target/count khác nhau, component/code cung cấp binding theo path/index step của snapshot; không thêm override vào Definition SO. Step binding là tùy chọn advanced, validate path và type trước khi chạy.

## 4. Chỉ phát một effect

API hiện có, tiếp tục hỗ trợ:

~~~csharp
var handle = host.Feedback.Play(
    FeedbackRequestBuilder.For(iconFlyEffect)
        .From(this)
        .IconsFrom(sourceAnchor)
        .To(targetAnchor)
        .WithIcon(coinSprite)
        .WithAmount(100)
        .Build());

await handle.WaitAsync(cancellationToken);
~~~

Code async dùng UniTask; cancellation của WaitAsync chỉ hủy lượt chờ. CancelWith trên request mới hủy playback.

Inspector đề xuất: FeedbackPlayer có một ô `Asset`, nhận effect hoặc Definition. Gán effect rồi nối Button.onClick → Play() là đủ; không có hai ô Effect/Definition cùng trống. Host và các anchor chỉ hiện theo loại feedback; camera/world/UI target vẫn là dependency cần thiết, không tự tìm toàn scene mỗi lần play.

### Gọi qua primitive service

Chốt lookup theo ID: database riêng theo loại (VFX, FloatingText, IconFly, UiPunch, CameraShake, ScreenFlash, Haptic), chứa danh sách reference SO tương ứng. Database đọc ID từ SO, không yêu cầu khai báo lại ID/settings trong từng entry. Definition có database riêng chỉ khi cần gọi Definition bằng ID; không bắt buộc khi dùng reference.

Host gán các database một lần lúc setup. Mỗi service tạo dictionary của riêng nó khi initialize; không quét Resources/AssetDatabase hoặc lookup toàn scene lúc Play. Các loại không dùng có thể để trống, Inspector chỉ hiển thị cấu hình của loại đã bật. Không tạo sẵn mọi database nếu sample/consumer không cần lookup loại đó.

VfxService.Play("vfx.hit", position) resolve từ VfxDatabase; các service khác tra database đúng loại qua overload ID đề xuất. Missing/duplicate ID và sai loại phải báo rõ; duplicate trong cùng database bị từ chối. ID có thể trùng giữa các loại vì lookup có service cụ thể. Facade Play(id) chỉ tra DefinitionDatabase; effect đơn gọi qua service đúng loại hoặc request reference, tránh lookup mơ hồ giữa nhiều database.

Database legacy VFX/text đang lưu entry inline: thêm schema reference SO, migration có preview và version marker; giữ adapter legacy trong giai đoạn chuyển. Không đổi GUID, không ghi lại settings hai nơi. FeedbackLibrary hiện có giữ cho tương thích/tooling tùy chọn, không là dependency lookup chung mới.

Giữ đường gọi primitive độc lập. IVfxService hiện có Play(string id, position/options), dùng database ID; ID tự tạo trên SO không tự đăng ký vào database. VfxService cũng triển khai IVfxPresetService.Play(VfxFeedbackSettings, VfxPlayOptions), nên direct settings không cần database.

Bổ sung extension overload trên IVfxService để nơi gọi viết `vfx.Play(vfxAsset, VfxPlayOptions.At(position))`. Extension resolve/validate snapshot từ SO rồi forward qua IVfxPresetService, không sửa interface ID-only cũ. Implementation thiếu capability phải trả handle Faulted với lỗi rõ ràng; không tự đăng ký database hoặc im lặng fallback. Shared typed resolver được dùng bởi facade và primitive overload để override/validation không lệch nhau.

VFX/text có direct preset capability hiện tại; các service khác cần kiểm tra signature và bổ sung extension tương ứng khi phù hợp. Primitive Play trả handle do caller sở hữu: caller Stop/Complete hoặc gắn cancellation theo lifecycle. Facade FeedbackRequest vẫn là đường dùng cho owner cancellation tự động và composition. Không tuyên bố primitive overload có toàn bộ owner policy của facade.

## 5. Folder sample

~~~text
Generated/
  Effects/
    Vfx/RewardVfx.asset
    FloatingText/RewardText.asset
    IconFly/RewardIcons.asset
    UiPunch/HudPunch.asset
    CameraShake/HitShake.asset
    ScreenFlash/ScreenFlash.asset
    Haptic/LightHaptic.asset
  Definitions/
    Rewards/       # Coin, Star, Energy, Gem
    UI/            # ButtonClick, ButtonConfirm, ButtonReject
    Gameplay/      # Hit, ComboPopup, LevelComplete, RewardPop
    Screen/        # SoftShake, SuccessFlash
  Databases/       # database riêng theo loại; chỉ tạo loại cần lookup ID
  Libraries/       # library hiện có nếu cần giữ compatibility/tooling
  Rigs/           # prefab reusable hiện có
  Scenes/FeedbackDemo.unity
~~~

Giữ art/icon/font tại folder hiện có; không thêm icon hoặc nhân bản prefab. Di chuyển asset bằng AssetDatabase.MoveAsset cùng meta để giữ GUID. Update builder/test/docs theo path mới. Bản import sandbox giữ art/font và working copy người dùng; không đồng bộ đè toàn folder từ source.

## 6. Migration và tương thích

Source hiện tại lưu Root bằng SerializeReference và có override/variant. Không xóa dữ liệu này trước khi chuyển đổi.

1. Snapshot bản source/import và working copy đang dirty. Ghi inventory GUID/reference.
2. Thêm schema mới và version marker. Giữ Root legacy ẩn và adapter playback trong giai đoạn chuyển đổi; definition mới chỉ author bằng list.
3. Bake settings variant đã resolve vào effect asset tương ứng rồi bỏ inheritance. Preserve asset GUID; dry-run báo cycle, missing asset, type mismatch trước khi ghi.
4. Chuyển Sequence/Parallel/Delay sang mode/list/DelayBefore. Nhóm lồng nhau cần Definition con đặt riêng dưới Definitions/Groups, chỉ tạo khi dữ liệu thật sự có nhóm; không nhân bản cả thư viện. Không flatten nếu làm đổi thời điểm phát hoặc completion.
5. Không đưa step override cũ vào schema SO mới. Preview phân biệt dữ liệu runtime (chuyển ra component/demo binding) và khác biệt preset tĩnh (cần bake thành preset độc lập tối thiểu nếu muốn giữ đúng visual). Không âm thầm bỏ khác biệt hoặc ghi đè 7 preset chung. Các graph chưa chuyển được giữ trong adapter legacy ẩn; gate phải ghi rõ phần chưa migrate. Mục tiêu sample vẫn 7 effect SO/4 icon, các biến thể demo truyền từ component/code. Inline legacy chuyển bằng tooling có preview, Undo và phạm vi rõ ràng.
6. FeedbackPlayer field definition cũ được chuyển sang ô Asset qua migration serialized, không mất Button UnityEvent liên kết tới Play().
7. Lookup ID chỉ trong migration window, chỉ yêu cầu rig khi legacy VFX/text ID cần resolve. Asset mới không thấy field này.
8. Không auto migrate mọi asset trong OnValidate. Báo schema legacy và mở migration tool khi cần. Chưa bỏ adapter legacy/API cũ cho đến khi source sample, consumer và sandbox đã kiểm chứng; nếu muốn loại bỏ hoàn toàn thì ghi breaking change/version riêng.

## 7. Thứ tự triển khai

| Bước | Phạm vi | Gate |
| --- | --- | --- |
| P0 | Inventory, migration preview, snapshots | Có kế hoạch chuyển từng graph mà không đổi timing/reference |
| P1 | Contracts base asset, mode/list/step, effect settings | SO không base/override; validate schema mới và cũ |
| P2 | Resolver/FeedbackService, UniTask scheduling | Sequence/Parallel/Staggered đúng timing, snapshot và lifecycle |
| P3 | Definition/Effect Inspector, FeedbackPlayer Inspector | SO không override; kéo asset đủ dùng; runtime bindings đúng loại |
| P4 | Migration, sample folder moves, builder/demo/library | 7 effect SO / 4 icon; 13 definition giữ hành vi; source/import refs đúng |
| P5 | Tests, clean consumers, docs/changelog/validation | Compile, focused tests, base/DOTween consumer, Windows build/smoke |

Files tham chiếu: Contracts/Composition/FeedbackDefinition.cs, FeedbackEffectAsset.cs, FeedbackGraphResolver.cs, FeedbackGraphValidation.cs; Contracts/Core/FeedbackRequest.cs; Runtime/Composition/FeedbackService.cs; Runtime/Core/FeedbackPlayer.cs; Editor/Inspectors/FeedbackDefinitionEditor.cs, FeedbackEffectEditor.cs; Editor/Generators/FeedbackDefinitionMigration.cs; Samples~/BasicFeedbackSample/Editor/FeedbackStarterAuthoring.cs.

## 8. Nghiệm thu

- Inspector và schema SO mới không còn Base Asset, Replace Settings hoặc Overrides, gồm cả Definition steps. Lookup IDs chỉ thuộc migration tool. Definition mới chỉ hiện mode/list và settings cần cho mode.
- Phát effect đơn bằng code và component không cần wrapper Definition. Required dependency bị thiếu phải báo đúng lỗi.
- Lookup ID mỗi loại đi qua database riêng, không cần Library chung. Thêm SO vào list database là đủ, không nhập lại ID/settings; duplicate/missing ID và facade chỉ tra Definition được kiểm tra bằng fixture.
- Kiểm tra timing bằng runtime event trace: Sequence chờ completion; Parallel cùng lúc; Staggered đúng offset; delay per-step và nhóm con giữ thời điểm.
- Hai request với override khác nhau không mutate SO; black/zero vẫn là override hợp lệ. Đổi asset giữa lúc delay không ảnh hưởng snapshot.
- Stop/Complete/cancel/owner destruction trong delay và nhóm đang chạy không leak object, task hoặc tween; nhiều caller chờ cùng handle được.
- Migration giữ GUID, UnityEvent, settings, timing, Required, unscaled và hành vi visual; chuyển override cũ thành dữ liệu component/runtime hoặc preset settings, không giữ override trong schema SO mới. Preview không ghi file; run lần hai không nhân bản asset. Bao gồm fixture graph lồng và variant legacy.
- IconFly: preset defaults và runtime args theo ID; Count=null/0/negative/over-budget; Amount độc lập Count; missing/destroyed anchors; hai lượt phát dùng sprite/count khác nhau không mutate preset.
- Demo real raycast đủ 14 channel, 4 reward, Play All/Stop/Complete/reopen; portrait/landscape vẫn đọc được.
- Run focused EditMode/PlayMode, clean base + optional DOTween, Windows build/player smoke. Counts 24/32/35 của lượt trước chỉ là baseline, không dùng làm bằng chứng refactor mới.
- Harness asmdef/package-check và Runtime/Editor boundary pass; mobile haptic/device performance ghi rõ chưa đo.

Baseline plan: manifest/lock đã inspect; toolkit 0.1.0-alpha.2, package source local 0.3.0 có Unreleased changes; compatibility registry không chứng minh bản chưa commit. Package-check pass trong lượt plan. Chưa sửa code hoặc chạy lại Unity tests trong lượt này.


## Implementation evidence

Implemented the final agreed schema: no SO overrides; shared ID base; mode/list Definitions; separate ID databases; typed runtime IconFly args. EditMode 26/26, base PlayMode 35/35, DOTween PlayMode 38/38; Windows build/player smoke pass, exit 0. See package VALIDATION.md and .omo/evidence/feedback-simplification/summary.json. Legacy graph reader retained; unsupported legacy delay layouts are reported for explicit redesign.
