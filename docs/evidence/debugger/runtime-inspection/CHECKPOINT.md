# Dreamy Debugger Runtime Scene inspection — 2026-11-23

Implementation continues in the original `D:/Game/dreamy-package-sandbox`, now on **main** at the user's later instruction. The previous Orca worktree was detached at its existing `f7933dd` commit to free main; its files were retained. Debugger baseline here is `1516f9f` / 0.1.8, fetched and verified equal to origin/main before edits. The user subsequently authorized committing and pushing all work. Package commit **`8593c75b4f17110fdbad09770bcd852e5b2785df`** has been pushed to the debugger repository's `origin/main`. The parent sandbox commit containing this checkpoint records that gitlink together with the plan, handoff and evidence. This is source publication; package version/tag 0.1.8 and Asset Store release status are unchanged.

## Tiến độ cập nhật — 2026-11-23

| Hạng mục trong plan | Tiến độ hiện tại |
| --- | --- |
| Preflight checkout/package/MCP | Đã xác nhận project gốc, nhánh main, package baseline và đúng Unity instance. |
| Session/index và Hierarchy | Đã triển khai candidate: scan theo budget, search hủy query cũ, filter, selection, reveal và virtualization. |
| Inspector và quyền sửa | Đã triển khai candidate: drawer theo kiểu, nested struct/collection, draft, conflict và kiểm tra access trước ghi. |
| UI/catalog | Đã lưu prefab; catalog có một Scene entry; 35 prefab không thiếu script; có ảnh runtime ở bốn kích thước. |
| Kiểm chứng Editor | 102/102 EditMode và 5/5 PlayMode Scene/Inspector pass. Regression rộng trước đó 101/102; lỗi Int64 còn lại tái hiện trên v0.1.8 chưa sửa. |
| Docs/sample/archive | Đã cập nhật hướng dẫn, ví dụ, XML kết quả, ảnh, fingerprint và archive kiểm tra cấu trúc. |
| Player/device/performance/release | Chưa đóng gate production. Theo yêu cầu người dùng, không chạy thêm build. Các giới hạn chi tiết được giữ ở phần dưới. |

Package source is committed and pushed; the sandbox's checkpoint commit owns the progress record. No release tag or public store submission was created.

## Implemented internal candidate

- Engine-owned session with budgeted loaded-scene/DDOL indexing, object-reference selection/expansion, reconciliation and unload cleanup. Debugger-owned runtime/presentation/HUD/services are excluded by reference.
- Cancelable, budgeted hierarchy projection, name search with ancestor context, active/scene filters, reveal, virtualization and bounded row reuse.
- Inspector adapters for Transform/RectTransform/Behaviour/activeSelf; eligible instance fields; bool toggles, enum dropdowns, precision-safe scalar/vector/color text inputs with Color preview, enum flags, readonly object reference/reveal, paged arrays/List and nested class/struct traversal with cycles/depth limits.
- Opt-in editing with confirmation, commit-time access policy, draft/Apply/Enter/Reload, conflict checks and nested struct/list-element write-back. Arbitrary properties/methods are excluded.
- Saved Scene, HierarchyRow and InspectorValueRow prefabs; catalog registration; additive DefaultTabs.Scene=6; responsive split/narrow navigation, keyboard/back handling and shared fonts/palette. Existing safe-area/input owners remain in the shell.
- Polling stops and drafts are discarded when the shell hides via CanvasGroup, the tab disables or the session locks. Refresh reconstructs collection/member sections explicitly.
- Package docs, editable QuickStart example and targeted preservation entries. Package version remains 0.1.8; this working candidate is not the published tag.

## Evidence and limits

`verification.json` and the XML files record actual test counts and timestamps; `source-sha256.json` fingerprints the 48 changed package files. `tested-source-sha256.json` retains their bytes at test time. Before pushing, trailing whitespace was removed from eight Unity-generated prefab/meta files; `git diff -w` confirmed this was whitespace-only, with no C#, GUID or serialized value changes. Archive verification was rerun on the resulting source. Latest focused checks cover eligibility, precision, overflow, nested struct/list/array writes, conflicts, canceled searches, hierarchy/DDOL lifecycle, UI raycasts, draft/blur/Enter/Apply and access revocation. The broad earlier PlayMode failure is the Int64 Cheats input restoration test; it also fails against untouched v0.1.8 (`baseline-int64-failure.xml`).

The four `runtime-world-space-*.png` files are actual runtime camera renders through the debugger's public world-space mode at 1920×1080, 1280×720, 390×844 and 1024×768. They prove these rendered layouts, not physical touch, IME, safe area or Device Simulator behavior. Exploratory Game View captures were affected by concurrent Game/Simulator viewports and are not final visual evidence.

Unity MCP is pinned to `dreamy-package-sandbox@4c906ebe8d7b45ef`. Last inspection verified the local package path, one valid Scene catalog entry, BootstrapScene clean, no Prefab Stage and no compile errors. Running the sandbox demo also exposed unrelated FoundationTutorialAdapter and Localization SmartFormat exceptions; they are not fixed by this package feature.

An earlier Windows player built and passed the nested-struct/long precision/access smoke before the final search/UI refinements. Android ARM64 IL2CPP/High stripping reached native compilation but failed with `No space left on device` on C:. A separate disabled-define consumer crashed while initializing LMDB during the same low-space period; that is not a passing define check. Owned validation consumers were moved to `D:/Game/DreamyDebuggerValidation`, freeing C: space. The user then said **no builds needed**; no further player/Android builds are requested or claimed.

## Remaining production gates

This is an internal candidate, not an Asset Store readiness declaration. The full approved plan remains the acceptance reference. Remaining work includes physical keyboard/touch/IME and safe-area matrix, 1k/10k CPU/GC/thermal and repeated lifecycle measurements, exact final-source player/AOT/artifact import validation when requested, and the existing Imported Presentation redistribution rights audit. Vector/color/flags inputs currently use explicit text entry rather than specialized popup editors. Do not advertise these gates as passed from Editor tests or reskinning.

Local archive verification covers structure/metas only. The candidate `.tgz` keeps manifest version 0.1.8 and must not be confused with the published v0.1.8 artifact.
