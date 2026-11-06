# Dreamy Debugger — kế hoạch redesign giao diện

Ngày nghiên cứu: **2026-11-05**. Baseline: package `8a0e19c`, project `beca4db`, preview.6. Trạng thái: **đã hoàn tất đề xuất + 3 concept**, chưa sửa Runtime/Editor/prefab, chưa thay release candidate. Ưu tiên redesign UI trước các cơ chế runtime mới trong plan trước.

Hướng đề xuất: **Dreamy đỏ / graphite, bố cục công cụ rõ ràng, card cheat theo tác vụ, HUD gọn theo module**. Một design system dùng chung runtime và Editor; mỗi môi trường giữ cách tương tác phù hợp. Không chuyển thành web dashboard hoặc phủ đầy logo.

## 1. Những gì cần sửa trong giao diện hiện tại

Đã xem ảnh đã nghiệm thu: runtime-ui/portrait.png, drawer-open.png và hud-phone.png; đối chiếu source DebuggerWindow, DebuggerRedesignAssets, DebuggerPalette, cấu hình HUD và sample Resource/Ownership. Đây là baseline đã lưu của phiên trước, không phải ảnh chụp mới trong lượt thiết kế.

| Quan sát | Tác động | Thay đổi đề xuất |
|---|---|---|
| Header đỏ thấp, logo/burger/title/pin/close chen nhau | Chữ và vùng bấm nhỏ, cảm giác cũ | Header có nhịp 56 logical units trên mobile; tách tên trang khỏi logo khi hẹp; action có hit area độc lập |
| Logo banner bị thu nhỏ còn cả dòng GAME STUDIO | Không đọc được tên phụ, giống thumbnail quảng cáo | Dùng wordmark Dreamy hiện có trong một vùng được thiết kế đúng; chỉ một logo visible. Bản wordmark runtime tối giản cần asset riêng được review, không phá banner gốc |
| Build GUID và HUD tools xuất hiện trước cheat game | Chiếm diện tích nhưng ít giúp tác vụ chính | Game cheats ở trên; Debugger tools thành foldout cuối trang; Build ở System |
| Balance, amount và Add là các hàng rời nhau | Khó hiểu trình tự thao tác; hàng trống và độ rộng thiếu nhất quán | Gom thành resource card có balance + input amount + action; giữ các option ID/action/permission hiện tại |
| Row xám khá phẳng, tất cả control cùng trọng lượng | Khó phân biệt readonly, input và command | Border, typography, alignment và states rõ; nút primary đỏ, readonly có hierarchy, input có vùng riêng |
| HUD có label/value dựa vào khoảng trắng, chữ phụ nhỏ | Số nhảy làm lệch cột; không đọc tốt khi thu nhỏ | Text label/value riêng, số tabular/monospace khi phù hợp, graph có legend ngắn, grip không đè dữ liệu |
| Editor window có banner lớn và nhiều khoảng cách dạng giới thiệu | Mất chỗ cho trường cấu hình cần dùng | Toolbar nhỏ + project config selector + navigation trái + form sections; preview riêng, không giả dữ liệu live khi HUD Off |

Giữ: đỏ chủ đạo, access 5 taps + 0000 mặc định, hamburger đóng/mở, panel pin/close, typed/attribute options, confirmation, một sampler, HUD Off và kéo được. Không đưa Report Bug/author/vendor/email/theme/setup trở lại runtime.

## 2. Nguồn tham khảo và cách chọn lọc

Các quyết định thiết kế bên phải là **đề xuất của Dreamy**, không phải tuyên bố những sản phẩm này dùng cùng kiến trúc Unity.

| Nguồn chính | Điều nguồn thể hiện | Học và áp dụng vào Dreamy | Không bê nguyên |
|---|---|---|---|
| [SRDebugger documentation](https://stompyrobot.uk/tools/srdebugger/documentation/) | Console filters, option controls, pinning và cấu hình layout | Giữ mô hình tác vụ quen thuộc và đường pin/unpin; control types chuẩn làm fallback | Vendor support/report/email flow; kiểu mỗi option một hàng bắt buộc |
| [Graphy repository](https://github.com/Tayx94/graphy) và [documentation ở commit đã đọc](https://github.com/Tayx94/graphy/blob/1068f3de2bc9f1e4a905150abd59f31e6dead679/Readme!%20-%20Graphy%20-%20Documentation.pdf) | FPS/RAM/audio, nhiều layout, threshold colors, module modes | HUD gồm text/graph có thứ bậc, metric có màu ngữ nghĩa, placement và refresh rate tách biệt | Không kéo cả singleton/module framework hoặc thay sampler hiện tại; chưa thêm spectrum/VR |
| [Chrome DevTools Console](https://developer.chrome.com/docs/devtools/console/reference) | Filter/search và chọn severity để giảm nhiễu | Toolbar filter cố định, badge đếm, dòng log dễ quét và pane chi tiết | Browser context/URL/JS command prompt không thuộc game debugger |
| [VS Code Custom Layout](https://code.visualstudio.com/docs/configure/custom-layout) | Sidebar/panel có thể ẩn, đổi vị trí và mức density | Navigation rõ, vùng content ổn định, kiểm soát panel thay vì đẩy close ra màn hình | Không xây docking framework IDE đầy đủ trong game |
| [shadcn/ui Sidebar](https://ui.shadcn.com/docs/components/base/sidebar) | Header/content/footer, nhóm navigation, trạng thái icon/offcanvas và mobile | Desktop rail/expanded sidebar; mobile drawer có scrim và toggle rõ | Chỉ học composition/states; không thêm React/web runtime hoặc copy code không phù hợp uGUI |
| [Android touch targets](https://developer.android.com/develop/ui/compose/accessibility/api-defaults) | Khuyến nghị target chạm ít nhất 48dp | Dùng 48 logical units làm mục tiêu thiết kế mobile, sau đó quy đổi/đo theo scaler và DPI thực | Không coi 48 Unity pixels tự động tương đương 48dp |
| [W3C contrast minimum](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html) | Chữ thường 4.5:1; chữ lớn 3:1 | Kiểm tra token thật và trạng thái focus/disabled; không dùng chỉ màu để truyền trạng thái | Không coi mockup raster là chứng nhận accessibility hoàn chỉnh |

Đã xem trực quan ảnh Chrome severity control từ tài liệu chính thức và trang 2 PDF Graphy trong clone pinned. GIF cũ trong Graphy README không fetch được; không coi GIF đó là ảnh đã inspect. Website shadcn/VS Code được đọc theo tài liệu cấu trúc và hành vi, không tuyên bố so sánh pixel. Không nhập asset bên ngoài vào Runtime trong lượt này.

## 3. Design system đề xuất

### Màu và nhận diện

| Token | Giá trị đề xuất | Dùng ở đâu |
|---|---|---|
| Canvas | `#16171A` | Nền panel |
| Surface | `#202126` | Card/form sections |
| Surface raised | `#292B31` | Input, hover và pane phụ |
| Border | `#303238` | Separator/card stroke 1 unit |
| Brand red | `#9E1008` | Header, primary button có chữ sáng |
| Accent coral | `#E95345` | Active rule, focus, icon/accent trên nền tối |
| Selected red | `#572A2A` | Tab/chip được chọn |
| Text | `#F4EFEA` | Nội dung chính |
| Muted | `#A5A3A2` | Label phụ, đơn vị |
| Metric | Mint/amber/red; RAM purple/amber | Giữ mapping theo cấu hình/legend; đỏ lỗi không đồng nghĩa mọi biểu đồ đỏ |

Đỏ tiếp tục là màu nhận diện chính ở header, active navigation và primary actions. Không rải nền đỏ lên mọi card/log/error. Tính contrast sơ bộ từ token sRGB: Ivory/Brand ≈7.26:1; Muted/Surface ≈6.40:1. Ivory/Accent chỉ ≈3.18:1: **không dùng coral làm nền nút chữ nhỏ màu ivory**; dùng Brand cho fill, Accent cho border/focus hoặc chọn text tối đã kiểm tra. Concept AI có thể lệch swatch; token và phép đo khi triển khai là chuẩn.

### Type, spacing và icons

- Một body family tương thích hiện có, ưu tiên Roboto đã có license; metric/value dùng tabular digits hoặc font số phù hợp sau khi xác minh glyph. Không thay font/TMP asset dùng chung của game.
- Logical size đề xuất: body 14–16, caption 12–13, section title 18, screen title 22, metric chính 28–32. Không giảm body bằng auto-size để nhét tên dài; wrap/truncate có cách xem đầy đủ.
- Grid 8: padding 16/24; gap 8/12/16; border 1; radius 6–8. Dùng 1–2 mức surface thay vì shadow/gradient lớn.
- Icons stroke đồng nhất, khoảng 18–20; vùng bấm mobile mục tiêu 48. Icon-only cần tooltip trên desktop và tên/label trong drawer; selected vừa đổi màu vừa có indicator/weight.
- Logo góc trái panel runtime có vùng cố định, preserve aspect; nếu drawer đang chứa logo thì header ẩn logo theo cùng state. Không tạo hai logo cùng visible. HUD không có banner/author/logo lớn.

## 4. Kiến trúc thông tin và responsive shell

Runtime giữ bốn destination: **Cheats, Console, Performance, System**. “Cheats”/“Performance” là nhãn UI đề xuất cho Options/Profiler; giữ enum/tab keys đang có để tránh phá binding/API. Không thêm một tab runtime Diagnostics giả định backend tương lai đã xong.

| Không gian usable sau safe area, theo đơn vị logical thiết kế | Navigation | Content |
|---|---|---|
| Hẹp, khoảng dưới 600 | Hamburger mở drawer overlay, có scrim, Back/ESC đóng drawer trước; không ép permanent rail | Một cột; page title xuống body; pin/close luôn ở header |
| Trung bình, khoảng 600–959 | Rail hẹp có thể mở rộng; toggle vẫn hoạt động | Một hoặc hai cột theo min card width, không ép input nhỏ |
| Rộng, từ khoảng 960 | Sidebar 160–184 hoặc rail 56–64 tùy user state | Main area + optional detail pane; card min khoảng 280–320 |

Đây là breakpoint đề xuất, **chưa phải raw Screen.width hoặc thông số prefab đã áp dụng**. Runtime panel hiện dùng ConstantPixelSize + UI scale/retina helpers, HUD dùng ScaleWithScreenSize 1920×1080 match .5. Phase 0 phải map usable layout units sang scaler thật trước khi chốt threshold. Không đổi scaler cả hệ thống chỉ để khớp ảnh concept; nếu cần migration phải có snapshot, binding và visual regression riêng.

- Mỗi nơi chỉ một SafeArea owner cho controls; full-bleed background có thể nằm ngoài inset.
- Drawer mobile overlay không đẩy phần game còn lại; khi expanded trên desktop content reflow có min width. Close/pin không cuộn đi.
- Hamburger toggle được cả lúc Time.timeScale=0; dùng unscaled transition khoảng 120–180 ms, có reduced-motion/instant path. Không animating LayoutGroup cả cây mỗi frame.
- Khi đóng panel, trả focus/input cho game; click ngoài drawer chỉ đóng drawer. Đóng panel và relock là hai hành động khác nhau.
- State UI search/filter/collapse/scroll được giữ trong phiên theo key ổn định; không lưu giá trị cheat vào UI preference. Favorites/persistence ngoài phiên cần policy riêng.

## 5. Thiết kế từng màn

### Cheats — ưu tiên tác vụ game

Header → title/search → category chips All/Pinned/Economy/Gameplay khi có dữ liệu → nhóm card → Debugger tools foldout cuối trang. Chip/section không hiện nếu game không đăng ký nhóm tương ứng. Build GUID, scene và thông tin thiết bị nằm ở System/status tối giản, không chiếm card đầu tiên.

**Resource card:** tên Coins, balance readonly nổi bật, grant amount có minus/input/plus, nút Add coins. Landscape input/action cùng hàng; portrait có thể xuống hàng theo min width. Giữ Int64 exact stepping và confirmation. Card thành công cập nhật balance và status theo kết quả game service, không tự cộng ở view để giả thành công. Input invalid, out of range và owner unavailable có state rõ; keyboard không che action.

**Ownership card:** No Ads + Owned/Not owned + Grant. Đây là grant-only sample, không dùng switch bật/tắt giả khả năng revoke. Đã owned thì action disabled/Owned; action gọi service game và refresh cùng registry.

**Generic controls:** bool, text, enum, number, readonly, action vẫn có prefab fallback. Card group metadata là **API/presentation contract mới cần thiết kế**, chưa có sẵn. Sample hiện dùng `.balance/.amount/.grant` và `.owned/.unlock`; group phải ánh xạ ID rõ ràng, không đoán bằng DisplayName, thứ tự hay từ “coins”. Không truy cập wallet/save từ view. Partial/attribute options vẫn cùng registry và permission.

**Pin:** tách pin panel với pin option/group bằng icon có context label. Nếu group pinning chưa có contract, dùng per-option selection như hiện tại; không vẽ một nút pin card giả hoạt động. Không mất quyền khi pin: value updates có authorization và relock vẫn chặn mutation.

**Search/collapse/favorites:** làm ở presentation theo stable IDs; không unregister option khi bị filter, không gọi getter để tính search text. Search/favorites/group controls trong concept là đề xuất mới, không phải tính năng đã wire.

### Console — đọc nhanh và chọn đúng lỗi

- Toolbar sticky: search, severity chips có count, Collapse, Clear. Clear có label/tooltip và không đứng sát close khi hẹp.
- Row: timestamp | severity icon | message | repeat count. Selected row tint nhẹ và left rule; không tô đỏ kín cả list. Copy/details không bắt double tap khó dùng trên mobile.
- Landscape dùng list + detail pane; portrait chọn row mở detail sheet có nút Back rõ, copy message/stack riêng.
- Auto-scroll chỉ khi đang ở cuối; khi người dùng kéo lên hiện “N new” để quay lại. Freeze view phải ghi “View paused”, không giả dừng log capture.
- Giữ virtualized list, bounded history, stable selected entry và giới hạn text. Empty/no-results/loading/unavailable đều có state ngắn; không nhấp nháy khi count tăng.
- Không thêm JS/C# evaluation prompt. Những thay đổi preserve-scroll/detail sheet là scope mới cần test lifecycle, không chỉ đổi màu.

### Performance panel và HUD

Panel dùng cùng type scale: frame-time graph + summary row + memory section. Chỉ số chưa đo không hiển thị. CPU Render/Update/GPU time không được dựng số cho đẹp. Audio RMS optional, không gọi nó là spectrum.

| HUD mode | Đề xuất hình thức | Giới hạn |
|---|---|---|
| Off mặc định | Không card; Settings thể hiện Off | Rule collecting riêng nếu host bật; đừng gộp Off UI với tắt mọi rule |
| FPS Only | Dòng `60 fps · 16.7 ms`, khoảng 170×56 logical | Không material graph; grip có không gian riêng |
| Compact | Hai hàng FPS/ms và RAM, khoảng 240×78 | Không biến thành dashboard nhiều ô |
| Detailed | Khoảng 280×205, FPS/ms, avg/1%/.1%, graph, cột Allocated/Reserved; chiều cao tăng nếu audio bật | Chỉ là kích thước khởi điểm; label dài, scale và safe area phải được đo |

Các size HUD là proposal trước kiểm tra scale, không phải phần trăm kích thước màn hình. Nếu grip 48 không vừa thì tăng card hoặc đặt grip gutter, không đè lên metric. Graph và label/value dùng rect riêng. Warmup dùng dấu — hoặc “warming up”; giá trị 0 lúc chưa đủ samples không giả là FPS tụt.

HUD kéo bằng grip, chỉ grip nhận raycast; clamp safe area, reset preset, giữ session placement. Snap-to-corner là tùy chọn mới, không tự snap làm giật vị trí đã chọn. Mở panel thì HUD ẩn; capture dùng suppression scope có sẵn. Giữ shared sampler và refresh rates hiện tại cho tới khi có phép đo mới; không lấy hình mockup chứng minh hiệu năng.

### System

Rows key/value có cột căn chỉnh, copy icon theo section, build ID rút gọn + copy đầy đủ. Nhóm Build / Device / Graphics / Runtime. Không có author, vendor promotion, signup hay support-email form. Không log định danh cá nhân mặc định chỉ để điền UI.

### Editor Settings

Header nhỏ + tên tool/version → project configuration selector → navigation trái năm trang **Overview, Access, Options, Performance, Diagnostics**. Tên và menu hiện có giữ tương thích; Editor ở width tối thiểu có sidebar gọn hoặc tabs cuộn được, không ép window rộng vượt desktop.

- Form chia section, label/value thẳng cột. Hiện rõ asset đang sửa và Locate; không thêm Apply button nếu dùng SerializedObject auto-save và Undo như hiện tại.
- Performance: mode segments Off/FPS/Compact/Detailed; vị trí, offset, scale, drag; Memory/Audio; threshold; warnings/Advanced foldout. Preview là snapshot/editor sample **được gắn nhãn Preview**, không chạy thêm sampler; UI chính vẫn hiển thị Off khi Off.
- Diagnostics chỉ có ở Editor: relay configuration và QA/report status; phân biệt saved/accepted/delivered. Gmail pending; không đưa lại vào runtime.
- Setup chỉ xuất hiện khi thiếu config/bootstrap trong Overview. Không có trang Theme hay Acceptance và không rải lại nút installer ở mọi trang.
- Access vẫn 5 taps/0000 mặc định; label mô tả được PIN dài/passphrase. Numeric modal hỗ trợ 4–32 digits (không cố render 32 chấm riêng gây tràn); passphrase dùng form phù hợp.

### Dialogs và state components

PIN: title ngắn, masked value, keypad mobile, Back/Unlock; show cooldown và lỗi có nội dung, không chỉ đổi đỏ. Confirmation: tên action, đối tượng, số lượng chính xác, Cancel và Confirm; disable khi request đang chạy nếu host contract có async state. Không hứa preview balance sau grant nếu service không cho biết kết quả/transaction. Ví dụ ảnh 9,825→9,925 chỉ là synthetic illustration.

Generic state: idle/hover/pressed/focus/selected/disabled/busy/unavailable/empty. Getter lỗi giữ card, hiện Unavailable và phục hồi khi dữ liệu có lại. Không che panel bằng popup cho từng getter throw. Reset save/delete là destructive style riêng và không đứng cạnh Add coins như primary ngang hàng.

## 6. Concept deliverables

Ảnh/prompt ở [thư mục thiết kế](../design/dreamy-debugger-redesign-2026-11-05/):

1. **01-runtime-cheats** — landscape + portrait, resource/ownership cards, navigation/search.
2. **02-console-hud** — console triage + FPS Only/Compact/Detailed.
3. **03-editor-settings** — Performance settings + PIN/confirmation components.

Tool: built-in `image_gen`; prompt đầy đủ trong `prompts.json` và từng `.prompt.txt`, có tham chiếu logo Dreamy/baseline screenshots. Ảnh là concept AI, không phải capture Unity hay sprite thay prefab. Chữ nhỏ, icon và swatch có thể lệch: binding/type/layout/state/contrast phải theo spec và được verify riêng. README concept ghi sai khác sau khi xem ảnh.

## 7. Phân phase triển khai sau thiết kế

| Phase | Việc làm | Deliverable | Gate nghiệm thu |
|---|---|---|---|
| 0 — khóa spec/baseline | Inventory prefab/variant/bindings/scaler/safe area; đo usable width; chốt token/type/spacing, control state và mock data | Baseline portrait/landscape/tablet + mapping component, test fixture offline | Không đổi GUID/listener; có danh sách excluded game fonts, SR original và sample customization |
| 1 — shell + design tokens | Header, one-logo state, navigation/drawer, body containers, generic typography/input/button; System rows | Shell prefab/variants và reusable states | Hamburger open/close, paused game, Back/ESC, close/pin luôn visible; input không lọt game khi panel đang modal |
| 2 — task cards | Resource/ownership presentation metadata, generic fallback, search/collapse; pin không phá permission | Sample card templates và game-owned bindings | Confirm grant đúng service, Int64 bounds/precision, refresh không rebuild, unavailable/recovery, owner disposal; no duplicate controls |
| 3 — console + performance | Sticky console toolbar/details, scroll selection; label/value HUD rects, safe grip, graph legend/warmup | Console + HUD 3 modes + Performance panel | Spam burst/clear/collapse không mất selected ownership; HUD Off/drag/rotation/raycast; không thêm sampler hoặc metric giả |
| 4 — Editor + dialogs | Compact settings, project config selector/preview, access/confirm states | Editor window và dialog prefabs | Undo/persistence đúng asset, không edit package config, preview không khởi tạo runtime; PIN dài/passphrase/cooldown/focus |
| 5 — hardening/release | Mobile screenshots, device interaction/performance, IL2CPP sample, migration notes và artifact rebuild | Matching receipts + new version candidate | Suite phù hợp pass, build từ artifact, không missing refs/GUID overlap, device gates ghi đúng chưa đạt nếu thiếu máy |

Ước lượng sơ bộ: 8–12 ngày công cho implementation + desktop verification, chưa gồm thời gian thiết bị/iOS hay vòng góp ý. Phụ thuộc lớn nhất là group metadata/pinning và migration scaler; có thể giảm scope đợt đầu còn Phase 0–1 + HUD typography + generic card style, giữ search/group composition sang đợt sau. Đây là estimate, không cam kết lịch.

## 8. Test matrix và performance guardrails

- Presets: 320×640, 390×844, 540×960, 960×540, 1024×768, 1920×1080; safe-area/notch giả lập và ít nhất thiết bị Android thật. Đo readable size ở native DPI, không chỉ phóng ảnh Editor.
- Dataset: 0 / 20 / 200 / 1000 options, tên dài/tiếng Việt, Int64 >2^53, readonly string dài, unavailable getter, categories đổi khi scene unload. Không gọi game save thật trong test UI.
- Interaction: touch/mouse/keyboard, multi-pointer/cancel, IME, focus loss, timeScale=0, open/close/relock, drawer-open capture và HUD drag rotation. Đảm bảo click decor không chặn game khi panel đóng.
- Regression: typed/attribute/partial cùng registry, confirmation và server authority không thay đổi; sample editable trong Assets, Runtime maintained; existing GUID/meta/fileID và listener bindings giữ nguyên.
- Hiệu năng: profile hidden/visible/HUD Off/3 modes, burst log và value refresh. Không polling getter mỗi frame để làm card đẹp; không structural rebuild mỗi lần số đổi. Batch dynamic item updates, cố định graph buffers và không material mới theo repaint.
- Animation nhỏ, unscaled, không blur/shader nền nặng; tách canvas chỉ khi profiler chứng minh có lợi, không tăng nested Canvas/ContentSizeFitter hàng loạt. UI mockup không là acceptance “không lag”.
- Nghiệm thu visual bằng ảnh render thật so với concept/spec. Bounds đúng không đủ chứng minh đẹp/đọc được; ảnh đẹp không đủ chứng minh hit target, state hay lifecycle đúng.

## 9. Boundary và rollback

- Lượt này chỉ thêm plan, prompts và concept dưới docs; không sửa package hoặc scene. SR gốc giữ nguyên.
- Khi triển khai: asset authoring bằng Unity API/Prefab Mode, preserved GUID, không edit YAML để đổi hàng loạt. Không xóa meta, không đổi namespace/asmdef chỉ cho reskin, không thay public registry vì đổi nhãn tab.
- Presenter/adapter làm việc qua registry hiện có; UI không sở hữu wallet, ad entitlement, save, consent hay network transport. Không thêm Dreamy Core/UI dependency bắt buộc.
- Không dùng ảnh concept nguyên tấm làm nền UI tương tác. Chỉ icon/sprite thích hợp mới thành asset riêng có license và mapping rõ.
- Token/layout migration có commit riêng và matching fixtures; game-owned variants/overrides cần review trước update. Có thể rollback view commit mà giữ data/service contracts nếu group metadata additive/fallback đúng.

Thứ tự đề xuất: **shell/typography → resource cards → HUD/console → Editor/dialogs → device hardening**, trước khi bổ sung flight recorder hoặc các tab runtime mới.


## Checkpoint kết thúc lượt thiết kế

Đã tạo và inspect 3 PNG 1586×992, lưu trong repo cùng prompt và SHA-256; [review notes](../design/dreamy-debugger-redesign-2026-11-05/README.md) chỉ rõ lệch contrast/threshold labels và các control mới chưa implement. Đã xác minh ảnh decode được và package working tree không đổi. Không chạy Unity tests vì không thay code/asset; receipt preview.6 trước đó không được dùng làm chứng nhận giao diện concept. Yêu cầu nghiên cứu + viết plan + gen concept hoàn tất; Phase 0–5 triển khai ở trên chưa bắt đầu.


## Cập nhật sau khi duyệt triển khai — 2026-11-05

Phần checkpoint thiết kế ở trên là lịch sử. Đã triển khai candidate `0.3.0-preview.1`: shell đỏ/graphite, drawer có đóng/scrim/back, resource/ownership cards và fallback, search/filter/collapse/pin, Console responsive, HUD riêng các cột, Editor forms, System rows, PIN/confirmation. Không thêm dependency Core/UI. Xem [checkpoint triển khai](DREAMY_DEBUGGER_UI_IMPLEMENTATION_CHECKPOINT.md) và [nghiệm thu](../evidence/debugger/ui-redesign/README.md).

Phase 0–4 hoàn thành phần triển khai và nghiệm thu Editor/consumer. Phase 5 hoàn thành build/integrity trong môi trường hiện có; kiểm thử điện thoại thật, notch/IME/rotation, p95/GC/thermal, GPU/capture dưới tải và iOS còn là gate trước production. Không coi ảnh AI hoặc build thành công là bằng chứng trải nghiệm thiết bị.
