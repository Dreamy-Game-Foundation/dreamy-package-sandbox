# Casual Audio Preset

Sample của Dreamy Audio. Import từ Window > Package Manager > Dreamy Audio > Samples > Import. Unity chép nội dung vào Assets/Samples/Dreamy Audio/0.1.0/Casual Audio Preset/.

## Cấu trúc và tích hợp

Giữ nguyên folder, .meta, asmdef và reference prefab khi chuyển vào project. Chỉ giữ một bản script/asmdef và một JSON cho mỗi key Resources/DataConfig. Bootstrap config/save/wallet/audio tại GameInstaller trước khi bật UI, theo [README package](../../README.md). Link tương đối này dùng trong source package; sau import, mở README package từ Package Manager.

Casual Audio Preset cung cấp Profile/Library và nhóm âm thanh. Basic Playback là hướng dẫn setup tối thiểu. Gán clip còn thiếu, thêm Library vào Profile rồi gán Profile cho AudioBootstrap hoặc root tự Initialize. ID phát có dạng libraryId/key. Reference Dreamy.Audio.Runtime; component bootstrap cần Dreamy.Audio.Runtime.Components. Không chạy hai luồng initialize song song.
