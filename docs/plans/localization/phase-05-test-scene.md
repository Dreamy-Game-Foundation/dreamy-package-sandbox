# Phase 5 — Scene LocalizationSettingsTest (P0)

[Tổng quan](README.md) · [Theo dõi tiến độ](TRACKING.md) · [Thiết kế chung](DESIGN.md)

Trạng thái: **Hoàn tất scene và tests**.

Phụ thuộc: [phase-04-settings-panel.md](phase-04-settings-panel.md).

Hierarchy dự kiến:

- LocalizationTestRoot: composition owner cho locale service/store/adapter và Settings presenters.
- Canvas + PanelManager/host UI đúng public contract, EventSystem đúng input project.
- DemoContent: title, greeting, reward với biến amount, nút mở Settings, current-locale label.
- SettingsPanel: panel thật qua PanelPresenterFactory.

Công việc:

- [x] Tạo scene mở và Play trực tiếp; root test cô lập cùng composition contracts của GameInstaller, không lôi toàn Shop/Missions bootstrap vào scene nhỏ.
- [x] Dùng IAudioService thật với cấu hình tối thiểu để Settings giữ hành vi audio hiện có; platform/review optional unavailable, không giả tích hợp SDK thật.
- [x] Init theo thứ tự: preference/backend → localization ready → register service/gateway → Settings service/presenter → enable demo UI.
- [x] Preload String Table pilot trước hiển thị; tránh UI rỗng lúc startup.
- [x] Demo nội dung ngoài Settings dùng cùng table/key/binding; thêm amount changer để chứng minh arguments cập nhật qua Smart Strings hiện có, không phụ thuộc làm xong global variable registry.
- [x] Nút Reset language preference chỉ trong test UI, có nhãn rõ; thông tin diagnostics kỹ thuật nằm component test/Inspector, không thêm vào production panel.
- [x] Teardown gỡ presenter, adapter, registrations và service theo instance ownership. Không tạo duplicate persistent root khi reload scene.
- [x] Không tự đổi scene startup/build order chính chỉ để chạy test; thêm build entry nếu cần smoke build sau khi scene chạy.

## Đầu ra

scene, profile/tables/fonts references và README cách mở/run.

## Điều kiện hoàn thành

- [x] mở scene độc lập, Play → mở Settings → chọn vi → cả trong/ngoài panel đổi; không reload scene; không console error mới.

## Ghi nhận thực hiện

- Revision/package versions: ghi trong `docs/evidence/localization/source-identity.json` (working copies chưa commit).
- Files thay đổi: đã triển khai trong localization package, Settings package và host theo phase.
- Lệnh kiểm tra và kết quả: xem validation và receipts bên dưới.
- Bằng chứng: lưu tại `docs/evidence/localization/phase-05/` khi thực hiện.
- Vấn đề còn mở: chưa ghi nhận.
- Ngày cập nhật: 2026-11-02; trạng thái theo phạm vi/gate ở trên.

## Kết quả thực hiện

Scene thực đã tạo; live apply, arguments, reopen/reload, cancellation và dispose có PlayMode tests. Menu Play Settings Test dùng reload sạch và khôi phục Editor settings sau khi dừng.

Xem [validation](../../evidence/localization/VALIDATION.md) và [cách chạy](RUN_TEST_SCENE.md).
