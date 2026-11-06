# Dreamy Debugger — plan UI v2 và concept

Ngày: 2026-11-06. **Trạng thái: hướng nhìn concept được người dùng đánh giá tốt; bộ UI chưa được chấp nhận, tài nguyên chưa chứng minh đủ để triển khai.**

Phạm vi lần này: đọc source, đối chiếu năm ảnh người dùng, nghiên cứu nguồn UI/font, xác định hành vi, chia phase và tạo concept. Không đổi C#, prefab, importer, GUID, config, asmdef hay phiên bản package ở bước thiết kế này. Concept là ảnh thiết kế, không phải ảnh nghiệm thu Unity.

Baseline: `com.dreamy.debugger` **0.1.2**. Package độc lập với Dreamy Core/UI. Giữ theme đỏ nhưng giảm mảng đỏ lớn. Giữ SR gốc để đối chiếu, không sửa nguồn SR. HUD tiếp tục không dùng safe area; panel và keypad có safe area.

## Điều chỉnh bắt buộc từ phản hồi người dùng

Concept hiện tại được giữ làm hướng nhìn mong muốn. Không coi bộ UI đang đề xuất là đã được chốt, không đánh dấu thiết kế hoàn tất chỉ vì ảnh AI đẹp. IBM Plex/Tabler và các nguồn bên dưới vẫn là ứng viên cho đến khi kiểm tra tài nguyên thực.

**Thứ tự bắt buộc: tài nguyên thật → mẫu Unity thật → chốt khả năng dựng → mới cân nhắc gen concept.** Không gen thêm ảnh khi chưa qua gate này. Không hạ chất lượng runtime rồi viện lý do ảnh concept chỉ để minh họa; nếu không tái tạo được một chi tiết, phải chỉ rõ chỗ thiếu và chọn giải pháp khả thi trước.

### Gate tài nguyên trước concept/triển khai diện rộng

1. Lập mapping cho từng phần nhìn thấy: wordmark, panel, button, input, toggle, pin/move/close/backspace, chấm log, font, graph. Mỗi phần phải có asset thật với đường dẫn/license hoặc giải pháp geometry/shader đã render được trong Unity. Link web hoặc hình AI chưa đủ.
2. Chọn và mở thử đúng file font/icon/sprite; kiểm tra glyph tiếng Việt, số/mono, padding, alpha và độ nét. Chưa chọn cả UI pack chỉ từ ảnh quảng cáo.
3. Dựng một màn specimen/prototype nhỏ trong project/scene kiểm tra riêng bằng **đúng tài nguyên dự kiến ship**: header/logo, một resource card, category chip, HUD, keypad và vài control Editor đại diện. Chụp Unity thật, không dùng ảnh AI thay cho kết quả render.
4. So với hướng nhìn hiện tại ở 100% zoom, ngang/dọc và canvas scale phù hợp. Ghi asset nào đáp ứng, asset nào thiếu, khác biệt nào còn lại; kiểm tra pointer cho các nút thiết yếu.
5. Đo texture/font sau import và nén trên target dự kiến. Không chỉ dựa kích thước file nguồn hoặc budget lý thuyết.
6. Chỉ khi bảng mapping không còn hạng mục chưa chứng minh mới đánh dấu resource gate passed. Nếu cần gen ảnh sau đó, chỉ dùng để sắp xếp đúng bộ tài nguyên/hình dạng/font đã xác thực; không thêm chi tiết đẹp nhưng không có cách dựng. Ưu tiên tiếp tục chỉnh prototype và ảnh Unity thật.

Các lỗi input đã xác nhận vẫn là P0; việc sửa chúng không cần chờ thay cả skin. Phạm vi hiện tại vẫn chỉ là plan, chưa sửa runtime.

## 1. Kết quả kiểm tra source và ảnh

| Hiện tượng | Bằng chứng / kết luận hiện tại | Hướng xử lý |
| --- | --- | --- |
| Pin một category thành dải control lớn che game | `DreamyPinnedOptionsService.Pin()` clone từng `OptionsControlBase` vào Container; `PinnedUIRoot` dùng FlowLayoutGroup. Chưa có presentation nhỏ dành riêng cho category. | Tạo category chip/card riêng, thu gọn mặc định; không đưa nguyên form editor ra overlay. |
| Logo như miếng sticker đỏ | `DreamyBanner.png` là **RGB 2172×724**, không có alpha; nền đã nằm trong artwork. | Tách wordmark alpha, đặt trực tiếp lên nền header; không gắn thêm nền đỏ riêng cho logo. |
| Chấm log méo thành ellipse | Ảnh cho thấy tỷ lệ bị kéo. Cần kiểm tra RectTransform/layout driver và Image type trong runtime. | Khóa ô icon 1:1, tắt flexible stretch, Simple + preserveAspect; không dùng Sliced cho chấm tròn. |
| Điểm bắt đầu graph không đồng nhất | FPS shader nhận ring buffer 128 phần tử; `HudHistoryGraphic.Point()` neo mẫu về xMax. Warm-up/plot padding có thể tạo cảm giác lệch. Chưa kết luận shader hay rect là nguyên nhân duy nhất. | Chốt một plot rectangle và contract warm-up chung; kiểm tra cả renderer Graphy và fallback bằng dữ liệu cố định. |
| Cancel/Delete không bấm được | Đọc prefab qua Unity: `CancelButton` dùng **Text làm targetGraphic**, Text có **raycastTarget=false**, không có Image hit surface. Callback tồn tại nhưng không có vùng hit riêng. | Sửa hit surface trước; test qua GraphicRaycaster và pointer thật. Không dùng `onClick.Invoke()` làm bằng chứng nhận chạm. |
| Cancel/Delete dùng chung nút | `CancelButtonPressed()` xóa ký tự nếu đã nhập, chỉ cancel khi rỗng; `RefreshState()` đổi chữ trên cùng nút. | Tách Cancel và Backspace thành hai nút độc lập. |
| Title lộ tên debugger | Service gán prompt như Unlock debugger/Enter PIN. | Bỏ title trên keypad; chỉ dots, phím số, Cancel và Backspace. Không thay policy bảo mật. |
| Nút move HUD chưa đẹp | Grip hiện dùng ký hiệu text và một vùng hit riêng. | Icon move/grip đồng bộ, nét mảnh; giữ hit target dễ kéo nhưng không làm phần nhìn lớn lên. |
| HUD thiếu thao tác nhanh | Hiện chỉ có drag và cấu hình trong Performance. | Tap vùng số FPS/header để đổi mode; thêm close riêng. Không để drag vô tình đổi mode. |
| Chữ tràn và separator lệch | Có nhiều control legacy với layout/auto-size không đồng nhất; header/rail/body có chủ sở hữu khác nhau. | Một hệ spacing/columns/dividers và quy tắc chữ theo loại nội dung. |
| Pin log chưa có UI rõ ràng | Package đã có DockConsoleController, IDockConsoleService và console filter state. | Hoàn thiện dock console thay vì tạo logger hoặc bộ nhớ log thứ hai. |
| Editor window chưa thống nhất | Window hiện dùng custom IMGUI cards/banner/tab layout. | Header nhỏ, tab gọn như SR; config/preview/action có thứ bậc rõ ràng. |

Source chính: `Runtime/Imported/Presentation/Scripts/Services/Implementation/DreamyPinnedOptionsService.cs`, `UI/Other/PinnedUIRoot.cs`, `UI/Other/DockConsoleController.cs`, `UI/Controls/PinEntryControl.cs`, `Runtime/Engine/GraphyHudGraph.cs`, `HudHistoryGraphic.cs`, `Editor/DebuggerWindow.cs` trong package. Các namespace/API nói trên là source hiện có; model overlay mô tả phía dưới là thiết kế mới, chưa phải API đã tồn tại.

## 2. Nguồn tham khảo và chọn hướng

| Nguồn | Học gì / chọn gì | Không mang theo |
| --- | --- | --- |
| [SRDebugger docs](https://stompyrobot.uk/tools/srdebugger/documentation/) và source gốc trong Assets | Phân biệt panel với dock; console pin/unpin; công cụ gọn theo tab; settings ít thao tác. Docs mô tả Pin dock console vào Game View. | Không bê kích thước form lớn thành pinned HUD; không thêm author banner, vendor report, signup hay theme setup. |
| [Graphy](https://github.com/Tayx94/graphy) | Phân cấp FPS/frametime, mode ngắn–dài, đồ thị nhỏ, màu theo ngưỡng. | Không thêm singleton/sampler/input collector thứ hai; không hiển thị breakdown CPU/GPU không đo được. |
| [Radix Themes](https://www.radix-ui.com/themes/docs/theme/overview) | Mặt phẳng, khoảng cách, control states, header/section hierarchy. Chỉ tham khảo hệ thiết kế để dựng uGUI/Editor tương ứng. | Không đưa React/CSS hoặc dependency web vào Unity. |
| [Tabler Icons](https://github.com/tabler/tabler-icons) — MIT | Chọn một bộ outline 24×24, stroke đồng nhất: pin, pin-off, grip/move, x, backspace, chevron, console, performance, search. | Không import toàn bộ thư viện hoặc dùng lẫn nhiều bộ icon. |
| [Kenney UI Pack — Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi) — CC0 | Đã so sánh như một bộ UI game khác; có PNG/vector để tham khảo shape. | Không chọn làm theme chính: viền sci-fi nhiều chi tiết cạnh tranh với dữ liệu/log. Không thêm toàn bộ pack vào build. |
| [IBM Plex](https://github.com/IBM/plex), [license](https://github.com/IBM/plex/blob/master/LICENSE.txt) — OFL | **Đề xuất chính: IBM Plex Sans Regular/Medium cho UI, Plex Mono Regular cho log/số liệu.** Cùng họ font, ít kiểu chữ. | Không dùng font display sci-fi cho stack trace hoặc nhãn dài. Coverage tiếng Việt và số/ký tự phải kiểm tra bằng file font thực tế. |
| [JetBrains Mono](https://github.com/JetBrains/JetBrainsMono) | Đối chiếu thay thế Plex Mono nếu log dễ đọc hơn ở size nhỏ. | Không bundle cả hai font mono chỉ để đổi theme. |

So sánh font khi dựng prototype: IBM Plex Sans/Mono (đề xuất), IBM Plex Sans + JetBrains Mono (phương án dự phòng), Geist hiện tại (baseline). So cùng chuỗi tiếng Việt, tên cheat dài, số lớn, dấu trừ, %, µs, stack trace và DPI; không chọn chỉ từ ảnh concept AI.

## 3. Ngôn ngữ hình ảnh

- Nền graphite `#181A1E`; surface `#22252A`; border `#34383F`; chữ chính `#ECEDEF`; chữ phụ `#A5ABB4`; accent đỏ `#C64150`.
- Đỏ dành cho active rail, pin icon đang bật, focus/danger. Category pin không làm nền cả section thành đỏ.
- Bỏ bevel/nút default Unity và banner to. Corner radius 4–6; divider một đường mảnh; không blur, không gradient/glow.
- Nhịp spacing 4/8/12/16. Header 40–48 logical units; rail 136–160 trên landscape, drawer trên màn hẹp. Chỉ một wordmark nhỏ 96–112 wide, giữ đúng aspect.
- Icon nhìn 14–20; ô icon vuông. Chấm severity 6–8 units; frame vuông và không nhận flexible width/height.
- Control interactive có hit target khoảng 44–48 logical UI units phù hợp canvas/device; icon nhìn có thể nhỏ hơn. Kiểm tra kích thước thực trên màn hình, không chỉ sizeDelta.
- Separator rail/content dùng cùng một boundary/column root; không cộng padding riêng ở từng tab. Horizontal separator chỉ nằm trong content, không vượt sang rail.

## 4. Runtime panel và pinned cheats

### Panel chính

Header nhỏ: wordmark trắng alpha → divider → hamburger/title → action pin của tab nếu có → close. Không chồng thêm logo ở sidebar khi header đã có logo.

Cheats có search và filter gọn; mỗi category header có chevron + tên + pin outline nhỏ. Category chỉ là nhóm; các option/cards bên trong dùng chung registry/access gate. Resource card vẫn gộp balance/amount/action. Thao tác nguy hiểm giữ dialog xác nhận và stale-owner validation.

### Pin category: nhỏ trước, mở khi cần

1. Pin header tạo **một chip category**, mặc định thu gọn, ví dụ `Economy · Coin 3M · Gem 816  ▾`.
2. Chip mặc định khoảng 180–240 wide, 32–40 high; không mặc định stretch hết chiều ngang. Màn hẹp giới hạn theo viewport.
3. Bấm chevron mở popover/card giới hạn chiều cao, scroll bên trong. Chỉ đọc summary cho đến khi người dùng mở control; các action cần xác nhận không chạy từ thao tác pin.
4. Resource/ownership dùng cùng card metadata với panel, không phá thành các mảnh balance/amount/button độc lập. Category không có card metadata có summary tên + số option rồi mở danh sách nhỏ.
5. Hiện tối đa ba chip; thêm `+N` mở danh sách overflow. Không để category lớn sinh hàng chục UI editor lúc pin.
6. Có grip, collapse và unpin nhỏ. Không dùng viền đỏ dày, phủ đỏ hay thay màu toàn bộ game overlay.
7. Khóa access thì ẩn control tương tác theo policy hiện có; đăng ký lại owner phải phục hồi đúng ID, không chạy setter/action.
8. Lưu bằng StableId của option. State của nhóm/vị trí cần key ổn định và migration rõ khi đổi category; giữ giới hạn preference store hiện tại. Không lưu giá trị cheat hoặc permission.

Cần một pinned presentation model/adapter riêng; không chỉ giảm scale các clone cũ. Adapter dùng chung nguồn giá trị và subscription của registry. Group pin/unpin vẫn có batching/cancel và ownership cleanup.

## 5. HUD, graph và console dock

### HUD

- Off vẫn là mặc định; full-viewport anchoring, không safe area; tự do kéo và nhớ vị trí.
- FPS Only: một dòng; Compact: thêm ms/RAM; Detail: thêm graph và thống kê. Default footprint nhỏ, tùy mode.
- Chạm vùng FPS/header để đổi `FPS → Compact → Detail → FPS`; chạm liên tiếp vẫn đổi theo thứ tự. Off dùng nút X riêng; không vô tình đi vào Off khi đang xem.
- Vùng graph/nội dung còn lại pass-through. Grip riêng dùng icon move/four-dot cùng bộ Tabler. Drag vượt threshold hủy click/cycle; pointer khác không giành drag. Close không đồng thời cycle.
- Dismiss ghi Off qua preference API; giữ last non-Off mode nếu cần khôi phục từ Performance. Tất cả vẫn đi qua allowlist, không ghi asset mỗi frame.
- Nút X nhỏ, nhưng hit target dễ bấm. Ẩn cả HUD khi mở keypad/panel; không lưu việc ẩn tạm thành Off.

### Contract graph

Một `plotRect` cho mỗi graph, cùng left/right inset với metric text. Axis, target line và sample geometry cùng gốc; không lấy origin từ image padding khác với shader UV.

Contract đề xuất: lúc warm-up, mẫu đầu ở xMin; samples đi trái → phải với cell width cố định theo capacity. Khi đầy, oldest ở trái, newest ở phải. Chưa có sample thì hiển thị nền/warm-up, không vẽ một cột khổng lồ. FPS/RAM/audio và fallback tuân cùng quy tắc. Các trục có thể khác đơn vị nhưng inset không lệch nhau.

Giữ một sampler, ring buffer có giới hạn; không allocate mảng/material mới mỗi lần Push. Đo FPS chỉ là dữ liệu thật đang có, không vẽ nhãn CPU/GPU Update/Render khi không đo được. Màu FPS green/yellow/red theo ngưỡng; không mặc định mọi cột đỏ.

### Pin log như ảnh 4

Đây là **dock Console ra Game View**, không phải bookmark một log entry. Tận dụng IDockConsoleService/DockConsoleController, shared history, filter state và virtualized rows.

- Icon pin ở header Console. Dock có toolbar mỏng: grip, fold, severity counts/filter, clear có xác nhận nếu cần theo luồng hiện tại, unpin/close.
- Mặc định collapsed thành một dòng; expanded khoảng 20–25% chiều cao, có min/max và scroll. Không che nửa màn hình khi vừa pin.
- Dock Top/Bottom; tôn trọng safe area để toolbar thao tác được; HUD riêng vẫn không safe area.
- Chọn log mở details/stack trace đầy đủ trong panel hoặc details drawer có giới hạn. Không copy lại toàn bộ log buffer thành một logger mới.
- Overlay manager giải quyết tránh chồng dock/chips/HUD khi có chỗ, giữ vị trí người dùng; cho phép reset vị trí. Không rebuild tất cả overlay mỗi frame.

## 6. Keypad và chữ

- **Bỏ hoàn toàn title**: không Unlock debugger, Enter PIN hay tên package; chỉ dots và control cần thiết. Counter PIN dài dạng `3 / 32` gọn, không thêm mô tả kỹ thuật.
- Numeric layout rõ ràng: `1 2 3 / 4 5 6 / 7 8 9 / Cancel 0 Backspace`. Hai nút cuối có hit surface riêng; Cancel luôn đóng nếu policy cho phép, Backspace chỉ xóa một ký tự. Không đổi tên và vai trò trên cùng nút.
- Titleless vẫn giữ gesture/PIN/cooldown/session; đây là thay đổi trình bày, không phải tăng bảo mật.
- Passphrase có field ẩn ký tự, Backspace theo input, Cancel/Submit riêng; responsive theo viewport/keyboard khi platform cung cấp vùng keyboard. Không giữ text sau close.
- UI label: auto-size trong khoảng min/max theo loại; đạt min vẫn không fit thì wrap hoặc ellipsis + xem đầy đủ. Không thu chữ vô hạn.
- Log/stack trace: font mono cố định, wrap/page/scroll; **không** shrink cả stack trace để nhét vào một ô.
- Số: mono, align phải, format nhất quán; các nhãn dài không làm input/action nhảy cột. Dots, badge và icon có aspect 1:1 độc lập với text layout.

## 7. Editor window

Giữ cửa sổ Settings quen thuộc theo tab, học SR về mật độ thông tin. Đề xuất sáu tab: General, Access, Layout, Cheats, Performance, Reports.

- Header thấp: wordmark alpha + Debugger + version nhỏ. Không banner lớn, author info, Setup Theme, rebuild sample hoặc quảng cáo.
- Config selector + trạng thái startup phía trên form; phân biệt Project configuration, config của authored bootstrap và preview trong Editor.
- Đã đối chiếu `SettingsWindow.DrawTabGeneral()`/`DrawTabLayout()` của SR: General gom loading/access; Layout có preview vị trí Console/Entry Trigger/Profiler/Options. Giữ lợi ích preview này, nhưng dùng tên Manual rõ ràng thay vì nhãn Disabled mang nghĩa chờ Init trong SR; không đánh đồng với Dreamy Mode Off.
- General: Automatic/Manual, Mode và hành động Open runtime khi Play; không có nhiều nút setup trùng nhau.
- Access: số tap, vị trí trigger, credential/session. Layout: các vị trí dock/pinned overlays và preview dọc/ngang.
- Performance: mode HUD, tap-to-cycle, close/drag và cảnh báo FPS; advanced FPS rules gập gọn. HUD safe area Off là chủ ý thiết kế, panel safe area On.
- Cheats: hướng dẫn partial/attribute, trạng thái registration và đường dẫn sample. Không thêm wallet/demo data giả vào package Runtime.
- Reports: Telegram/local diagnostics theo config hiện có; không Gmail, không report bug của package và không ghi token vào project.
- Preview chỉ render data giả và không sửa game/prefs thật. Reset preview khác Reset local preferences; thao tác reset thật phải rõ ràng.
- Giữ IMGUI trước để giảm phạm vi đổi kiến trúc. Không đồng thời chuyển UI Toolkit trừ khi có bằng chứng IMGUI không đáp ứng layout; không thêm dependency chỉ để reskin.

## 8. Texture/font nhẹ nhưng giữ nét

Tham khảo [Unity texture formats](https://docs.unity3d.com/6000.0/Documentation/Manual/texture-compression-formats.html) và [Sprite import settings](https://docs.unity3d.com/6000.0/Documentation/Manual/texture-type-sprite.html). Preset cuối phải kiểm tra trên target.

- Ưu tiên master vector/alpha gốc của Dreamy nếu có. Nếu phải tách từ RGB hiện tại, giữ nguyên nét chữ, xử lý viền đỏ/halo và so trên cả nền sáng/tối; không dùng chữ AI vẽ lại làm logo production. Giữ bản gốc để đối chiếu.
- Wordmark alpha crop sát glyph, giữ aspect; bản nguồn không có nền đỏ. Cỡ đóng gói mục tiêu tối đa khoảng 256 wide, variant 512 nếu thực tế DPI đòi hỏi; không đưa banner 2172×724 vào UI chỉ rộng 100 units.
- Panel/button dùng flat 9-slice 16–32 px; separator/circle có thể dùng geometry. Không dùng texture lớn cho nền phẳng hoặc graph.
- Chỉ chọn khoảng 20–32 icon cần dùng; xuất PNG white alpha để tint. Atlas UI mục tiêu **512×512** một page, mipmaps off, Read/Write off, padding chống bleed.
- Mobile có hỗ trợ: thử ASTC 4×4 cho logo/icon alpha, 6×6 cho surface ít chi tiết; chốt theo ảnh nén thực. Desktop chọn format hỗ trợ alpha phù hợp target. Không áp một format lên mọi máy.
- PNG file nhỏ không đồng nghĩa GPU memory nhỏ; Crunch cũng không thay thế phép đo runtime. So dung lượng package, player build, residency, load/upload và draw calls.
- 512² RGBA32 không mip = **1 MiB**; ASTC 4×4 lý thuyết = **256 KiB** trước overhead. Đây là phép tính, không phải receipt build.
- Font: mục tiêu 2 weight Sans + 1 Mono, tổng source font mới ≤1 MiB nếu đáp ứng coverage; chưa đo. Không bundle cả bộ variable font/weights để dùng vài kiểu.
- Giai đoạn này giữ uGUI Text để không kéo theo migration TMP. Không nén lossy glyph atlas chỉ để tiết kiệm số KB. Nếu chọn SDF/TMP sau này, tách migration/phụ thuộc/atlas budget thành gate riêng.
- Gỡ font/sprite cũ khỏi catalog/build chỉ sau audit reference và fallback. Không xóa meta hàng loạt, không đổi font của game, giữ OFL/MIT/CC0 notices.

## 9. Phase và nghiệm thu

| Phase | Việc thực hiện tiếp theo | Điều kiện nghiệm thu |
| --- | --- | --- |
| 0 — baseline/input blockers + resource gate | Ghi baseline/input repro và dựng specimen Unity bằng tài nguyên thật theo gate phía trên. Lập asset-to-control mapping cùng owner rail/header/pinned/dock. | Có repro và asset mapping; ảnh Unity thật chứng minh chất lượng/khả năng dựng, font/importer được kiểm tra; không coi Invoke callback hay ảnh AI là nghiệm thu. |
| 1 — keypad/hit targets | Tách Cancel/Backspace, sửa Graphic hit surfaces và capture gesture, bỏ title, thêm text fitting. | Tap số → Delete → Cancel thật; Delete khi rỗng không tự Cancel; CanCancel=false không đóng; short/long PIN, sai PIN/cooldown, passphrase, rotation, drag/no accidental submit. |
| 2 — design foundation | Alpha wordmark, chọn font bằng specimen, icon set/ratios, token spacing/borders, đồng nhất separator. | So ảnh 100%/DPI; không ellipse, không chữ tràn, không strip logo đỏ; reference/GUID/no-missing-script pass. |
| 3 — category pins | Category summary/chip, expansion/overflow, same registry/card adapter, cleanup/preferences. | Pin category 1/10/100/1000 option vẫn mở chip nhỏ; không chạy mutation; unpin/dispose/restore đúng ID, bounded allocation/UI work. |
| 4 — HUD/graph | Move icon, close, tap-cycle, unified plot coordinates và warm-up. | Tap/drag/close không xung đột; Off restore đúng, 1/2/128/129 samples đúng x origin; no extra sampler/material leak. |
| 5 — pinned logs | Console pin UI, bounded dock modes, shared filters/history/details, overlay placement. | Collapse/expand/unpin/filter/clear/details khi log spam; không duplicate subscriptions/history; không đè input game ngoài vùng dock. |
| 6 — Editor redesign | Form/tab/preview mới và trình bày config owners; bỏ nút thừa. | Resize window hẹp/rộng không cắt field; preview không ghi config/prefs thật; Undo/dirty/save hợp lệ; không runtime Editor reference. |
| 7 — release/performance | Platform texture overrides, catalog/font stripping audit, docs/migration, package patch/minor theo API thực tế. | Cài **Git remote** và **tgz** sạch, import sample, parity + compile/EditMode/PlayMode, actual pointer tests và rendered evidence. Dev/release/IL2CPP/device gate ghi riêng. |

Phases 1–2 ưu tiên trước skin diện rộng. Sau concept được chốt mới triển khai từng phase; không coi việc tạo plan là phê duyệt publish, tag hoặc đổi code hiện tại.

### Ma trận bắt buộc

- Viewports: 640×300, 852×393, 1280×720, 1920×1080; 393×852 và 1080×2400; safe area lệch trái/phải, portrait/landscape rotation; các mức DPI/canvas scale.
- Pin: category nhỏ/lớn, partial pin/mixed state, option đổi nhóm/owner bị hủy, restored preferences, hidden main panel, locked/unlocked.
- Input: GraphicRaycaster trúng đúng button, pointer down/up/click, multi-touch, drag threshold, keyboard, focus/pause. Thêm một actual end-to-end pointer path, không chỉ gọi method trực tiếp.
- Fonts: chuỗi tiếng Việt, 100+ ký tự tên category, số lớn/âm/thập phân, log/stack rất dài; ellipsis phải có cách xem đầy đủ.
- Performance: baseline + sau đổi CPU/GC/UI rebuild/draw calls/texture memory; chụp/gửi report đang idle/busy. Gate CPU/GPU trên máy thật không được đánh dấu pass từ ảnh editor.
- Release guard: giữ patch Git ignore đã sửa trong 0.1.1. So **shipped files** với exact commit; local consumer pass không chứng minh Git package có đủ file.

## 10. Concept và giới hạn

Hai board ở `docs/concepts/debugger-redesign-v2/`:

1. [runtime-concept.png](../concepts/debugger-redesign-v2/runtime-concept.png): runtime panel, category chip, HUD, console dock và keypad không title.
2. [editor-concept.png](../concepts/debugger-redesign-v2/editor-concept.png): Editor Access/Layout/Performance, preview và typography direction.

Board là định hướng shape/hierarchy/màu; cỡ chữ/texture thực phải được dựng lại bằng layout có đo đạc, không cắt ảnh concept làm atlas UI. Text hoặc icon AI vẽ không chính xác không được trở thành contract. Source code và các gate ở trên là contract chức năng.

**Đã có:** source audit, nguồn tham khảo, plan và ảnh hướng nhìn. **Chưa đạt resource gate:** chưa chốt bộ UI, chưa có specimen Unity từ tài nguyên dự kiến ship. Thiết kế chưa được xác nhận khả thi đầy đủ. Sửa lỗi runtime/keypad/pin, thay font/assets, pointer/device acceptance và release vẫn chưa thực hiện. Workflow `docs/agents/dreamy-workflow.md` không có trong checkout; dùng checkpoint thủ công này, không tuyên bố native harness đã chạy.


## Implementation checkpoint — 2026-11-07

Material Compact runtime/editor source is implemented at debugger commit cef4acb. Real texture extraction, asset mapping, runtime controls, responsive layout, pointer checks, clean Git import and archive acceptance are recorded in [visual review](../evidence/debugger/material-implementation/README.md) and its interactive gallery. Final source: 73 PlayMode and 84 package-only EditMode tests pass. Retain physical-device/texture/performance and native Editor screenshot gates as pending; do not mark the full broad resource/device gate complete from these fixtures. No tag or remote release was performed.
