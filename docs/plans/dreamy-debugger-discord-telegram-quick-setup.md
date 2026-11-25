# Dreamy Debugger · Thiết lập nhanh Discord và Telegram

Studio cấu hình một lần cho mỗi môi trường. Tester mở **Report**, mô tả lỗi và gửi. Unity Package Manager lấy dependency của package. Máy chạy relay chỉ cần Python 3 với thư viện chuẩn.

## Luồng setup đề xuất trong Unity

Mở **Window → Dreamy Debugger → Telegram & Reports → Set up delivery**.

| Bước | Studio thao tác | Công cụ xử lý |
| --- | --- | --- |
| **1. Chọn nơi nhận** | Chọn **Discord** hoặc **Telegram**. Có thể thêm đích thứ hai sau. | Mở hướng dẫn đúng đích. Nếu chạy relay local, kiểm tra Python, SQLite và TLS. |
| **2. Kết nối** | Discord dán webhook. Telegram nhập token ở prompt ẩn, nhắn bot rồi chọn chat tìm được. Có thể nhập Chat ID thủ công. | Launcher kiểm tra cấu hình, khởi động relay local và đưa URL về Editor khi đúng tiến trình đã sẵn sàng. |
| **3. Gửi thử** | Bấm **Send test** và tìm mã report trong đúng kênh/chat. | Gửi đúng một mẫu JSON tới đích đã chọn. Hiện **Relay accepted** khi SQLite nhận và **Delivered** khi đã có xác nhận từ đích. |

- **Thiếu Python.** Hiện **Copy install command** theo hệ điều hành và **Check again**. Người dùng chạy lệnh cài, rồi công cụ kiểm tra lại. Dùng Python có sẵn nếu kiểm tra đạt.
- **Đã có relay.** Chọn **Use an existing relay** và nhập HTTPS URL, ingest key. Màn hình này dùng cho studio có hạ tầng QA sẵn. Host giữ webhook và bot token.
- **Thử local.** Relay chỉ nghe loopback. Giữ URL trong phiên test. Cổng bận thì đề xuất cổng khác. Nút **Stop local relay** chỉ dừng tiến trình do công cụ khởi động.
- **Xác nhận giao nhận.** Tắt nút gửi khi đang xử lý. Sau 202, giữ report ID để đối chiếu, kể cả khi đã đóng cửa sổ. Relay local đọc receipt từ SQLite. Với relay từ xa, bản đầu dùng **Copy report ID** để kiểm trong chat hoặc nhờ người vận hành tra SQLite. UI chỉ hiện **Delivered** khi có receipt thực tế.
- **Dùng cho QA.** Sau khi xác nhận tin nhắn, nút **Use in QA build** ghi URL HTTPS và ingest key vào cấu hình QA được chọn. Đổi đích hoặc credential thì chạy test lại. Một thay đổi ở bước trước chỉ làm mất hiệu lực kết quả test liên quan.
- **Tester.** Khi có relay, một nút **Gửi báo cáo** lưu local trước và xếp hàng gửi. Offline hiện **Đã lưu · Chờ gửi**. Khi relay nhận thì hiện **Relay đã nhận**. Khi upload tắt, nút chính là **Lưu báo cáo** và lịch sử có **Xuất báo cáo**.

### Phạm vi triển khai

Wizard chung, tự điền URL và nút runtime gộp là phần thiết kế mới. Nút mở terminal cho Telegram đã triển khai và kiểm tra từ Unity 6000.4.12f1 trên Linux. Giữ trang Settings và outbox hiện tại. Ưu tiên tái sử dụng launcher Telegram, endpoint test riêng và cách tra receipt hiện có. Discord local đã có launcher nhập webhook ẩn. Telegram discovery nên dùng cùng lần nhập token với bước chọn chat. Cài Python, relay QA và logic gửi là các bước có thông báo riêng, để biết ngay lỗi ở đâu.

Nghiệm thu luồng mới bằng các ca có sẵn Python, thiếu Python, cổng bận, sai ingest key, Telegram không thấy chat, gửi mẫu tới đúng đích và Android QA offline rồi có mạng lại qua HTTPS. Kiểm tra trên Bash, zsh và PowerShell. Bước gửi mẫu không dùng backlog của game. Trên Editor, xác nhận lại report ID trước khi bấm thử lại nếu request timeout.


### Checkpoint · Nhớ token Telegram và tự chạy relay

Đã có hai tùy chọn trong Editor. **Remember bot token on this computer** cài credential helper trong Library ở lần thiết lập đầu và lưu token vào credential store của hệ điều hành. **Start saved relay when Unity opens** chạy ở các phiên Editor sau, dùng lại relay cùng project/chat hoặc khởi động relay local. Có nút khởi động ngay và quên token. Linux đã kiểm tra ghi, đọc, xóa token mẫu bằng Secret Service thật, khởi động relay loopback và dùng lại cùng PID. Token test không được gửi ra Telegram. Cần kiểm chứng trực tiếp Windows và macOS. [Hướng dẫn](../../LocalPackages/com.dreamy.debugger/Documentation~/REPORTING.md#remember-the-token-and-start-the-local-telegram-relay-automatically)


### Checkpoint · Nhớ webhook Discord và tự chạy relay

**Local relay setup** có hai tab Discord và Telegram. Discord kiểm tra quyền đọc webhook trước khi lưu, dùng kho mật khẩu riêng và relay local cổng 8766. Đã xác nhận Unity 6000.4.12f1 biên dịch và Linux Secret Service lưu/xóa webhook mẫu tách biệt với token Telegram. Smoke test chạy relay từ credential đã lưu và dùng lại tiến trình đang chạy. Kiểm tra webhook thật và Windows/macOS còn mở. [Hướng dẫn](../../LocalPackages/com.dreamy.debugger/Documentation~/REPORTING.md#remember-a-discord-webhook-and-start-the-local-relay-automatically)

## Hướng dẫn dùng ngay với package hiện tại

### 1. Cài dependency

Sandbox đã dùng package local. Project ngoài sandbox cài source hoặc archive chứa revision có nút **Send Telegram Test Report**. Tag `v0.1.8` cũ chưa chứa nút này. Unity tự lấy uGUI và các module trong manifest. [Manifest package](../../LocalPackages/com.dreamy.debugger/package.json)

Relay dùng thư viện chuẩn Python, SQLite và TLS đi kèm. Với lượt thử local, bỏ qua pip, virtualenv và Docker.

| Máy chạy relay | Cài nhanh nếu chưa có Python |
| --- | --- |
| Windows | Cài [Python Install Manager](https://www.python.org/downloads/windows/), mở PowerShell mới rồi chạy `py install 3.13`. Dùng `py -3.13` hoặc `python` nếu đã trỏ đúng bản. |
| macOS | Nếu có Homebrew, dùng `brew install python`. Có thể cài trực tiếp từ [Python.org](https://www.python.org/downloads/macos/). |
| Ubuntu/Debian | `sudo apt update && sudo apt install -y python3 ca-certificates` |
| Arch Linux | `sudo pacman -S python ca-certificates` trên hệ thống đã cập nhật. |

Đối chiếu tài liệu [Python trên Windows](https://docs.python.org/3/using/windows.html) và [Homebrew Python](https://formulae.brew.sh/formula/python).

Từ root sandbox, kiểm tra ba thành phần relay dùng:

```sh
cd "LocalPackages/com.dreamy.debugger/Tools~/relay"
python3 --version
python3 -c "import ssl, sqlite3; print(ssl.OPENSSL_VERSION, sqlite3.sqlite_version)"
python3 server.py --help
```

Trên PowerShell, thay `python3` bằng `python` hoặc `py -3.13`. Project cài bằng UPM có thể vào cửa sổ **Telegram & Reports** và chọn **Open Telegram relay setup folder**.

### 2A. Discord trong Editor

Cách nhanh hiện tại là vào **Local relay setup → Discord → Start Discord Relay in Terminal**. Nếu bật nhớ webhook, launcher sẽ dùng kho mật khẩu hệ điều hành và có thể tự chạy lần sau. Các lệnh bên dưới dành cho cách chạy thủ công.

1. Trong Discord mở **Server Settings → Integrations → Webhooks**, tạo webhook và chọn kênh thử. [Discord Help](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks)
2. Mở terminal riêng trong thư mục relay. Chạy đúng khối lệnh bên dưới và dán webhook tại prompt ẩn. Giữ terminal mở. Dùng **Ctrl+C** để dừng sau khi thử.
3. Trong Unity mở **Window → Dreamy Debugger → Telegram & Reports**, chọn configuration và đặt **Relay URL** là `http://127.0.0.1:8765/reports`. Nếu relay yêu cầu ingest key, nhập giá trị tương ứng trong **Limited Client Key**. Bấm **Send Discord Test Report** và đối chiếu report ID cùng file JSON trong kênh. Có thể giữ runtime upload tắt.

**Bash hoặc zsh**

```sh
(
  printf 'Discord webhook URL: '
  IFS= read -rs DREAMY_DISCORD_WEBHOOK || exit 1
  printf '\n'
  export DREAMY_DISCORD_WEBHOOK
  export DREAMY_BIND=127.0.0.1 DREAMY_FAKE_DELIVERY=0 DREAMY_DESTINATIONS=discord
  python3 server.py --db ./data/discord-local.sqlite
)
```

**Windows PowerShell**

```powershell
$webhook = Read-Host "Discord webhook URL" -AsSecureString
try {
    $env:DREAMY_DISCORD_WEBHOOK = [System.Net.NetworkCredential]::new("", $webhook).Password
    $env:DREAMY_BIND = "127.0.0.1"
    $env:DREAMY_FAKE_DELIVERY = "0"
    $env:DREAMY_DESTINATIONS = "discord"
    python server.py --db ./data/discord-local.sqlite
} finally {
    Remove-Item Env:DREAMY_DISCORD_WEBHOOK,Env:DREAMY_BIND,Env:DREAMY_FAKE_DELIVERY,Env:DREAMY_DESTINATIONS -ErrorAction SilentlyContinue
    $webhook.Dispose()
}
```

### 2B. Telegram trong Editor

Trong **Telegram & Reports**, dùng **Find Chat ID in Terminal** hoặc **Start Telegram Relay in Terminal** để Unity tự mở terminal đúng script. Các lệnh bên dưới là cách mở thủ công tương đương.

1. Nếu chưa có bot, dùng **@BotFather → /newbot**. Mở chat đích và nhắn `/start`. Với group, thêm bot rồi gửi `/start@TenBot`. Với channel, cấp quyền đăng bài cho bot. [Telegram Bot Tutorial](https://core.telegram.org/bots/tutorial)
2. Nếu chưa biết ID, chạy lệnh discovery rồi dùng Chat ID của đúng chat vừa mở. Với bot đã có webhook hoặc poller đang dùng, điền ID đã biết hoặc dùng bot riêng để thử.
3. Chạy launcher. Nhập token ở prompt ẩn và Chat ID khi được hỏi. Chờ **Relay ready in background** và sao chép **Editor Relay URL**.

```sh
python3 telegram_setup.py --discover-chat
python3 telegram_setup.py
```

4. Trong cửa sổ **Telegram & Reports**, điền Relay URL và ingest key nếu có. Bấm **Send Telegram Test Report**, tìm report ID cùng file JSON trong chat. Lượt test không gửi ảnh game, log hoặc backlog. [Luồng Telegram hiện tại](../../LocalPackages/com.dreamy.debugger/Documentation~/REPORTING.md#telegram-test-from-the-editor)

Chạy lần lượt nếu cả hai relay local cùng dùng cổng `8765`. Nếu muốn chạy đồng thời, đặt `DREAMY_PORT` khác cho một tiến trình và dùng URL tương ứng. Launcher Telegram ghi PID vào `data/telegram-local.pid`. Xác nhận đúng PID của relay trước khi dừng tiến trình lúc thử xong.

### 3. Xem kết quả và sửa lỗi

**Relay accepted** xác nhận đã lưu SQLite. Khi cần xem đích đã nhận, chọn lệnh tương ứng và thay `REPORT_ID` bằng mã ở Editor:

```sh
python3 server.py --db ./data/discord-local.sqlite --delivery-status REPORT_ID
python3 server.py --db ./data/telegram-local.sqlite --delivery-status REPORT_ID
```

| Dấu hiệu | Cách xử lý |
| --- | --- |
| Editor không kết nối | Mở `http://127.0.0.1:8765/health` trên máy chạy Editor, chỉnh cổng theo URL thực tế và kiểm tra relay còn chạy. |
| Relay từ chối | Kiểm tra ingest key, live mode và đích đã cấu hình. Nếu dùng HTTPS proxy, chuyển tiếp route `/reports/discord-test` hoặc `/reports/telegram-test`. |
| Accepted nhưng chưa có tin | Tra receipt. `pending` đang chờ gửi hoặc retry, `dead` có thông tin lỗi, `delivered` là API đích đã xác nhận. |
| Telegram không thấy chat hoặc trả 400/403 | Kiểm tra Chat ID, gửi lệnh cho bot đúng chat và xem quyền đăng file. Nếu group vừa chuyển thành supergroup, lấy lại ID. |
| Discord gửi lỗi | Xác nhận webhook chưa bị xóa, trỏ đúng kênh và relay có bản sửa User-Agent. |
| Chỉ thử lưu offline | Chạy relay với `DREAMY_FAKE_DELIVERY=1` và tạo một report mẫu từ tab **Report**. Nút test cho từng đích yêu cầu live mode. |

## Từ Editor sang thiết bị QA

Trên máy relay, chạy `server.py` dưới supervisor, dùng thư mục SQLite bền vững và đặt reverse proxy HTTPS phía trước. Cấu hình secret phía server và một ingest key cho môi trường QA. Unity dùng **HTTPS Relay URL** và **Limited Client Key**. Loopback của điện thoại trỏ về chính điện thoại. [Cấu hình triển khai relay](../../LocalPackages/com.dreamy.debugger/Tools~/relay/README.md#remote-deployment-configuration)

Trong **Telegram & Reports**, nhập URL và key, chọn đích rồi bấm **Send test report**. Sau khi xác nhận mẫu đến đúng đích, bật **Enable sending in game** và chạy lại Play Mode hoặc build QA. Tester vào **Report → Send report** để lưu và thử gửi ngay bằng một lần bấm. Khi mất mạng, report được giữ lại để thử gửi sau. Khi dự án tắt upload, nút chuyển thành **Save report**. Phần **Local relay setup (desktop testing)** chứa các nút chạy relay local. Có thể cấu hình đồng thời hai đích bằng `DREAMY_DESTINATIONS=discord,telegram`. [Các thao tác Report hiện tại](../../LocalPackages/com.dreamy.debugger/Documentation~/REPORTING.md)

Checkpoint 2026-11-25. Luồng một lần bấm và màn hình kết nối đã được triển khai trong package. Môi trường QA trên điện thoại cần URL HTTPS vận hành thực tế. Dreamy chưa cung cấp relay dùng chung cho khách cài package.

Unity 6000.4.12f1 batchmode trong dự án tách biệt chạy đạt 3 PlayMode và 18 EditMode. PlayMode kiểm tra lưu local, một lần bấm khi relay nhận, giữ report khi relay lỗi, đóng form giữa lúc gửi và nút còn thao tác được ở bố cục dọc/ngang hẹp. EditMode kiểm tra outbox và giao thức relay với transport giả lập. Android qua HTTPS chưa được thử trên thiết bị.

Checkpoint định dạng tin nhắn. Discord có thẻ tóm tắt và ảnh xem trước. Telegram có caption trên file JSON và mã report trên ảnh đi kèm. Issue key tìm nhóm lỗi tự động theo app, loại lỗi và frame liên quan. Relay đang vận hành cần cập nhật và khởi động lại để dùng định dạng mới.

`python3 -m unittest discover -s LocalPackages/com.dreamy.debugger/Tools~/relay` đạt 37/37 trên mã đã cập nhật. Hai adapter được kiểm tra qua request giả lập, gồm file JSON, ảnh, giới hạn caption và Issue key. Chưa gửi mẫu vào kênh thật.

Checkpoint report thử đầy đủ. Editor có **Send full test report** với ảnh JPEG mẫu 1440 × 810, ba stack frame, thiết bị, FPS và bộ nhớ giả lập. Discord nhóm chẩn đoán theo môi trường, hiệu năng và lỗi. Telegram chuyển JPEG dưới dạng file để giữ byte gốc. Ảnh report runtime dùng tối đa 1440 px và ưu tiên JPEG quality 92 trong giới hạn 24 MiB buffer và 512 KiB file.

Relay đạt 38/38 test. Unity 6000.4.12f1 batchmode trong dự án tách biệt đạt 24 EditMode và 4 PlayMode, gồm kiểm tra decode ảnh mẫu và chọn kích thước theo bộ nhớ. Gói kiểm tra chứa ảnh mẫu và Unity metadata. Chưa gửi qua kênh thật hoặc đo capture trên Android.

Checkpoint khôi phục relay local. Telegram cổng 8765 và Discord cổng 8766 đang chạy tiến trình khởi động trước bản full test. Hai hàng đợi trống trước khi restart bằng cùng cấu hình. Sau restart, cả hai trả HTTP 200 và `supportsFullTest: true`. Editor báo rõ khi relay local đang chạy bản cũ và gửi loopback trực tiếp. Đạt 39 test relay và 20 EditMode, gồm hai phép kiểm tra Unity chọn đúng cổng đang chạy bằng health check. Lượt kiểm tra không gửi tin vào kênh thật.

Checkpoint nút **Test exception** trong tab Report. Nút ghi một exception có stack trace và dùng luồng auto report hiện tại. Unity 6000.4.12f1 batchmode trong dự án tách biệt đạt 4/4 PlayMode, gồm bấm qua nút Report, lưu exception tự động và xác nhận stack trace. Lượt kiểm tra dùng outbox cô lập.

Checkpoint report P0 và điều hướng Console. JSON có stack lỗi tối đa 32.768 ký tự, tối đa 100 log gần nhất kèm stack trong 256 KiB context. Gộp lỗi đang chờ theo thông điệp chuẩn hóa, mức độ và frame liên quan, giữ cooldown và 10 báo cáo tự động mỗi phiên. Runtime hiển thị Pending, Sending, Sent to relay và Failed, cho phép retry bản Failed với ID cũ. Debugger kiểm tra credential nhà cung cấp trong cấu hình build và trước khi tạo relay transport. Console hỗ trợ Inspect cho log mang object context khi Hierarchy mở được.

Kiểm tra Unity 6000.4.12f1 batchmode trên bản nguồn tách biệt đạt 6 PlayMode và 21 EditMode. Relay Python đạt 40 test, có kiểm tra stack Unicode vượt giới hạn byte cũ và so sánh JSON đính kèm. Hai relay local 8765 và 8766 đã nạp bản mới, trả HTTP 200 với giới hạn 3.000.000 byte. Chưa kiểm tra APK Android phát hành hoặc gửi sang kênh thật.
