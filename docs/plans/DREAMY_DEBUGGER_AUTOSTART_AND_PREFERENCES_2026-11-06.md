# Dreamy Debugger — tự khởi động và ghi nhớ runtime settings

Ngày: 2026-11-06. Trạng thái: **plan trước implementation**. Baseline đã commit: package `d8711e7` (preview.3), sandbox `8038fd2`.

## 1. Kết quả người dùng cần nhận

**Project mới:** import package → Play một scene bất kỳ → tap góc trên trái 5 lần → nhập 0000 → dùng panel. Không bắt buộc import sample, thêm bootstrap, tạo config, rebuild visuals hay setup theme.

**Chỉnh HUD:** bật Compact, chọn góc hoặc kéo vị trí, chỉnh scale/memory/audio → thoát Play hoặc đóng app → lần sau giữ các lựa chọn đó. Off là mặc định của lần đầu/reset, không phải lệnh ép tắt ở mọi lần chạy.

**Game riêng:** partial container vẫn do game sở hữu và đăng ký sau khi service game ready. Tự có panel không đồng nghĩa package tự biết wallet, save, ads hay cheat của game. Quick Start chỉ là tài liệu/example tùy chọn.

**Project cũ:** giữ hoạt động của bootstrap/config/variant và ID đã có; không tạo runtime trùng hoặc vô hiệu ý định Manual/Off. Không thay dependency Core/UI hoặc chuyển quyền sở hữu save vào debugger.

## 2. Hiện trạng đã kiểm tra

- `Runtime/Imported/Presentation/Scripts/AutoInitialize.cs` của Dreamy còn hàm khởi tạo nhưng không còn attribute auto-start; Runtime/Engine/DebuggerRuntime hiện cần component và catalog do bootstrap cung cấp.
- `PresentationAssets.Load` chỉ lấy asset từ catalog, không có loader Resources tự phục hồi. Chỉ gọi lại DreamyDebug.Init không đủ để thay thế bootstrap.
- Config được clone khi runtime tạo; không ghi runtime mutation vào SO gốc.
- HUD mode, position và free offset hiện là session state. SetPosition/SetMode/ResetPosition và drag có nhiều entry point; lưu ở mỗi UI callback sẽ bỏ sót API hoặc ghi nhầm state tạm.
- Runtime builtins hiện có HUD mode, position, reset và Build; cần bổ sung các setting HUD được hỗ trợ thực sự, không chỉ hiển thị field không có effect.
- Screenshot tạm ẩn HUD/panel rồi phục hồi. Đây là transient presentation state, không phải lựa chọn cần ghi nhớ.
- Preview.3 đã có lease/notification, batched structural updates và fallback polling. Cần giữ các contract đó.
- SR tham khảo dùng auto-initialization, settings mặc định và runtime asset loading: https://stompyrobot.uk/tools/srdebugger/documentation/#configuration.

## 3. Nguyên tắc và phạm vi lưu

### 3.1. Thứ tự ưu tiên

1. **Policy/giới hạn của project và build** luôn có quyền cuối: Mode Off, Manual startup, release restrictions, capture/upload policy, access.
2. Config project cung cấp giá trị mặc định cho các field hợp lệ.
3. Preferences cục bộ ghi đè các field UI nằm trong allowlist.
4. Thao tác user/API trong session có ưu tiên trên kết quả restore đang tải. Không ghi đè thao tác mới bằng file cũ vừa load xong.

Preferences không được bật cheat UI/HUD trong build không cho phép, tự unlock hoặc vượt giới hạn config. Không dùng package version hoặc buildGUID làm namespace file khiến mọi lần update đều mất settings.

### 3.2. Ma trận persistence

| Dữ liệu | Chính sách |
|---|---|
| HUD Off/FPS Only/Compact/Detailed | Lưu sau thao tác user/API lâu dài; lần đầu mặc định Off |
| Góc HUD, Free position | Lưu; vị trí kéo chuẩn hóa theo vùng safe area, không lưu pixel tuyệt đối |
| HUD scale, memory/audio visibility, drag enabled | Lưu; validate/clamp rồi áp vào renderer/sampler thật |
| FPS good/caution thresholds/colors; low-FPS warning settings | Lưu khi được expose thành setting runtime; validate thứ tự/ngưỡng/sustain/cooldown |
| Console severity filters, collapse mode | Lưu; restore qua service, không tạo notification/save vòng lặp |
| Category collapse và danh sách option pin | Lưu ID/trạng thái UI, không lưu giá trị option. Resolve pin khi option đăng ký; có bounds/cleanup |
| Search text, selected log, stack trace, scroll, trạng thái drawer | Session-only; không giữ text nhạy cảm hoặc khôi phục panel đang mở |
| PIN/passphrase, unlock/session token | Không thuộc preference DTO; không tự unlock |
| Relay URL, bot/mail token, limited key, screenshot/upload/consent policy | Không ghi vào local UI preferences; giữ đúng owner config/server và policy hiện có |
| Time Scale, Target FPS của game, God Mode, balance, grant amount, entitlement | Không tự persist/replay như UI settings. Game sở hữu state/save và quyết định riêng |
| Log/report queue | Giữ cơ chế outbox hiện có; Reset Preferences không xóa report hoặc save game |

Không serialize toàn bộ DebuggerConfiguration hoặc mọi option trong registry để "lưu settings". Dùng DTO allowlist/versioned rõ ràng.

## 4. Kiến trúc dự kiến

Tên class/API mới dưới đây là thiết kế đề xuất, chưa phải API có sẵn.

### 4.1. Một đường khởi tạo

- Project startup setting có **Automatic / Manual**, mặc định Automatic. DebuggerMode.Off vẫn là policy tắt, không tạo thêm toggle Disabled chồng chéo.
- Engine có một coordinator, ví dụ `DebuggerBootstrap.Initialize(...)`, dùng chung cho auto hook và explicit API. Idempotent và main-thread owned.
- Auto hook chạy khi game Play/start. Thiết kế ưu tiên khởi tạo đầy đủ sau scene đầu để nhận biết bootstrap authored; nếu cần console trước scene, dùng bounded early log buffer riêng, không tạo UI/service trùng.
- Phát hiện cả bootstrap inactive/disabled/Mode Off, không chỉ kiểm tra Instance khác null. Không tự tạo fallback rồi vô tình bật lại setup bị game tắt.
- Khi manual bootstrap xuất hiện muộn hơn automatic runtime, cần handover có teardown/generation rõ ràng trước khi manual owner khởi tạo. Không Destroy user GameObject chỉ vì duplicate; chỉ dọn root do package tự sở hữu. Đường legacy duplicate phải được review và test migration.
- Reset trạng thái khi Enter Play với domain reload/scene reload bật hoặc tắt. Không giữ static provider/runtime cũ, listener trùng hoặc khôi phục access đã unlock.

### 4.2. Cấp assets mà không kéo UI vào mọi release build

Không đặt catalog tham chiếu toàn bộ panel/font/shader vào Resources luôn ship, vì sẽ làm mất lợi ích release stripping.

Phương án ưu tiên để spike/kiểm chứng:

- **Editor:** provider thuộc Editor assembly dùng AssetDatabase để cung cấp catalog/config defaults cho Engine; Engine không reference UnityEditor. Không cần tạo asset trong project chỉ vì import/Play.
- **Player:** build adapter tạo payload Resources nhỏ, game-owned và tạm thời. Development/QA payload mới được tham chiếu UI catalog; ReleaseDiagnostics chỉ có config/text policy, catalog null; release mặc định không tạo UI payload.
- Missing custom config dùng defaults hợp lệ; chỉ tạo config project khi user chủ động sửa/tạo. Sample không tham gia boot mặc định.
- Build staging có marker/ownership, cleanup ở success/failure và recovery sau Editor crash; không rename thư mục package, không sửa GUID hoặc asset package để bật/tắt build.
- Chỉ dùng Editor build hook/API sau khi xác minh trên Unity 6000.4.12f1. Spike phải chứng minh cả build UI lẫn build stripping trước khi coi phương án này chốt. Nếu API build context cho phép đưa temporary asset vào build an toàn hơn, chọn phương án đó và ghi bằng chứng trong plan/checkpoint.

Không thay thế bootstrap bằng một Resources asset luôn kéo UI vào release. Không tạo prefab/config thật hàng loạt lúc import để giả trải nghiệm zero-setup.

### 4.3. Một owner cho runtime preferences

- Domain DTO/schema + validation/merge ở phần C# thuần; Engine adapter nhận user intent, load/save và áp state lên HUD/presentation.
- Authoring config, desired preference state và transient display state tách riêng. Config clone không trở thành file persistence.
- API đổi setting lâu dài và control runtime đi qua cùng owner. Giữ overload/signature cũ tương thích; nếu cần thêm API, ưu tiên SetHudPosition/SetHudScale/ResetLocalPreferences để hỗ trợ cả lúc HUD chưa được tạo.
- SetHud Off vẫn ghi được lựa chọn khi Hud == null. Chọn vị trí khi HUD Off không cần khởi tạo renderer.
- Configure, restore, Fit/safe-area adaptation, capture hide/restore và teardown không phát sinh save như một thao tác user.
- Public HUD methods đang có cần được review: thông báo thay đổi về owner hoặc forward vào owner, tránh API trực tiếp đổi renderer nhưng không lưu. Mỗi origin được xác định rõ thay vì suppress-saving toàn cục suốt một async capture.

### 4.4. File store và hiệu năng

- Không phụ thuộc Dreamy Datasave/Core. Dùng store nhỏ dưới Application.persistentDataPath/DreamyDebugger/Preferences với scope hash theo app/channel; Editor thêm project scope để hai project không dùng nhầm settings.
- Không dùng buildGUID làm scope. Build flavor/editor/dev/release cần tách hợp lý để preference development không mở overlay ở release.
- JSON có schema version và byte/entry bounds. Mục tiêu cap 32 KiB; pins/categories có giới hạn và loại dữ liệu tùy chọn vượt budget thay vì tăng file vô hạn.
- Validate enum, finite numbers, scale, màu, thresholds và tọa độ trước apply. Invalid field fallback riêng, không làm vô hiệu toàn bộ debugger.
- Read/load một lần; ưu tiên background I/O. Trong lúc load: HUD chưa áp defaults có thể gây flash rồi bị restore; giữ phần display phù hợp đến khi ready. Nếu user chỉnh trước khi load xong, merge chỉ các field chưa bị chạm trong session.
- Dirty state cập nhật trong memory, debounce ghi khoảng 0.5 giây; mỗi app/scope chỉ có một writer. Serialize snapshot nhỏ rồi làm file I/O ngoài main thread; không Task.Run mỗi slider frame.
- Drag chỉ cập nhật visual trong OnDrag; kết thúc/cancel/focus loss lưu vị trí cuối một lần. Không save mỗi Update/OnDrag/Fit/Refresh.
- Ghi temp + atomic commit/backup theo API filesystem đã verify. Khôi phục file cũ hoặc mới hợp lệ sau interrupted write. Không xóa settings chỉ vì parse fail; giữ backup/quarantine có giới hạn.
- Queue flush khi pause/drag end/quit; không dùng Wait/Result để block frame. Force-kill/power loss vẫn có thể mất thay đổi chưa commit; không hứa OnApplicationQuit luôn chạy.
- Writer không gọi Unity API sau runtime destroy. Old-runtime write/restore không được ghi đè intent của runtime/session mới khi handover.

### 4.5. HUD position và transient effects

- Free position lưu normalized center trong safe area và giới hạn hợp lệ theo kích thước card. Khi đổi orientation/DPI/notch: reapply/clamp để không mất HUD ngoài màn hình.
- Clamp do resize không ghi đè tọa độ ý định đã lưu; chỉ thao tác kéo/chọn vị trí/reset mới thay preference. Tránh drift khi xoay màn hình nhiều lần.
- Reset HUD position chỉ xóa override position và trở về vị trí mặc định config; không reset mode/scale/audio.
- Reset Local Preferences là thao tác riêng: restore allowlist về defaults và lưu. Không xóa cả thư mục DreamyDebugger vì nơi đó còn outbox/report.
- Capture tạm ẩn HUD/panel phải giữ preference cũ. Restore/load hoàn tất trong lúc capture không được làm HUD xuất hiện lại giữa ảnh đang chụp.

## 5. Luồng UI/README sau thay đổi

- Settings/Overview mặc định giải thích Automatic, có thể Play scene bất kỳ để dùng.
- Cheat Panel window khi chưa Play hiển thị hướng dẫn Play; không cảnh báo "thiếu bootstrap" trong Automatic.
- Manual là lựa chọn nâng cao; lúc đó mới hiện Add bootstrap/API startup.
- Config project là tùy chọn tùy chỉnh, không phải bước mở panel bắt buộc. Tạo asset khi user chủ động chỉnh, không làm bẩn project ở lần import đầu.
- Runtime Debugger tools có các HUD settings được support thật và Reset Preferences với phạm vi rõ.
- Khi giá trị đang override config, UI cho biết giá trị hiện tại; thao tác reset đưa về default. Không bắt người dùng hiểu catalog/build payload để sử dụng panel.
- README mở đầu bằng import → Play → tap/PIN. Sample, game service binding, Manual mode và production relay chuyển thành các nhánh riêng.

## 6. Phases và nghiệm thu

| Phase | Việc cần làm | Điều kiện hoàn thành |
|---|---|---|
| 0 — spike và baseline | Xác minh Editor/runtime startup order, build payload và stripping; chụp hash scene/config/meta, giữ receipt preview.3 làm lịch sử | Prototype consumer chứng minh import-only Editor + dev player có UI; release không kéo UI catalog. Chốt API build hook bằng bằng chứng, không suy đoán |
| 1 — auto-bootstrap | Startup mode/defaults, coordinator, asset provider, manual/legacy precedence, duplicate/lifetime handling | Scene không host mở panel được; scene có host không trùng; Off/inactive/Manual không bị auto override; Play/reload/additive/handover không leak |
| 2 — HUD preferences | DTO/store, load/merge, debounce/atomic IO, mode/position/scale/memory/audio/drag, reset | Thoát/mở lại giữ HUD; first-use/reset Off; Free position đúng safe area; không ghi config/SO; drag không ghi liên tục |
| 3 — runtime settings rộng hơn | Threshold/colors/warnings, console filters/collapse, pin/category state; allowlist/reset API | Chỉ prefs UI được lưu; game cheat/security/report policy không bị serialize/replay; unknown pins resolve an toàn khi feature ready |
| 4 — migration và onboarding | Settings window, README/sample/manual API, config upgrade giữ meta/GUID | Project mới không cần sample/bootstrap/config để mở panel; project cũ giữ behavior/ID và có đường rollback |
| 5 — tests và artifacts | Source/clean consumer, fresh archive, dev/release/ARM64 IL2CPP builds, receipt/checksum và profile cơ bản | Tests/builds đúng fingerprint qua; failure/recovery được thử; device-only gates còn thiếu ghi rõ |

Thứ tự làm Phase 0 → 1 → 2 trước; không đưa tất cả settings vào store ngay từ đầu nếu auto/build ownership chưa được chứng minh. Tách commit implementation theo phase, không gộp build staging/persistence/UI vào một thay đổi khó review.

## 7. Ma trận test bắt buộc

| Trường hợp | Kiểm chứng |
|---|---|
| Import fresh, không sample/host/config | Panel mở bằng trigger và API trong scene bất kỳ; không tạo asset/dirty scene ngoài ý định |
| Automatic + authored active/inactive/Off bootstrap | Một owner duy nhất; manual intent thắng; không destroy user root hoặc bỏ config |
| Auto chạy trước, manual bootstrap load sau | Handover sạch, game registration rebind, listener/service/capture generation không lẫn |
| Manual và non-development release | Không autostart; explicit API hoạt động theo policy; release default không UI asset/catalog |
| Domain/scene reload on/off, Play nhiều lần, additive scenes | Không stale static, duplicate UI, event leak hoặc giữ unlock |
| Lần đầu / saved On / saved Off / reset | Defaults đúng; restore preference đúng, không ép Off ở mọi lần chạy |
| HUD Off nhưng đổi vị trí/scale | Lưu được mà không cần instantiate graph/HUD |
| Kéo/xoay màn hình/safe area mới | Đúng normalized position, clamp, không drift; chỉ save khi có intent |
| Slider/drag event dồn, pause/quit | Writer đơn, debounce, không file I/O mỗi frame; flush không block; write count được đo |
| Load chậm rồi user chỉnh / old session save xong muộn | Không overwrite intent mới; generation/single writer đúng |
| Capture hide/restore trùng load hoặc user chỉnh | Không ghi mode tạm, không bật HUD giữa capture; local preference không bị mất |
| File corrupt/truncated/oversize/unknown schema/read-only storage | Debugger vẫn dùng được với fallback; giữ recovery bounded; không spam log/report |
| Config default thay đổi / app cập nhật / app/project khác | Precedence rõ, schema migration hoặc reset revision có kiểm soát; không dùng nhầm scope |
| Console và pin restore lúc game chưa đăng ký cheat | Giữ IDs chờ resolve, không save giá trị, không unpin hết do startup snapshot rỗng |
| Token/PIN/game state/outbox | Không có trong prefs; Reset Preferences không xóa file khác hoặc chạy lại grant/action |
| Tạo mới SDK/API và shutdown | Runtime không reference Editor/Core, async callbacks/writer không chạm owner đã hủy |

Tests dùng memory/temp stores và scope riêng; không đọc/ghi preferences hoặc save thật của người dùng. Không tắt auto-bootstrap toàn cục chỉ vì UNITY_INCLUDE_TESTS: có consumer auto-start smoke riêng, test fixture kiểm soát startup mode/scope rõ ràng. Các regression preview.3 về access, stale confirmation, notification, dynamic UI, capture và ID phải tiếp tục qua.

## 8. Compatibility, bằng chứng và rollback

- Dự kiến candidate preview.4 cho feature mới; không dùng receipt preview.3 để chứng nhận source mới.
- Snapshot existing prefab/SO/meta trước migration; chỉ Unity API khi chỉnh serialized assets. Không xóa meta, không ghi runtime prefs vào authoring assets, không thay font game.
- Chính sách scope/schema và allowlist phải có test thuần C#; startup/UI/lifecycle có PlayMode; build inclusion có inspection receipt. Profile ghi rõ desktop hay device, không tuyên bố không lag chỉ từ async/debounce.
- Native device touch/DPI/rotation, p95/p99/GC/thermal, iOS và force-kill/power-loss cần device evidence; build IL2CPP không thay thế runtime test.
- Main MCP từng không kết nối đúng sandbox; phải discover/pin đúng instance trước stateful actions. Nếu unavailable, consumer riêng; không refresh/test project khác.
- Rollback về hai commit baseline nêu đầu. Sau khi có schema mới, downgrade phải bỏ qua preference schema chưa biết, không phá file hoặc ảnh hưởng outbox/save.
- Không push, tag, publish hoặc gửi Telegram/email thật trong task này nếu không có yêu cầu riêng.

## 9. Definition of done

Người mới import có panel khi Play; người dùng chỉnh HUD rồi mở app/Play lại thấy lựa chọn đã lưu; project cũ không cần sửa scene để tiếp tục chạy. Release không vô tình ship UI vì Resources payload, preferences không biến thành cheat/save/security store, và có receipt mới chứng minh các phase thay vì chỉ tài liệu.

Plan này chưa thay đổi behavior preview.3. Các tên API mới và build adapter là đề xuất để triển khai theo phases, không được ghi vào README như tính năng đã có trước nghiệm thu.


## 10. Kết quả triển khai — 2026-11-06

Phases 0–4 đã triển khai trong candidate 0.3.0-preview.4. Phase 5 đã qua nghiệm thu source/archive (mỗi bên 92 EditMode + 51 PlayMode), 10 test tích hợp project, build Linux development/release và Android ARM64 IL2CPP development. Player development khôi phục Compact qua lần chạy thứ hai; release không runtime/UI asset; forced-failure staging được dọn. MCP sandbox kiểm chứng lại một owner, unlock và restore HUD; trả Off/thoát Play sau thử. Xem [bằng chứng và giới hạn](../evidence/debugger/autostart-preferences/README.md). Các gate thiết bị thật, iOS, production privacy/relay vẫn mở; Gmail pending. Các đoạn đề xuất ở trên được giữ như hợp đồng đã duyệt, không phải trạng thái còn chờ làm của các phase đã nghiệm thu.
