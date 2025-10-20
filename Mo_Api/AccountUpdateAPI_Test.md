# Account Update API - Hướng dẫn sử dụng

## Các API Endpoints đã hoàn thành:

### 1. **PUT /api/Account/update-profile** - Cập nhật thông tin cá nhân và KYC

**Request Body:**
```json
{
  "username": "newusername",
  "email": "newemail@example.com", 
  "phone": "0123456789",
  "identificationF": "/images/kyc/kyc_f_123_20241201120000.jpg",
  "identificationB": "/images/kyc/kyc_b_123_20241201120000.jpg"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Cập nhật thông tin thành công",
  "data": {
    "id": 123,
    "username": "newusername",
    "email": "newemail@example.com",
    "phone": "0123456789",
    "identificationF": "/images/kyc/kyc_f_123_20241201120000.jpg",
    "identificationB": "/images/kyc/kyc_b_123_20241201120000.jpg",
    "updatedAt": "2024-12-01T12:00:00Z"
  }
}
```

### 2. **POST /api/Account/upload-kyc** - Upload ảnh căn cước

**Form Data:**
- `identificationF`: File ảnh mặt trước căn cước
- `identificationB`: File ảnh mặt sau căn cước

**Response:**
```json
{
  "success": true,
  "message": "Upload ảnh KYC thành công",
  "data": {
    "identificationF": "/images/kyc/kyc_f_123_20241201120000.jpg",
    "identificationB": "/images/kyc/kyc_b_123_20241201120000.jpg"
  }
}
```

### 3. **GET /api/Account/profile** - Lấy thông tin profile (đã có sẵn)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "username": "username",
    "email": "email@example.com",
    "phone": "0123456789",
    "balance": 1000000,
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-12-01T12:00:00Z",
    "roles": ["Buyer", "Seller"],
    "isEKYCVerified": true,
    "totalOrders": 10,
    "totalShops": 2,
    "totalProductsSold": 50,
    "identificationF": "/images/kyc/kyc_f_123_20241201120000.jpg",
    "identificationB": "/images/kyc/kyc_b_123_20241201120000.jpg"
  }
}
```

## Cách test với Postman:

### Test Update Profile:
1. **Method**: PUT
2. **URL**: `https://localhost:7283/api/Account/update-profile`
3. **Headers**: 
   - `Authorization: Bearer YOUR_JWT_TOKEN`
   - `Content-Type: application/json`
4. **Body** (raw JSON):
```json
{
  "username": "testuser",
  "email": "test@example.com",
  "phone": "0123456789"
}
```

### Test Upload KYC:
1. **Method**: POST
2. **URL**: `https://localhost:7283/api/Account/upload-kyc`
3. **Headers**: 
   - `Authorization: Bearer YOUR_JWT_TOKEN`
4. **Body** (form-data):
   - `identificationF`: [Chọn file ảnh mặt trước]
   - `identificationB`: [Chọn file ảnh mặt sau]

## Validation Rules:

### UpdateProfileRequest:
- `username`: Required, 3-50 ký tự
- `email`: Required, định dạng email hợp lệ
- `phone`: Optional, định dạng số điện thoại hợp lệ

### Upload KYC:
- Chỉ chấp nhận file: .jpg, .jpeg, .png, .gif
- Phải upload đầy đủ 2 file (mặt trước và mặt sau)
- File được lưu trong `wwwroot/images/kyc/`

## Error Handling:

### Common Errors:
- **401 Unauthorized**: Token không hợp lệ hoặc hết hạn
- **400 Bad Request**: Dữ liệu không hợp lệ hoặc đã tồn tại
- **500 Internal Server Error**: Lỗi server

### Example Error Response:
```json
{
  "success": false,
  "message": "Username đã tồn tại"
}
```
