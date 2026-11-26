# Kế hoạch ThunderPunch Debugger cho Unity Asset Store

Ngày lập 2026-11-27. Publisher **ThunderPunch Studio**. Tên sản phẩm làm việc **ThunderPunch Debugger**.

## Tiến độ triển khai

- Đã tạo [repo GitHub private](https://github.com/Thunder-Punch-Studio/com.thunderpunch.debugger) dưới organization **Thunder-Punch-Studio**. Submodule sandbox trỏ đến preview `b2e53a9`, phiên bản `0.1.0-preview.1`.
- Preview độc lập có Log Capture và Editor Log Monitor, dùng mã và giao diện mới. Unity 6000.4.12f1 chạy qua 3/3 test ThunderPunch khi cài riêng và 3/3 khi cài cùng Dreamy trong project sạch. Việc kiểm thử cả hai debugger trong scene gameplay vẫn cần thực hiện. Dry-run qua 2/2 test Python. Quét 16 GUID ThunderPunch và 570 GUID Dreamy cho kết quả 0 GUID trùng.
- Đã tạo [quy tắc sync](../debugger/sync-policy.md), [kiểm kê nguồn](../debugger/asset-provenance.md) và [ledger](../debugger/sync-ledger.jsonl). Allowlist thương mại còn trống. Chưa port hệ thống Dreamy, thử sync hai chiều hay chuẩn bị nộp Asset Store.

## Mục tiêu và cách duy trì

Tạo submodule riêng tại `LocalPackages/com.thunderpunch.debugger`, trỏ tới repo riêng do ThunderPunch Studio sở hữu. Package có ID `com.thunderpunch.debugger`, namespace gốc `ThunderPunch.Debugger`, bộ UI và tài sản thương hiệu riêng. Package Dreamy tiếp tục phát hành theo ID, API, GUID và lịch sử version của nó. Khách cài ThunderPunch chỉ cần các dependency đã công bố của ThunderPunch.

**Dùng Dreamy làm upstream cho phần logic chung đã được phép phân phối.** Sửa logic chung một lần tại upstream rồi đồng bộ qua pipeline có danh sách cho phép. Nếu lỗi được phát hiện ở ThunderPunch, thêm regression test và đưa bản sửa logic chung vào Dreamy trước, sau đó đồng bộ lại ThunderPunch. UI, logo, menu, prefab trình bày và cấu hình sản phẩm do từng package sở hữu. Agent có thể đưa cải tiến hành vi UI về Dreamy qua PR riêng áp dụng theme của Dreamy. Mỗi repo có branch, changelog, tag và lịch phát hành riêng.

## Baseline và phạm vi

- Sandbox ở `main`, checkout package Dreamy tại `9027182`, manifest ghi `0.1.8` và Unity tối thiểu `6000.0`. Checkout đang có thay đổi local sau đợt hardening. Chốt một commit upstream chứa chính xác phần thay đổi đã review, rồi ghi full SHA và kết quả test tương ứng. Giữ nguyên nhánh sandbox và bảo toàn công việc local trong lúc chuẩn bị.
- Baseline kiểm thử Windows dùng Unity `6000.4.12f1`, với 178 EditMode, 115 PlayMode và 48 Python đạt trong đợt hardening. Dùng số liệu đó làm mốc tham chiếu. Chạy lại trên commit baseline và trên từng phiên bản ThunderPunch được chọn để phát hành. [QA hiện tại](../../QAPlans/07_DIRECT_QA_REPORTING.md#2026-11-27-verification)
- Lập bảng parity cho bootstrap và access mode, Console, Cheats, FPS/HUD, Hierarchy/Inspector, Screenshot/Export, report local, Direct QA và Relay. Ghi trạng thái từng mục là `Shared`, `ThunderPunch implementation`, `Internal only` hoặc `Deferred`, với lý do và bài test. Kế hoạch mặc định cho bản thương mại là báo cáo local. Đưa từng luồng gửi ra ngoài vào bản Store sau khi có đánh giá riêng về dữ liệu, cấu hình mặc định và yêu cầu của Unity.

## 0. Xác minh quyền phân phối

1. Lập `Documentation~/ASSET_PROVENANCE.md` cho nguồn C#, prefab, hình, font, icon, Graphy, sample và công cụ kèm theo. Mỗi dòng ghi đường dẫn, nguồn và revision, chủ sở hữu, license, quyền phân phối source trên Asset Store, notice cần giữ, phương án thay thế và người duyệt.
2. Rà `Runtime/Imported/Foundation`, `Runtime/Imported/Presentation` và tài sản gốc StompyRobot/SRDebugger trước. Release note của Dreamy ghi phạm vi phân phối nội bộ và giữ giới hạn của vendor. Chỉ đưa phần được xác minh vào source thương mại. Nếu thiếu quyền với một phần cần thiết, xây implementation và tài sản gốc cho phần đó qua contract đã mô tả, rồi chạy các test hành vi tương ứng. Giữ nguyên attribution và notices bắt buộc. [Release note Dreamy](../../LocalPackages/com.dreamy.debugger/Documentation~/RELEASE.md#remaining-devicedeployment-gates)
3. Rà license theo chính sách Unity ngay cả với tài sản có license công khai. Chuẩn bị `Third-Party Notices.txt` và nội dung disclosure tương ứng. Kiểm tra thêm tên publisher, quyền sử dụng logo, thông tin hỗ trợ và disclosure nếu quy trình tạo nội dung thuộc trường hợp Unity yêu cầu. [Unity Submission Guidelines](https://assetstore.unity.com/publishing/submission-guidelines) · [Asset Store Provider Agreement](https://unity.com/legal/provider)

**Điều kiện qua bước** — Có bằng chứng quyền cho mọi thành phần đưa vào repo/artifact thương mại. Mục chưa xác minh ghi `BLOCKED` cùng phương án thay thế. Trong lúc đó vẫn có thể làm inventory, thiết kế gốc và test fixture.

## 1. Tạo repo, submodule và cô lập định danh

1. Tạo repo ThunderPunch riêng từ baseline đã lọc theo inventory. Nếu Git history Dreamy chứa mã hoặc tài sản hạn chế, khởi tạo history sạch cho repo ThunderPunch và ghi SHA gốc trong ledger nội bộ. Pin gitlink của repo mới trong sandbox. Bắt đầu bằng consumer sạch riêng cho mỗi package.
2. Đổi manifest, URL tài liệu, package path, namespace, tên `.asmdef`, tham chiếu assembly theo tên/GUID, `InternalsVisibleTo`, `link.xml`, reflection strings, sample/test assemblies và build staging. Rà thêm define symbols, Editor menu, settings, PlayerPrefs, Resources, đường dẫn report/screenshot, app ID mẫu và tên biến môi trường. Dùng tên riêng trong ThunderPunch để hai package cùng tồn tại mà không trùng namespace, settings hoặc bootstrap.
3. Giữ toàn bộ GUID hiện có ở Dreamy. Với ThunderPunch, sinh bảng `guid-map.json` một lần cho tài sản có nguồn chung. Dùng bảng này để remap tham chiếu nội bộ, giữ GUID ngoài package đã xác minh, và cấp GUID mới riêng cho tài sản độc lập. Commit bảng map cùng source được phép. Giữ GUID ThunderPunch ổn định trong mọi bản update sau lần import đầu.
4. Kiểm chứng ba cấu hình **Dreamy riêng**, **ThunderPunch riêng**, **cài cả hai**. Với cấu hình cùng cài, kiểm tra project import sạch và chạy một debugger được host chọn trong từng scene. Nếu cần chuyển project từ Dreamy sang ThunderPunch, phát hành hướng dẫn chuyển riêng cho host script, prefab/catalog, custom cheats và settings. Phiên bản đầu chỉ cần export/import dữ liệu có chủ đích và có bản sao lưu. Ghi rõ phần cần nối lại thủ công.

## 2. Thiết kế và triển khai giao diện ThunderPunch

1. Thiết kế bản nhận diện gốc gồm logo, icon set, typography, màu, spacing và layout. Duyệt mockup cho **Editor Setup**, **runtime overlay**, **Console**, **Hierarchy/Inspector**, **Cheats**, **HUD**, **Screenshot** và **Report** trước khi thay prefab.
2. Nối UI mới vào các hành vi đã được kiểm chứng. Dùng prefab, style tokens và logo riêng của ThunderPunch. Rà thêm onboarding, demo scene, ảnh chụp listing, README và menu trong Unity. Mặc định setup gọn với các hành động chính hiển thị trước; phần cấu hình QA nằm trong màn hình nâng cao.
3. Chụp và kiểm tra Editor ở cửa sổ dock hẹp/rộng, runtime ở desktop, Android dọc/ngang, safe area và bàn phím mềm. Kiểm tra các prefab đã có overrides trong consumer ThunderPunch sau một vòng update mẫu.

## 3. Pipeline đồng bộ và hướng dẫn cho agent

Tạo các file sau khi triển khai. `AGENTS.md` trong hai repo dẫn tới quy trình chung và nhắc đọc ledger trước khi sửa.

| File | Nội dung |
| --- | --- |
| `docs/debugger/sync-policy.md` tại sandbox | Bảng ownership theo feature/path, luật phân loại, quy trình sửa hai chiều, cách rollback và chạy test. |
| `docs/debugger/sync-ledger.jsonl` tại sandbox | Một dòng cho mỗi thay đổi được review. Lưu SHA hai repo, đường dẫn, version dự kiến, quyết định port/exclude, ảnh hưởng tương thích và bằng chứng test. |
| `Tools~/sync/` tại ThunderPunch | Script chạy dry-run, báo diff và xuất danh sách file thay đổi. Dùng upstream SHA đã pin, allowlist theo file, namespace/GUID map và validation. Chạy ngoài Unity import. |
| `CHANGELOG.md` ở mỗi repo | Ghi Added, Changed, Fixed, Security và Migration khi có. Mục nội bộ liên kết change ID trong ledger. Mục công khai mô tả hành vi khách nhận được. |
| `Documentation~/COMPATIBILITY.md` và `MIGRATION.md` | Ma trận Unity/platform/render pipeline và hướng dẫn upgrade theo cặp version, gồm API cũ, serialized assets và đường dẫn lưu trữ. |

### Luồng bắt buộc cho mỗi thay đổi

1. Cấp ID dạng `DBG-<issue>`. Ghi lỗi, cách tái hiện, phạm vi file và tác động lên public API, GUID, tên assembly, save/settings, wire format, IL2CPP và UI.
2. Chọn một loại `shared`, `dreamy-only`, `thunderpunch-only` hoặc `adapted`. Với `shared`, commit fix và test ở upstream trước rồi tạo PR ThunderPunch từ SHA đó. Với `adapted`, tạo hai bản triển khai tương ứng, liên kết cùng change ID và kiểm tra cùng hành vi bằng fixtures. Mục chỉ áp dụng một package phải có lý do trong ledger.
3. Script sync chỉ ghi các file đã cấp quyền. Giữ các thư mục theme, prefab độc lập và tài sản vendor ngoài vùng sync. CI từ chối file chưa phân loại, GUID trùng, tham chiếu lỗi, asset bị cấm hoặc diff lần chạy thứ hai. Xung đột phải được giải quyết trong PR bằng map hoặc lớp tích hợp tương ứng, có review.
4. Ghi test receipt và version đầu tiên chứa thay đổi ở từng package. PR dùng chung được đóng khi cả hai bên có SHA và bằng chứng, hoặc khi phía còn lại được đánh dấu `Deferred` với lý do và issue theo dõi. Đồng bộ mở PR để review. Lịch release của mỗi package do người duy trì quyết định.

Ví dụ bản ghi nội bộ cho lỗi Cheats đã phát hiện trong QA. Dòng này minh họa cấu trúc, các SHA sẽ được điền từ triển khai thực tế.

| ID | Thay đổi | Dreamy | ThunderPunch | Tương thích và test |
| --- | --- | --- | --- | --- |
| `DBG-<issue>` | Khôi phục số trong ô Cheats sau khi từ chối edit | SHA commit fix, version | SHA port hoặc adapter mới, version | Giá trị game và text cùng giữ `126` khi nhập `0`. API/GUID giữ nguyên. Link PlayMode XML mỗi bên. |

## 4. Release candidate và nộp Asset Store

1. Tạo consumer sạch từ đúng artifact định phát hành. Chạy EditMode, PlayMode có graphics, test tương thích Dreamy-only/ThunderPunch-only/cài cùng, sample import, compile sau upgrade, scan missing script/GUID và scan phạm vi source không phát hành. Test `Off` và `ReleaseDiagnostics` để xác nhận absence của QA credentials và privileged senders. Nếu công bố Android, chạy ARM64 IL2CPP trên thiết bị thật với Hierarchy/Inspector và screenshot. Nếu công bố gửi report qua provider, kiểm thêm payload, offline/restart, retry và privacy với account QA.
2. Tạo artifact standalone từ allowlist. Rà source, sample, documentation, licenses và notice. Kiểm tra artifact không chứa file local, log, config nội bộ, credentials hay công cụ nội bộ chưa duyệt. Lưu SHA-256, commit source, phiên bản, phiên bản Unity và test receipts. Xác nhận cách upload trong Publisher Portal của ThunderPunch Studio. Nếu tài khoản đủ điều kiện dùng UPM, chạy validation UPM; nếu dùng Asset Store Tools theo luồng thông thường, kiểm import artifact tương ứng. [Hướng dẫn publish của Unity](https://assetstore.unity.com/publishing/publish-and-sell-assets) · [Unity UPM Submission Guidelines](https://assetstore.unity.com/publishing/submission-guidelines#upm)
3. Soạn listing với nền tảng thực sự đã test, yêu cầu cài đặt và giới hạn tính năng. Chuẩn bị Quick Start offline, demo gốc, changelog, migration guide, ảnh sản phẩm, contact support và quy trình cập nhật. Tạo tag riêng và giữ artifact trước để hỗ trợ rollback. Chỉ submit sau khi quyền phân phối, artifact và release notes được duyệt.

## Điều kiện hoàn thành

- [ ] ThunderPunch cài độc lập từ artifact thương mại và có UI, logo, namespace, định danh Unity riêng.
- [ ] Consumer Dreamy hiện tại update mà vẫn giữ scenes/prefabs, config, custom cheats và dữ liệu.
- [ ] Hai package cài chung không trùng GUID, assembly, menu hoặc bootstrap ngoài lựa chọn của host.
- [ ] Chạy thử hai lượt bảo trì. Một fix từ Dreamy được port sang ThunderPunch. Một fix tìm thấy ở ThunderPunch được đưa ngược về Dreamy. Cả hai có changelog, ledger, test receipts và tag/revision tương ứng.
- [ ] Phần vendor, định dạng submit và các claim trên Store có bằng chứng. Các nhánh chưa test trên thiết bị hoặc chưa có quyền phân phối giữ trạng thái `BLOCKED`.
