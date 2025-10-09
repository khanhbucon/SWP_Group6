# Admin Account Management System

## Tổng quan
Hệ thống quản lý tài khoản dành cho Admin với đầy đủ các tính năng quản lý, tìm kiếm và thao tác trên tài khoản người dùng.

## Tính năng chính

### 1. Dashboard Admin
- **URL**: `/Admin/Dashboard`
- **Mô tả**: Trang tổng quan với thống kê và quick actions
- **Tính năng**:
  - Hiển thị thống kê tổng quan (sẽ được phát triển thêm)
  - Quick actions để truy cập nhanh các chức năng
  - Thông tin hệ thống

### 2. Quản lý tài khoản
- **URL**: `/Admin/Accounts`
- **Mô tả**: Trang chính để quản lý tất cả tài khoản trong hệ thống
- **Tính năng**:
  - Hiển thị danh sách tất cả tài khoản
  - Tìm kiếm theo username, email, phone
  - Hiển thị thông tin chi tiết: ID, Username, Email, Phone, Balance, Roles, Status, eKYC, Stats, Created Date
  - Các action buttons cho từng tài khoản

### 3. Các Action có sẵn

#### Grant Seller Role
- **Mô tả**: Cấp phép cho user trở thành Seller
- **Điều kiện**: User chưa có role "Seller"
- **API**: `POST /api/account/admin/{id}/grant-seller`
- **UI**: Button màu vàng với icon store

#### Toggle Ban User
- **Mô tả**: Khóa/mở khóa tài khoản
- **Logic**: 
  - Nếu `IsActive = true` → Khóa tài khoản (`IsActive = false`)
  - Nếu `IsActive = false` → Mở khóa tài khoản (`IsActive = true`)
- **API**: `POST /api/account/admin/{id}/toggle-ban`
- **UI**: Button màu đỏ (ban) hoặc xanh (unban)

#### View Account Details
- **Mô tả**: Xem chi tiết tài khoản (đang phát triển)
- **UI**: Button màu xanh với icon eye

## Cấu trúc dữ liệu

### AdminAccountListVm
```csharp
public class AdminAccountListVm
{
    public List<AccountListItem> Accounts { get; set; }
    public string? SearchTerm { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

### AccountListItem
```csharp
public class AccountListItem
{
    public long UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public decimal? Balance { get; set; }
    public bool? IsActive { get; set; }
    public List<string> Roles { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int TotalOrders { get; set; }
    public int TotalShops { get; set; }
    public int TotalProductsSold { get; set; }
    public bool IsEKYCVerified { get; set; }
}
```

## API Endpoints

### 1. Get All Accounts
- **Method**: `GET`
- **URL**: `/api/account/all`
- **Authorization**: `Admin` role required
- **Response**: 
```json
{
  "Success": true,
  "Data": [
    {
      "UserId": 1,
      "Username": "admin",
      "Email": "admin@example.com",
      "Phone": "0123456789",
      "Balance": 1000000,
      "IsActive": true,
      "Roles": ["Admin"],
      "CreatedAt": "2024-01-01T00:00:00Z",
      "TotalOrders": 5,
      "TotalShops": 2,
      "TotalProductsSold": 10,
      "IsEKYCVerified": true
    }
  ]
}
```

### 2. Grant Seller Role
- **Method**: `POST`
- **URL**: `/api/account/admin/{id}/grant-seller`
- **Authorization**: `Admin` role required
- **Response**:
```json
{
  "Success": true,
  "Message": "Cấp phép Seller thành công",
  "Account": {
    "Id": 1,
    "Username": "user1",
    "CurrentRoles": ["Buyer", "Seller"],
    "UpdatedAt": "2024-01-01T00:00:00Z"
  }
}
```

### 3. Toggle Ban User
- **Method**: `POST`
- **URL**: `/api/account/admin/{id}/toggle-ban`
- **Authorization**: `Admin` role required
- **Response**:
```json
{
  "Success": true,
  "Message": "Khóa tài khoản thành công",
  "Account": {
    "Id": 1,
    "Username": "user1",
    "IsActive": false,
    "UpdatedAt": "2024-01-01T00:00:00Z"
  }
}
```

## Cách sử dụng

### 1. Truy cập Admin Dashboard
1. Đăng nhập với tài khoản có role "Admin"
2. Click vào dropdown "Admin" trong navigation bar
3. Chọn "Dashboard" hoặc "Quản lý tài khoản"

### 2. Quản lý tài khoản
1. Truy cập `/Admin/Accounts`
2. Sử dụng search box để tìm kiếm tài khoản
3. Click các action buttons để thực hiện thao tác:
   - 🏪 **Grant Seller**: Cấp phép Seller
   - 🚫 **Ban/Unban**: Khóa/mở khóa tài khoản
   - 👁️ **View Details**: Xem chi tiết (đang phát triển)

### 3. Tìm kiếm
- Nhập từ khóa vào search box
- Hệ thống sẽ tìm kiếm theo:
  - Username
  - Email
  - Phone number
- Click "Tìm kiếm" hoặc nhấn Enter

## UI/UX Features

### Responsive Design
- Tương thích với mobile và desktop
- Table responsive với horizontal scroll
- Button groups được tối ưu cho touch devices

### Loading States
- Loading overlay khi thực hiện actions
- Spinner animation
- Auto-dismiss alerts sau 5 giây

### Visual Feedback
- Success/Error alerts với icons
- Color-coded status badges
- Hover effects trên cards và buttons

### Accessibility
- ARIA labels cho screen readers
- Keyboard navigation support
- High contrast color scheme

## Security

### Authorization
- Tất cả endpoints yêu cầu `Admin` role
- JWT token validation
- CSRF protection

### Data Validation
- Input sanitization
- SQL injection prevention
- XSS protection

## Performance

### Optimization
- Lazy loading cho large datasets
- Efficient LINQ queries
- Minimal database calls

### Caching
- Response caching (có thể implement sau)
- Client-side caching cho search results

## Troubleshooting

### Common Issues

1. **"Không có quyền truy cập"**
   - Kiểm tra user có role "Admin" không
   - Kiểm tra JWT token còn hợp lệ không

2. **"Không tìm thấy tài khoản"**
   - Kiểm tra ID tài khoản có tồn tại không
   - Kiểm tra database connection

3. **"Có lỗi xảy ra khi cấp phép Seller"**
   - Kiểm tra user đã có role "Seller" chưa
   - Kiểm tra role "Seller" có tồn tại trong database không

### Debug Mode
- Enable detailed error messages trong development
- Check browser console cho JavaScript errors
- Check API logs cho server-side errors

## Future Enhancements

### Planned Features
1. **Pagination**: Thêm phân trang cho large datasets
2. **Bulk Actions**: Thao tác hàng loạt trên nhiều tài khoản
3. **Advanced Filters**: Filter theo role, status, date range
4. **Export Data**: Export danh sách tài khoản ra Excel/CSV
5. **Audit Log**: Ghi log tất cả thao tác admin
6. **Real-time Updates**: WebSocket cho real-time notifications

### Performance Improvements
1. **Server-side Pagination**: Giảm memory usage
2. **Database Indexing**: Tối ưu query performance
3. **Caching Layer**: Redis cache cho frequently accessed data
4. **CDN**: Static assets delivery optimization

## Support

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra logs trong console
2. Verify API endpoints hoạt động đúng
3. Check database connectivity
4. Contact development team với error details

