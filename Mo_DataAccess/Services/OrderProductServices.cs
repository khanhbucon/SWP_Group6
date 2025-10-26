using Microsoft.EntityFrameworkCore;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;

namespace Mo_DataAccess.Services;

public class OrderProductServices:GenericRepository<OrderProduct>,IOrderProductServices
{
    public OrderProductServices(SwpGroup6Context context) : base(context)
    {
    }

    public async Task<OrderHistoryListResponse> GetUserOrdersAsync(long userId, string? status = null)
    {
        var query = _context.OrderProducts
            .Include(o => o.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Shop)
                        .ThenInclude(s => s.Account)
            .Include(o => o.Account)
            .Where(o => o.AccountId == userId)
            .AsQueryable();

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        var totalCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        var orderResponses = new List<OrderHistoryResponse>();

        foreach (var order in orders)
        {
            var product = order.ProductVariant?.Product;
            var shop = product?.Shop;
            var shopOwner = shop?.Account;
            
            // Get ProductStore codes for this order
            var productStores = await _context.OrderProductProductStores
                .Include(op => op.ProductStore)
                .Where(op => op.OrderProductId == order.Id)
                .Select(op => op.ProductStore)
                .ToListAsync();

            var productCodes = productStores.Select(ps => new ProductStoreInfo
            {
                Content = ps.Content,
                Value = ps.Value,
                Status = ps.Status,
                StatusDisplay = ps.Status switch
                {
                    "AVAILABLE" => "Có sẵn",
                    "USED" => "Đã sử dụng",
                    "EXPIRED" => "Hết hạn",
                    _ => ps.Status
                }
            }).ToList();

            var orderResponse = new OrderHistoryResponse
            {
                OrderId = order.Id,
                Status = order.Status?.ToUpper() ?? "PENDING",
                StatusDisplay = (order.Status?.ToUpper() ?? "PENDING") switch
                {
                    "PENDING" => "Đang xử lý",
                    "PROCESSING" => "Đang xử lý",
                    "CONFIRMED" or "CONFIRM" => "Đã xác nhận",
                    "COMPLETED" => "Hoàn thành",
                    "SHIPPED" => "Đã giao",
                    "DELIVERED" => "Đã nhận",
                    "CANCELLED" or "CANCELED" => "Đã hủy",
                    "REJECTED" => "Đã từ chối",
                    "FAILED" => "Thất bại",
                    _ => order.Status ?? "PENDING"
                },
                StatusClass = (order.Status?.ToUpper() ?? "PENDING") switch
                {
                    "PENDING" or "CONFIRMED" or "CONFIRM" => "success", // Thay đổi từ warning sang success
                    "PROCESSING" => "info",
                    "COMPLETED" or "DELIVERED" => "success",
                    "SHIPPED" => "primary",
                    "CANCELLED" or "CANCELED" or "REJECTED" => "secondary",
                    "FAILED" => "danger",
                    _ => "secondary"
                },
                Quantity = order.Quantity,
                TotalAmount = order.Quantity * (order.ProductVariant?.Price ?? 0), // Tính lại từ Quantity × Price
                TotalAmountDisplay = $"{(order.Quantity * (order.ProductVariant?.Price ?? 0)):N0} VNĐ",
                CreatedAt = DateTime.Now, // TODO: Add CreatedAt field to OrderProduct model
                CreatedAtDisplay = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                
                // Product Info
                ProductId = product?.Id ?? 0,
                ProductName = product?.Name ?? "N/A",
                ProductDescription = product?.Description,
                ProductImage = product?.Image,
                ProductImageBase64 = product?.Image != null ? Convert.ToBase64String(product.Image) : string.Empty,
                
                // Variant Info
                ProductVariantId = order.ProductVariantId,
                VariantName = order.ProductVariant?.Name ?? "N/A",
                VariantPrice = order.ProductVariant?.Price ?? 0,
                VariantPriceDisplay = $"{(order.ProductVariant?.Price ?? 0):N0} VNĐ",
                
                // Shop Info
                ShopId = shop?.Id ?? 0,
                ShopName = shop?.Name ?? "N/A",
                ShopDescription = shop?.Description,
                ShopEmail = shopOwner?.Email,
                ShopPhone = shopOwner?.Phone,
                
                // Product Codes
                ProductCodes = productCodes,
                
                // Additional flags
                HasCodes = productCodes.Any(),
                CanCancel = (order.Status?.ToUpper() ?? "PENDING") == "PENDING" || 
                             (order.Status?.ToUpper() ?? "PENDING") == "PROCESSING" ||
                             (order.Status?.ToUpper() ?? "PENDING") == "CONFIRMED" ||
                             (order.Status?.ToUpper() ?? "PENDING") == "CONFIRM"
            };

            orderResponses.Add(orderResponse);
        }

        // Calculate summary statistics
        var allUserOrders = await query.ToListAsync();
        var totalSpent = allUserOrders.Sum(o => o.Quantity * (o.ProductVariant?.Price ?? 0)); // Tính lại từ Quantity × Price
        var totalOrders = allUserOrders.Count;
        var completedOrders = allUserOrders.Count(o => 
            (o.Status?.ToUpper() ?? "") == "COMPLETED" || (o.Status?.ToUpper() ?? "") == "DELIVERED");
        var pendingOrders = allUserOrders.Count(o => 
            (o.Status?.ToUpper() ?? "") == "PENDING" || 
            (o.Status?.ToUpper() ?? "") == "PROCESSING");
        var confirmedOrders = allUserOrders.Count(o => 
            (o.Status?.ToUpper() ?? "") == "CONFIRMED" || 
            (o.Status?.ToUpper() ?? "") == "CONFIRM");
        var cancelledOrders = allUserOrders.Count(o => 
            (o.Status?.ToUpper() ?? "") == "CANCELLED" || 
            (o.Status?.ToUpper() ?? "") == "CANCELED" ||
            (o.Status?.ToUpper() ?? "") == "REJECTED");

        return new OrderHistoryListResponse
        {
            Orders = orderResponses,
            TotalCount = totalCount,
            TotalSpent = totalSpent,
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            ConfirmedOrders = confirmedOrders,
            CancelledOrders = cancelledOrders
        };
    }

    public async Task<OrderHistoryResponse?> GetOrderDetailAsync(long orderId, long userId)
    {
        var order = await _context.OrderProducts
            .Include(o => o.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Shop)
                        .ThenInclude(s => s.Account)
            .Include(o => o.Account)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.AccountId == userId);

        if (order == null)
            return null;

        var product = order.ProductVariant?.Product;
        var shop = product?.Shop;
        var shopOwner = shop?.Account;

        // Get ProductStore codes
        var productStores = await _context.OrderProductProductStores
            .Include(op => op.ProductStore)
            .Where(op => op.OrderProductId == order.Id)
            .Select(op => op.ProductStore)
            .ToListAsync();

        var productCodes = productStores.Select(ps => new ProductStoreInfo
        {
            Content = ps.Content,
            Value = ps.Value,
            Status = ps.Status,
            StatusDisplay = ps.Status switch
            {
                "AVAILABLE" => "Có sẵn",
                "USED" => "Đã sử dụng",
                "EXPIRED" => "Hết hạn",
                _ => ps.Status
            }
        }).ToList();

        return new OrderHistoryResponse
        {
            OrderId = order.Id,
            Status = order.Status?.ToUpper() ?? "PENDING",
            StatusDisplay = (order.Status?.ToUpper() ?? "PENDING") switch
            {
                "PENDING" => "Đang xử lý",
                "PROCESSING" => "Đang xử lý",
                "CONFIRMED" or "CONFIRM" => "Đã xác nhận",
                "COMPLETED" => "Hoàn thành",
                "SHIPPED" => "Đã giao",
                "DELIVERED" => "Đã nhận",
                "CANCELLED" or "CANCELED" => "Đã hủy",
                "REJECTED" => "Đã từ chối",
                "FAILED" => "Thất bại",
                _ => order.Status ?? "PENDING"
            },
            StatusClass = (order.Status?.ToUpper() ?? "PENDING") switch
            {
                "PENDING" or "CONFIRMED" or "CONFIRM" => "success",
                "PROCESSING" => "info",
                "COMPLETED" or "DELIVERED" => "success",
                "SHIPPED" => "primary",
                "CANCELLED" or "CANCELED" or "REJECTED" => "secondary",
                "FAILED" => "danger",
                _ => "secondary"
            },
            Quantity = order.Quantity,
            TotalAmount = order.Quantity * (order.ProductVariant?.Price ?? 0), // Tính lại từ Quantity × Price
            TotalAmountDisplay = $"{(order.Quantity * (order.ProductVariant?.Price ?? 0)):N0} VNĐ",
            CreatedAt = DateTime.Now,
            CreatedAtDisplay = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            
            ProductId = product?.Id ?? 0,
            ProductName = product?.Name ?? "N/A",
            ProductDescription = product?.Description,
            ProductImage = product?.Image,
            ProductImageBase64 = product?.Image != null ? Convert.ToBase64String(product.Image) : string.Empty,
            
            ProductVariantId = order.ProductVariantId,
            VariantName = order.ProductVariant?.Name ?? "N/A",
            VariantPrice = order.ProductVariant?.Price ?? 0,
            VariantPriceDisplay = $"{(order.ProductVariant?.Price ?? 0):N0} VNĐ",
            
            ShopId = shop?.Id ?? 0,
            ShopName = shop?.Name ?? "N/A",
            ShopDescription = shop?.Description,
            ShopEmail = shopOwner?.Email,
            ShopPhone = shopOwner?.Phone,
            
            ProductCodes = productCodes,
            HasCodes = productCodes.Any(),
            CanCancel = (order.Status?.ToUpper() ?? "PENDING") == "PENDING" || 
                         (order.Status?.ToUpper() ?? "PENDING") == "PROCESSING" ||
                         (order.Status?.ToUpper() ?? "PENDING") == "CONFIRMED" ||
                         (order.Status?.ToUpper() ?? "PENDING") == "CONFIRM"
        };
    }

    public async Task<PurchaseResponse> PurchaseProductAsync(long userId, PurchaseRequest request)
    {
      
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Kiểm tra ProductVariant tồn tại
            Console.WriteLine($" Validating ProductVariant: {request.ProductVariantId}");
            var productVariant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == request.ProductVariantId);

            if (productVariant == null)
            {
                Console.WriteLine($" ProductVariant not found: {request.ProductVariantId}");
                return new PurchaseResponse
                {
                    Success = false,
                    Message = "Sản phẩm không tồn tại"
                };
            }
            
            Console.WriteLine($" ProductVariant found: {productVariant.Product.Name}, Price: {productVariant.Price}");

            // 2. Check available ProductStore codes với PESSIMISTIC LOCKING để tránh race condition
            Console.WriteLine($"🔒 Locking and checking available ProductStore codes...");
            
            // Sử dụng raw SQL với UPDLOCK để lock rows khi đọc
            var sql = @"
                SELECT * FROM ProductStore WITH (UPDLOCK, ROWLOCK)
                WHERE ProductVariantId = @p0 AND Status = 'AVAILABLE'
                ORDER BY Id
                OFFSET 0 ROWS FETCH NEXT @p1 ROWS ONLY
            ";
            
            var availableCodes = await _context.ProductStores
                .FromSqlRaw(sql, request.ProductVariantId, request.Quantity)
                .ToListAsync();

            Console.WriteLine($"📦 Available codes found (locked): {availableCodes.Count}/{request.Quantity}");

            if (availableCodes.Count < request.Quantity)
            {
                Console.WriteLine($"❌ Not enough codes available");
                await transaction.RollbackAsync();
                return new PurchaseResponse
                {
                    Success = false,
                    Message = $"Không đủ mã sản phẩm. Chỉ còn {availableCodes.Count} mã"
                };
            }
            
            Console.WriteLine($"🔒 Codes locked successfully, proceeding with purchase...");

            // 3. tinh  total amount
            var totalAmount = request.Quantity * productVariant.Price;
            Console.WriteLine($" Total amount calculated: {totalAmount}");

            // 4. kiem tra  user balance
            Console.WriteLine($" Checking user balance for UserId: {userId}");
            var user = await _context.Accounts.FindAsync(userId);
            
            if (user == null)
            {
                Console.WriteLine($" User not found: {userId}");
                return new PurchaseResponse
                {
                    Success = false,
                    Message = "Người dùng không tồn tại"
                };
            }
            
            Console.WriteLine($" User balance: {user.Balance}, Required: {totalAmount}");
            
            if (user.Balance < totalAmount)
            {
                Console.WriteLine($" Insufficient balance");
                return new PurchaseResponse
                {
                    Success = false,
                    Message = "Số dư không đủ để mua hàng"
                };
            }

            // 5. Tao OrderProduct Voi PENDING status
            Console.WriteLine($" Creating OrderProduct...");
          var orderProduct = new OrderProduct
          {
              AccountId = userId,
              ProductVariantId = request.ProductVariantId,
              Quantity = request.Quantity,
              TotalAmount = totalAmount,
              Status = "PENDING" // dua product vao trang thai pending truoc 
          };

            _context.OrderProducts.Add(orderProduct);
            await _context.SaveChangesAsync();
            Console.WriteLine($" OrderProduct created with ID: {orderProduct.Id}");

            // 6. Tao  PaymentTransaction
            Console.WriteLine($" Creating PaymentTransaction...");
           var paymentTransaction = new PaymentTransaction

           {
               UserId = userId,
               Type = "MuaHang",
               Amount = totalAmount,
               PaymentDescription = $"Mua sản phẩm {productVariant.Product.Name}",
               Status = "PENDING",  // luc nay van dang la dang xu ly transaction
               CreatedAt = DateTime.Now
           };
            _context.PaymentTransactions.Add(paymentTransaction);
            Console.WriteLine($" PaymentTransaction created");

            // 7. Assign ProductStore codes
            Console.WriteLine($" Assigning {availableCodes.Count} codes...");
            foreach (var code in availableCodes)
            {
                Console.WriteLine($"   Assigning code: {code.Id} = {code.Value}");
                
                var orderProductStore = new OrderProductProductStore
                {
                    OrderProductId = orderProduct.Id,
                    ProductStoreId = code.Id
                };
                _context.OrderProductProductStores.Add(orderProductStore);

                // Update ProductStore status
                code.Status = "SOLD";
                code.UpdatedAt = DateTime.Now;
            }
            Console.WriteLine($" All codes assigned");

            // 8. Update user balance
            Console.WriteLine($" Updating user balance: {user.Balance} -> {user.Balance - totalAmount}");
            user.Balance -= totalAmount;
            Console.WriteLine($" User balance updated");

            // 9. Update OrderProduct status to CONFIRMED (mã đã được gán)
            Console.WriteLine($" Updating OrderProduct status to CONFIRMED...");
            orderProduct.Status = "CONFIRMED";
            paymentTransaction.Status = "COMPLETED";
            Console.WriteLine($" Status updated to CONFIRMED");

            Console.WriteLine($" Saving all changes to database...");
            await _context.SaveChangesAsync();
            Console.WriteLine($" All changes saved successfully");

            await transaction.CommitAsync();
            Console.WriteLine($" Transaction committed");

            // 10. Return success response
            Console.WriteLine($" PURCHASE SUCCESS! OrderId: {orderProduct.Id}");
            Console.WriteLine($"=== PURCHASE END ===");
            
            return new PurchaseResponse
            {
                Success = true,
                OrderId = orderProduct.Id,
                Status = "CONFIRMED",
                TotalAmount = totalAmount,
                TotalAmountDisplay = $"{totalAmount:N0} VNĐ",
                ProductCodes = availableCodes.Select(code => new ProductCodeInfo
                {
                    Content = productVariant.Product.Name,
                    Value = code.Value,
                    Status = "SOLD",
                    StatusDisplay = "Đã sử dụng"
                }).ToList(),
                Message = "Mua hàng thành công!"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($" ERROR in PurchaseProductAsync: {ex.Message}");
            Console.WriteLine($" Inner Exception: {ex.InnerException?.Message}");
            Console.WriteLine($" Inner Exception Details: {ex.InnerException?.ToString()}");
            Console.WriteLine($" Stack Trace: {ex.StackTrace}");
            
            await transaction.RollbackAsync();
            Console.WriteLine($" Transaction rolled back");
            
            // Return more detailed error
            throw new Exception($"Lỗi khi mua hàng: {ex.Message}. Inner: {ex.InnerException?.Message}");
        }
    }
}