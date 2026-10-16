# Test Progression trong sandbox

Mở `Scenes/ProgressionRewardsSandbox.unity` rồi Play. Phiên setup này đã đặt
Editor playModeStartScene tới scene demo để Play không tự mở Bootstrap.
Muốn chạy game chính trở lại, đặt Play Mode Start Scene về
`Assets/_Project/Scenes/BootstrapScene.unity` hoặc clear setting để chạy scene đang mở.

Save của scene nằm trong `Application.persistentDataPath/DreamyProgressionRewardsSandbox`;
khác save game và khác sample directory mặc định.

## Kịch bản nhanh

1. Bấm **Fail next Card delivery**, sau đó **Grant 300 XP**.
   XP=300, level=4; rank-2 pending ở Card; Coin/Gem đã ack.
   Rank-3 vẫn có thể delivered; rank-4 không có reward.
2. Bấm **Retry rank-2**: chỉ phần chưa nhận được cấp tiếp, Coin/Gem không tăng lại.
3. Bấm **Fail journal ack after destination saved**, rồi **Grant 300 XP**.
   XP vẫn commit; model yêu cầu reload. Bấm **Reload owners and recover** hoặc Stop/Play.
4. Bấm **Fail final reward completion save**, rồi **Grant 300 XP**.
   Khi đã ack nhưng chưa Completed, UI giữ Pending. Reload sẽ hoàn tất.
5. Stop/Play kiểm tra XP, level, reward và inventory vẫn được giữ.

Catalog có 8 level, threshold cách nhau 100. Sau khi hết milestone, để bắt đầu lượt test
mới mà giữ dữ liệu cũ: Stop, chọn `Progression Rewards Demo`, đổi `Save Directory`
trong Inspector sang tên mới (ví dụ `DreamyProgressionRewardsSandbox_Run2`), lưu scene và Play.

UI hiện là diagnostic IMGUI trong Screen.safeArea. Không cần Canvas/EventSystem.
Sample Card/Chest/Cosmetic là inventory demo riêng, có receipt persistent.
