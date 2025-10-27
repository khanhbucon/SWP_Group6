# Transaction History Feature

## Tổng quan
Tính năng lịch sử giao dịch cho phép người dùng xem và theo dõi tất cả các giao dịch tài chính trong hệ thống.

## Tính năng đã hoàn thành

### 1. API Endpoints
- **GET** `/api/account/transaction-history` - Lấy danh sách lịch sử giao dịch với phân trang
- **GET** `/api/account/transaction-history/{id}` - Lấy chi tiết một giao dịch cụ thể

### 2. Frontend Pages
- **Transaction History** (`/Transaction/History`) - Trang danh sách giao dịch
- **Transaction Details** (`/Transaction/Details/{id}`) - Trang chi tiết giao dịch

### 3. Tính năng chính

#### Dashboard Summary
- **Tổng thu**: Tổng số tiền đã nhận (DEPOSIT, REFUND)
- **Tổng chi**: Tổng số tiền đã chi (PURCHASE, WITHDRAW, ORDER_PAYMENT)
- **Số dư**: Chênh lệch giữa thu và chi
- **Tổng giao dịch**: Số lượng giao dịch

#### Transaction Table
- Hiển thị danh sách giao dịch với thông tin:
  - ID giao dịch
  - Loại giao dịch (với icon và màu sắc)
  - Số tiền (màu xanh cho thu, đỏ cho chi)
  - Mô tả
  - Trạng thái (với badge màu)
  - Ngày tạo
  - Nút xem chi tiết

#### Transaction Details
- Thông tin chi tiết giao dịch
- Tóm tắt với icon và màu sắc
- Các thao tác có thể thực hiện

### 4. Loại giao dịch hỗ trợ
- **DEPOSIT**: Nạp tiền vào ví
- **PURCHASE**: Mua hàng
- **WITHDRAW**: Rút tiền
- **REFUND**: Hoàn tiền
- **TRANSFER**: Chuyển khoản
- **ORDER_PAYMENT**: Thanh toán đơn hàng

### 5. Trạng thái giao dịch
- **SUCCESS**: Thành công (màu xanh)
- **PENDING**: Đang xử lý (màu vàng)
- **FAILED**: Thất bại (màu đỏ)
- **CANCELLED**: Đã hủy (màu xám)

## Cách sử dụng

### 1. Truy cập lịch sử giao dịch
1. Đăng nhập vào hệ thống
2. Click vào "Lịch sử giao dịch" trong navigation bar
3. Hoặc truy cập trực tiếp: `/Transaction/History`

### 2. Xem chi tiết giao dịch
1. Trong danh sách giao dịch, click nút "Chi tiết"
2. Hoặc truy cập trực tiếp: `/Transaction/Details/{id}`

### 3. Tìm kiếm và lọc
- Sử dụng thanh tìm kiếm của DataTable
- Sắp xếp theo các cột
- Phân trang tự động

## Cấu trúc Files

### Backend (Mo_Api)
```
Mo_Api/ApiController/
└── AccountController.cs          # API endpoints

Mo_DataAccess/Services/
├── PaymentTransactionServices.cs # Business logic
└── Interface/
    └── IPaymentTransactionServices.cs

Mo_Entities/ModelResponse/
└── TransactionHistoryResponse.cs # Response models
```

### Frontend (Mo_Client)
```
Mo_Client/
├── Controllers/
│   └── TransactionController.cs  # MVC Controller
├── Services/
│   └── TransactionService.cs      # API Client
├── Models/
│   └── PaymentTransactionVm.cs    # View Models
├── Views/Transaction/
│   ├── History.cshtml            # Danh sách giao dịch
│   └── Details.cshtml            # Chi tiết giao dịch
└── wwwroot/css/
    └── transaction.css           # Custom styles
```

## API Documentation

### Get Transaction History
```http
GET /api/account/transaction-history?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

**Response:**
```json
{
  "Success": true,
  "Data": {
    "Transactions": [
      {
        "Id": 1,
        "UserId": 1,
        "Type": "DEPOSIT",
        "Amount": 1000000,
        "Description": "Nạp tiền vào ví",
        "CreatedAt": "2024-01-01T10:00:00Z",
        "Status": "SUCCESS",
        "TypeDisplay": "Nạp tiền",
        "StatusDisplay": "Thành công",
        "AmountDisplay": "1,000,000 VNĐ",
        "IsIncome": true,
        "IsExpense": false
      }
    ],
    "TotalCount": 10,
    "PageNumber": 1,
    "PageSize": 20,
    "TotalIncome": 2000000,
    "TotalExpense": 500000,
    "NetAmount": 1500000
  }
}
```

### Get Transaction Details
```http
GET /api/account/transaction-history/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "Success": true,
  "Data": {
    "Id": 1,
    "Type": "DEPOSIT",
    "Amount": 1000000,
    "Description": "Nạp tiền vào ví",
    "Status": "SUCCESS",
    "CreatedAt": "2024-01-01T10:00:00Z"
  }
}
```

## Security

### Authorization
- Tất cả endpoints yêu cầu JWT token hợp lệ
- User chỉ có thể xem giao dịch của chính mình
- Admin có thể xem tất cả giao dịch (có thể mở rộng)

### Data Validation
- Input sanitization
- SQL injection prevention
- XSS protection

## Performance

### Optimization
- Pagination để giảm tải dữ liệu
- Efficient LINQ queries
- Database indexing trên UserId và CreatedAt

### Caching
- Có thể implement Redis cache cho frequently accessed data
- Client-side caching cho search results

## Testing

### Sample Data
Chạy script `TransactionHistorySampleData.sql` để thêm dữ liệu mẫu:

```sql
-- Thêm các giao dịch mẫu với các loại và trạng thái khác nhau
-- Xem file TransactionHistorySampleData.sql để biết chi tiết
```

### Test Cases
1. **Authentication**: Kiểm tra yêu cầu đăng nhập
2. **Authorization**: Kiểm tra user chỉ xem được giao dịch của mình
3. **Pagination**: Kiểm tra phân trang hoạt động đúng
4. **Search**: Kiểm tra tìm kiếm và lọc
5. **Responsive**: Kiểm tra giao diện trên mobile

## Future Enhancements

### Planned Features
1. **Export Data**: Export lịch sử giao dịch ra Excel/CSV
2. **Advanced Filters**: Filter theo loại, trạng thái, khoảng thời gian
3. **Real-time Updates**: WebSocket cho real-time notifications
4. **Transaction Categories**: Phân loại giao dịch chi tiết hơn
5. **Receipt Generation**: Tạo hóa đơn PDF cho giao dịch

### Performance Improvements
1. **Server-side Pagination**: Giảm memory usage
2. **Database Indexing**: Tối ưu query performance
3. **Caching Layer**: Redis cache cho frequently accessed data
4. **CDN**: Static assets delivery optimization

## Troubleshooting

### Common Issues

1. **"Không thể tải lịch sử giao dịch"**
   - Kiểm tra JWT token còn hợp lệ không
   - Kiểm tra API endpoint hoạt động đúng không
   - Kiểm tra database connection

2. **"Không tìm thấy giao dịch"**
   - Kiểm tra ID giao dịch có tồn tại không
   - Kiểm tra user có quyền xem giao dịch đó không

3. **"Giao diện không hiển thị đúng"**
   - Kiểm tra CSS và JS files được load đúng không
   - Kiểm tra DataTables library
   - Kiểm tra browser console cho errors

### Debug Mode
- Enable detailed error messages trong development
- Check browser console cho JavaScript errors
- Check API logs cho server-side errors

## Support

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra logs trong console
2. Verify API endpoints hoạt động đúng
3. Check database connectivity
4. Contact development team với error details
