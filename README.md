# 🏍️ SmartMotoRental

Hệ thống quản lý cho thuê xe máy thông minh được xây dựng bằng ASP.NET Core MVC.

## 📋 Mô tả

SmartMotoRental là một ứng dụng web quản lý dịch vụ cho thuê xe máy, cho phép người dùng xem danh sách xe, xem chi tiết, đặt thuê và quản lý các giao dịch thuê xe. Hệ thống hỗ trợ quản lý từ phía admin và trải nghiệm người dùng thân thiện.

## ✨ Tính năng

### Người dùng
- 📱 Xem danh sách xe máy có sẵn
- 🔍 Xem chi tiết thông tin từng xe máy
- 💰 Xem giá thuê theo giờ/ngày
- 📍 Xem vị trí xe
- ⭐ Đánh giá và nhận xét xe
- ❤️ Yêu thích xe
- 📝 Đặt thuê xe
- .....

### Quản trị viên
- 🛠️ Quản lý danh sách xe máy (CRUD)
- 📊 Quản lý đơn thuê
- 👥 Quản lý người dùng
- 📢 Quản lý thông báo
- 💬 Quản lý chat/liên hệ
- ....

## 🛠️ Công nghệ sử dụng

- **Framework**: ASP.NET Core 7.0 (MVC)
- **Database**: SQLite
- **ORM**: Entity Framework Core 7.0
- **Frontend**: 
  - Bootstrap 5
  - jQuery
  - HTML5/CSS3
  - Razor Views

## 📦 Yêu cầu hệ thống

- .NET 7.0 SDK hoặc cao hơn
- Visual Studio 2022 / Visual Studio Code / JetBrains Rider
- Git (tùy chọn)

## 🚀 Cài đặt và Chạy

### 1. Clone repository

```bash
git clone <repository-url>
cd SmartMotoRental
```

### 2. Khôi phục dependencies

```bash
cd SmartMotoRental
dotnet restore
```

### 3. Tạo database và chạy migrations

```bash
dotnet ef database update
```

### 4. Chạy ứng dụng

```bash
dotnet run
```

Hoặc sử dụng Visual Studio:
- Nhấn `F5` hoặc chọn "Run" từ menu

### 5. Truy cập ứng dụng

- **HTTP**: `http://localhost:5047`
- **HTTPS**: `https://localhost:7030`


## 🗄️ Database Schema

### Các bảng chính:
- **Motorbikes**: Thông tin xe máy
- **Users**: Thông tin người dùng
- **Rentals**: Đơn thuê xe
- **Reviews**: Đánh giá và nhận xét
- **Favorites**: Xe yêu thích
- **Notifications**: Thông báo
- **ChatLogs**: Lịch sử chat
- **Suggestions**: Gợi ý
- **BikeImages**: Hình ảnh xe


## 🔧 Cấu hình

### Connection String

File `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=smart_moto_rental.db"
  }
}
```

### Thay đổi Port

Chỉnh sửa `Properties/launchSettings.json` để thay đổi port mặc định.

## 📄 License

Project này được phát triển cho mục đích học tập và nghiên cứu.

**Lưu ý**: Đây là phiên bản phát triển. Một số tính năng có thể đang trong quá trình hoàn thiện.

