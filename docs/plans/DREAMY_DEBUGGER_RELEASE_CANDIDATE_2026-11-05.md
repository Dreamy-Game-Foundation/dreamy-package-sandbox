# Dreamy Debugger preview.6 — checkpoint hoàn tất

Ngày 2026-11-05. Hoàn tất sửa nhóm chặn candidate A1–A6 của audit và các lỗi lifecycle/fallback tái hiện được. **Candidate nội bộ đã qua kiểm tra desktop/build; chưa tag, commit, push hay publish.** Production/device gates vẫn mở. Không đưa Dreamy Core thành dependency bắt buộc.

## Thay đổi và nghiệm thu

| Finding | Thay đổi | Bằng chứng mới |
|---|---|---|
| A1 observer làm mất lease | Isolate từng subscriber, trả lease và dispose an toàn; giữ LastNotificationError | EditMode observer throw + observer kế tiếp + dispose, permissions |
| A2 dữ liệu cheat bị cũ | ValuesChanged riêng với structural Changed; refresh sau action/write; poll 4 Hz khi visible/pinned; getter lỗi hiện Unavailable, phục hồi không dựng lại row | PlayMode action đổi coins/status/owned, game đổi ngoài registry, cùng instance control; broken getter/recovery |
| A3 FPS warning chậm | Cửa sổ gần 0.5 giây dùng chung sampler, tách khỏi lịch sử 1024 frame | Stable 60 → 20 FPS, sustain 3 giây, warning trong 3–4 giây thay vì khoảng 28.65 giây |
| A4 legend không có phép đo | Ẩn Render/Update; chỉ công bố frame-time series | Prefab assertion + 28 prefab không Missing Script |
| A5 version/docs/status lệch | Manifest/runtime cùng 0.2.0-preview.6; author Dreamy Game Studio; README/changelog/tracking/migration/support/reporting/sample/release docs; acceptance khác delivery; Gmail pending | Test version agreement; archive/source byte-match |
| A6 chưa có artifact/build mới | UPM .tgz cố định, consumer sạch từ chính archive, Linux dev/release và Android IL2CPP ARM64 | 77 EditMode +25 PlayMode +20 relay pass; ba build thành công; Linux release có 0 packaged debugger assets |
| P2 shader/HUD | Lazy material, thiếu shader ẩn graph/collapse, vẫn text | PlayMode FPS-only không tạo renderer; missing shader fallback |
| P2 service lifecycle | Generation invalidates cache, ownership-safe unregister, chặn tạo service trong cleanup; phát hiện lại keyboard listener | Restart manager → cheat, HUD, console hoạt động; full suite nối tiếp không nhiễm cache |
| P2 capture | Operation giữ ownership đến khi GPU/encode kết thúc; abort trước readback; refcount hide overlay; sensitive-screen recheck | Disabled-capture regression và source review; GPU/cancel/late callback trên thiết bị vẫn pending, không đánh dấu pass từ headless |
| IL2CPP sample | link.xml mẫu game-owned, docs nhắc đổi assembly/type sau khi copy | Reflected GameDebugOptions tồn tại trong output IL2CPP; runtime device smoke vẫn pending |
| Đóng gói relay | Whitelist script/docs cụ thể; guard local-state/path/required files/Unity metadata | Artifact không có database/log/pid/pyc/cache; không đọc/gửi credential |

## Kết quả cuối

- Source consumer: **77/77 EditMode +25/25 PlayMode**, không skip.
- Final archive consumer: **77/77 EditMode +25/25 PlayMode**, không skip.
- Relay từ archive: **20/20**, fake/local, không gửi Telegram/Gmail thật.
- Linux development, Linux release, Android development **IL2CPP ARM64**: build thành công từ archive.
- Final artifact SHA-256: `a5048a904b00e31d8d6c07dc243042ee7b838746d5a973de3b06b79946df5120`.
- Archive: `/tmp/dreamy-preview6-artifact/com.dreamy.debugger-0.2.0-preview.6-final.tgz` (2,385,353 bytes; file local tạm để review).
- 397 owned meta GUID: không trùng nội bộ, không overlap SR gốc, không đổi meta đã có.
- MCP xác nhận `DreamyDebug @ Dreamy.Debugger.Presentation` và `SRDebug @ StompyRobot.SRDebugger` cùng được load; 0 console errors. Đây là compile coexistence, chưa chứng nhận chạy đồng thời hai bootstrap.
- Main scene `Assets/_Project/Scenes/BootstrapScene.unity`, không dirty, không Play. Không sửa source/meta SR gốc. Các staged thay đổi có trước, kể cả Settings.asset SR, giữ nguyên.
- Evidence: [verification.json](../evidence/debugger/release-preview.6/verification.json), XML, relay receipt, build reports, archive-check và source-files.json.

## Gate còn mở / backlog

1. Device input/notch/rotation/high-DPI, enter/exit Play tùy chọn 2×2 và script hot reload đầy đủ; restart test không thay thế các gate này.
2. GPU capture cancel/disable/destroy khi chờ frame/readback/encode, low-memory, overlay exclusion và policy thay đổi trên target API. Không có chứng nhận “không lag”; cần cold/warm p95/p99 CPU/GC/VRAM, pin/thermal và A/B build/performance trên máy yếu.
3. Production consent/disclosure, masking, withdrawal/cancel/purge queued data; remote HTTPS relay và phone→Telegram delivery. Gmail pending. Native crash/ANR/OOM và iOS/Xcode chưa nghiệm thu.
4. Full SR Settings parity, compact inline resource cards, option filter/group/width metadata, structural diff/pooling sau khi profile 20/200/1000 options; per-module Graphy states, typography và advanced rules là backlog, không tự thêm vào candidate.
5. Build success và preserved type chưa chứng minh UI/reflection trên thiết bị; thử upgrade/rollback trong từng game trước khi chốt release.

## Lỗi quan sát và cách khắc phục

- Teardown callback tự tạo service làm cache Options trỏ vào registry cũ: tái hiện bằng chuỗi full suite, chặn creation trong cleanup, rerun final artifact pass.
- `with sqlite3.Connection` chỉ commit/rollback, không đóng connection: dùng contextmanager đóng trong finally; test close + rollback, 20 test relay pass không còn ResourceWarning.
- Npm files whitelist theo thư mục lấy cả dữ liệu local dù Git ignore: hủy/ghi đè archive thử bị chặn, đổi whitelist sang từng script/doc; final archive guard pass. Không phát hành archive thử.
- Immutable UPM cần `.meta` của THIRD_PARTY_NOTICES: bổ sung vào whitelist; final consumer không còn cảnh báo missing meta.
- Một assertion dùng tên legend cũ đã được sửa theo “Frame time”; không dùng receipt fail làm nghiệm thu.
- AGENTS trỏ tới `docs/agents/dreamy-workflow.md` không tồn tại; CLI/checkpoint workflow không sẵn. Dùng checkpoint/evidence thủ công, không tạo hook hay background model call.

Phạm vi sửa candidate đã hoàn tất. Chốt stable/production hoặc publish cần những gate tương ứng ở trên; không coi chúng đã đạt từ build desktop.


## Commit theo yêu cầu tiếp theo

Package candidate đã commit tại `8a0e19c`; 804 file phân phối vẫn khớp archive đã nghiệm thu. Parent project commit giữ toàn bộ thay đổi đang có theo yêu cầu, gồm font, cấu hình mẫu/project, SR PIN và Editor Settings; các receipt trên chứng minh package candidate, không mở rộng thành nghiệm thu riêng mọi thay đổi project. Chưa push/tag/publish. Đề xuất runtime tiếp theo ở [DREAMY_DEBUGGER_RUNTIME_NEXT_2026-11-05.md](DREAMY_DEBUGGER_RUNTIME_NEXT_2026-11-05.md).
