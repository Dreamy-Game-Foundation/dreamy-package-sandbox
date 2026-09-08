# Dreamy Package Development Sandbox

Sandbox Unity project để phát triển, sửa và test độc lập các UPM package `com.dreamy.*` của organization `Dreamy-Game-Foundation`.

Thư mục hiện tại là project sandbox. Package source được đặt ở thư mục sibling `../Packages`, tương đương layout:

```text
DreamyFoundation/
├── Packages/
│   ├── com.dreamy.core/
│   ├── com.dreamy.ui/
│   ├── com.dreamy.audio/
│   ├── com.dreamy.assets/
│   ├── com.dreamy.editor-tools/
│   ├── com.dreamy.datasave/
│   ├── com.dreamy.dataconfig/
│   ├── com.dreamy.feedback/
│   └── com.dreamy.localization/
└── Sandbox/dreamy-package-sandbox/  # thư mục hiện tại
```

Không copy source package vào `Assets` và không sửa `Library/PackageCache`. `Packages/manifest.json` dùng local `file:../Packages/...` để Unity nhận thay đổi trực tiếp từ Git repository của package.

## Setup

Từ thư mục hiện tại:

```bash
./setup.sh
```

Script sẽ tạo `../Packages`, clone repository còn thiếu, in trạng thái từng repository và bỏ qua repository đã tồn tại. Script không reset, checkout hoặc discard local changes. Cần SSH access tới GitHub organization.

Sau khi clone, mở thư mục hiện tại bằng Unity Hub/Unity Editor. Nếu package manifest hoặc assembly name thực tế khác convention hiện tại, xử lý lỗi dependency trong package source; không thêm coupling hoặc sửa public API chỉ để sandbox compile.

## Sandbox layout

```text
Assets/Sandbox/
├── Core/
├── UI/
├── Audio/
├── Assets/
├── DataSave/
├── DataConfig/
├── Feedback/
├── Localization/
└── EditorTools/
```

Mỗi nhóm có asmdef riêng. Các reference được giữ tối thiểu: UI/Audio/Assets/DataSave/DataConfig/Feedback/Localization/EditorTools chỉ reference `Dreamy.Core` và package tương ứng; `EditorTools` chỉ chạy trong Editor. Scene hoặc test component của từng package có thể đặt trong nhóm tương ứng.

## Workflow

```text
Package source
    ↓
Sandbox local test
    ↓
Template integration test
    ↓
Commit / PR
    ↓
Version bump
    ↓
Git tag
    ↓
Template dùng released Git version
```

Development:

```text
Sandbox
   ↓ file:
Local Packages
```

Production/template:

```text
Template
   ↓ Git tag
com.dreamy.* vX.Y.Z
```

Sandbox này chỉ kiểm thử package, không trở thành template project thứ hai và không chứa gameplay-specific code như `GameManager`, `PlayerManager` hoặc `ShopManager`.

## Verification

Kiểm tra tĩnh nhanh:

```bash
python3 -m json.tool Packages/manifest.json >/dev/null
python3 -m json.tool Packages/packages-lock.json >/dev/null
git status --short
```

Unity sẽ tự cập nhật `packages-lock.json` sau khi các local package đã được clone và manifest của từng package được resolve.
