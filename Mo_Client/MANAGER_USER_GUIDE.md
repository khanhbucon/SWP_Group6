# Trang Quản lý Người dùng (Manager User)

## Tổng quan
Trang Manager User là một tính năng admin cho phép quản lý tất cả người dùng trong hệ thống. Trang này hiển thị danh sách đầy đủ các thông tin người dùng và cung cấp các chức năng quản lý cơ bản.

## Tính năng

### 1. Hiển thị danh sách người dùng
- **ID người dùng**: Mã định danh duy nhất
- **Tên đăng nhập**: Username của người dùng
- **Email**: Địa chỉ email
- **Số điện thoại**: Số điện thoại liên hệ
- **Số dư**: Số tiền trong ví của người dùng
- **Trạng thái**: Hoạt động hoặc bị khóa
- **Vai trò**: Các role của người dùng (User, Seller, Admin)
- **Ngày tạo**: Thời gian tạo tài khoản
- **Tổng đơn hàng**: Số lượng đơn hàng đã thực hiện
- **Tổng cửa hàng**: Số lượng cửa hàng sở hữu
- **Sản phẩm đã bán**: Tổng số sản phẩm đã bán
- **Xác thực eKYC**: Trạng thái xác thực danh tính

### 2. Chức năng quản lý
- **Khóa/Mở khóa tài khoản**: Thay đổi trạng thái hoạt động của người dùng
- **Cấp phép Seller**: Cấp quyền bán hàng cho người dùng (chỉ khi đã xác minh eKYC)

### 3. Tính năng giao diện
- **DataTables**: Bảng có thể sắp xếp, tìm kiếm và phân trang
- **Responsive**: Tối ưu cho mọi thiết bị
- **Thông báo**: Hiển thị kết quả các thao tác
- **Xác nhận**: Hỏi xác nhận trước khi thực hiện các thao tác quan trọng

## Cách sử dụng

### Truy cập trang
1. Đăng nhập với tài khoản có role Admin
2. Vào menu Admin > Quản lý người dùng
3. Hoặc truy cập trực tiếp: `/Account/ManagerUser`

### Quản lý người dùng
1. **Khóa tài khoản**: Click nút "Khóa" (màu vàng) → Xác nhận
2. **Mở khóa tài khoản**: Click nút "Mở khóa" (màu xanh) → Xác nhận
3. **Cấp phép Seller**: 
   - Nếu đã xác minh eKYC: Click nút "Cấp Seller" (màu xanh dương) → Xác nhận
   - Nếu chưa xác minh eKYC: Nút "Cấp Seller" sẽ bị disable (màu xám)

### Tìm kiếm và lọc
- Sử dụng thanh tìm kiếm của DataTables để tìm người dùng theo bất kỳ trường nào
- Sắp xếp theo các cột bằng cách click vào header
- Sử dụng phân trang để duyệt qua nhiều trang dữ liệu

## API Endpoints được sử dụng

### 1. Lấy danh sách người dùng
```
GET /api/account/Admin/GetAllAccount
```

### 2. Khóa/Mở khóa người dùng
```
POST /api/account/admin/{id}/banUser
```

### 3. Cấp phép Seller
```
POST /api/account/admin/{id}/grant-seller
```

## Cấu trúc file

### Models
- `Mo_Client/Models/ListAccountResponse.cs`: Model hiển thị thông tin người dùng

### Controllers
- `Mo_Client/Controllers/AccountController.cs`: Controller xử lý các action
  - `ManagerUser()`: Hiển thị danh sách người dùng
  - `BanUser(long userId)`: Khóa/mở khóa người dùng
  - `GrantSellerRole(long userId)`: Cấp phép Seller

### Services
- `Mo_Client/Services/AuthApiClient.cs`: Client gọi API
  - `GetAllUsersAsync()`: Lấy danh sách người dùng
  - `BanUserAsync(long userId)`: Khóa/mở khóa người dùng
  - `GrantSellerRoleAsync(long userId)`: Cấp phép Seller
  - `SetToken(string token)`: Set token cho authentication

### Views
- `Mo_Client/Views/Account/ManagerUser.cshtml`: Giao diện trang quản lý

### CSS
- `Mo_Client/wwwroot/css/site.css`: Styling cho trang Manager User

## Bảo mật
- Yêu cầu đăng nhập với `[Authorize]` attribute
- Kiểm tra token trong cookie trước khi gọi API
- **Kiểm tra role Admin**: Chỉ người dùng có role "Admin" mới có thể truy cập
- Redirect về trang chủ nếu không có quyền Admin
- Tất cả các action đều có kiểm tra role Admin

## Dependencies
- Bootstrap 5: UI framework
- DataTables: Bảng tương tác
- Font Awesome: Icons
- jQuery: JavaScript library

## Lưu ý
- **Trang này chỉ dành cho Admin** - Có kiểm tra role nghiêm ngặt
- **Cấp phép Seller yêu cầu eKYC**: Chỉ người dùng đã xác minh danh tính mới được cấp quyền Seller
- Nếu không có role Admin sẽ bị redirect về trang chủ với thông báo lỗi
- Tất cả các thao tác đều có xác nhận để tránh nhầm lẫn
- Token được tự động lấy từ cookie và set vào HTTP header
- Responsive design hoạt động tốt trên mobile và desktop
