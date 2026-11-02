# Plan: shared pooling contract in Dreamy Core

Status: implemented; Core 1.2.0 contract, Template backend và focused regression tests đã được bổ sung. Xem release evidence để biết kết quả validation.

## Quyết định

Chuyển contract pooling dùng chung vào `com.dreamy.core`, namespace dự kiến
`Dreamy.Core.Pooling`. Giữ backend `LeanPoolService` và phần tích hợp LeanPool
trong Template. `GameInstaller` tiếp tục tạo, đăng ký và dispose implementation.
Core không tự tạo singleton pool và không phụ thuộc LeanPool, Feedback hoặc Editor.

Đây là bước đưa ranh giới service dùng chung vào Core, không chuyển nguyên thư mục
Pooling sang Core. Chỉ tạo package adapter riêng khi có consumer ngoài Template
cần dùng cùng backend; chưa cần thêm package ở bước này.

## Căn cứ từ code hiện tại

- Core 1.1.2: `package.json` có dependencies rỗng; Runtime asmdef có references rỗng.
- `Assets/_Project/Scripts/Pooling/IPoolService.cs` là contract GameObject/Component
  dùng chung, hiện thuộc namespace `Dreamy.Template.Pooling`.
- `LeanPoolService.cs` và `PooledBehaviour.cs` trực tiếp phụ thuộc `Lean.Pool`.
- `Pools.cs` là facade resolve service qua `ServiceLocator`.
- `GameInstaller.InstallOtherServices()` đã tạo và đăng ký `IPoolService`;
  `OnDestroy()` đã shutdown Feedback trước khi dispose pool.
- `LeanPoolService.Spawn()` dùng API LeanPool static; dictionary của service chỉ
  ghi nhận các pool tạo bằng `Preload()`. `DespawnAll()` đang gọi API toàn cục.
  Vì vậy chưa có đảm bảo rằng service chỉ cleanup object/pool do mình sở hữu.
- Feedback Runtime không reference Core; VfxService, FloatingTextService và
  IconFlyService đang tự quản lý pool và lifecycle riêng.
- Chưa tìm thấy project-local `toolkit.json` hoặc compatibility catalog.
  Các nhận định ở đây dựa trên manifest, lockfile, asmdef và source được đọc.

## Phạm vi và quyền sở hữu

| Thành phần | Nơi đặt | Trách nhiệm |
| --- | --- | --- |
| IPoolService | Core/Runtime/Pooling | Contract backend-neutral |
| LeanPoolService | Template/Pooling | Spawn, return, preload, cleanup bằng LeanPool |
| PooledBehaviour | Template/Pooling | Callback theo Lean IPoolable |
| Pools facade | Template/Pooling | Giữ tương thích code game hiện tại |
| GameInstaller | Template/Bootstrap | Chọn backend, đăng ký, sở hữu lifecycle |
| Pool Feedback/Audio | Package đang sở hữu | Giữ lifecycle chuyên biệt trong giai đoạn này |

Consumer package có thể nhận `IPoolService` từ composition root; tránh resolve
ServiceLocator ở từng instance. Chiều dependency: Template → Core và LeanPool.
Core không có dependency ngược về Template hay backend.

## Các bước triển khai

1. **Chốt hành vi contract.** Giữ các overload hiện có để giảm migration;
   định nghĩa rõ `DespawnAll` chỉ tác động phạm vi service, preload count/capacity,
   persistence qua scene, delayed despawn và hành vi khi thiếu component `T`.
   Nếu spawn rồi không có `T`, trả instance về pool trước khi báo lỗi để tránh leak.
   `Dispose` của backend phải idempotent và không cho spawn sau dispose.

2. **Chuyển contract vào Core.** Move `IPoolService.cs` cùng `.meta`, cập nhật
   namespace và import ở backend/facade/GameInstaller. Giữ facade `Pools` và
   `PooledBehaviour` trong Template. Kiểm tra consumer cũ trước khi quyết định có
   cần interface bridge; registration theo Type nên interface cũ và mới không
   tự động thay thế nhau. Core Runtime tiếp tục không reference LeanPool/Editor.

3. **Sửa ownership backend.** Mọi spawn đi qua pool được service theo dõi,
   kể cả prefab chưa preload. Không gọi global `LeanPool.DespawnAll()` để cleanup
   service. Dùng pool API đã kiểm chứng trong source LeanPool; quản lý cả active
   instance, inactive instance và delayed return. Return object ngoài ownership
   phải có hành vi rõ ràng. Chốt pool persist trong root DontDestroyOnLoad;
   kiểm chứng semantics của tham số `persistent` hiện tại trước khi thay đổi.

4. **Tách InstallPoolService trong GameInstaller.** Gọi sau phần foundation cần
   thiết và trước consumer dùng pool. Tránh bootstrap trùng hoặc replace service
   đang sống mà không cleanup. Teardown: shutdown consumer → unregister đúng
   registration mà installer sở hữu → dispose backend. Xử lý init thất bại sau
   khi pool đã được tạo. Không tự đăng ký từ package Core.

5. **Docs và version.** Cập nhật Core README/changelog, Template usage và dependency
   version theo chính sách release của package. Giữ `.meta` GUID khi move, rà soát
   serialized references nếu sau này đổi base class hoặc MonoBehaviour.

6. **Validation.** Chạy focused EditMode/PlayMode tests cho spawn/return/reuse,
   preload/capacity, thiếu component, delayed return, scene unload, dispose hai
   lần, bootstrap thất bại và pool độc lập không bị service này despawn/destroy.
   Import Core trong consumer không cài LeanPool để kiểm chứng dependency sạch.
   Chạy lại test bootstrap của Template và kiểm tra Console compile errors.

## Feedback: giai đoạn riêng, chỉ khi cần

Không đổi pool nội bộ Feedback trong migration contract. Nếu muốn chia sẻ backend,
trước tiên thêm seam rent/return có ownership theo host/prefab vào Feedback và
adapter ở integration layer. Giữ backend mặc định để Feedback chạy độc lập;
GameInstaller inject adapter vào host trước khi initialize.

Feedback vẫn sở hữu cancellation, animation reset, giới hạn active effect,
completion callback và release khi host shutdown. Kiểm chứng UI reparent,
transform reset và stop/replay trước khi thay thế pool nội bộ.

## Điều kiện hoàn tất

- Core dùng được độc lập, không cần LeanPool hay Feedback.
- Template chỉ có một registration pool do GameInstaller sở hữu.
- Dispose không ảnh hưởng pool ngoài phạm vi service; không còn object/timer
  do service sở hữu sau teardown.
- Consumer cũ compile, targeted tests pass, không mất serialized reference.
- Feedback vẫn hoạt động với pool nội bộ như trước migration.
