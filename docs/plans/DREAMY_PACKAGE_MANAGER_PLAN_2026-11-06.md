# Dreamy Package Manager — kế hoạch mở rộng

Ngày: 2026-11-06. Trạng thái: đã triển khai MVP bước 1–3; xem checkpoint triển khai cuối tài liệu.

## Mục tiêu

Mở rộng cửa sổ `Tools/Dreamy/Package/Package Manager` trong `com.dreamy.editor-tools` để khám phá toàn bộ package Dreamy, xem package đã cài, version đang dùng, release mới nhất và cài/cập nhật theo nguồn phù hợp.

## Căn cứ đã kiểm tra

- Owner hiện tại: `LocalPackages/com.dreamy.editor-tools/Editor/Package/DreamyPackageToolsWindow.cs`. IMGUI, đọc direct dependencies từ manifest; có search, Add, Remove, Refresh, Resolve, mở manifest/lock. Chưa có catalog, resolved-version lookup hoặc release discovery.
- Unity sandbox: `6000.4.12f1`; package editor-tools local khai báo `1.0.1`, minimum Unity `6000.0`. README đang ghi 6000.4+, cần thống nhất yêu cầu khi triển khai.
- Manifest/lock sandbox dùng nguồn local cho các package Dreamy, nên chuỗi `file:` không phải installed SemVer. Danh sách installed cần bao gồm dependency gián tiếp.
- `.gitmodules` cung cấp danh sách repository khởi đầu, nhưng không đủ làm catalog cho consumer và không bảo đảm chứa mọi package mới.
- Assembly `Dreamy.EditorTools.Editor` chỉ chạy Editor, đã reference Newtonsoft.Json. Có test assembly Editor dùng cho capture; cần giữ các thay đổi capture đang dở nguyên vẹn.
- Workflow không có trong sandbox; đã đọc bản global `/home/tutrinh/.codex/docs/agents/dreamy-workflow.md`. Recall lesson theo scope không có kết quả.
- Compatibility global đang đánh dấu editor-tools drift và không xác nhận public headless/dry-run/JSON contract. Plan đề xuất contract mới; không coi private window là API automation đã được hỗ trợ. Catalog ở template lân cận là snapshot ngày 2026-11-03, không dùng làm bằng chứng latest.

## Thiết kế cửa sổ

Giữ menu và entry point hiện tại; thêm hai tab **Dreamy Catalog** và **Project Packages**. Tab Project Packages giữ tiện ích quản lý package tổng quát đang có.

Toolbar: search theo tên/ID, lọc All / Installed / Not installed / Updates / Issues; lựa chọn Stable mặc định và Include prerelease; Refresh Installed; Check Updates; thời điểm kiểm tra gần nhất.

| Cột | Nội dung |
| --- | --- |
| Package | Display name, package ID, nhóm Foundation / Feature / Tool |
| Installed | Version resolved hoặc Not installed |
| Latest stable | Release stable mới nhất và trạng thái dữ liệu |
| Recommended | Version phù hợp với Unity/dependency đã kiểm tra; Unknown nếu chưa đủ dữ liệu |
| Source | Git / Registry / Local / Embedded; Direct / Transitive |
| Status | Up to date / Update available / Local development / Preview / Unknown / Incompatible / Error |
| Action | Install / Update / Versions / Details tùy trạng thái |

Panel chi tiết: mô tả, repository, documentation, changelog, dependency và constraint, minimum Unity, version đã chọn, Git tag/commit nếu có, lỗi cụ thể kèm Retry. Latest upstream và compatible recommendation phải hiển thị riêng.

## Catalog và dữ liệu version

1. Bundle catalog JSON có schema version trong package: package ID, display name, category, repository, loại nguồn cài, URL/path UPM, link docs. Bao gồm cả package chưa cài; seed từ repo đã kiểm chứng, không đọc `.gitmodules` của consumer làm điều kiện hoạt động.
2. Catalog là danh sách package chính thức có owner cập nhật trong quy trình release/onboarding package. Catalog bundled bảo đảm dùng offline; hỗ trợ refresh từ catalog remote có cấu hình và schema validation. Remote lỗi thì giữ bản hợp lệ cuối cùng, hiển thị tuổi dữ liệu. Không tự coi mọi repository trong tổ chức là package phát hành.
3. Installed provider lấy dữ liệu package đã resolve của Unity, đối chiếu manifest/lock để phân biệt direct, transitive, nguồn và Git revision. Package Dreamy chưa có trong catalog vẫn xuất hiện với trạng thái Uncatalogued.
4. Release provider riêng cho Git và Registry. Với Git, đọc tag/release và xác minh metadata `package.json` tại revision/path tương ứng; chỉ công bố bản có tag/version khớp. Bỏ draft, phân loại prerelease, so SemVer đúng cả prerelease/build metadata; không suy latest từ branch HEAD hoặc version local.
5. Không buộc Unity consumer có GitHub CLI. Nếu repo private cần credential, dùng cơ chế credential được cấu hình ngoài package; không lưu token trong asset, manifest, catalog hoặc log. Thiếu quyền và rate limit là trạng thái có thể Retry, không biến thành Not installed hoặc Up to date.
6. Cache metadata theo project trong `Library/DreamyPackageManager/`, có fetchedAt, source và revision. Request có timeout, retry giới hạn, concurrency giới hạn, dedupe; đóng window hủy lookup do window sở hữu. Không gửi mạng trong OnGUI và không tự chạy kiểm tra mỗi lần repaint.

## Install / Update và dependency

- Nút Install/Update mở preview cụ thể: package/version/tag/commit đích, thay đổi manifest dự kiến, dependency bổ sung/đổi và xung đột. Đây là tương tác của tool khi triển khai, không phải yêu cầu xác nhận cho việc lập plan.
- Git install/update pin release xác định, giữ đúng repo/path; không chuyển sang branch mới nhất. Registry dùng version đã chọn. Cho phép Versions để chọn bản khác/downgrade với preview rõ ràng.
- Với Dreamy Git packages, chuẩn bị dependency closure: dependency SemVer không có registry cần map sang nguồn Git đã biết. Dependency bên thứ ba không tự suy URL từ ID. Thiếu mapping/conflict thì chặn Apply và nêu dependency cần giải quyết.
- Local/Embedded: hiển thị version local và latest để đối chiếu; action mặc định Open Folder / View Release. Không tự đổi `file:` sang Git và không checkout/pull submodule. Chuyển nguồn là action riêng có preview rõ thay đổi manifest.
- Transitive dependency: mặc định xem owner/dependents; nếu muốn pin trực tiếp phải thể hiện việc thêm direct dependency. Remove hiển thị các dependent và không cung cấp thao tác gây graph đã biết không hợp lệ.
- Operation coordinator sở hữu request độc lập vòng đời window: serialize các thay đổi UPM, chặn click trùng và theo dõi Resolve đến khi có kết quả. Đóng/mở cửa sổ và domain reload cần đối chiếu lại installed state trước khi cho thao tác tiếp; không tự chạy lại operation không rõ kết quả.
- Lưu snapshot manifest/lock và operation intent trong Library trước Apply. Sau Apply đọc lại graph, kiểm tra version/hash/source thực tế và compile status. Lỗi hiển thị thay đổi đã xảy ra; khôi phục là thao tác riêng, kiểm tra file không bị người dùng thay đổi thêm, resolve lại. Không hứa atomic rollback hoặc khôi phục toàn bộ project.
- Tự update editor-tools cần kiểm tra riêng vì assembly của chính tool có thể reload giữa operation.

## Phân tách trách nhiệm dự kiến

Giữ window làm view. Tách các lớp Editor-only bên dưới `Editor/Package/`:

- Catalog model/reader và installed provider.
- Release providers, SemVer comparer và cache.
- State evaluator: installed/latest/recommended/source/status.
- Change planner: dependency graph và preview không mutation.
- Operation coordinator: thực thi, theo dõi kết quả, reconciliation sau reload.

Không reference Runtime của các package Dreamy để liệt kê metadata; giữ `.meta` và entry point hiện tại. Pure planner/model có thể test độc lập với GUI; public automation contract chỉ bổ sung nếu có nhu cầu và được test/document rõ.

## Lộ trình

1. **Catalog + installed:** schema/catalog seed, installed provider, hai tab, search/filter/details; phân biệt local/transitive/unknown. Chốt endpoint remote, quyền truy cập và nguồn release dựa trên repo thực tế.
2. **Release lookup:** Git/registry providers, SemVer, Stable/Preview, cache/offline/auth/rate limit, latest và compatibility diagnostics. Ban đầu Recommended chỉ có khi đủ metadata; không gắn nhãn đã test từ metadata.
3. **Install/Update:** dependency planner, preview, coordinator, reconcile/recovery, Versions và self-update. Ra MVP sau khi hoàn thành bước này.
4. **Tiện ích tiếp theo:** Update Selected theo một dependency plan chung, export report, dependency graph, import sample theo contract Unity đã xác minh. Update All chỉ thêm sau khi graph và recovery được chứng minh.

## Tiêu chí nghiệm thu và kiểm tra

- Catalog hiển thị package chưa cài; installed version đúng với Git/local/registry và dependency gián tiếp. Latest không xác định được thì ghi Unknown, không giả Up to date.
- Test Editor cho catalog/schema, merge installed, SemVer (v prefix, prerelease, 1.9/1.10), tag/version mismatch, cache stale/offline, dependency closure/conflict/cycle, source protection và operation reconciliation.
- Dùng fake release/installed providers để test lỗi 401/403/429, timeout, đóng window và click trùng; test hành vi, không test lại cách vẽ IMGUI.
- Clean consumer: install rồi update package Git pinned cùng dependency; remove, resolve thất bại và recovery; kiểm tra manifest/lock/resolved metadata và compile. Thử self-update editor-tools bằng bản release test có kiểm soát.
- Sandbox local: Check Updates chỉ đọc metadata, không đổi manifest, submodule hoặc source local; Project Packages giữ Add/Remove/Resolve hiện tại.
- Kiểm tra UI thực trong Unity 6000.4.12f1: window nhỏ, search/filter, loading/error/retry, reload và Console. Chạy focused Editor tests và compile consumer; static check không thay thế GUI/UPM evidence.
- Cập nhật README/CHANGELOG và compatibility evidence khi triển khai; chỉ bump/tag release sau kiểm tra. Báo riêng pass/fail/degraded cho từng gate.

## Checkpoint

Planning complete. Chỉ thêm tài liệu này; chưa sửa code, manifest, lock hoặc package version. Chưa query release remote nên tài liệu không tuyên bố version upstream hiện tại. Bước tiếp theo nếu triển khai: bước 1, xác minh catalog/release transport rồi làm provider và UI read-only trước.


## Checkpoint triển khai — 2026-11-06

- Đã triển khai catalog 21 package, project additions/overrides và remote catalog tùy chọn; thêm package qua Catalog Settings hoặc JSON, không sửa C#.
- Đã có resolved installed list, direct/transitive/local/unknown, tìm kiếm/lọc, stable/prerelease/version picker, release metadata, dependency preview, commit-pinned Install/Update, chuyển nguồn riêng, operation journal/restore, export và giữ Project Packages. Entry manifest chưa resolve vẫn có thể xem/xóa trong Project Packages.
- Clean consumer: 46/46 Editor tests pass; sandbox GUI: 45 pass, 1 screenshot batch-only test skip, 0 fail. GitHub/registry lookup thật và Git install/update/remove/restore/self-update được kiểm tra trong consumer tách biệt.
- Unity sandbox được restart theo yêu cầu; MCP đã kết nối lại, scene sạch và Console không compile/OnGUI errors. Script reconnect tạm đã xóa.
- Static consumer package-check và sandbox asmdef pass. Sandbox package-check còn fail do BOM của asmdef StompyRobot có sẵn; không sửa file ngoài phạm vi. Pixel inspection bị giới hạn do desktop provider không hỗ trợ screenshot/AT-SPI timeout.
- Evidence: `docs/evidence/package-manager/verification.json`, test receipts, source fingerprints và smoke harness. Chưa bump/tag/publish. Bước 4 chỉ có Export; bulk update/sample import để lần mở rộng tiếp theo.
