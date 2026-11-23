# Scene Workspace V2 — design checkpoint

Cập nhật ngày 2026-11-23. **Hoàn tất phần UI tinh gọn và chỉnh sửa runtime trong phạm vi concept đã duyệt.**

- **Dùng thử** mở `Assets/_Project/Scenes/HierarchyInspectorLab.unity`, bấm Play và nhập PIN lab `0000`. Inspector tự chọn Physics cube và bật quyền chỉnh sửa.
- **Package** Sandbox dùng commit Debugger [`d823717`](https://github.com/Dreamy-Game-Foundation/com.dreamy.debugger/commit/d823717762592699d48f13647452bd2ac4c54ae6). Đây là commit tính năng trên main của package.
- **Kiểm chứng mới nhất** Unity 6000.4.12f1, 39/39 EditMode, 9/9 PlayMode. Lab đã được thử trực tiếp qua MCP. [Receipt](../../evidence/debugger/hierarchy-v2/simplified-ui.json)
- **Còn lại** kiểm chứng touch, bàn phím ảo, safe area và player build trên thiết bị thật. Các thao tác cấu trúc như Add/Remove Component, multi-edit và reparent thuộc P2.

Các mục bên dưới giữ lại lịch sử theo từng lần triển khai.

## Concept tinh gọn trong Unity

- Đã đưa concept đã duyệt vào các prefab Hierarchy. Active, Tag và Layer ở đầu Inspector. Component dùng một hàng gập mở với icon. Copy/Pin và tìm thuộc tính mở theo nhu cầu từ menu. Refresh, Undo, Redo và các lệnh còn lại nằm trong cùng menu.
- Số và vector chỉnh trực tiếp trên desktop với một vùng Apply/Cancel chung. Chạm một trục trên mobile mở bảng số riêng cho trục đó. Dọc và ngang thấp dùng một pane. Rigidbody mở sẵn Mass, Use Gravity và Is Kinematic, cùng mục More properties cho các trường còn lại.
- Giữ bộ icon Dreamy, vẽ glyph Rigidbody và Box Collider bằng hình học, giữ GUID của các asset. Gallery Unity Editor Icons chỉ dùng để tham khảo.
- Đạt 39/39 EditMode và 9/9 PlayMode. Chạy Lab trực tiếp qua MCP, mở khóa, đổi Mass từ 1 lên 2,75, Undo về 1 và mở picker Layer. Đã xem render 1280×720, 390×844, 844×390. [Receipt](../../evidence/debugger/hierarchy-v2/simplified-ui.json)

## Concept tinh gọn

Đã tạo [prototype tương tác](concept-simplified.html) và [ghi chú thiết kế](SIMPLIFIED_CONCEPT.md) theo yêu cầu duyệt concept trước. Giữ palette Dreamy và luồng cây → object → component → giá trị. Đã xem render desktop, dọc và ngang thấp. Tại thời điểm duyệt bản HTML, Unity còn ở checkpoint P1. Mục trên ghi kết quả triển khai sau duyệt.

## P1 · Chỉnh sửa và đối chiếu runtime

- Undo/Redo giữ 64 lệnh chỉnh trực tiếp, kiểm tra quyền và giá trị hiện tại, reset khi unlock phiên mới. Undo của nested member và phần tử collection được để lại vì identity có thể đổi khi game chạy.
- Picker Tag tìm theo catalog, Color có swatch và slider RGBA, Flags giữ bit ngoài enum, reference chỉ nhận kiểu tương thích trong scene đang load. Layer vẫn dùng danh sách layer có tên. Reveal và Copy hoạt động từ hàng thuộc tính.
- Hierarchy nhận `type:`, `tag:`, `layer:`. Pin đưa field lên đầu Inspector của object. Compare lưu giá trị ở các hàng hiện có, ghép component theo type và occurrence. Cấu hình project hỗ trợ icon, nhãn, nhóm và chọn built-in drawer.
- Add/Remove Component, multi-edit và reparent đã được đánh giá và chuyển sang P2. Thiết bị thật, IME và build chưa được kiểm chứng trong lượt này. [Plan và quyết định](../../plans/DREAMY_DEBUGGER_HIERARCHY_LAB_AND_MOBILE_UX.md) · [Receipt P1](../../evidence/debugger/hierarchy-v2/p1.json)

## Hierarchy Lab và mobile UX

- Đã triển khai P0 của [plan Hierarchy Lab](../../plans/DREAMY_DEBUGGER_HIERARCHY_LAB_AND_MOBILE_UX.md). Mở `Assets/_Project/Scenes/HierarchyInspectorLab.unity`, Play và nhập PIN lab `0000`. Tab tự mở ở Physics cube. Lab có 33 object author sẵn, một EventSystem lúc chạy và cấu hình Development riêng.
- Khi quyền sửa được cấp bởi cấu hình Development/QA và phiên đã unlock, badge **Editing** hiện ngay. Quyền ghi bị khóa khi relock. Scene lab chạy độc lập, có Simulate, Play tone, Spawn, Reparent, Destroy và Reset samples.
- Mouse/trackpad giữ kéo nhãn. Touch kéo để cuộn và tap vào số mở editor có bước chỉnh, ±, trục XYZ(W), nhập chính xác và Apply/Cancel. Các trường chuẩn hóa có slider 0–1. Draft tồn tại qua chuyển layout dọc/ngang và bị hủy khi mất quyền.
- 23/23 kiểu component trong Lab có sprite phù hợp theo audit runtime. Catalog prefab chứa 41 sprite, gồm icon tùy chỉnh cho script Lab. Unity kiểm tra 35 prefab không thiếu script.
- 9/9 PlayMode và 18/18 EditMode đạt. Luồng PIN thực trong Lab chọn đúng cube. Mô phỏng touch xác nhận Mass giữ 1 trước Apply, đổi thành 2,75 sau Apply và relock chặn ghi. Ảnh ở 390×844 và 844×390 đã xem. Chưa chạy xác nhận thiết bị thật, IME hoặc player build. [Receipt](../../evidence/debugger/hierarchy-v2/lab-and-touch.json)

## Sửa quyền chỉnh sửa và mở rộng Inspector

- Plan tiếp theo đã hoàn tất ở mức thiết kế, gồm auto-edit, scene Lab, checklist icon và tương tác touch. [Hierarchy Lab và mobile UX](../../plans/DREAMY_DEBUGGER_HIERARCHY_LAB_AND_MOBILE_UX.md)

- Tái hiện trong Play Mode từ BootstrapScene. Runtime tạo tự động đọc `Assets/DebuggerConfiguration.asset` với `AllowSceneEditing` tắt. Cấu hình `SandboxDebuggerConfiguration.asset` đã bật ở lượt trước thuộc một prefab khác.
- Bật quyền trên asset đang dùng và đổi nhãn thao tác thành **Enable editing**. Kiểm tra luồng mở Hierarchy, xác nhận quyền và nhập Mass. Giá trị Rigidbody đổi từ 1 thành 2,75 qua nút Apply của Inspector đang chạy.
- Component header, cây và dropdown dùng sprite cho foldout. Thêm sprite theo loại component, gồm các hình collider phổ biến và Rigidbody 2D. Các kiểu riêng có thể đặt sprite theo tên kiểu đầy đủ trong `HierarchyComponentIcons`.
- Mở rộng thuộc tính runtime cho Camera, Light, physics 2D/3D, AudioSource, Animator, renderer, Canvas và các control uGUI thông dụng. Picker reference, custom editor drawer và các thao tác authoring asset vẫn nằm ngoài phạm vi đã hoàn thành.

## Implementation cập nhật

- Đúng MCP instance `dreamy-package-sandbox@5f78127b991a2b92`, Unity 6000.4.12f1, local debugger path đã xác nhận. Không thao tác project planet-bunker đang mở khác.
- Đã lưu lại ba prefab bằng maintainer Editor authoring tool, giữ GUID/catalog key/enum cũ; label thành Hierarchy và style icon cây riêng. Thêm sprite cây/cube original, Camera dùng glyph Material hiện có.
- Toolbar/nút copy gọn, tên object một dòng, bỏ type/ID/hint lặp, tree có icon và inactive/selection. Wide Inspector inline, narrow stacked; compact navigation khi width <760 hoặc height <360 Canvas units. Splitter có minimum pane. Có ảnh runtime cả dọc/ngang và tablet.
- Vector2/3/4/VectorInt có từng ô X/Y/Z/W. Scalar name và axis label kéo ngang preview, thả commit; Shift fine. Vertical drag chuyển cho ScrollRect. Mất access/focus/hide/unbind/cancel dừng gesture. Scalar có ±; integer step 1, floating 0.1, checked overflow, không qua double để lưu long/ulong.
- Active checkbox qua explicit SetActive adapter. Tag nhập tên project tag hiện có, Layer dropdown từ LayerMask.LayerToName; chỉ thay object đang chọn, không recursive children. Access/conflict/nested write-back đi qua InspectionMember gateway.
- Bật `AllowSceneEditing` trên `Assets/_Project/Debugger/SandboxDebuggerConfiguration.asset` cho sandbox Development để dùng thử; vẫn cần unlock và confirm Edit. Package default vẫn off.
- 18/18 targeted EditMode và 8/8 PlayMode pass; receipts [ở đây](../../evidence/debugger/hierarchy-v2/). Console cuối không có errors, 35 prefab không missing script, catalog có đúng một entry Scene. BootstrapScene cuối clean, Play Mode stopped.
- Ảnh runtime camera qua public world-space mode ở 1920×1080, 1280×720, 390×844, 844×390, 1024×768, 768×1024 đã được xem. Không phải physical touch/IME/safe-area evidence. Visual review run `2d606999-2cab-4a30-b9b8-ff57ca91b36d` lưu ảnh ngoài Git trong `.dreamy/visual-review/`.
- Unity-generated YAML/meta trailing whitespace được normalize sau test; C# bytes không đổi. Source fingerprint và normalization receipt đi cùng test jobs. Demo PlayMode sinh dynamic TMP atlas ngoài scope; đã giữ backup ở `/tmp/dreamy-hierarchy-generated-font-atlas.asset` và trả source asset về baseline sạch ban đầu. Không sửa saved scene.

### Phạm vi của lần triển khai V2 đầu tiên

Các mục chưa có ở lần triển khai đầu được cập nhật theo hai checkpoint P0 và P1 ở trên. Continuous live preview, polish scene-header/filter-menu và các gate thương mại, thiết bị, hiệu năng vẫn được theo dõi trong plan V2.

## Baseline và phạm vi

- Parent sandbox: `b79b485`, debugger: `8593c75b4f17110fdbad09770bcd852e5b2785df`, manifest version `0.1.8`.
- User yêu cầu thiết kế đẹp, dễ dùng như Hierarchy/Inspector thực; chỉnh nhanh từng ô/axis, nghiên cứu kỹ UnityRuntimeInspector, icon/sprite phù hợp thương mại và plan chi tiết.
- Steering mới nhất: bắt buộc chú ý màn hình **dọc, ngang và responsive**, bao gồm xoay/resize, safe area và keyboard.
- Chỉ sửa plan và tạo prototype HTML/checkpoint trong sandbox. Chưa sửa C#, prefab, package assets/version/tag hay chạy Unity build/test.

## Hoàn tất trong nhiệm vụ design

### Revision theo chỉ đạo đồng nhất theme

- Theme concept và plan hiện dùng Material Compact hiện có: Background `#101216`, Surface `#1C1F25`, Control `#272C34`, Accent `#CB465D`, Selected `#39212C`; thay hướng xanh lam của revision đầu.
- Tab hiển thị **Hierarchy**, icon cây riêng; plan giữ key/address/enum Scene và GUID để không phá consumer. Việc đổi nhãn/icon trong prefab Unity vẫn thuộc implementation tiếp theo.
- Concept dùng Barlow hiện có (nhúng cùng OFL notice), icon SVG generic tự vẽ cho tree/cube/camera/light/layout/Transform/script. Không copy artwork Unity từ gallery. Font số của HTML có monospace fallback; Unity vẫn dùng JetBrains Mono sẵn có.
- Bỏ metadata/hint/counter thường trực, đưa tag/layer vào Details, ẩn footer idle; giữ lỗi/draft/conflict theo ngữ cảnh. Portrait/landscape rules vẫn giữ.
- Đã tra gallery jasursadikov/unity-editor-icons và README: là icon built-in Unity Editor; chưa xác nhận quyền bundle PNG cho sản phẩm bán. Plan chọn tương đương có provenance rõ, không dùng EditorGUIUtility trong runtime.
- Đã xem render desktop mới 1360×960 sau đổi theme/icon. Đây là concept, chưa phải UI package đã thay.

- Viết lại [plan](../../plans/DREAMY_DEBUGGER_RUNTIME_HIERARCHY_INSPECTOR_2026-11-23.md): concept Graphite Studio, design tokens, source research, desktop/portrait/low-landscape/tablet, orientation transition, typed drawers, transaction/precision/history, asset ledger, phased work và acceptance.
- Đọc source UnityRuntimeInspector tại `2d461b975725f4713aee5eb3857661e361ab8d27`, gồm hierarchy, numeric/range/vector/Transform/color/reference, bound input, skin và recycled list; phân biệt hành vi upstream với đề xuất Dreamy mới.
- Tra nguồn license MIT/BSD upstream, Apache Material icons, MIT Tabler và ISC/MIT Lucide. Không import các tài nguyên này vào package trong nhiệm vụ design.
- Tạo [concept tương tác](concept.html) tự chứa, minh họa selection/search/foldout, numeric fields, stepper, desktop scrub, range và two-page responsive. Dữ liệu giả, không nối runtime.
- Đã xem render HTML desktop 1360×960 và portrait 390×844; rà lại portrait Inspector sau chọn Player. Nhận thấy vùng axis nhỏ bị chật bởi ±, đã bỏ inline ± trên phone nhỏ; kiểu sheet đầy đủ nằm trong plan.
- Đã xem lại render sau sửa ở portrait 390×844 và landscape thấp 844×390; bản ngang thấp chuyển two-page compact, bản dọc giữ axis fields và bỏ inline stepper chật. Đây là browser resize/render, chưa phải rotation của ứng dụng Unity.
- `git diff --check` đạt; các link nội bộ trong plan tồn tại. Submodule debugger không có thay đổi cục bộ. Reload HTTP helper từng gặp `ERR_CONNECTION_REFUSED` sau gián đoạn phiên; đã mở trực tiếp file HTML và lấy render mới thành công.

## Giới hạn kiểm chứng

HTML render chỉ minh họa concept. Nó chưa mô phỏng toàn bộ scene tree expand, commit/conflict/history, color picker, safe area/IME hay numeric sheet. Pointer scrub HTML chỉ có preview đơn giản. Chưa chứng nhận Unity layout, touch thật hoặc device performance.

Kết quả V1 giữ nguyên ở [checkpoint cũ](../../evidence/debugger/runtime-inspection/CHECKPOINT.md); không dùng 102/102 EditMode và 5/5 focused PlayMode đó để đóng gate V2. Build thêm vẫn ngoài chỉ đạo hiện tại theo yêu cầu trước của user.

## Tiếp tục implementation

1. Preflight đúng Unity instance và project path; đo live CanvasScaler/available rect/SafeArea, dirty scene và prefab stage.
2. Author tree/header và lát cắt Numeric + Transform theo concept ở portrait, landscape rộng/thấp và tablet, giữ serialized references.
3. Xây transaction/access/conflict/read-back foundation trước khi nối quick-edit gesture vào gameplay.
4. Kiểm tra state qua rotation/resize/keyboard, pointer thực và pool lifecycle; chụp Unity renders mới, ghi receipts theo source chính xác.
5. Mở rộng drawer sau khi lát cắt đạt visual và interaction acceptance. Commercial redistribution audit của Imported Presentation vẫn là gate phát hành.
