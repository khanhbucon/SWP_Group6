# Mo_Client - Giao diện Login/Register

## Tổng quan
Dự án này cung cấp giao diện đăng nhập và đăng ký tài khoản với thiết kế hiện đại và responsive.

## Tính năng đã hoàn thành

### 1. Giao diện Đăng nhập (`/Account/Login`)
- Form đăng nhập với tên đăng nhập/email và mật khẩu
- Checkbox "Ghi nhớ đăng nhập"
- Link "Quên mật khẩu"
- Link chuyển đến trang đăng ký
- Validation phía client và server
- Hiển thị thông báo lỗi

### 2. Giao diện Đăng ký (`/Account/Register`)
- Form đăng ký với các trường:
  - Tên đăng nhập (3-50 ký tự)
  - Email (validation email)
  - Số điện thoại (10-11 chữ số)
  - Mật khẩu (tối thiểu 6 ký tự)
  - Xác nhận mật khẩu
- Validation mật khẩu khớp nhau
- Validation số điện thoại theo pattern
- Hiển thị thông báo thành công/lỗi
- Link chuyển đến trang đăng nhập

### 3. Thiết kế UI/UX
- **Responsive Design**: Tương thích với mọi thiết bị
- **Modern UI**: Sử dụng gradient, shadow, animation
- **Font Awesome Icons**: Icons đẹp mắt cho các button và elements
- **Bootstrap 5**: Framework CSS hiện đại
- **Custom CSS**: Styling riêng cho auth pages

### 4. Tính năng kỹ thuật
- **Token Authentication**: Lưu access token và roles trong cookies
- **API Integration**: Kết nối với Mo_Api backend
- **Error Handling**: Xử lý lỗi và hiển thị thông báo
- **Form Validation**: Validation phía client và server
- **Security**: HttpOnly cookies, HTTPS support

## Cấu trúc Files

```
Mo_Client/
├── Controllers/
│   └── AccountController.cs          # Xử lý login/register/logout
├── Models/
│   ├── LoginVm.cs                    # Model cho form đăng nhập
│   └── RegisterVm.cs                 # Model cho form đăng ký
├── Services/
│   └── AuthApiClient.cs              # Client gọi API authentication
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml              # Giao diện đăng nhập
│   │   └── Register.cshtml           # Giao diện đăng ký
│   ├── Home/
│   │   └── Index.cshtml              # Trang chủ với demo
│   └── Shared/
│       └── _Layout.cshtml            # Layout chung
└── wwwroot/
    └── css/
        └── site.css                  # Custom CSS cho auth pages
```

## Cách sử dụng

### 1. Chạy ứng dụng
```bash
cd Mo_Client
dotnet run
```

### 2. Truy cập các trang
- **Trang chủ**: `https://localhost:5001/`
- **Đăng nhập**: `https://localhost:5001/Account/Login`
- **Đăng ký**: `https://localhost:5001/Account/Register`

### 3. Navigation
- Sử dụng menu navigation ở header
- Hoặc click các button trên trang chủ
- Hoặc click link chuyển đổi giữa login/register

## API Endpoints cần thiết

Backend API cần cung cấp các endpoints:

### POST `/api/auth/login`
```json
{
  "identifier": "username_or_email",
  "password": "password",
  "rememberMe": true
}
```

### POST `/api/auth/register`
```json
{
  "username": "username",
  "email": "email@example.com",
  "phone": "0123456789",
  "password": "password"
}
```

## Cấu hình

### appsettings.json
```json
{
  "Api": {
    "BaseUrl": "https://localhost:7234"
  }
}
```

## Tính năng nổi bật

1. **Giao diện đẹp**: Gradient background, card design, smooth animations
2. **User Experience**: Intuitive navigation, clear error messages
3. **Responsive**: Hoạt động tốt trên mobile, tablet, desktop
4. **Accessibility**: Proper labels, ARIA attributes, keyboard navigation
5. **Security**: Token-based authentication, secure cookies
6. **Performance**: Optimized CSS, minimal JavaScript

## Browser Support
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Tương lai
- [ ] Thêm tính năng "Quên mật khẩu"
- [ ] Social login (Google, Facebook)
- [ ] Two-factor authentication
- [ ] Email verification
- [ ] Profile management
