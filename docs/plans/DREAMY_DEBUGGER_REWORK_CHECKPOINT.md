# Dreamy Debugger — checkpoint triển khai

Ngày: 2026-11-04. Unity: 6000.4.12f1, Linux.
Trạng thái: implementation checkpoint; toàn bộ P0–P6 chưa được nghiệm thu.

## Thay đổi đã áp dụng

- P1: window chính còn General/Access/Options/Performance/Diagnostics. Bỏ tab Theme/Acceptance và nút mở demo; dựng visual chuyển Advanced. Starter setup chỉ hiện khi chưa có config. Bootstrap action chỉ hiện nếu scene thiếu runtime.
- P1: palette đỏ chủ đạo trên nền charcoal, metric có màu riêng; tắt các node Logo/DreamyBanner/DreamyBrand trên runtime panel, footer promo và About tab. Giữ asset/node không hoạt động để bảo toàn reference. Giữ copyright và notices bên ngoài runtime UI.
- P2: thích nghi **Graphy G_FpsMonitor**, commit `1068f3de2bc9f1e4a905150abd59f31e6dead679`, MIT. Một sampler cung cấp average và 1%/0.1% low; low metrics tính mỗi 30 samples, ring 1024. Chưa vendor shader/font/prefab đầy đủ của Graphy.
- P2: HUD 4 góc + Free, offset/scale, vị trí ban đầu, đổi/reset qua Options; màu/ngưỡng FPS riêng, RAM/audio opt-in trong Detailed, audio mặc định tắt. HUD nằm trong SafeArea hiện có. Collector FPS chỉ chạy khi HUD, profiler tab hoặc FPS rule cần dữ liệu.
- P3: `AttributedCheatOptions.From` chuyển Category/DisplayName/Browsable/Sort/Range/Increment/readonly/action qua CheatRegistry. Mutation vẫn kiểm access, action reflected luôn cần confirmation. Bổ sung byte/sbyte/short/ushort/uint cho adapter, bên cạnh các kiểu typed cũ.
- P3: sample `GameDebugOptions` và `.Economy` là partial **cùng game asmdef**, constructor nhận delegate game. Runtime package không tham chiếu game. Sample là code người dùng có thể sửa; Runtime là maintained package code.
- P4 bước đầu: action/bool/enum dùng chiều rộng nhỏ hơn qua FlowLayoutGroup sẵn có; numeric/text giữ rộng hơn. Chưa có resource card Inline, search hay virtualization mới. Coin/No Ads sandbox tiếp tục đi qua service wallet/entitlement cũ, không đổi ID hay save.
- P5: rule FPS unscaled, warmup 5s tính từ lúc rule khởi tạo, sustain/cooldown cấu hình, recovery +5 FPS/2s, tối đa 10 lần/session; chỉ log local, không tự capture/send. Hàng đợi error thread-safe cap 32, drop oldest/counter, bounded message/stack. Chỉ bỏ origin `[DreamyDebugger.Report]`, không bỏ mọi lỗi `[Dreamy]`.
- Diagnostics: ẩn email input legacy và ngừng lưu email vào PlayerPrefs; Gmail tiếp tục pending.

## Bảo toàn và evidence

- Snapshot trước sửa: `/tmp/dreamy-rework-baseline.zip`; danh sách thay đổi có sẵn trước task: `docs/evidence/debugger/rework/baseline.txt`.
- Không chỉnh source/prefab/meta của `Assets/Packages/StompyRobot` trong đợt này. SR settings đã dirty từ trước, không revert.
- `asset-audit.json`: số node branding giữ nguyên (Logo 1, DreamyBanner 1, DreamyBrand 2); tất cả tắt bằng Unity API. Không đổi GUID meta có sẵn so với snapshot.
- Editor log ghi nhiều lần builder hoàn tất, mỗi lần **28 prefabs, no missing scripts**. Thư mục staging đã được chuyển về package.
- Consumer độc lập `/tmp/dreamy-debugger-consumer-final` chỉ có debugger và Unity Test Framework; không có Core/UI/InputSystem/Graphy package ngoài.
- Test receipts cuối cùng được chép vào `docs/evidence/debugger/rework/`; xem `verification.json` thay vì dùng log cũ để kết luận.
- Không gửi report thực và không flush outbox trong đợt này; không đổi cấu hình Telegram/token/chat ID.

## Gate còn mở

1. Đã xem ảnh Dreamy fixture offline ở 320×640, 960×540, 1024×768 và HUD; lưu cùng evidence. Còn ảnh baseline SR cùng preset, device safe-area/touch/interaction thật. Fixture dùng giá trị synthetic, không phải sandbox wallet hay chứng nhận hiệu năng.
2. Full settings parity SR: Layout/Input, pinned UI, shortcuts/collision, console settings, enabled/default tabs cần expose đầy đủ và có ledger riêng. Settings window hiện chưa đạt parity toàn bộ.
3. Graphy UI/shader selective reuse còn là spike. Renderer hiện vẫn là HudHistoryGraphic của Dreamy; chưa có bằng chứng CPU/GC/GPU tốt hơn Graphy hoặc thiết bị yếu.
4. Attribute adapter chưa có Description tooltip/INotifyPropertyChanged forwarding; polling legacy còn dùng. Cần matrix IL2CPP/linker/domain reload. Partial sample chưa thay catalog typed đang dùng trong sandbox.
5. P4 cần metadata width/row/group, resource inline card, filter/search và pooling khi nhiều options.
6. P5 mới là rule FPS đơn: chưa có generic AND/OR metric rules, callbacks/bookmark, loading suppression do host cung cấp hay capture/report actions.
7. P6 chưa làm: consent/sensitive-screen/masking contract, withdrawal/cancel/purge outbox, native fatal crash evidence và đo Android thực tế. Release chưa được nghiệm thu; checkbox AllowScreenshots không thay cho consent của host.

## Lỗi và lessons trong lượt này

- NUnit và ComponentModel có Category trùng tên: dùng tên đầy đủ trong fixture.
- Test truy cập controller kế thừa Foundation cần reference Foundation trực tiếp trong test asmdef; đã bổ sung.
- Builder move/import nhiều assets gây main Editor rất chậm trên volume hiện tại. Builder đã thu hẹp staging sang Presentation/UI, Presentation/Assets và Prefabs, không di chuyển source/asmdef nữa. Không chạy consumer tests cùng lúc builder move thư mục, không coi MCP timeout là builder fail nếu asset restore/log receipt chứng minh đã hoàn tất.
- Script runner đã sửa để xóa XML cũ trước mỗi run: compiler failure không được tái dùng receipt Passed của lần trước.
- Workflow được AGENTS trỏ tới `docs/agents/dreamy-workflow.md` không tồn tại và dreamy CLI không có; checkpoint này là fallback thủ công.

## Receipt cuối lượt

- Fresh consumer: **72/72 EditMode, 18/18 PlayMode pass**, 0 failed/skipped. XML và fingerprints trong evidence.
- BootstrapScene đã mở lại không dirty sau cả visual và shutdown fixture. Main Editor còn reload assembly sau sửa cuối; receipt compile/test cuối là consumer riêng. Editor layout/Simulator và Game view selection được khôi phục; các size test tạm đã xóa.
- Đã phát hiện bằng ảnh và sửa thêm header đỏ hardcoded, VerticalLayoutGroup ghi đè FlowLayoutGroup, cùng Summary HUD thiếu chiều cao cho 1%/0.1% low.
- Tình huống đổi vị trí HUD khi HUD đang Off cũng đã có test: khi bật HUD dùng đúng vị trí vừa chọn.
- Phát hiện thật khi thoát Play Mode: service đã shutdown trước runtime làm OnDestroy NullReferenceException; đã sửa null-safe ở runtime/option bridge/HUD/sampler. Thêm regression service quit → root destroy → runtime destroy; receipt mới pass. Cleanup giữ reference root từ Awake, không gọi Hierarchy.Get để tránh sinh root mới trong OnDestroy.
- Đợt sửa không commit/push; giữ các thay đổi của người dùng có trước.

- Những lỗi shutdown quan sát trong Editor không bị coi là đã pass theo các receipt cũ: sau sửa đã chạy lại consumer, 72 EditMode + 18 PlayMode pass (10:36 UTC); fingerprint khớp source cuối.
- Main Editor có thể vẫn đang compile/reload source cuối; không lấy timeout MCP làm bằng chứng compile thất bại, và không gọi đó là receipt compile main thành công.

## Điều chỉnh theme theo yêu cầu tiếp theo

Giữ đỏ làm màu nhận diện chủ đạo: Brand #9E1008, Accent #E95345, Selected #572A2A; nền và controls thường vẫn charcoal. Header/tab/selection dùng palette đỏ. FPS series vẫn teal, RAM reserved tím và good/caution/critical theo setting; tách metric khỏi brand để thay theme không đổi ý nghĩa dữ liệu. Không bật lại logo/banner/About. Đây là chỉnh màu sau receipt 90 test của đợt trước; không coi fingerprint/ảnh cũ là receipt cho màu mới.

Nghiệm thu chỉnh theme: Apply thành công, 28 prefab không Missing Script; API đọc header thực tế #9E1008 và FPS series #60C8AA; scene không dirty. Consumer compile + 6/6 ReworkTests pass. Receipt riêng `red-theme.json` / `red-theme-EditMode.xml`; ảnh và fingerprint đợt trước không đại diện cho màu mới.

## Runtime UI 2026-11-05 — hoàn tất

Yêu cầu logo góc trái, bỏ report package khỏi runtime, sidebar toggle và HUD Off/drag/Graphy visuals đã hoàn tất ở [checkpoint riêng](DREAMY_DEBUGGER_RUNTIME_UI_2026-11-05.md). Receipt mới: 72 EditMode + 21 PlayMode pass; 28 prefab valid, 0 GUID đổi; scene/layout được khôi phục. Thay thế các ghi chú cũ về không có logo runtime và chưa tái sử dụng renderer/font Graphy. Các gate production/device của plan tổng vẫn mở.
