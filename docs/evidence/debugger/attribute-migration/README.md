# Partial/attribute migration — 0.3.0-preview.2

2026-11-05, Unity 6000.4.12f1 Linux. Toàn bộ cheat game nằm trong một container GameCheatOptions partial, chia theo Economy, Inventory, Entitlements, Diagnostics, Time, Application và Wallet helpers. Host đăng ký một lần và không còn tham chiếu assembly sample. Quick Start mặc định cũng dùng ProjectCheats partial; typed API/templates vẫn được giữ cho compatibility.

32 ID và 10 card cũ giữ nguyên, gồm resource balance/amount/action và ownership owned/unlock. ResourceCheat/OwnershipCheat attributes cung cấp role, title và stable ID; CheatId giữ ID độc lập khi đổi tên member. Metadata không sở hữu wallet/save; grant/ownership vẫn đi qua service game. Invalid role/type hoặc duplicate ID làm atomic registration thất bại.

## Nghiệm thu

- Project: **10/10 EditMode**, [receipt](project-tests.json). Bao gồm persist/load, entitlement effect một lần, rejected wallet, lock/range/confirmation, disposal và exact ID/card inventory.
- Source package + imported sample: **81/81 EditMode, 37/37 PlayMode**, XML source-*.xml.
- Consumer từ gói xuất: **81/81 EditMode, 37/37 PlayMode**, XML artifact-*.xml.
- PlayMode mới thực sự dựng attributed resource/ownership cards, Confirm làm tăng balance và refresh text; ownership đã grant ẩn action.
- Build Linux development/release và Android development IL2CPP ARM64 từ cùng archive: [receipts](builds/), [hashes và biên bản](verification.json).
- Gói UPM: [0.3.0-preview.2](artifacts/com.dreamy.debugger-0.3.0-preview.2.tgz); guard whitelist/meta và source/archive byte equality được ghi trong verification.json.

[Hướng dẫn game](../../../../Assets/_Project/Debugger/README.md) · [Hướng dẫn package](../../../../LocalPackages/com.dreamy.debugger/Documentation~/CHEAT_OPTIONS.md).

Mẫu reflected methods đều cần confirmation, kể cả sample grant vốn không cần trước migration. Giữ read-only và setter như tài liệu; UI grouping vẫn dùng cùng registry/controls của preview.1. Không đổi prefab/game scene/config, không xóa hoặc regenerate meta cũ. Source SandboxGameCheats.cs được chuyển bằng AssetDatabase.MoveAsset sang GameCheatOptions.Wallet.cs, giữ nguyên meta identity. Các partial mới được Unity tạo meta riêng.

## Giới hạn

Build Android chỉ chứng minh package + imported sample compile/link được, không chứng minh runtime trên điện thoại hoặc reflected container game đã được IL2CPP build. GameCheatOptions được kiểm tra trong Editor với wallet test/save tạm; chưa build/chạy bản game thật trên thiết bị. Giữ project link.xml và nghiệm thu stripping sau khi đổi type/member. Không có kiểm chứng p95/GC/thermal mới, không gửi Telegram/email hoặc cấp tài nguyên vào save đang chơi. BootstrapScene vẫn sạch trong Edit Mode.

Preview.1 images/builds và prototype partial trước đó là lịch sử. Receipt preview.2 mới dùng cho migration này; chưa commit, tag, push hoặc publish.

Observed failures: fixture attribute Category ban đầu trùng với NUnit.Category, đã qualify namespace và compile sạch. Kết quả 9-test lúc compiler chưa reload bị loại, chạy mới đúng 10 tests. npm pack lần đầu chạy ở sandbox không có package.json; đã sửa working directory trước export cuối.


Documentation follow-up: package README was rewritten after this archive/build receipt to explain first import, setup and API usage. The archive above retains the README verified at archive time; this does not constitute a new binary/build verification. Current README checks: ../readme-guide/verification.json.


Options workflow preview.3 supersedes this candidate with owned notifications, opt-in confirmation for plain methods, and incremental updates. Fresh receipts: ../options-workflow/README.md.
