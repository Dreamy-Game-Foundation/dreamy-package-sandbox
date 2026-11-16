# Kế hoạch cải thiện Dreamy Tutorial cho nhiều thể loại game

Ngày: 2026-11-16. Trạng thái: đã triển khai phần package M1–M5 ở source 0.3.0; M6 giữ điều kiện theo nhu cầu. Consumer độc lập đạt 56/56 EditMode và 34/34 PlayMode. Visual/device gates còn thiếu được ghi rõ trong [evidence](../evidence/tutorial-extensibility/README.md).

## 1. Mục tiêu và baseline

Mục tiêu là một package tutorial tái sử dụng giữa puzzle, action, idle, simulation và các game có UI/world khác nhau. Tích hợp game mới chủ yếu bằng config, view, target adapter và gameplay bridge; không thêm nhánh theo thể loại vào model.

Baseline kiểm tra: `com.dreamy.feature.tutorial` 0.2.0, Unity 6000.4.12f1. Bộ test hiện tại chạy lại thành công: 33 EditMode và 17 PlayMode. Ba probe riêng tái hiện: mất pending completion sau controller reopen; callback không hợp lệ xóa retry; 3D target query sai physics scene trong local physics scene. Receipt và probe nằm tại `/tmp/tutorial-review-20261116`; đây là evidence tạm thời, cần đưa regression phù hợp vào package khi triển khai.

Workflow và compatibility registry được AGENTS tham chiếu chưa có trong workspace. Bản kế hoạch này dựa trên code/manifests và consumer đã chạy; không suy rộng thành tương thích mọi Unity 6, player hay thiết bị.

## 2. Kiến trúc đích và quyền sở hữu

| Phần | Sở hữu | Điểm mở rộng |
| --- | --- | --- |
| Config | Flow, step, ID, message key, khai báo điều kiện và target | Dữ liệu có version, validator |
| Model/service | State, token, pending transition, checkpoint, kết quả command | Contract nhỏ theo capability |
| Presenter | Ánh xạ state sang view, intent và snapshot target | Custom view, presentation options |
| Unity integration | Lifecycle, target UI/2D/3D, projection và UI raycast | Adapter theo môi trường |
| Game | Gameplay authority, spawn, damage, economy, input thật, scene, camera, localization | Signal/condition bridge và policy |
| Observer | Diagnostics, log, analytics adapter | Subscribe sự kiện có cấu trúc |

Runtime tiếp tục không phụ thuộc UnityEngine/UnityEditor. Unity integration phụ thuộc Runtime; game phụ thuộc contract/adapter cần dùng. Analytics vendor, Input System cụ thể và logic từng thể loại nằm ngoài Runtime.

Giữ các contract hiện có khi đủ dùng. Không thêm một bộ interface cho mỗi game genre. Capability tùy chọn được tách khỏi `ITutorialService`/`ITutorialView` hiện hành khi cần giữ tương thích với implementation của consumer. Các tên API mới trong kế hoạch đều là đề xuất, chưa tồn tại.

Không tăng abstraction chỉ để phục vụ giả định. Một extension point mới phải giải quyết ít nhất hai consumer khác nhau hoặc một nhu cầu cụ thể đã xác định; protocol cần mô tả owner, lifecycle, kết quả lỗi và contract test.

## 3. M1 — Sửa correctness và bảo toàn completion

Ưu tiên cao nhất; làm trước các tính năng mở rộng.

### 3.1 Pending transition thuộc model/service

- Thay closure retry sống trong presenter bằng pending transition có cấu trúc: operation ID, flow/step ID, loại thao tác, đích checkpoint, lỗi lưu gần nhất. Không chứa Unity object/delegate gameplay.
- Nhận completion hợp lệ một lần, tạo candidate checkpoint, thử lưu rồi mới công bố trạng thái bước kế tiếp.
- Nếu lưu thất bại, giữ pending operation trong model; view chỉ phản ánh trạng thái và phát retry intent. Snapshot cần cho biết có retry được hay không và lỗi thuộc thao tác nào.
- Lệnh sai/token cũ/Busy không xóa hoặc thay pending operation. Retry thao tác cũ có operation identity riêng, không phụ thuộc token UI bị thu hồi khi suspend.
- Trong lúc pending, mặc định chặn advance/completion khác và skip để không ghi đè kết quả. Nếu sau này cho phép cancel/skip pending thì phải là policy và command rõ ràng.
- Pending vẫn tồn tại khi presenter bị dispose, controller disable/pause/rebind cùng service. Suspend vẫn thu hồi callback token; resume không được âm thầm xóa lỗi pending.
- Sau khi lưu thành công mới phát completion/advance event một lần. Retry nhiều lần không chạy lại gameplay.

Giới hạn cần nêu rõ: pending trong RAM chỉ bảo vệ vòng đời UI/controller, không bảo đảm sống qua process kill hoặc storage không ghi được. Với hành động không thể lặp lại, host cần đối soát từ gameplay state đã lưu hoặc action ID có tính idempotent. Muốn bền vững xuyên process phải thiết kế transaction/journal và phối hợp persistence của gameplay; không hứa exactly-once bằng token tutorial.

### 3.2 Target 3D

- Query physics scene sở hữu collider, thống nhất semantics scene ownership với adapter 2D.
- Kiểm tra lại active collider, camera, mask, trigger và khoảng ray theo clipping. Không thay global physics settings.
- Giữ nguyên serialized fields và meta GUID hiện có.

Nghiệm thu M1:

- Chuyển ba probe thành regression kiểm tra hành vi mong muốn; các lỗi cũ phải làm test thất bại trước sửa.
- Cover save failure → invalid callback → retry; save failure → disable/reopen hoặc pause/resume → retry; retry lặp; token cũ; skip trong pending.
- Pending đã commit không phát event hai lần; lỗi được render lại bởi presenter mới.
- Target 3D nhận input đúng trong default/local physics scene và tôn trọng occluder/mask.

## 4. M2 — Contract ổn định và khả năng chẩn đoán

Phụ thuộc M1.

- Mô tả bảng transition chính thức cho Idle, WaitingForTarget, ActiveStep, Suspended và terminal state, cùng trạng thái pending persistence. Không thêm enum value nếu capability/snapshot bổ sung đã đủ biểu đạt.
- Phân biệt step/session token dùng chống callback cũ với operation ID dùng retry persistence và correlation.
- Diagnostics snapshot gồm flow/step ID, status, completion mode, target thiếu, lý do target chưa sẵn sàng nếu adapter cung cấp, waiting elapsed, pending operation và error code.
- Chỉ mở rộng target diagnostics bằng capability tùy chọn; target/resolver cũ vẫn hoạt động.
- Giữ error code ổn định cho game map sang localization. Exception message kỹ thuật dành cho diagnostics, không bắt buộc đưa thẳng ra UI.
- Quy định ownership service/controller: một service có một presentation owner hoạt động tại một thời điểm; đánh giá guard phát hiện bind trùng trước khi cần hỗ trợ nhiều view đồng thời.
- Bổ sung read-only progress query nếu consumer cần đọc flow completed/skipped và checkpoint mà không gọi TryStart.
- Có semantics lifecycle rõ cho observer/subscription; leaf không tự resolve ServiceLocator.

Nghiệm thu M2: game chỉ từ snapshot biết được tutorial đang chờ gì và có thể retry hay không; consumer dùng contract cũ vẫn compile; contract tests kiểm tra cleanup và stale callback.

## 5. M3 — Kết nối gameplay, input và recovery theo policy

Phụ thuộc M2. Đây là phần chính để tái sử dụng giữa nhiều game.

### 5.1 Gameplay signal và condition bridge

- Giữ `Next`, `TargetClick`, `HostSignal` cho các flow hiện tại. Game xác nhận gameplay thành công rồi mới gửi signal.
- Xây sample bridge nhận dependency tường minh, capture token khi bắt đầu action, hủy subscription khi step kết thúc/owner bị disable.
- Signal phải gắn với step/session hiện hành; không nhận event gameplay cũ làm hoàn thành bước mới có cùng signal key.
- Với mục tiêu kiểu AND/OR, count hoặc state đã đạt, thử nghiệm qua condition provider ở game trước. Sau khi có hai use case, trích capability nhỏ vào package.
- Phân biệt event xảy ra sau khi step bắt đầu với predicate có thể đã đúng từ trước. Predicate cần đánh giá ngay khi enter/resume; event/count cần reset và checkpoint policy rõ.
- Không đưa truy vấn Enemy, Weapon, Inventory hoặc Economy trực tiếp vào TutorialModel. Core chỉ nhận kết quả qua bridge.

### 5.2 Input policy

- Cung cấp request có scope như `ui.shop`, `player.move`, `player.attack`; host map sang input/action map thực tế.
- Gate cần handle theo owner, release idempotent, không mở khóa phần input đang bị modal/system khác giữ.
- Release trên completed, skipped, suspended, dispose, initialization failure và teardown. Test hai gate owner cùng giữ một scope.
- Spotlight giữ nhiệm vụ UI raycast. Game quyết định cho phép gameplay chạy khi chờ target, save failed hoặc application resume.
- Thay đổi mặc định chặn input phải là opt-in hoặc được version hóa; không âm thầm đổi hành vi catalog cũ.

### 5.3 Waiting/recovery

- Giữ timeout không tự advance. Cho cấu hình default của host và override từng step nếu thực tế cần.
- Policy quyết định hint, tìm lại target, dựng lại fixture, suspend hoặc skip theo AllowSkip. Không để core tự load scene/spawn gameplay.
- Kết quả recovery async chỉ áp dụng nếu step/operation vẫn hiện hành; host sở hữu cancellation.
- Target bắt buộc/optional là schema bổ sung có nhu cầu rõ; fallback catalog cũ vẫn coi toàn bộ danh sách hiện tại là bắt buộc.

Nghiệm thu M3: một thao tác gameplay chỉ chạy một lần dù save retry; input gate không leak; event/count/predicate không lẫn semantics; target mất và xuất hiện lại không làm sai tiến trình.

## 6. M4 — Data, migration và authoring

Phụ thuộc M2; phần schema condition phụ thuộc kết quả M3.

- Tách khái niệm catalog revision với save schema version. Định nghĩa policy khi catalog mới/cũ hơn checkpoint, flow bị bỏ, step bị đổi hoặc migration không tìm thấy.
- Không reset toàn bộ tutorial để vượt lỗi migration. Trả diagnostics có thể xử lý; cung cấp migration explicit và kiểm thử bằng fixture save cũ.
- Chỉ bump save version khi thật sự đổi format persistent state. Giữ flow/step ID và field cũ nếu có thể.
- Mở rộng Editor validator từ JSON sang kiểm tra catalog, localization key qua adapter, và target trong phạm vi scene/prefab được chọn.
- Phân biệt error chắc chắn với warning: target runtime spawn hoặc scene khác có thể chưa hiện diện lúc authoring.
- Bổ sung cửa sổ inspect/debug nhẹ: flow hiện tại, checkpoint, target unresolved, pending save và kết quả command. Reset/force-complete chỉ là debug action tường minh, không chạy ngầm khi mở cửa sổ.
- Làm form authoring/preview tối thiểu trước; node graph chỉ triển khai khi branching thực sự là yêu cầu.

Nghiệm thu M4: catalog/save 0.2.0 vẫn dùng được theo policy; lỗi config có đường dẫn flow/step cụ thể; validator không báo target spawn động là lỗi chắc chắn; tool không sửa scene/save khi chỉ inspect.

## 7. M5 — Quan sát và chứng minh trên nhiều thể loại

Phụ thuộc M2/M3; có thể làm diagnostics observer trước authoring UI.

- Phát structured lifecycle event: flow/step entered, waiting started/ended, completion persisted, skipped, suspended/resumed, save failed/retried, target timed out.
- Event có flow/step ID, catalog revision, session/operation correlation khi phù hợp, reason và duration theo clock inject. Không đưa PII hoặc payload gameplay tự do vào event chung.
- Phân biệt gameplay completion received với checkpoint persisted để analytics không đếm completion hai lần khi retry.
- Adapter analytics nằm ở host; lỗi observer không được làm mất checkpoint hoặc ngăn observer khác nhận thông báo. Quy định cách log observer failure và cấm reentrant command từ callback.
- Dựng consumer/sample nhỏ theo ma trận dưới đây; dùng chung runtime và cùng contracts.

| Thể loại | Kịch bản nghiệm thu | Chứng minh |
| --- | --- | --- |
| Puzzle 2D | Drag piece → gameplay xác nhận placement | Gesture, world/UI target, signal |
| Action 3D | Di chuyển → bắn → hit đúng mục tiêu | Input scope, async action, token, condition bridge |
| Idle/tycoon | Upgrade → đạt production threshold | Predicate đã đúng, persistence/reload |
| Simulation/strategy | Chọn object → đặt building → xác nhận | Target spawn động, camera/scene, recovery |

Mỗi sample có test luồng chính, mất target, suspend/resume và save failure; sample UI có screenshot kiểm chứng ở các tỉ lệ màn hình liên quan. Mobile cần ít nhất một kiểm chứng trên thiết bị trước khi tuyên bố production mobile.

Nghiệm thu M5: thêm game sample không cần sửa model theo genre; analytics adapter có thể tháo ra; lifecycle event không duplicate sau retry/reopen.

## 8. M6 — Branching và flow orchestration theo nhu cầu đã chứng minh

Chỉ mở sau khi M1–M5 ổn định và có consumer cần; chưa đưa vào đợt sửa correctness.

- Branch: điều kiện/route được host quyết định qua contract, lưu route đã chọn để reload có kết quả xác định.
- Prerequisite/eligibility: game quyết định thời điểm start; orchestrator tùy chọn xử lý queue/priority, không biến model thành gameplay manager.
- Mặc định một flow active. Chỉ hỗ trợ concurrent flow khi đã định nghĩa arbitration cho view, input gate, target và persistence.
- Nếu thêm graph, validator phải kiểm tra route trỏ thiếu, terminal không đạt được, cycle và chính sách loop hữu hạn. Checkpoint dùng step ID, không index.
- Không thêm scripting language, reflection action executor hoặc plugin registry tổng quát trước khi có yêu cầu cụ thể.

## 9. Cách chia thay đổi và gate phát hành

| Thay đổi | Nội dung | Phụ thuộc |
| --- | --- | --- |
| PR1 | Pending completion trong model, retry/lifecycle regressions | Baseline |
| PR2 | Physics scene 3D và target regressions | Độc lập PR1 |
| PR3 | State contract, diagnostics, progress query cần thiết | PR1 |
| PR4 | Gameplay bridge, input/recovery policy và contract tests | PR3 |
| PR5 | Migration fixtures, validator và inspector | PR3/PR4 theo schema |
| PR6 | Observer, sample nhiều genre, tài liệu tích hợp | PR3/PR4 |

PR1 và PR2 là nhóm sửa lỗi ưu tiên; có thể xử lý độc lập nhưng không yêu cầu chạy agent song song. Mỗi PR cập nhật checkpoint công việc khi kéo dài qua nhiều lượt và ghi receipt đúng source đã kiểm tra.

- Đợt sửa lỗi giữ API/save schema có thể phát hành patch sau khi xác nhận tương thích. API/capability/schema bổ sung dự kiến ở minor tiếp theo; không chốt version trước khi biết diff cuối.
- Tránh thêm member bắt buộc vào interface hiện hữu; nếu không tránh được phải ghi breaking change và hướng migration consumer.
- Chạy đúng tầng kiểm chứng: model/persistence/contract ở EditMode; controller/physics/input lifecycle ở PlayMode; Unity visual/device ở gate liên quan.
- Dùng isolated consumer verifier hiện có; đưa regression mới vào đúng assembly để script chạy được. Kiểm tra cả import sample mới và consumer cũ.
- Receipt mới gắn fingerprint, dependency versions và kết quả; không dùng baseline 50 test để chứng nhận source đã sửa.
- Không đổi meta GUID, serialized reference hoặc assembly dependency ngoài phạm vi cần thiết. Không tối ưu allocation/mesh trước khi profiler xác định chi phí đáng xử lý.

## 10. Tiêu chí hoàn tất mục tiêu mở rộng

1. Bốn consumer/sample ở M5 chạy bằng cùng runtime, không có nhánh genre trong model.
2. Gameplay, presentation, input và analytics thay được độc lập qua contract có lifecycle rõ.
3. Callback cũ, target mất, lưu lỗi và controller reopen không làm mất pending completion trong cùng service lifetime.
4. Save/catalog cũ có policy tương thích và migration fixtures.
5. Người tích hợp biết chính xác phần package cung cấp, phần host cần triển khai và những giới hạn xuyên process/device chưa được bảo đảm.

Thứ tự khuyến nghị: M1 → M2 → M3 → M4/M5 → đánh giá nhu cầu M6. Không cần viết lại toàn bộ package để đạt mục tiêu này.
