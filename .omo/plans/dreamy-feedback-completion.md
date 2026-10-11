# Plan hoàn thiện Dreamy Feedback

Ngày: 2026-10-10. Phạm vi lượt này: khảo sát, kiểm tra baseline và lập kế hoạch; chưa triển khai thay đổi package/asset.

## 1. Mục tiêu và tiêu chí sử dụng

- Người dùng import sample, kéo `GameFeedbackRig` vào scene, chọn preset và gán các anchor cần thiết là có thể dùng.
- Mỗi loại effect có ScriptableObject riêng, dùng lại ở nhiều feedback; kéo asset vào danh sách để thêm effect.
- Có override theo từng effect/definition và từng request, không sửa asset dùng chung trong lúc chạy.
- Demo thể hiện được từng effect, trạng thái thành công/thất bại, Stop/Complete, cùng một thư viện asset có phong cách thống nhất.
- Bất đồng bộ runtime dùng UniTask, có cancellation và cleanup theo owner; DOTween vẫn là adapter tùy chọn.

## 2. Baseline đã kiểm tra

| Mục | Bằng chứng / trạng thái |
| --- | --- |
| Unity | `ProjectSettings/ProjectVersion.txt`: 6000.4.12f1; MCP xác nhận đúng `D:/Game/dreamy-package-sandbox`, Windows Editor |
| Feedback | `LocalPackages/com.dreamy.feedback/package.json`: 0.3.0; package HEAD `9646c3a9ddeb0b45f4dbbb6dd0188590756c39cd`; package working tree sạch tại thời điểm khảo sát |
| Toolkit canonical | `D:/Game/dreamy-codex-toolkit/toolkit.json`: 0.1.0-alpha.2, alpha; harness tại `harness/dreamy-harness` |
| Compatibility | Registry toolkit vẫn ghi Feedback 0.1.0 và commit cũ: drift so với package đang khảo sát. Source hiện tại là bằng chứng cấu trúc, chưa coi là compatibility đã xác minh cho 0.3.0 |
| Dependency | Manifest/lock: Feedback local; UniTask Git hash `e5acc106ee196bc5a32fb14cdf2987b0f96d11e0`; package Feedback yêu cầu UniTask 2.5.10. Không suy ra version Git chỉ từ requirement |
| Rendering/UI | URP 17.4.0 trong manifest/lock, GraphicsSettings có pipeline asset; UGUI/TMP dùng trong runtime/sample. Chưa kiểm tra toàn bộ quality tier/platform |
| Runtime | 7 primitive: VFX, haptic, floating text, icon fly, screen flash, camera shake, UI punch; graph gồm Sequence/Parallel/Delay và các primitive |
| Async | Runtime đã dùng UniTask; `FeedbackHandle.Completion` và `FeedbackPlayback` vẫn dùng Task/TaskCompletionSource, runtime gọi `AsUniTask()` |
| Data | `FeedbackDefinition` giữ graph `FeedbackNode`; options phần lớn inline; VFX/text/shake database giữ entry inline. Chưa có per-effect SO hoặc base/override |
| Demo | MCP đọc được 14 channel button, không có reference button null. Chưa chạy kiểm thử pointer trong lượt này |
| Lỗi hiện tại | MCP: `FeedbackRig` và asset `GameFeedbackRig` đều có VFX `reward` prefab null và text `default` prefab null |
| Reference cụ thể | `PortableVfxDatabase.asset` trỏ fileID `4776649979299155757`, nhưng root GameObject của `RewardParticles.prefab` hiện là `8555849183882478860`; text database trỏ fileID `6888085729542182150` không tồn tại trong `RewardText.prefab`. Cả source Samples~ và sample đã import có tình trạng này |
| Console baseline | Có `VFX id 'reward' has no prefab`, `Floating text style 'default' has no prefab`, `Feedback 'Hit' skipped optional Vfx 'reward'` |
| Editor state | FeedbackDemo đang mở, `isDirty=true`, không Play Mode, không prefab stage. Không lưu/reload scene hoặc chạy test đổi scene trong lượt khảo sát |
| Kiểm tra mới | `node ../dreamy-codex-toolkit/harness/dreamy-harness asmdef .`: pass, 81 asmdefs; `package-check .`: pass. Hai gate này không chứng minh reference hay hình ảnh đúng |
| Evidence cũ | `.omo/evidence/feedback-authoring/summary.json`: 11 EditMode / 25 PlayMode pass; `.omo/evidence/feedback-starter/summary.json`: 28 sandbox / 24 consumer PlayMode pass. Đây là evidence lịch sử, không thay cho việc rerun trên revision/asset hiện tại |

Working tree sandbox có nhiều thay đổi sample/package khác. Khi triển khai phải giữ nguyên các thay đổi này; cần snapshot scene chưa lưu trước thao tác stateful. Không tự save/discard scene đang mở.

## 3. Thiết kế dữ liệu đề xuất

Chọn **per-effect SO + composite definition + library tùy chọn**. Không bắt người dùng đăng ký một effect ở cả graph lẫn ba database khác nhau.

Tên type bên dưới là API dự kiến, chưa tồn tại:

```text
FeedbackEffectAsset (base: ID, tên hiển thị, metadata)
  VfxFeedbackAsset          prefab, lifetime, prewarm, scale
  FloatingTextFeedbackAsset prefab/style, màu, duration, format
  IconFlyFeedbackAsset      sprite, trajectory, count, size, ease
  UiPunchFeedbackAsset      strength, duration, ease
  CameraShakeFeedbackAsset  amplitude, frequency, duration
  ScreenFlashFeedbackAsset  color, alpha, duration
  HapticFeedbackAsset       type, throttle

FeedbackDefinition
  Sequence / Parallel / Delay (giữ graph hiện có)
  Effect step -> FeedbackEffectAsset + override cục bộ

FeedbackLibrary (SO, tùy chọn)
  nhóm asset/preset, stable ID, lookup dùng cho demo/code
```

### Thao tác của người dùng

1. Kéo rig dùng sẵn vào scene, bind camera nếu dùng world text/shake.
2. Tạo Definition, kéo `CoinFly.asset`, `RewardBurst.asset`, `RewardText.asset`, `HudPunch.asset` vào step list; mặc định Sequence, có thể nhóm Parallel.
3. Gán Definition vào FeedbackPlayer, gán source/target HUD và Button.onClick.
4. Thêm effect mới bằng cách kéo SO vào definition. API chơi một effect riêng cũng nhận trực tiếp SO, không cần tạo composite hay thêm vào library.

Library dùng để nhóm nội dung và tra cứu ID; direct-reference playback không yêu cầu library. Editor có Add Selected Assets/Scan Folder để cập nhật library một cách rõ ràng. Runtime dùng reference được serialize, không quét AssetDatabase hay toàn bộ Resources.

### Override

- Thứ tự: preset gốc → preset variant → override trên step → override của request.
- Variant chỉ kế thừa preset cùng loại, có mask rõ cho field được override; unchecked lấy từ base. Validate chu trình và độ sâu kế thừa.
- Phase đầu ưu tiên step override và request override; thêm variant sau khi resolver/migration pass để giảm phạm vi refactor đồng thời.
- Request tiếp tục giữ amount, intensity, sprite, world position, source/target; bổ sung override kiểu dữ liệu cụ thể theo nhu cầu đã có như count/duration/color, tránh dictionary object/reflection.
- Resolver tạo snapshot riêng cho playback. Hai request cùng preset có override khác nhau phải độc lập; asset không chứa runtime Transform, token hoặc handle.
- Stable ID không dựa vào tên file/display name. Duplicate ID trong library là lỗi rõ ràng; direct reference là lựa chọn mặc định khi authoring.
- Data SO thuộc Feedback vì chứa Unity prefab/sprite/material. Không thêm dependency DataConfig/Datasave chỉ để lưu cấu hình effect. DataConfig nếu cần chỉ dùng mapping ID/tuning ở integration của game.

### Tương thích và pooling

- Giữ fields/enum/API cũ trong migration đầu; thêm effect reference cho node mới. Nếu node có SO thì dùng resolver mới, nếu không thì dùng đường inline/ID cũ.
- Editor migration có dry-run, Undo, scope rõ, không sửa `.meta` hay rewrite YAML trực tiếp. Converted definition tham chiếu SO và giữ thứ tự, Required, timing, text format.
- Primitive service nhận resolved settings mới; VFX/text được pool lazy từ reference SO. Pool key cần phân biệt prefab và các setting ảnh hưởng ownership/lifetime; limit tổng cùng limit preset, không prewarm mỗi node/request.
- ID API cũ tiếp tục hoạt động qua database cũ; không âm thầm đổi semantics. Khi ổn định, cập nhật migration guide và chọn version theo compatibility thực tế.
- Default sample, library và rig phải dùng được trong clean consumer không có Core/DOTween/asset thương mại.

## 4. UniTask và lifetime

- Tiếp tục UniTask cho delay, sequence, parallel, animation, follow, despawn và cleanup; không Task.Run cho Unity API.
- Thêm extension runtime `WaitAsync` trả UniTask để caller sử dụng thống nhất. Giữ Completion Task để tương thích và để Contracts tiếp tục không phụ thuộc UniTask trong đợt đầu.
- Đây là bridge có chủ ý, không tuyên bố đã loại hết Task/allocation. Chỉ thay public completion contract nếu có evidence và migration riêng.
- Cancellation của request dừng playback; cancellation của riêng người chờ chỉ hủy wait, không tự dừng effect của caller khác.
- Nhiều observer phải chờ cùng completion an toàn; không dùng một UniTask single-consumer cho sequence, sibling watcher và caller cùng await.
- Audit `.Forget()` và UniTaskVoid: exception phải được ghi nhận và settle Faulted; mọi terminal path release lease và settle completion đúng một lần.
- Stop không phát bước chưa chạy; Complete theo semantics hiện có hoàn tất cả bước pending đúng một lần. Destroy/disable owner không ảnh hưởng request khác.
- Test đổi scene, destroyed target/camera, timeScale=0, timeout, Stop/Complete lặp, handle cũ sau pool reuse và service reinitialize.

## 5. Kiểm tra demo

Chưa đánh dấu các channel là pass chỉ dựa trên text “Playing” hoặc handle composite Completed: optional effect có thể bị skip mà composite vẫn hoàn thành.

| Nhóm | Kiểm tra cần thực hiện |
| --- | --- |
| VFX | reward/hit có prefab hợp lệ, particle thật sự phát, material đúng, despawn/Stop release |
| Floating text | font/TMP resources, style prefab đúng component, text/amount/color, projection và cleanup |
| Icon fly | đủ 5 style: Straight/Arc/ScatterMagnet/Fountain/Spiral; source/HUD, moving target, sprite override, Count khác Amount |
| UI punch | transform thay đổi rồi restore; gọi chồng, disable target, Stop/Complete |
| Screen flash | overlay hiện đúng alpha/màu, raycast không chắn input, clear khi Stop/disable |
| Camera shake | vật world nhìn thấy bị dịch, camera restore, không chỉ UI overlay; không conflict follow rig |
| Haptic | toggle/throttle/settings; Editor hiển thị trạng thái simulated/unsupported. Android/iOS thiết bị thật để xác nhận rung |
| Composite | Reward/Hit/ButtonClick và mọi preset mới; Sequence/Parallel/Delay, Required/optional failure |
| Panel controls | click EventSystem thật cho 14 channel, 4 reward, Play All/Stop/Complete; bật/tắt/reopen không nhân listener |
| Responsive | portrait/landscape, short landscape scroll, safe area, button raycast, panel close rồi mở demo khác |

Thay switch index trong demo bằng danh sách entry serialize gồm label/preset/requirements khi authoring SO đã ổn định. Mỗi entry có target type và báo thiếu camera/HUD/prefab trước play. Trạng thái hiển thị Running/Completed/Stopped/Unavailable/Faulted; không báo thành công khi effect không phát.

Giữ preview rig và game rig riêng. Preview shake có một nền/world marker đơn giản để quan sát; game rig không mang camera/demo UI vào scene game.

## 6. Asset scope (approved update)

Keep the four existing resource icons. Add exactly seven shared effect SO: VFX, floating text, icon fly, UI punch, camera shake, screen flash and haptic. Reuse existing prefabs, materials and fonts. Reward and trajectory variations use step/request overrides instead of extra preset assets. Keep the thirteen existing composite definitions and one optional library. No new icon atlas or expanded art set.

## 7. Thứ tự triển khai và gate

| Phase | Files/owner phạm vi | Deliverable và điều kiện qua |
| --- | --- | --- |
| P0: sửa baseline | sample source `Samples~/BasicFeedbackSample/Generated`, bản import dưới Assets, validators/tests | Snapshot scene dirty; rebind bằng Unity serialization API đúng prefab/component; source và imported reference resolve; bắt lỗi missing refs ngay trước play. Fresh smoke VFX/text pass |
| P1: SO authoring | `Contracts/Composition`, data types mới, `Runtime/Composition`, VFX/text services, `Editor/Inspectors`, `Editor/Validators`, EditMode tests | Per-effect SO, direct playback, step override, snapshot; dragging asset đủ để play; inline graph cũ vẫn hoạt động |
| P2: library/variant/migration | data library, resolver, editor menu/inspectors, migration tooling | Library lookup, duplicate/cycle validation, variant override mask, dry-run migration; serialization round-trip và không mutate shared preset |
| P3: async hardening | `Contracts/Core/FeedbackHandle.cs` nếu thực sự cần, runtime service/backend/lifetime, optional DOTween sample tests | UniTask WaitAsync/exception/cancellation semantics; multi-wait, terminal statuses, Stop/Complete, stale lease, reinitialize pass |
| P4: art + preset library | sample Art/Prefabs/Presets/Generated, sample builder, README | Bộ visual thống nhất, starter rig đã nối sẵn, license đủ; clean consumer import dùng ngay, không bắt chạy builder |
| P5: demo + integration | sample controls/definitions/builder/tests; `Assets/_Project` Feedback integration chỉ khi cần | UI chọn entry SO, runtime status chính xác; real pointer coverage, visual capture portrait/landscape, Foundation close/reopen pass |
| P6: hoàn tất package | package.json/asmdefs/docs/changelog/VALIDATION, compatibility evidence | Compile + focused EditMode/PlayMode, base consumer và optional adapter consumer, player build/smoke; ghi rõ gate thiết bị chưa chạy, chọn version và cập nhật registry theo source đã xác minh |

P0 → P1 → P2; P3 dựa trên lifetime/resolver P1/P2; P4 sau schema ổn định; P5 sau asset/preset sẵn; P6 cuối cùng. Không giao sub-agent trong lượt plan này.

## 8. Validation và definition of done

Lệnh tĩnh đã chạy, cũng là gate đầu/cuối khi triển khai:

```powershell
node ../dreamy-codex-toolkit/harness/dreamy-harness asmdef .
node ../dreamy-codex-toolkit/harness/dreamy-harness package-check .
```

Các assembly gate thực thi sau khi có snapshot/fixture an toàn:

- EditMode `Dreamy.Feedback.Tests.EditMode`: resolver/override, cycle/duplicate, migration, serialization, unresolved reference và wrong component.
- PlayMode `Dreamy.Feedback.Tests.PlayMode`: primitive/composite completion, cancellation, isolation, pooling/budget và multi-observer.
- PlayMode `Dreamy.Feedback.Sample.Tests`: từng nút và observable output, all presets, source/imported consistency, starter rig với HUD của consumer.
- Optional `Dreamy.Feedback.Dotween.Tests.PlayMode`; Foundation Feedback fixture khi integration thay đổi.
- Clean consumer base không Core/DOTween/URP/asset thương mại, sau đó consumer có DOTween sample. Validate chính Sample UPM, không chỉ thư mục đã sửa trong sandbox.
- Windows development player build + smoke fail khi có exception/missing required effect. Mobile build/haptic thật ghi gate riêng, không suy ra từ Editor pass.
- Static Runtime/Contracts không `UnityEditor`; inspect asmdef graph và source vì parse JSON pass chưa chứng minh dependency direction.
- Evidence tại `.omo/evidence/feedback-completion/`: baseline, revision/hash, test XML/count, Console trước/sau, consumer import/build log, screenshot và license inventory. Test 0 count/skip không tính pass.

Hoàn tất khi: sample import sạch chạy được ngay; mỗi effect có SO dễ thêm; override cô lập; mọi nút demo có kết quả quan sát được; không mất reference/GUID; các gate bắt buộc pass trên revision cuối. “Haptic hỗ trợ thiết bị” chỉ xác nhận nếu đã đo trên thiết bị thật; nếu chưa, giới hạn được ghi rõ.

## Implementation result

Completed with the reduced asset scope. EditMode 24/24; base PlayMode 32/32; DOTween PlayMode 35/35; Windows player build/smoke pass, exit 0. See package VALIDATION.md and .omo/evidence/feedback-completion/summary.json for evidence and untested device gates.
