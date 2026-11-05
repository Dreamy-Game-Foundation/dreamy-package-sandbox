# Audit trước release: SRDebugger, Graphy và Dreamy Debugger

Ngày: 2026-11-05. Kết luận: **giữ release ở trạng thái HOLD**, sửa nhóm P1 và kiểm thử candidate trước khi chốt tag. Có thể hướng tới private/internal preview tiếp theo; chưa đủ bằng chứng gọi là production-ready.

## 1. Phạm vi và độ tin cậy

- SRDebugger **1.12.1**: đọc source/prefab và SettingsWindow của bản gốc trong `Assets/Packages/StompyRobot`. Không sửa bản gốc.
- Graphy **4.0.0**, commit `1068f3de2bc9f1e4a905150abd59f31e6dead679`: đọc Runtime/Editor, prefab FPS, shader/helper và PDF documentation. PDF page 2 được xem trực quan; chưa render toàn bộ Graphy prefab thành một scene riêng trong audit này.
- Dreamy: package HEAD `0e66ce113c9af77a2cbac5922b8e8e63cf5d9726` + worktree UI/reporting đang sửa; manifest **0.2.0-preview.5**, runtime `DreamyDebug.Version` vẫn **0.2.0-preview.1**. Chưa bump preview.6, chưa commit/tag/push.
- Unity **6000.4.12f1**, sandbox URP; các API claims dựa source đang cài, không suy từ tên package.
- Receipt `docs/evidence/debugger/runtime-ui/verification.json`: **72 EditMode + 21 PlayMode pass**, fingerprints implementation/test/asset vẫn khớp, không drift. Đây là kết quả worktree, không phải chứng nhận một release commit mới.
- Audit mới dùng probe managed-only trong đúng MCP sandbox, không Play game, không sửa scene/settings, không gửi report. Kết quả nằm ở `docs/evidence/debugger/pre-release-audit/probes.json`.
- Phân loại: **đã xác nhận bằng probe**, **source xác nhận**, **rủi ro suy ra cần test**, **chưa nghiệm thu**. Không biến thiếu receipt thành khẳng định code bị lỗi.

## 2. So sánh chức năng

| Nhóm | SRDebugger gốc | Graphy | Dreamy hiện tại | Quyết định |
|---|---|---|---|---|
| Vai trò | Console, options, profiler và system/debug panel | Performance monitor và conditional debugging | Internal debug panel + game cheats + performance + diagnostics | Kết hợp có chọn lọc; không gom mọi chức năng thành một màn hình |
| Startup/access | Auto-load hoặc manual; trigger, entry code 4 chữ số trong SettingsWindow | Prefab, enable-on-startup/keep-alive, hotkeys | Explicit bootstrap; 1–20 taps, PIN 4–32 digits, passphrase, session/cooldown, checks trên registry | Giữ explicit bootstrap và authorization; credential local chỉ ngăn mở nhầm |
| Console | Collapse/filter/search, stack/copy, virtualized list, pinned console | Log actions của debugger; không thay thế console đầy đủ của SR | Kế thừa phần console, có bounded threaded ingress | Giữ; stress-test throughput và hidden-state cost |
| Options/cheats | Reflection, Category/DisplayName/Browsable/Sort/Range/Increment, read-only, INotify plumbing, partial cùng assembly | Callback/UnityEvent actions; không có cheat editor tương đương | Typed registry, confirmation/lease, delegate-bound game samples, attribute adapter và game-owned partial | Giữ boundary game-owned; bổ sung value notifications và AOT proof |
| FPS/RAM/audio | Frame graph/memory, pinned profiler | FPS/RAM, audio spectrum + dB, device info; module modes/positions | FPS + 1%/.1%, RAM, RMS dB; System tab riêng; một sampler, Graphy FPS shader/font | Giữ FPS/RAM; audio là optional. Không gọi RMS meter là FFT/spectrum |
| Module visibility | Tab/pinned tools và vị trí | FULL/TEXT/BASIC/BACKGROUND/OFF theo module | Off/FpsOnly/Compact/Detailed cho card; RAM/audio flags; backend rule có thể lấy samples riêng | Chưa parity per-module/background UI; không cần đủ hết để ra internal preview |
| Performance rules | Không phải điểm mạnh chính của panel | DebugPacket, AND/OR, comparers/metrics, once/repeat, log/screenshot/break/callback | Một low-FPS rule với warmup/sustain/hysteresis/cooldown và local warning | Sửa đường đo rule trước; generic rules để phase sau |
| Reporting | Vendor flow/form, custom handler | Screenshot action, không có relay/outbox như Dreamy | Async capture, redaction, bounded outbox/retry, Telegram relay, QA fake tools | Giữ backend; runtime package-support tab đã bỏ. Gmail pending |
| Release/ownership | Framework dịch vụ và helpers nội bộ | Singleton/module collectors riêng | Pure domain + Engine/Presentation/Foundation; scene stripping; samples editable | Giữ độc lập Core; không đổi nền kiến trúc trong cùng lượt chốt release |

## 3. So sánh giao diện và phần nên học

### Từ SRDebugger

**Nên giữ/học:** shell console/options/system, tab/pin/close rõ, console virtualization; Settings có General/Layout/Shortcuts/Advanced, preview vị trí pinned tools, phát hiện shortcut trùng, UI scale/opacity, enabled/default tabs, rich text/history cap và cursor/EventSystem policy.

**Dreamy đã cải thiện:** theme đỏ riêng; một logo active luân phiên header/rail; burger toggle ở mọi width; content co giữ close/pin trong màn hình; options flow thay vì ép mọi control một dòng; tap/PIN dài hơn; game-owned cheat services.

**Còn thiếu:** Settings vận hành chưa đủ parity SR. Các mục default/enabled tabs, trigger/pinned positions, shortcuts/conflicts, console settings, opacity/layer/camera và input ownership chưa được expose thống nhất qua project config/window. Có legacy capability không đồng nghĩa người dùng có flow cấu hình rõ.

**Nên bỏ/giữ ngoài runtime:** vendor signup/invoice/order/email, About/author promotion, setup/theme/acceptance buttons trong flow chính. Giữ notices/license và tooling Advanced. Không xóa đồng loạt class/meta chỉ vì tab không còn hiển thị.

### Từ Graphy

**Nên giữ/học:** phân vai màu good/caution/critical; chữ số và legend rõ; cập nhật text/graph riêng; module OFF/BACKGROUND rõ nghĩa; vị trí/scale theo module; short statistics vs historical statistics; reusable conditions/actions.

**Dreamy đã reuse thật:** G_FpsMonitor được thích nghi thành window dùng chung, GraphMobile shader + G_GraphShader helper, Roboto fonts và license. Không khởi tạo thêm Graphy singleton hoặc kéo nguyên asmdef có reference Input System vào package độc lập.

**Còn khác UI:** Dreamy là card ghép, chưa phải layout module độc lập của Graphy. RAM/audio còn renderer riêng. Labels/values dùng khoảng trắng trong cùng Text, dễ lệch khi số thay đổi; nên tách label/value thành cột có alignment. Chữ phụ/window legend ở màn nhỏ cần test đọc thật trên điện thoại; DPI compensation và SR retina scaling cần kiểm riêng. Ảnh Editor chưa chứng minh tương đương trên màn notch/high-DPI.

**Nên học tiếp:** module typography và layout theo hàng/cột, legend, live preview, collapse từng section. Không chép audio FFT/VR/XR/advanced HUD mặc định nếu game không cần. Screenshot action phải đi qua pipeline/policy chung; pause/break dành cho Editor/internal.

### Riêng cheat UI Dreamy

- Coin: hướng tới card `balance | amount | Add`, No Ads: `owned | Grant`, cùng một registry/wallet hiện tại.
- Pin đã có. Search/filter và collapse theo category, favorites riêng, inline/group/width metadata chưa hoàn thiện.
- Nhóm Debugger hiện chiếm nhiều diện tích trước game cheats (Build, HUD mode/position/reset). Có thể thu gọn vào Performance/System hoặc nhóm collapse; không xóa chức năng vừa thêm.
- Build GUID toàn số 0 trong Editor ít hữu ích ở đầu danh sách; nên dùng label Editor/version + copy ở System/Advanced.
- Chọn một body font/spacing thống nhất giữa panel và HUD. Logo nền trong suốt sẽ hòa vào header tốt hơn ảnh có nền đỏ riêng, nhưng không cần đổi art để chặn release kỹ thuật.

## 4. Finding chặn chốt candidate (P1)

### A1 — Register không an toàn khi observer lỗi — ĐÃ REPRO

`Runtime/Options/CheatRegistry.cs:16–29`: thêm options rồi gọi Changed trước khi trả Registration. Một subscriber ném exception làm caller nhận lỗi, nhưng options đã nằm trong registry và caller không có lease.

Probe: observer throw = true; registry còn **1** registration. Có thể gây duplicate khi re-enable, stale getter giữ owner cũ, và không cleanup được theo lifecycle bình thường.

Sửa: tách thành công của registration khỏi lỗi notification; isolate observer errors hoặc có rollback/lifecycle contract rõ. Test observer lỗi, observer tiếp theo, return lease và disposal; không chỉ test duplicate validation.

### A2 — Value refresh của typed cheats thiếu đường truyền — SOURCE + PROBE

`CheatRegistry.Set/Execute` không phát value signal. `OptionsPresentationBridge` tạo PropertyReference từ lambda, không giữ target INotifyPropertyChanged. `DataBoundControl` refresh khi bind/enable/notification; `OptionsTabController` refresh theo visibility/selection, không có polling định kỳ hiệu lực (queue flag không được set true trong source hiện tại).

Probe: action đổi balance **100 → 110**, registry notifications sau action = **0**. Điều này tự nó chưa là test pixel UI, nhưng chuỗi source xác nhận không có đường invalidation cho readonly balance/status đang mở. Sample README hiện nói controls poll values, cần sửa theo contract thật.

Sửa: value-change IDs, subscribe/dispose game notifications hoặc fallback refresh có nhịp khi visible. Sau Set/action, cập nhật các field liên quan. Không dùng structural Changed để destroy/rebuild toàn bộ list cho mỗi giá trị.

Test bắt buộc: panel đang mở, Add Coins/Confirm làm balance và LastResult đổi ngay; No Ads cập nhật owned; game tự tiêu tài nguyên cũng cập nhật; pin và relock vẫn đúng.

### A3 — Rule FPS dùng cửa sổ không phù hợp với cảnh báo ngắn — ĐÃ REPRO

`DebuggerRuntime.Update` lấy AverageFrameTime từ Graphy window 1024 frames cho threshold rule. Cửa sổ này phù hợp thống kê lịch sử, nhưng làm phản ứng chậm.

Mô phỏng managed: 1024 frame ở 60 FPS rồi 20 FPS; threshold 30, sustain 3s; warning phát sau khoảng **28,65s** kể từ lúc giảm FPS (warmup đã qua). Unit test FpsThresholdRule nhận trực tiếp fps thấp không thấy lỗi tích hợp này.

Sửa: chọn metric/cửa sổ thời gian ngắn được định nghĩa rõ cho rule, giữ long-window average/lows cho HUD. Test toàn tuyến sampler → rule với đổi mức FPS, recovery, cooldown, focus/loading và timeScale=0.

### A4 — Profiler legend còn nói Render/Update dù không có phép đo — SOURCE + PREFAB INSPECTION

`ProfilerServiceImpl` chỉ đẩy FrameTime và OtherTime; RenderTime/UpdateTime để 0. Prefab Profiler vẫn có hai legend Render/Update với toàn bộ ancestor active; graph control còn đọc các fields này.

Sửa trước khi giới thiệu như profiler: thu gọn thành frame-time graph đúng dữ liệu đang có hoặc tích hợp nguồn timing được xác minh. Không diễn giải 0 thành chi phí Render/Update thực và không gắn nhãn GPU timing.

### A5 — Version, tài liệu và lời hứa delivery lệch nhau — ĐÃ REPRO / SOURCE

- Manifest = preview.5; MCP đọc DreamyDebug.Version = preview.1; CHANGELOG chưa có thay đổi UI/Graphy/relay mới.
- README còn menu Control Center, Theme/Acceptance tabs và mô tả màu FPS cũ.
- Editor ReportPage nói destination delivery được xác nhận bằng relay response, trong khi client chỉ nhận durable relay acceptance; đích Telegram có thể còn retry/fail.
- Support/tracking vẫn nói chưa có Telegram thật, nhưng parent evidence có một lần live destination delivered ngày 2026-11-04. Phải ghi đây là historical environment-specific receipt, không tự nâng thành fresh release acceptance.

Sửa: một candidate version thống nhất; changelog/migration/support matrix theo source; UI phân biệt Saved locally / Relay accepted / Destination delivered, chỉ nói Delivered khi có trạng thái đích thật. Không thêm email/URL author giả; author name hiện đã đúng Dreamy Game Studio.

### A6 — Candidate chưa có artifact/build proof mới — CHƯA NGHIỆM THU

93 tests khớp code hiện tại; Linux/Android receipts còn lại thuộc preview.1. Các thay đổi mới thêm shader, fonts, reflection adapter và prefab/navigation chưa có player/IL2CPP receipt tương ứng. link.xml preserve cả Presentation/Foundation/Engine nhưng không tự preserve game-owned partial members ở assembly của game.

Cần test candidate tarball/UPM import + sample import, build development/release, shader inclusion, IL2CPP reflection và stripping. Android/iOS runtime support chỉ công bố theo bằng chứng đã có; internal Editor-only preview có thể ghi giới hạn platform rõ ràng.

## 5. Rủi ro cần kiểm và phần tối ưu (P2)

| Vấn đề | Điều đã biết | Việc cần làm |
|---|---|---|
| Shader unsupported | GraphyHudGraph.Configure return sớm khi shader null/unsupported, Image mặc định vẫn enabled; chưa có fallback state rõ | Test unsupported path; graph ẩn/collapse và vẫn giữ text, tránh ô trắng/màu lỗi |
| Hot reload/disable-enable | Lượt UI trước có service bị cleanup; current ServiceManager có latch cleaning, OnDisable cleanup, kế thừa singleton OnEnable | Test enter/exit Play 2×2 options, script reload, disable-enable; không coi destroy-order test là đủ |
| UI structural rebuild | Registry Changed → bridge thay toàn container → OptionsTabController Clear/Populate | Profile 20/200/1000 options, batch registrations; tách structural/value notifications rồi diff/pool nếu đo thấy cần |
| Bad getter | DataBoundControl gọi getter ngoài try block xử lý UI | Một getter lỗi cần hiện unavailable; không làm hỏng toàn panel/registration. Test getter throw, owner mất và readonly stale |
| Screenshot overlap | CaptureCoordinator có busy nhưng CaptureHiddenAsync mỗi caller tự hide/restore HUD/panel | Rủi ro caller busy/fallback restore overlay khi caller khác còn capture. Test manual + automatic chồng nhau; dùng suppression scope/refcount/single owner nếu tái hiện |
| Capture cancel/disable | Timeout hoàn thành task; state busy phụ thuộc coroutine/readback/encode callback; OnDisable chỉ unsubscribe low-memory | Test không có end frame, disable/destroy khi chờ frame/readback/encode, resume/retry; không để capture kẹt busy |
| CPU copy / texture cost | Readback callback GetData.ToArray copy trên main thread; source RT full-resolution trước downscale; JPEG ở worker | Đo từng đoạn, cold/warm p95/p99, raw/VRAM/GC; async không đồng nghĩa toàn pipeline không giật |
| HUD shader allocation | Configure khởi tạo graph material ngay cả khi mode đầu chỉ FPS text | Cân nhắc lazy-init khi graph thật sự bật sau khi đo cold cost; giữ một material owned, cleanup và không tạo lại theo mỗi update |
| Metric presentation | Text current FPS lấy last-frame tại nhịp 4Hz; avg/lows là cửa sổ 1024 | Định nghĩa current/smoothed/history, warmup hiển thị rõ; tránh QA nhầm một frame do Editor/tool stall là FPS của build |
| Input/touch | Đã có raycast burger và một-pointer grip test, clamp corners | Test nhiều pointer/cancel/focus loss, notch/rotation/high DPI, keyboard/IME, new/legacy input, back/ESC, HUD không chặn gameplay |
| Catalog và build size | Catalog vẫn tham chiếu BugReportPopover/Sheet/Tab dù navigation đã bỏ; linker preserve-all ba assembly | Audit refs và built player content trước khi tách optional assets/narrow preserve. Không xóa meta/GUID hoặc cắt helpers hàng loạt |

704 KiB Foundation source, 3.8 MiB Presentation assets/source và khoảng 392 KiB Graphy visual files chỉ là dung lượng trên đĩa. APK ~248 MB trong receipt cũ là **toàn consumer build**, không phải overhead của debugger. Đo A/B build cùng config mới kết luận được.

## 6. Production reporting là gate riêng

Hiện có AllowScreenshots, SensitiveScreen hook, UploadsEnabled và configuration snapshot; chưa có contract hoàn chỉnh cho consent/withdrawal, recheck policy trước capture/persist/upload, cancellation và purge outbox khi quyền thay đổi, region masking. Không suy rằng một checkbox asset là production policy hoàn chỉnh.

Trước production: host policy có trạng thái rõ; sensitive screen/masking; hủy capture/upload đang chạy; xử lý ảnh/log đã xếp hàng khi withdrawal; retention và quyền truy cập relay; offline/restart/low-memory tests. Không cần thêm Gmail vào release để lấp chỗ này. Native crash/OOM/ANR không đảm bảo ảnh mới; test bằng build riêng và SDK/OS evidence tương ứng, không cố crash scene người dùng.

## 7. Bộ test/gate đề nghị trước release

| Gate | Tình huống trọng tâm | Bằng chứng cần có |
|---|---|---|
| Registry & bindings | Observer throw; getter throw; Add Coin/No Ads/live game changes khi panel vẫn mở; owner disposal | Regression EditMode/PlayMode; không options mồ côi hoặc UI stale |
| Metrics correctness | Stable → low FPS qua sampler; recovery/cooldown; focus/loading; labels theo dữ liệu thật | Deterministic integration tests + UI inspection |
| Navigation/HUD | 390×844, landscape/tablet; high DPI/notch; 4 modes; RAM/audio combinations; drag/reset/rotation/cancel; actual pointer raycast | Screenshots + input assertions; device touch smoke |
| Lifecycle | Domain reload on/off × scene reload on/off; repeated Play; scene transition; disable-enable; hot reload | Không trùng service/event/option, không exception/leak sau vòng lặp |
| Capture/relay | Overlap, cancel/disable, GPU failure, sensitive policy changes, offline/restart, 429/5xx/ack loss/destination failure | Fake server + exact saved image inspection, resource counters; real send chỉ khi được chỉ định |
| Candidate import | Packed artifact consumer, sample single asmdef, docs links, GUID co-existence với SR, không Core/UI/InputSystem | Artifact SHA + compile/tests trên artifact đã giải nén |
| Build/stripping | Dev UI; release-off; ReleaseDiagnostics text-only; Android IL2CPP/high stripping; built-in/URP shader refs | Fresh BuildReports + packed asset/code audit theo từng mode |
| Performance | Debugger Off / FPS / Detailed; log flood; nhiều options; capture/send khi gameplay chạy | Target-device CPU p50/p95/p99, GC, memory peak, draw calls, frame spikes; xác định budget theo game/hardware |

## 8. Nên giữ, nên bỏ và thứ tự làm tiếp

**Giữ:** console SR, typed authorization/leases/confirmation, project-owned partial/delegate cheats, đỏ thương hiệu và metric colors riêng, một sampler, Graphy shader/font, HUD Off và grip-only raycast, async text-first diagnostics/outbox, fake QA flow.

**Học tiếp:** SR Settings/shortcuts/layout preview và value notifications; Graphy module typography, per-module state, metric semantics và rule model. Chỉ thêm phần phục vụ game thật.

**Bỏ khỏi flow chính:** author/vendor marketing/signup/email form; runtime package support reporting; demo/theme/setup/acceptance buttons sau setup; duplicate controls, build GUID lớn ở đầu game cheats. Giữ notices và Advanced tools. Audio FFT/VR/XR/generic rule builder/favorites là optional backlog, không phải điều kiện tự động để preview này release.

**Không đổi dependency Core ngay trước release.** Debugger cần dùng được khi bootstrap/Core có vấn đề. Game host đang nối service Core vào cheat bằng delegate. CoreIntegration là đề xuất tương lai nếu nhiều game lặp glue; chưa có adapter package đó trong bản hiện tại. Thay service framework phải là migration riêng có lifecycle/AOT tests.

Thứ tự:

1. Sửa A1/A2/A3/A4 và các đường lifecycle/fallback cần tái hiện; thêm tests theo luồng thực tế.
2. Chốt contract tính năng preview, cập nhật README/CHANGELOG/package metadata/VersionInfo/TRACKING/SUPPORT/MIGRATION/sample docs và relay status. Giữ author Dreamy cùng notices SR/Graphy/Roboto đầy đủ; xác nhận phạm vi phân phối nội bộ phù hợp với nguồn vendor.
3. Bump candidate preview.6 đồng bộ, tạo artifact cố định, chạy tests/build trên artifact; ghi hash/commit và Known Issues.
4. Khi gates nội bộ pass mới chốt private/internal preview. Stable/production chờ thêm device, profiling, privacy và native fault acceptance.

Audit này hoàn tất; **chưa sửa runtime, chưa bump version, chưa chốt release**. Các lỗi mới không bị che bởi receipt 93 tests cũ. Source/runtime/prefab vẫn giữ nguyên trong lượt phân tích.


## Kết quả sau audit

Nhóm A1–A6 và các sửa lifecycle/fallback đã hoàn tất trong candidate preview.6; xem [checkpoint nghiệm thu](DREAMY_DEBUGGER_RELEASE_CANDIDATE_2026-11-05.md). Nhận định HOLD ở trên là thời điểm audit trước sửa. Candidate nội bộ hiện qua tests/build; stable/production và device/privacy gates vẫn mở.
