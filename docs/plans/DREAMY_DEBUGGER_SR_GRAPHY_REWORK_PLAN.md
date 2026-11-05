# Dreamy Debugger — SR parity, Graphy HUD và tinh giản UI

Ngày khảo sát: 2026-11-04. Trạng thái: **đang triển khai; đã sửa runtime/prefab, nghiệm thu từng phần**.

Checkpoint và receipt: [DREAMY_DEBUGGER_REWORK_CHECKPOINT.md](DREAMY_DEBUGGER_REWORK_CHECKPOINT.md). Các checkbox bên dưới là gate của toàn phase; không tự đánh dấu xong chỉ vì một phần code đã có.

## 1. Phạm vi và nguồn đối chiếu

- SR gốc đã được import lại: `Assets/Packages/StompyRobot`. Chỉ đọc; không move, sửa source, đổi GUID hoặc prefab của SR.
- SR Settings thực tế: `SRDebugger/Scripts/Editor/SettingsWindow.cs`, SettingsEditor, Settings.cs, SROptions.cs, SROptions.Attributes.cs, SROptions.Test.cs, scanner và OptionsTabController.
- Dreamy: DebuggerWindow, DebuggerConfiguration, DebuggerRedesignAssets, DebuggerPalette, DebugOptions, OptionsPresentationBridge, PerformanceHud/HudHistoryGraphic, AboutTab và panel prefab.
- Graphy: đọc source clone riêng ở /tmp, không cài vào manifest/Assets. Version **4.0.0**, commit **1068f3de2bc9f1e4a905150abd59f31e6dead679**. Đọc GraphyManager/GraphyDebugger, Editor inspectors, G_FpsMonitor/G_FpsGraph, RAM/audio, asmdef, shader/prefab và license font.
- [Graphy source](https://github.com/Tayx94/graphy/tree/1068f3de2bc9f1e4a905150abd59f31e6dead679), [GraphyManager](https://github.com/Tayx94/graphy/blob/1068f3de2bc9f1e4a905150abd59f31e6dead679/Runtime/GraphyManager.cs), [conditions/actions](https://github.com/Tayx94/graphy/blob/1068f3de2bc9f1e4a905150abd59f31e6dead679/Runtime/GraphyDebugger.cs).
- MCP hiện không có DebugPanelRoot đang load; khảo sát logo/author lần này là **source + prefab**, chưa phải screenshot nghiệm thu runtime mới.

## 2. Findings cụ thể, không coi migration là đã đủ parity

1. SR Settings có General, Layout, Bug Reporter, Shortcuts, Advanced; Dreamy hiện tập trung vào setup/sample/theme/validation và chưa đưa đủ các setting vận hành ra window.
2. `DebuggerRedesignAssets.StylePanel` gắn cùng banner vào sidebar Logo, Main/Header/DreamyBanner và footer/Image. Prefab còn các node DreamyBrand. Đây là nhiều vị trí branding độc lập; cần audit cả active/inactive/nested trước khi bỏ.
3. `DebuggerPalette.Brand/Accent/Selected` đều là các sắc đỏ. HudHistoryGraphic tô vùng FPS bằng Theme.Brand; PerformanceHud hardcode màu đỏ ở FPS critical và reserved RAM. Màu nhận diện đã bị dùng làm màu metric.
4. AboutTab.prefab vẫn có `Format: SRDebugger {0}` và `www.stompyrobot.uk`. Không xóa copyright/license khỏi source/notices; chỉ bỏ author/promotional UI ở sản phẩm runtime.
5. Attributes legacy vẫn có: Category, DisplayName, Browsable(false), Sort; NumberRange/Increment ở control. Property không có public setter được coi read-only; INotifyPropertyChanged có plumbing. Không thấy Description được scanner chuyển thành tooltip; chưa có adapter metadata tương đương đầy đủ cho typed registry.
6. `DebugOptions` vẫn khai báo partial nhưng nằm trong package Presentation assembly. Partial trong assembly game không thể nối vào class package. Ngoài ra chưa thấy call site cho `DebugOptions.OnStartup()` trong Runtime hiện tại: không coi Current/auto-register đã được nghiệm thu.
7. Reflection container truyền thẳng vào presentation, còn typed CheatRegistry là đường có authorization/lifecycle rõ. Hai đường cần được thống nhất trước khi quảng bá attribute compatibility.
8. NumberControl đã thêm Int64; typed registry hiện chỉ nhận bool/string/int/long/float/double/enum. Legacy NumberControl nhận thêm byte/sbyte/short/ushort/uint: cần công bố rõ và test adapter, không tuyên bố tất cả số là tương đương.
9. Graphy có TOP_LEFT/TOP_RIGHT/BOTTOM_LEFT/BOTTOM_RIGHT/FREE; module FULL/TEXT/BASIC/BACKGROUND/OFF; màu/ngưỡng tốt/cảnh báo/critical riêng; FPS/RAM/audio/device info; average và 1%/0.1% low.
10. Graphy DebugPacket có comparer, AND/OR, initial delay, repeat delay, execute-once, console log/warning/error, screenshot, Debug.Break, UnityEvent/callback. Delay dùng Time.deltaTime; screenshot gọi ScreenCapture.CaptureScreenshot. Cần thích nghi unscaled time và pipeline capture hiện có.
11. Graphy runtime asmdef có reference GUID `75469ad4d38634e559750d17036d5f7c`: đã resolve tại project thành **Unity.InputSystem.asmdef**. Chép nguyên bản sẽ vi phạm gate consumer không cần Input System.
12. Gmail/email pending. Form runtime legacy còn ghi email vào PlayerPrefs trong khi handler Dreamy không dùng nó. Không tiếp tục mở rộng signup/email/vendor endpoint.

## 3. Quyết định kiến trúc và phần tái sử dụng Graphy

**Chọn reuse source/prefab/shader Graphy có kiểm soát cho performance HUD**, không tiếp tục vẽ lại toàn bộ bằng bảng đỏ. Dùng dark card, metric typography/legend và graph của họ làm baseline; không bê author/promotional UI vào runtime.

- Vendor phần cần thiết vào vùng ThirdParty/Graphy có commit/license ledger, GUID Dreamy mới. Không sửa bản clone hoặc SR gốc. Không thêm dependency Dreamy Core/UI.
- Giữ code/namespace upstream khi không cần đổi, thêm lớp facade trong Dreamy. Font có license riêng: Northwest yêu cầu giữ tên và file license; Roboto có LICENSE riêng. Chỉ đóng gói font thực sự dùng; giữ notices đầy đủ.
- Dùng một performance backend: dự kiến Graphy monitor là owner, `IProfilerService` legacy đọc qua adapter; retire sampling trùng ở ProfilerServiceImpl/PerformanceHud/HudHistoryGraphic sau khi consumer đã chuyển. Không khởi tạo cả Graphy singleton và Dreamy sampler độc lập.
- Trước khi chốt backend, spike consumer sạch đo off/FPS/full và verify IProfilerService consumers. Nếu Graphy monitor không đáp ứng budget, giữ renderer Graphy nhưng cấp dữ liệu từ sampler chung đã tối ưu; ghi ADR từ số đo, không âm thầm giữ hai bộ sampling.
- Tách input adapter khỏi asmdef bắt buộc; hotkey qua input bridge hiện có. XR/audio nâng cao optional. Giữ shader/prefab material refs bằng Unity API và test player stripping.
- Performance rule engine dùng samples backend chung. Mượn condition/action model của Graphy, không dùng action screenshot upstream để vượt policy/backpressure/cancellation của Dreamy.
- Ghi rõ Graphy memory là loại memory nào và audio RMS dB khác spectrum FFT; không gắn nhãn CPU/GPU timing nếu chưa có measurement.

## 4. Window mới: Settings vận hành, không phải bảng công cụ authoring

Một entry chính `Window → Dreamy Debugger → Settings`, thêm shortcut mở runtime panel. Nội dung chính:

| Nhóm | Setting cần có |
|---|---|
| General & Access | Mode/preset, bootstrap status, default tab, enabled tabs, số tap, gap/timeout, PIN/passphrase, TTL/cooldown, trigger enable |
| Layout & Input | Preview vị trí trigger/HUD/pinned console/pinned options, safe area, scale/opacity, layer/camera nâng cao, shortcut & ESC, cảnh báo trùng hotkey |
| Console & Options | Log levels/history/collapse/search/rich text, group/order/layout compact, pin, session persistence policy |
| Performance | FPS/RAM/audio/device modules, vị trí ban đầu + offset, module mode, màu/ngưỡng, history/refresh, reset stats, rule list |
| Diagnostics | Policy log/ảnh, relay trạng thái, capture limits, manual report, pending/retry status; không email/signup |

- Bỏ Theme/Setup/Acceptance khỏi tab chính. Chuyển installer, regenerate prefabs, theme authoring, catalog selection và validation sang menu Advanced/Developer hoặc test tooling. Không xóa capability kiểm chứng; chỉ bỏ khỏi flow sử dụng thường xuyên.
- Không bày nhiều nút Install/Add bootstrap/Open demo trong mọi trang. Nếu chưa cấu hình, một empty-state có một action hợp lệ; sau setup biến mất. Demo chỉ ở Samples/Help.
- Không expose private credential hoặc trạng thái consent như flag tiện tay bật cho player production.
- Đối chiếu settings SR: enable/load/default tab; trigger/code; pinned profiler/options/console positions; shortcuts/duplicate detection; console cap/collapse/rich text; opacity, scale/layer/camera; disabled tabs, cursor/EventSystem/unload. Mỗi setting ghi rõ đã expose, thay thế hoặc chủ ý bỏ.
- Auto-load SR, vendor API signup/invoice/order/email, rate/donate/author links: bỏ khỏi Dreamy flow.

## 5. Runtime panel và options layout

### Branding và màu

- Runtime panel: header có tên tab + close/back, **mặc định không có banner lớn**. Nếu bật branding, tối đa một logo nhỏ; không lặp trong footer/sidebar/main header và pinned UI.
- Bỏ author links, website/version SR hiển thị; About thu gọn thành version Dreamy/build nếu cần. Thông báo third-party chuyển sang Documentation/Notices hoặc Editor Help phù hợp.
- Phân vai palette: neutral surfaces/text; interactive selection; semantic good/caution/critical; series colors FPS/RAM/audio. Brand màu đỏ không được truyền mặc định sang graph, controls và mọi selection.
- FPS bình thường xanh/teal, caution amber, critical đỏ cấu hình được. RAM 3 màu phân biệt; không dùng đỏ để ngụ ý Reserved RAM luôn nguy hiểm. Có preset neutral/color-accessible, legend và số để không chỉ dựa màu.
- Audit prefab qua LoadPrefabContents/SerializedObject; không search-replace YAML layout. Builder idempotent, chỉ touch owned assets. Giữ GUID Dreamy hiện tại.

### Cheat/options không ép mỗi option một hàng

- Hỗ trợ Auto/FullWidth/HalfWidth/ActionGrid/Inline theo metadata. Phone hẹp chuyển thành một cột, ngang/tablet cho 2 cột khi min-width đủ; tránh nested scrolling.
- Resource card mẫu: `Coins  9,825 | Amount [125] | [+ Add]` dùng một nhóm gọn thay cho balance/amount/action ba hàng tách rời. Gem tương tự. Đây là layout/presenter binding tới cùng registry, không thêm wallet thứ hai.
- Bool và enum ngắn có thể ghép đôi; action ngắn thành grid 2–3 nút khi đủ rộng. Chuỗi dài/log/destructive action chiếm full width.
- Giữ label dễ đọc, touch target mục tiêu ít nhất 44 logical px, focus order/keyboard, clear disabled/read-only states. Confirmation cho thao tác ghi dữ liệu vẫn hoạt động.
- Group collapse, pin/favorites, search/filter theo category/tên khi số lượng lớn. Virtualization/pooling theo viewport; không instantiate lại toàn bộ list chỉ vì một giá trị thay đổi.
- Đối chiếu FlowLayoutGroup/Category của SR trước khi viết layout engine mới. Không diễn giải khả năng width layout cũ thành đã có responsive resource card.

## 6. Attributes và partial game options

| Khả năng | Hiện tại | Plan |
|---|---|---|
| Category/DisplayName/Browsable(false) | Reflection scanner có | Adapter bảo toàn vào registry; fixture đối chiếu SR |
| Sort | Reflection có, typed bridge luôn priority 0 | Metadata order thống nhất, stable tie-break |
| NumberRange/Increment | Legacy control có, typed mới truyền range | Thêm increment vào metadata; range/step đến UI và validation |
| Read-only | Dựa public setter | Test private setter/getter-only tất cả loại; không hứa hỗ trợ ReadOnlyAttribute nếu chưa implement |
| Description tooltip | Chưa thấy scanner support | Extension Dreamy mới, phân biệt với parity SR |
| INotifyPropertyChanged | Có legacy plumbing | Adapter subscribe/dispose, fallback polling có nhịp; không double subscription |
| Method action | void, public instance, không tham số | Giữ rule; parameter/static/async có diagnostic rõ, chưa quảng bá tự hỗ trợ |
| Confirm/Group/Width/Row | Chưa có metadata chung | Extension mới; không gán nhầm là attribute có sẵn của SR |
| Partial options | Package class có partial, game khác assembly không nối được | Game-owned partial class cùng một project asmdef |

Mẫu cần ship/import:

- `GameDebugOptions.cs`: class partial của game, dependency inject qua constructor, INotifyPropertyChanged.
- `GameDebugOptions.Economy.cs`, `.Gameplay.cs`, `.Entitlements.cs`: **cùng namespace và cùng asmdef**; có ví dụ Category/DisplayName/Sort/Range/Increment/Browsable/readonly/action.
- Một host đăng ký container vào adapter typed registry, IDisposable lease; không bypass Access/confirmation. Registry không tham chiếu class game.
- Partial không được dùng để cắt ngang hai asmdef. Không yêu cầu sửa file nằm trong package Runtime; không tạo một DebugOptions.Current global thứ hai ngoài ý muốn.
- Test EditMode + IL2CPP preserve/linker và enter-play/domain-reload on/off, scene reload/re-enable. Phân biệt compatibility tên thuộc tính và compatibility hành vi.

## 7. Performance HUD / threshold debugging

- Vị trí **ban đầu** cấu hình: 4 góc + Free; offset/safe-area, UI scale/opacity. Có runtime menu đổi vị trí, reset về vị trí ban đầu; drag chỉ khi bật chế độ Move để HUD thường không chặn input game.
- FPS/RAM/audio/device bật tắt độc lập; BASIC/TEXT/FULL/OFF phù hợp, default FPS only. Audio/spectrum/advanced không chạy collector khi không có consumer. State session mặc định, lưu preference chỉ opt-in/versioned; không ghi config authored trong runtime.
- FPS current/average/frame ms, min/max cửa sổ, 1% low/0.1% low khi đủ samples; ghi cửa sổ và trạng thái warmup. 1% low không được gắn nhãn nhầm thành p99 frame time. Refresh text/graph/sampling tách riêng.
- Một ring buffer dùng chung cho HUD, panel và rule engine. Graphy G_FpsMonitor giữ 1024 samples và tìm slowest samples; đo chi phí trên máy yếu trước khi dùng nguyên thuật toán mỗi frame. Percentile/low calculations có thể chạy nhịp thấp hơn.
- Cấu hình rule: metric/comparer/value, AND/OR, warmup/loading suppression, sustain duration, hysteresis, cooldown, max/session, execute once hoặc re-arm.
- Mặc định cảnh báo: average FPS < 30 trong 3s sau warmup 5s; re-arm khi >35 trong 2s; cooldown 60s. Đây là giá trị khởi đầu để QA điều chỉnh theo game 30/60/120 FPS, không phải ngưỡng phổ quát.
- Actions: local log, callback, bookmark performance snapshot; capture/report opt-in theo policy. Pause Editor chỉ Editor/internal; không pause gameplay production. Rule mặc định không tự gửi Telegram.
- Rule event đánh dấu origin PerformanceRule, không vòng lặp log→auto-report→rule/log. Dedupe/cap áp dụng cho cả trigger tay và tự động theo policy phù hợp.

## 8. Diagnostics scope đi kèm

- Gmail/email và signup vendor: pending/out of scope. Có thể ẩn form legacy thay vì xóa type/GUID khi vẫn cần migration.
- Report package: Editor Help/export thông tin package tối thiểu; không lấy toàn bộ player context mặc định.
- Manual QA: giữ fake button + async screenshot/relay đang dùng; tách simulate-no-network khỏi send-real rõ ràng.
- Production: policy trước capture và trước upload; host cung cấp consent/sensitive-screen state. Screenshot opt-in, text-first; deny/withdrawal và outbox cleanup được test. Firebase adapter là phase sau, không kéo Firebase vào core hiện tại.
- Không hứa crash fatal/OOM/ANR có ảnh mới; không thay background screen capture/MediaProjection vào tool.
- Đưa hai lỗi review trước vào backlog: pendingError ghi đè nhiều lỗi; lọc toàn bộ prefix [Dreamy] bỏ sót lỗi thật. Sửa cùng phase telemetry/rules, không bỏ qua bằng cách tắt báo lỗi.

## 9. Các phase và gate nghiệm thu

### P0 — Freeze/baseline và parity ledger
- [ ] Snapshot diff/commit riêng các thay đổi UI/report đang có; tránh commit lẫn font/logo hoặc việc của người khác.
- [ ] Chụp SR và Dreamy cùng resolution/preset; đọc SettingsWindow/inspector thực tế, ghi từng setting vào parity ledger.
- [ ] Snapshot active/inactive/nested branding nodes, string SR hiển thị và màu controls/graph. SR gốc chỉ đọc, scene đối chiếu chỉ bật một debugger.
- Gate: có ảnh baseline và danh sách keep/replace/drop; không claim screenshot từ audit source lần này.

### P1 — Gỡ spam và tinh giản window
- [x] Bỏ logo lặp, author/vendor promo runtime; phân vai semantic colors.
- [ ] Áp dụng Settings IA ở mục 4; chuyển setup/theme/validation khỏi flow chính, một empty-state action.
- [x] Builder không nhân banner sau nhiều lần chạy; đã kiểm ảnh panel portrait/landscape/tablet, test pin/confirmation và lifecycle. Gate thiết bị thật còn mở.
- Gate: screenshot portrait/landscape/tablet; mặc định 0 banner lớn, branding enabled tối đa 1; không còn author SR trong runtime; notices còn nguyên; không Missing Script.

### P2 — Spike và tích hợp Graphy có chọn lọc
- [ ] Import copy có license ledger + GUID mới; không sửa originals; phân tách InputSystem/XR optional.
- [ ] Prototype reuse UI/shader/modules, bridge một backend; bỏ sampler trùng sau khi consumers đã chuyển.
- [ ] Layout positions/default/reset/move mode, module toggles, neutral/semantic colors.
- Gate: consumer thiếu Core/UI/InputSystem/Graphy external vẫn compile; Built-in/URP và IL2CPP shader refs đúng; đo off/FPS/full CPU/GC/RAM trước/sau; không double AudioListener sampler.

### P3 — Attribute adapter + project partial sample
- [ ] Parity tests và metadata model ở mục 6; hỗ trợ typed và reflected qua cùng authorization boundary.
- [ ] Ship partial mẫu qua Samples~; chuyển sandbox catalog theo cách đọc được, giữ stable ID/save behavior.
- Gate: attribute matrix đều có receipt; reflection không vượt khóa, same-asmdef partial hoạt động, disable/re-enable không trùng option/listener, IL2CPP không mất member.

### P4 — Cheat layout compact
- [ ] Resource card, inline rows, action grids, auto breakpoints, collapse/search/pin.
- [ ] Long labels/read-only/bounds/confirmation/scroll focus/accessibility.
- Gate: 320px/phone notch/landscape/tablet; không overlap/clipping; thêm Coin và No Ads vẫn gọi đúng service; hàng trăm option không rebuild toàn list khi update value.

### P5 — Performance rules và error queue
- [ ] Threshold sustained/hysteresis/warmup/cooldown/rearm, actions safe, bounded error queue/fingerprint.
- [ ] Clock unscaled, skip loading/focus loss; không recursive reporting.
- Gate: fake samples crossing threshold chỉ fire đúng lượt; timeScale=0 ổn; không spam log/report khi FPS dao động; queue burst và memory cap có test.

### P6 — Diagnostics policy và mobile acceptance
- [ ] Policy log/ảnh/upload thống nhất, withdrawal/outbox, sensitive-screen/mask contract; không thêm Gmail.
- [ ] Android device trước, release stripping/QA riêng; crash/report native cases phân loại đúng.
- Gate: ảnh được kiểm tra trực quan, offline/restart/low memory/GPU unsupported/cancel; không gửi ảnh khi chưa được phép; production không có cheats; báo cáo p50/p95/p99/GC/capture overhead trên máy yếu thực tế.

## 10. Thứ tự thực hiện đề xuất

**P0 → P1 → P2 → P3 → P4 → P5 → P6.** Mốc review sau P1, spike P2 và P4 phải có ảnh so sánh; không triển khai một lượt tất cả thay đổi serialized assets. Plan này ưu tiên sửa vấn đề nhìn thấy trước, sau đó hoàn thiện behavior/parity. Không coi việc clone Graphy/read source là đã tích hợp hay đã đạt performance budget.
