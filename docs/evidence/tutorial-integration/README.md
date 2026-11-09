# Tutorial integration 0.2.0

Đã triển khai P0/P1: custom view, world 2D, compatibility/dependencies/docs, multi-focus/input, waiting recovery và gesture sample. Không đổi gameplay authority/save schema hoặc GUID prefab/scene cũ.

Consumer cuối: **33/33 EditMode, 17/17 PlayMode**, không skip. Xem `verification.json`, `editmode.xml`, `playmode.xml`, manifest/lock consumer và package `Verification~/README.md` để chạy lại. Gate cuối chạy sau khi sửa recovery button bị toggle trong mỗi waiting render.

Observed failures trong quá trình làm:
- Harness Settings tổng dừng ở imported sample mismatch có sẵn; chuyển sang test Tutorial trực tiếp.
- Fixture 2D đầu tiên đặt camera và collider đồng vị trí; fixture lifecycle chạy nhầm EditMode. Đã sửa vị trí và chuyển lifecycle sang PlayMode; receipt cuối thay thế kết quả thất bại đó.
- Consumer tối giản thiếu Audio module mà DOTween cần. Consumer cuối dùng built-in modules của host; không sửa dependency ngoài Tutorial.
- Rà soát phát hiện recovery button bị tắt/bật mỗi Tick. Đã sửa và thêm test giữ button active qua waiting render, không nhân subscription sau reopen.

Tag `v0.2.0` và commit package `fa5dae0` đã push lên origin/main; chi tiết tại `release.json`. Chưa có player/device/visual game acceptance.
