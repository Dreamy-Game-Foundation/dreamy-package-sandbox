# Dreamy Debugger — UI concept

2026-11-05. Hoàn tất nghiên cứu, plan và 3 concept bằng **built-in image_gen**. Chưa sửa giao diện Unity. Baseline package `8a0e19c`, parent project `beca4db`.

[Plan chi tiết, nguồn và phases](../../plans/DREAMY_DEBUGGER_UI_REDESIGN_2026-11-05.md).

| Concept | Nội dung |
|---|---|
| [01 — Runtime cheats](01-runtime-cheats.png) | Sidebar desktop, drawer mobile, resource cards, grant-only ownership, header Dreamy đỏ |
| [02 — Console + HUD](02-console-hud.png) | Severity filters, log details và ba mức HUD |
| [03 — Editor + dialogs](03-editor-settings.png) | Form Performance settings, PIN và confirmation |

## Review và ràng buộc khi dựng thật

Đã xem cả 3 ảnh. Hướng tổng thể đạt: đỏ làm brand, hierarchy rõ hơn, một logo mỗi màn, resource card có balance/amount/action, close/burger rõ, HUD có grip riêng và Editor form thẳng cột. Đây là hình định hướng, không có control nào mới được wire bằng việc tạo ảnh.

Các chi tiết của ảnh cần làm đúng theo spec/source thay vì trace pixel:

- Một số surface/button có highlight hoặc gradient nhẹ dù prompt yêu cầu flat; prefab thật dùng flat color, không thêm blur/shadow shader để bắt chước ánh sáng ảnh.
- Chữ ivory trên coral chưa đủ contrast cho body nhỏ: dùng Brand #9E1008 làm button fill; Accent #E95345 dùng focus/indicator theo bảng token trong plan.
- Caution threshold trong ảnh Editor có ký hiệu ≤ gây hiểu sai: UI thật phải biểu diễn **Good ≥60; Caution từ 30 đến dưới 60; Critical dưới 30** theo ví dụ cấu hình, có validation thứ tự thresholds. Không đổi phép đo để khớp chữ AI.
- Numeric keypad đang minh họa PIN 4 ký tự; sản phẩm cần PIN dài và passphrase theo config. Không cố nhét 32 ô vào dialog.
- Ví dụ preview balance 9,825→9,925 chỉ được hiện nếu contract game cho phép tính/preview; view không tự cộng trước rồi coi grant đã thành công.
- No Ads có badge Owned và disabled action Owned là minh họa trạng thái grant-only. Khi dựng thật nên bỏ label dư hoặc giữ slot ổn định có accessible explanation; không thêm revoke toggle.
- Search/category chips, foldout, inline groups, preview và pin card là scope proposal; grouping cần stable ID metadata/fallback. Không đoán resource bằng tên và không giả nút pin group đã có implementation.
- Column caption FPS ONLY / COMPACT / DETAILED, nền slate và dòng PROPOSED UI là phần thuyết trình, không phải UI phủ lên game.
- Ảnh không chứng minh viewport thật 390×844, hit area 48dp, contrast, safe area hay no-lag. Size/scale/input phải nghiệm thu trong Unity và trên máy thật.

## Nguồn tham khảo

Nguồn primary đã duyệt: [SRDebugger](https://stompyrobot.uk/tools/srdebugger/documentation/), [Graphy](https://github.com/Tayx94/graphy), [Chrome DevTools](https://developer.chrome.com/docs/devtools/console/reference), [VS Code layout](https://code.visualstudio.com/docs/configure/custom-layout), [shadcn sidebar](https://ui.shadcn.com/docs/components/base/sidebar), [Android touch targets](https://developer.android.com/develop/ui/compose/accessibility/api-defaults) và [W3C contrast](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html). Chi tiết điều học/điều bỏ ở plan.

Logo gốc và ảnh baseline trong repo được dùng làm reference đầu vào. Không copy UI code/web dependencies hoặc asset Internet vào package Runtime. Website/reference screenshots không được coi là asset được cấp quyền tái phân phối trong package.

## Tái tạo và kiểm tra

Prompt đầy đủ: [prompts.json](prompts.json); từng file `.prompt.txt` tương ứng 01/02/03. Tool built-in `image_gen` với background opaque; không dùng CLI/API key. Reference image paths được lưu ở metadata file. Tất cả output gốc được copy nguyên bản từ generated_images vào thư mục này, không ghi đè art game.

[generation-receipt.json](generation-receipt.json) lưu dimensions/checksum; cả ba ảnh PNG 1586×992 đã decode/verify thành công. Đây là kiểm tra artifact, không phải PlayMode acceptance. Thay đổi lượt này chỉ ở docs; package/scene/SR gốc giữ nguyên. Các phase triển khai trong plan chưa bắt đầu.


## Sau khi triển khai

Mô tả “chưa sửa Unity” ở đầu là checkpoint lịch sử của lượt thiết kế. Candidate UI `0.3.0-preview.1` đã được triển khai và kiểm tra; [ảnh chạy thật và nghiệm thu](../../evidence/debugger/ui-redesign/README.md). Ảnh concept vẫn chỉ là tài liệu tham khảo.
