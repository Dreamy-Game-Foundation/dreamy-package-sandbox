# Plan cải tiến Dreamy Feedback: facade, composite và modular services

Ngày: 2026-10-09. Trạng thái: đã triển khai 0.3.0 theo phê duyệt tiếp theo; báo cáo kiểm chứng tại `LocalPackages/com.dreamy.feedback/VALIDATION.md`.

## 1. Mục tiêu và baseline

Cải tiến `com.dreamy.feedback` thành package presentation tái sử dụng. Gameplay gọi `IFeedbackService` để phát composite từ `FeedbackDefinition`, hoặc gọi primitive trực tiếp. Definition sở hữu graph; request/context chỉ mang dữ liệu runtime. Animation implementation có thể dùng DOTween nhưng core API không chứa `Tween`, `Sequence`, `Ease` hoặc namespace DOTween.

Baseline đã đọc: manifest/lock của sandbox, package manifest/asmdefs, Runtime, test hiện tại, hai sample, Foundation integration và quy tắc thiết kế local.

| Hiện trạng từ source | Hướng xử lý |
| --- | --- |
| Feedback `0.2.0`, sandbox Unity `6000.4.12f1`; dependency Core, UniTask, UGUI và engine modules | Giữ baseline engine; tách dependency theo assembly và kiểm consumer tối giản |
| Có VFX, Haptic, Floating Text, Icon Fly, Screen Flash, Camera Shake | Tái sử dụng primitive/pool hiện tại, bổ sung UI Punch |
| Chưa có `IFeedbackService`, `FeedbackDefinition`, `FeedbackRequest`, `IUiFeedbackService` | Thêm contracts và facade |
| Context chỉ có WorldPosition, Parent, FollowTarget, Source | Bổ sung target, amount, intensity và lifetime rõ ràng |
| Sequence là danh sách action; chỉ chờ delay, không chờ primitive kết thúc; mỗi Play gọi Stop lượt trước | Executor theo execution độc lập; Sequence/Parallel thực sự theo completion |
| FeedbackHandle chỉ có IsValid/Stop; IconFly.Fly trả void | Playback state và completion chung; thay API cũ bằng handle thống nhất theo quyết định bỏ legacy |
| FeedbackHost bắt buộc VFX/text/camera và có PlayReward cùng layout/icon reward hardcode | Host chỉ composition/lifecycle; module tùy chọn; reward thuộc sample/project presenter |
| Economy sample gọi sequence/text/icon riêng; Foundation gọi Host.PlayReward | Chuyển sang definition/request chung và giữ một owner cho shared-wallet feedback |
| DOTween nằm trong sandbox, không có trong dependency Feedback; cache có DOTween.dll và DOTween.Modules | Adapter riêng; kiểm compile có và không có DOTween |

Repo cha đang có thay đổi sẵn ở `FoundationDemoPanel.prefab`. Khi implementation phải giữ thay đổi đó, chỉ bổ sung integration cần thiết. Feedback submodule sạch tại thời điểm khảo sát.

Không tìm thấy `toolkit.json`, compatibility catalog và harness toolkit trong repository/vị trí sibling được chỉ ra bởi tài liệu cũ. Không coi mô tả validation Windows trước đây là chứng cứ cho môi trường Linux hiện tại. Không tìm thấy skill `omg-*` trong các thư mục skill đã kiểm tra; sử dụng Dreamy skills hiện có và source thực tế.

## 2. Ownership và dependency đích

```text
Gameplay / Presenter
  -> Contracts: IFeedbackService hoặc primitive interfaces
  -> FeedbackRequest + FeedbackContext

FeedbackService -> graph executor -> primitive services -> pool / animation backend
FeedbackDefinition -> authored nodes + preset references
FeedbackHost -> composition, root, module configuration, shutdown

Optional DOTween adapter -> Feedback contracts/runtime + DOTween
Optional Core integration -> Feedback runtime + Dreamy.Core
Editor -> Feedback runtime/contracts; Runtime không reference Editor
Economy sample/project -> Feedback + Economy/UI/Audio
```

- `Dreamy.Feedback.Contracts`: context/request, handle/state, definitions/nodes, primitive interfaces và options; không Core, UniTask, TMP hoặc DOTween. Có Unity types cần thiết cho API Unity.
- `Dreamy.Feedback.Runtime`: executor, primitive implementations, pools và host. UniTask là chi tiết implementation; public completion contract không bắt consumer phụ thuộc UniTask.
- `Dreamy.Feedback.Integration.Core`: registry scoped hiện tại; host nhận registration từ composition root. Không ServiceLocator lookup tại graph node/pool instance.
- Giữ tên assembly Runtime hiện tại cho Unity script/assets đang thuộc assembly đó khi có thể. Các interface/options chuyển assembly phải được kiểm downstream compile; mọi serialized type đổi assembly cần migration riêng.
- DOTween adapter ưu tiên UPM package riêng `com.dreamy.feedback.dotween`, có asmdef riêng, do consumer chủ động cài và inject. Cách này tránh missing assembly reference trong package base khi DOTween vắng mặt. Đối chiếu assembly/DLL thực tế trước khi khai báo dependency adapter; không sao chép tên assembly giả định.
- Backend Unity/UniTask hiện tại vẫn dùng được khi không cài adapter. Chỉ một backend điều khiển từng playback. Chọn backend lúc composition, không global toggle trong gameplay.
- Giữ Floating Text và Screen Flash. Không thêm dependency Economy/UI/Audio vào package base.

## 3. API dự kiến

Tên/signature dưới đây là thiết kế mới, chưa phải API đã tồn tại.

```csharp
public interface IFeedbackService
{
    FeedbackHandle Play(FeedbackRequest request);
    void StopAll();
}

// Ví dụ consumer: graph đã author trong asset rewardDefinition.
var request = FeedbackRequestBuilder.For(rewardDefinition)
    .From(source)
    .To(target)
    .At(worldPosition)
    .WithAmount(delta)
    .WithIntensity(1f)
    .Build();
var handle = feedback.Play(request);
```

- `FeedbackRequest`: definition, context, owner/cancellation tùy chọn. Không chứa mutable graph, pool hoặc service lookup.
- `FeedbackContext`: giữ tương thích constructor/At hiện có; thêm Target, Amount (`long` để khớp wallet), Intensity, explicit world position. Source giữ Unity Object; Target là Transform, có thể là RectTransform.
- Parent/FollowTarget giữ nghĩa hiện tại, không đánh đồng Target với FollowTarget. Context snapshot dữ liệu giá trị khi Play; target/follow reference được kiểm còn sống lúc dùng.
- World position không dùng chung ngầm cho UI coordinates. Node IconFly chuyển tọa độ qua UI root/camera explicit; target UI có thể follow hoặc snapshot theo option đã author. Sprite/preset mặc định thuộc definition, không hardcode trong builder.
- Builder chỉ dựng context/request và validate input. Không có `.AddVfx()`, `.Sequence()`, `.Parallel()` để dựng graph gameplay.
- Primitive có API playback trả `FeedbackHandle` hoặc handle tương thích có cùng completion contract. Entry point VFX/Fly/text hiện tại được giữ bằng overload/wrapper khi signature cũ cần bảo toàn. Không overload chỉ bằng kiểu trả về.
- `IUiFeedbackService.Punch(...)` trả handle. Haptic dùng operation tức thời: handle terminal ngay sau yêu cầu phát/skip; không giả lập completion vật lý của rung.
- Không đưa Initialize/database/scene lifecycle vào facade gameplay mới. Host/concrete service sở hữu setup; API legacy đã loại bỏ theo yêu cầu mới.

## 4. Definition và graph authoring

- `FeedbackDefinition` là ScriptableObject có stable ID, root node và clock policy (scaled/unscaled). Definition là dữ liệu dùng chung; executor không ghi runtime state vào asset.
- Node types: Vfx, Haptic, IconFly, UiPunch, CameraShake, Delay, Sequence, Parallel; thêm FloatingText và ScreenFlash để giữ capability đang dùng.
- Dùng typed serializable nodes qua `[SerializeReference]`, drawer chọn node type và chỉnh children trong Inspector. Chỉ các type built-in ở đợt này; chưa xây plugin executor registry tổng quát.
- Sequence chạy children theo thứ tự, child sau bắt đầu khi child trước terminal thành công/skip. Parallel khởi chạy children cùng lượt và hoàn thành khi tất cả terminal; không lấy duration lớn nhất để đoán completion.
- Delay là node độc lập. Không giữ hai loại delay có ý nghĩa chồng lấn trong API mới. Migration chuyển delay trước action thành Delay node đứng trước primitive.
- Params node tham chiếu preset/database hiện tại khi hợp lý. Binding amount/intensity/source/target có kiểu rõ ràng; không expression engine hoặc dictionary payload tùy ý.
- Intensity mặc định 1, được clamp/validate theo primitive; Amount không đồng nghĩa số icon. Definition author quy tắc map amount sang text và giới hạn icon count.
- VFX loop/manual lifetime phải có giới hạn duration trong composite; validator báo lỗi nếu Sequence sẽ chờ vô hạn. Direct primitive vẫn có thể dùng manual stop.
- Validation: root/children null, cycle, depth/node limit, ID trùng, missing prefab/preset, duration không hợp lệ, parameter không finite, target/module bắt buộc thiếu. Editor authoring phải round-trip serialization sau reload.
- Policy mặc định: module/node optional thiếu thì Skip với diagnostic; node required lỗi thì execution Faulted và stop siblings. Diagnostic có context definition/node và giới hạn spam.

## 5. Handle, execution và lifetime

- Mỗi Play có execution ID riêng và state chia sẻ để các bản copy handle quan sát cùng lượt: Running, Completed, Stopped, Faulted; invalid handle không sở hữu execution.
- Handle cung cấp state, `Stop()`, `Complete()` và completion notification/await bằng contract .NET không lộ tween. Terminal transition và callback chỉ xảy ra một lần.
- `Stop()`: hủy delay, chặn child chưa bắt đầu, stop toàn bộ child active của lượt đó, release instance thuê và restore transform/overlay. Không phát completion thành công.
- `Complete()`: bỏ thời gian chờ, finalize effect đã bắt đầu và resolve child còn lại theo thứ tự graph; one-shot chưa chạy chỉ phát một lần. Haptic đã phát không lặp; shake/punch restore base; icon về đích trước release; VFX stop/release theo preset. Không cấp reward từ Complete/callback.
- Stop/Complete sau terminal là no-op. Graph lớn có node limit để tránh Complete tạo burst không giới hạn. UI giải thích rõ Complete có thể phát one-shot chưa chạy.
- Handle composite kết thúc theo tất cả children, kể cả lỗi/cancel; primitive backend phải báo terminal kể cả tween bị kill hoặc target bị destroy. Các lỗi được quan sát tại boundary, không để task treo hoặc exception không được nhận.
- Play mới không mặc định hủy composite cũ. Có budget số execution active và số effect/module; quá budget trả terminal skip/diagnostic, không allocation vô hạn.
- Concurrency của primitive phải explicit: VFX/text/icon chạy độc lập theo lease; UI Punch cùng property dùng replace; ScreenFlash cùng overlay dùng replace; camera shake ban đầu dùng replace trên cùng rig. Child bị replace chuyển terminal Stopped; Sequence dừng theo policy cancellation, không treo chờ.
- Owner bị disable/destroy/cancellation làm stop lượt thuộc owner. Panel đóng chỉ stop handle của panel; không StopAll shared host. Scene/host shutdown stop mọi execution rồi dispose module/pool/registration.
- Pool lease dùng generation; handle cũ không điều khiển instance được reuse. Release idempotent, reset đầy đủ transform/UI state, ngăn callback cũ push lại pool sau Clear/Dispose.
- UI punch chụp base scale theo session; camera shake dùng offset rig riêng để không ghi đè camera follow. Restore chỉ khi session còn sở hữu property.
- Haptic giữ Enabled/throttle/platform fallback hiện tại. Phân biệt native pattern/intensity nằm ngoài đợt này; không quảng cáo fallback Unity có nhiều mức rung.

## 6. Sample và Foundation integration

| Nơi sửa | Hành vi/đầu ra |
| --- | --- |
| `Samples~/BasicFeedbackSample` | Button direct primitive; composite Sequence/Parallel; demo Amount/Intensity/target; Stop, Complete, simultaneous plays |
| Basic definitions | `Hit`, `Reward`, `ButtonClick`; prefab root UI/world và camera offset rig explicit; fallback backend chạy đủ demo |
| Optional DOTween sample trong adapter | Cùng facade/definition, khác backend; README cài dependency và injection; không bắt Basic import DOTween |
| `Samples~/Feedback Economy` | Presenter chỉ phản ứng committed balance delta rồi gửi request; graph author text/icon/punch/haptic/VFX; giữ audio ở presenter/adapter sample |
| `FeedbackHost` | Thêm facade/module setup; chuyển reward layout/config khỏi host sang sample definitions |
| `GameInstaller` / `FoundationFeatureDemos` | Register facade cùng primitive; project presenter/config giữ reward definition, source/target; một owner cho shared-wallet playback |
| `Assets/Samples/Dreamy Feedback/...` | Đồng bộ source sample đang import để sandbox thực sự dùng API mới; cập nhật generated assets qua Unity |
| Builders / tests / docs | Sinh definitions, wire refs mới, validate dependency và giữ workflow import/rebuild an toàn |

Reward sample graph dự kiến: Sequence(Parallel(VFX, Haptic, FloatingText), IconFly, UiPunch). Basic thêm graph có Delay rõ ràng để minh họa thứ tự và cancellation.

Các ràng buộc sample:

- Retry transaction không cấp/replay reward lần hai; reopen panel không nhân subscription.
- Panel dùng shared host không tự shutdown host; hủy request của panel khi UI target mất. Feedback bootstrap có target của bootstrap, không lấy anchor của panel đã đóng.
- Giữ fix canvas scale/anchors/sorting, back/close và pointer raycast hiện tại.
- Source phát hành là `Samples~`; sửa generated prefab/scene bằng Unity, bảo toàn GUID/reference. Không nối YAML bằng tay.
- Basic không phụ thuộc Economy/UI/Audio, render pipeline cụ thể hoặc DOTween. Giữ font/TMP prerequisites hiện tại và kiểm portrait/landscape.

## 7. Migration và phiên bản

- Phiên bản implementation `0.3.0`, breaking change theo yêu cầu không giữ legacy.
- Xóa `IFeedbackSequenceService`, sequence databases/services và `Host.PlayReward`; chuyển call sites sang definition/request. Không cung cấp deprecated bridge hoặc conversion legacy.
- Giữ GUID scene/prefab/database primitive, nâng cấp asset qua Unity serialization và kiểm reference. Definitions mới có test serialization round-trip.
- Common handle do service tạo; gameplay chỉ dùng playback state/Stop/Complete.
- Cập nhật README, getting-started, migration guide, CHANGELOG và VALIDATION. Không tái sử dụng evidence cũ để ghi pass cho revision mới.
- Feedback là submodule: giữ diff package và repo cha riêng, báo revision/gates chính xác. Không commit/push/tag trong bước lập plan.

## 8. Thứ tự triển khai và tiêu chí hoàn thành mỗi bước

| Bước | Công việc | Gate trước bước kế |
| --- | --- | --- |
| 1. Baseline/contract | Ghi API usage/asset refs; characterize legacy sequence và cancellation; thêm contracts context/request/state; chuyển toàn bộ consumer | Compile consumer hiện tại; không mất serialized refs |
| 2. Primitive lifetime | Chuẩn hóa completion và Stop/Complete; handle icon group; UI Punch; budget/lease reset | Test stale handle, reuse, target destroy, repeated terminal calls |
| 3. Graph executor | Definition/nodes, execution độc lập, Sequence/Parallel/Delay, failure/owner policies | Deterministic scheduler tests chứng minh thứ tự, parallel join và cancellation |
| 4. Host/integration | Composition module tùy chọn, registry Core adapter, facade registration; di chuyển reward glue | Host với chỉ một module hoạt động; initialize/shutdown lặp lại; replacement registration an toàn |
| 5. DOTween adapter | Backend implementation, dependency/asmdef, lease-safe kill/complete, explicit injection | Consumer sạch có DOTween và consumer sạch không DOTween đều compile/chạy |
| 6. Editor/migration | Drawers, validators, ID generation, asset upgrade bảo toàn GUID | Definition serialization round-trip; lỗi config rõ; primitive assets giữ GUID |
| 7. Samples/project | Basic/Economy/optional adapter sample; builders; imported sandbox và Foundation | Import mới, PlayMode user flow, retry/reopen/close/raycast, stop shared owner đúng |
| 8. Docs/final review | Docs/migration/version proposal, diff và evidence | Ghi pass/unavailable chính xác; không Runtime -> Editor; không DOTween trong core API |

Không xây graph editor dạng canvas, asset streaming/Addressables framework, native haptic SDK hoặc thêm DI framework trong đợt này. Performance cải tiến tập trung pool, active budget và tránh task/closure vô hạn; đo allocation trước khi tối ưu sâu.

## 9. Validation cần chạy

1. **Static đầu tiên:** JSON/asmdef parse; dependency cycle; Runtime -> Editor; core -> DOTween; sample declarations/file refs; git diff whitespace. Dùng harness nếu tìm được, nếu không ghi rõ script static thay thế và phạm vi kiểm được.
2. **EditMode:** context/request builder; validation tree; serialization round-trip; definition graph round-trip; ID generation. Executor test dùng controlled completion và fake clock, không chỉ assert đúng tên type.
3. **PlayMode lifecycle:** Sequence chờ child; Parallel join; delay cancellation; concurrent Play độc lập; Stop/Complete exactly once; graph lỗi stop siblings; owner destroy; stale pool/tween callback; replace same target; looping VFX guard; budget cap; shutdown/reinitialize.
4. **Samples/integration:** direct/composite controls; amount/intensity/target; target destroy; reward retry/reopen không duplicate; panel đóng không stop shared feedback khác; canvas/back/close/raycast regression; no missing scripts/refs; Console không error.
5. **Clean consumers:** base + Basic không DOTween; base + adapter + DOTween; Economy với dependency đã document. Test core/contracts assembly không bắt buộc Core/TMP/UniTask; runtime hiện tại vẫn có dependency đúng khai báo.
6. **Player:** desktop development build/smoke từ consumer mới; TMP/asset references và adapter link validation. Android/iOS/haptic thật chỉ đánh dấu pass khi có build/device evidence; Editor không chứng minh rung vật lý.

Evidence mới đặt ở `.omo/evidence/feedback-modular/`, kèm environment/revision và gate thực sự chạy. Nếu Unity/MCP/batchmode unavailable, chỉ báo static result và chính xác gate còn thiếu.

## 10. Definition of done cho implementation

- Gameplay sample gọi facade từ definition/request hoặc primitive trực tiếp; không effect graph hardcode trong builder/host.
- Sequence/Parallel đúng completion; Play độc lập; handle Stop/Complete và owner cleanup không leak, không ảnh hưởng pooled lease khác.
- Base package chạy không DOTween; adapter có kiểm chứng consumer riêng; API core không lộ tween types.
- Host hỗ trợ module tùy chọn; package không sở hữu wallet/reward/save/navigation.
- Cả sample source, imported sandbox, Foundation integration và migration/docs khớp API mới, qua gate phù hợp ở trên.
- Phân biệt implementation đã kiểm chứng và gate chưa chạy; không tuyên bố release/mobile support chỉ từ static checks.

## 11. Kết quả triển khai

- MCP đã xác nhận đúng project Linux trước khi thao tác; source/sample/prefab/scene được nâng cấp bằng Unity.
- Adapter nằm tại `LocalPackages/com.dreamy.feedback.dotween`, tách khỏi base; core API không chứa DOTween types.
- Basic có 5 trajectory casual/puzzle, composite reward/hit/click, Stop/Complete; board world-space giúp thấy camera shake. Economy và Foundation dùng facade/context mới.
- Sandbox: 29/29 PlayMode, 4/4 EditMode; kiểm lại Basic 2/2 sau thay material/shader portable.
- Consumer sạch không Core/DOTween/URP: 22/22 PlayMode, Linux build và error-sensitive player smoke thành công.
- Evidence mới tại `.omo/evidence/feedback-modular`; kết quả consumer DOTween và giới hạn thiết bị được ghi trong VALIDATION.

## 12. Điều chỉnh sample theo phản hồi tiếp theo

Bỏ Economy sample và toàn bộ puzzle tiles. Tách prefab GameFeedbackRig dùng trong game khỏi rig preview; thêm FeedbackPlayer, 4 reward presets và sprite override. Xóa one-time upgrade/generator tooling. Giữ font/confetti mới trong sandbox; source portable dùng asset mặc định có license kèm theo. README tập trung thao tác import và HUD integration. Gates mới: sandbox 28/28, base consumer 24/24; xem VALIDATION cho evidence hiện tại.

## Điều chỉnh đóng gói DOTween — 2026-10-10

Theo yêu cầu mới, quyết định package adapter riêng ở trên được thay bằng sample `Samples~/DOTweenFeedbackSample` trong `com.dreamy.feedback`. Consumer cài DOTween rồi chủ động import sample; giữ GUID và tên assembly. Package base không thêm dependency DOTween. Sandbox dùng bản sample import trong Assets; bỏ package adapter khỏi manifest/testables.
