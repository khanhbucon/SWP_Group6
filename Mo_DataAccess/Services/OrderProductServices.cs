using Microsoft.EntityFrameworkCore;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using Mo_DataAccess.Services.Interface;
using Microsoft.Data.SqlClient;
using Hangfire;

namespace Mo_DataAccess.Services;

public class OrderProductServices:GenericRepository<OrderProduct>,IOrderProductServices
{
    // Test: hold time before releasing payout (minutes)
    private const int SellerPayoutHoldMinutes = 2;
    private readonly INotificationService _notificationService;
    private readonly IBackgroundJobClient? _jobs;

    public OrderProductServices(SwpGroup6Context context, INotificationService notificationService, IBackgroundJobClient? jobs = null) : base(context)
    {
        _notificationService = notificationService;
        _jobs = jobs;
    }
    public async Task<(bool Success, string Message, long OrderId, string IdempotencyKey)> PreparePurchaseAsync(long userId, PurchaseRequest request)
    {
        // Quick checks without locking rows
        var productVariant = await _context.ProductVariants
            .Include(pv => pv.Product)
            .FirstOrDefaultAsync(pv => pv.Id == request.ProductVariantId);
        if (productVariant == null)
        {
            return (false, "Sản phẩm không tồn tại", 0, string.Empty);
        }
        var availableCount = await _context.ProductStores
            .CountAsync(s => s.ProductVariantId == request.ProductVariantId && s.Status == "AVAILABLE");
        if (availableCount < request.Quantity)
        {
            return (false, $"Không đủ mã sản phẩm. Chỉ còn {availableCount} mã", 0, string.Empty);
        }
        var user = await _context.Accounts.FindAsync(userId);
        if (user == null)
        {
            return (false, "Người dùng không tồn tại", 0, string.Empty);
        }
        var totalAmount = request.Quantity * productVariant.Price;
        if (user.Balance < totalAmount)
        {
            return (false, "Số dư không đủ để mua hàng", 0, string.Empty);
        }

        var order = new OrderProduct
        {
            AccountId = userId,
            ProductVariantId = request.ProductVariantId,
            Quantity = request.Quantity,
            TotalAmount = totalAmount,
            Status = "PENDING"
        };
        _context.OrderProducts.Add(order);
        await _context.SaveChangesAsync();
        
        await _notificationService.CreateOrderNotificationAsync(userId, order.Id, "PENDING");

        var key = Guid.NewGuid().ToString("N");
        return (true, "Đã ghi nhận đơn hàng, đang xử lý", order.Id, key);
    }

    public async Task ProcessPurchaseJobAsync(long userId, long orderId, PurchaseRequest request, string idempotencyKey)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var orderProduct = await _context.OrderProducts.FirstOrDefaultAsync(o => o.Id == orderId && o.AccountId == userId);
            if (orderProduct == null)
            {
                await transaction.RollbackAsync();
                return;
            }   
            if ((orderProduct.Status ?? string.Empty).ToUpper() == "CONFIRMED")
            {
                await transaction.CommitAsync();
                return;
            }

            var productVariant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == request.ProductVariantId);
            if (productVariant == null)
            {
                orderProduct.Status = "FAILED";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }

            var sql = @"
                SELECT * FROM ProductStores WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
                WHERE ProductVariantId = @p0 AND Status = 'AVAILABLE'
                ORDER BY Id
                OFFSET 0 ROWS FETCH NEXT @p1 ROWS ONLY
            ";
            var availableCodes = await _context.ProductStores
                .FromSqlRaw(sql, request.ProductVariantId, request.Quantity)
                .ToListAsync();
            if (availableCodes.Count < request.Quantity)
            {
                orderProduct.Status = "FAILED";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }

            var user = await _context.Accounts.FindAsync(userId);
            if (user == null)
            {
                orderProduct.Status = "FAILED";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }
            var totalAmount = request.Quantity * productVariant.Price;
            if (user.Balance < totalAmount)
            {
                orderProduct.Status = "FAILED";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }

            var paymentTransaction = new PaymentTransaction
            {
                UserId = userId,
                Type = "MuaHang",
                Amount = totalAmount,
                PaymentDescription = $"Mua sản phẩm {productVariant.Product.Name}",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };
            _context.PaymentTransactions.Add(paymentTransaction);

            foreach (var code in availableCodes)
            {
                orderProduct.ProductStores ??= new List<ProductStore>();
                orderProduct.ProductStores.Add(code);
                code.Status = "SOLD";
                code.UpdatedAt = DateTime.Now;
            }

            user.Balance -= totalAmount;
            orderProduct.Status = "CONFIRMED";
            paymentTransaction.Status = "COMPLETED";

            if (productVariant.Stock.HasValue)
            {
                productVariant.Stock = Math.Max(0, productVariant.Stock.Value - request.Quantity);
            }
            productVariant.UpdatedAt = DateTime.Now;

            var shop = await _context.Shops.Include(s => s.Account).FirstOrDefaultAsync(s => s.Id == productVariant.Product.ShopId);
            if (shop?.Account != null)
            {
                var feePercent = productVariant.Product.Fee ?? 0m;
                var feeAmount = Math.Round(totalAmount * (feePercent / 100m), 2, MidpointRounding.AwayFromZero);
                var sellerIncome = totalAmount - feeAmount;
                var sellerTransaction = new PaymentTransaction
                {
                    UserId = shop.Account.Id,
                    Type = "BanHang",
                    Amount = sellerIncome,
                    PaymentDescription = $"Bán sản phẩm {productVariant.Product.Name} (Order #{orderProduct.Id})",
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow
                };
                _context.PaymentTransactions.Add(sellerTransaction);

                await _context.Set<Notification>().AddAsync(new Notification
                {
                    UserId = shop.Account.Id,
                    Type = "Payment",
                    Title = "Doanh thu đang tạm giữ",
                    Content = $"{sellerIncome:N0} VNĐ từ đơn hàng #{orderProduct.Id} đang được tạm giữ và sẽ giải ngân sau {SellerPayoutHoldMinutes} phút nếu không có khiếu nại.",
                    RelatedEntityType = "PaymentTransaction",
                    RelatedEntityId = sellerTransaction.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await _notificationService.CreateOrderNotificationAsync(userId, orderProduct.Id, "CONFIRMED");
            await _notificationService.CreatePaymentNotificationAsync(userId, paymentTransaction.Id, "MuaHang", totalAmount);

            // Schedule seller payout release via Hangfire after hold window
            if (_jobs != null)
            {
                _jobs.Schedule<IOrderProductServices>(svc => svc.ReleaseSellerPayoutAsync(orderProduct.Id), TimeSpan.FromMinutes(SellerPayoutHoldMinutes));
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
            
            // Get ProductStore codes for this order (via many-to-many navigation)
            var productStores = await _context.ProductStores
                .Where(ps => ps.OrderProducts.Any(op => op.Id == order.Id))
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
                TotalAmount = order.Quantity * (order.ProductVariant?.Price ?? 0), 
                TotalAmountDisplay = $"{(order.Quantity * (order.ProductVariant?.Price ?? 0)):N0} VNĐ",
                CreatedAt = DateTime.Now, 
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

        // Get ProductStore codes (via many-to-many navigation)
        var productStores = await _context.ProductStores
            .Where(ps => ps.OrderProducts.Any(op => op.Id == order.Id))
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
            // Kiểm tra stock trước khi mua
            if (productVariant.Stock.HasValue && productVariant.Stock < request.Quantity)
            {
                await transaction.RollbackAsync();
                return new PurchaseResponse
                {
                    Success = false,
                    Message = $"Không đủ tồn kho. Chỉ còn {productVariant.Stock} sản phẩm"
                };
            }


            // 2. Check available ProductStore codes với PESSIMISTIC LOCKING để tránh race condition
                      
            // Sử dụng raw SQL với UPDLOCK để lock rows khi đọc
            var sql = @"
                SELECT * FROM ProductStores WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
                WHERE ProductVariantId = @p0 AND Status = 'AVAILABLE'
                ORDER BY Id
                OFFSET 0 ROWS FETCH NEXT @p1 ROWS ONLY
            ";
            
            var availableCodes = await _context.ProductStores
                .FromSqlRaw(sql, request.ProductVariantId, request.Quantity)
                .ToListAsync();

            Console.WriteLine($" Available codes found (locked): {availableCodes.Count}/{request.Quantity}");

            if (availableCodes.Count < request.Quantity)
            {
                await transaction.RollbackAsync();
                return new PurchaseResponse
                {
                    Success = false,
                    Message = $"Không đủ mã sản phẩm. Chỉ còn {availableCodes.Count} mã"
                };
            }
            

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
            // Notify buyer that order is pending (sync path compatibility)
            await _notificationService.CreateOrderNotificationAsync(userId, orderProduct.Id, "PENDING");

            // 6. Tao  PaymentTransaction
            Console.WriteLine($" Creating PaymentTransaction...");
           var paymentTransaction = new PaymentTransaction

           {
               UserId = userId,
               Type = "MuaHang",
               Amount = totalAmount,
               PaymentDescription = $"Mua sản phẩm {productVariant.Product.Name}",
               Status = "PENDING",  // luc nay van dang la dang xu ly transaction
               CreatedAt = DateTime.UtcNow
           };
            _context.PaymentTransactions.Add(paymentTransaction);

            // 7. Assign ProductStore codes
            Console.WriteLine($" Assigning {availableCodes.Count} codes...");
            foreach (var code in availableCodes)
            {
                Console.WriteLine($"   Assigning code: {code.Id} = {code.Value}");

                // Link via many-to-many navigation
                if (orderProduct.ProductStores == null)
                {
                    orderProduct.ProductStores = new List<ProductStore>();
                }
                orderProduct.ProductStores.Add(code);

                // Update ProductStore status
                code.Status = "SOLD";
                code.UpdatedAt = DateTime.Now;
            }

            // 8. Update user balance
            user.Balance -= totalAmount;

            // 9. Update OrderProduct status to CONFIRMED (mã đã được gán)
            orderProduct.Status = "CONFIRMED";
            paymentTransaction.Status = "COMPLETED";
            // 9.x. Trừ stock của ProductVariant
            if (productVariant.Stock.HasValue)
            {
                productVariant.Stock = productVariant.Stock.Value - request.Quantity;
                if (productVariant.Stock < 0)
                {
                    productVariant.Stock = 0; // Đảm bảo không bị âm
                }
            }
            productVariant.UpdatedAt = DateTime.Now;


            // 7.x. Tính doanh thu seller và tạo giao dịch bán hàng ở trạng thái PENDING (escrow)

            // Lấy Shop và Seller (chủ shop)
            var shop = await _context.Shops
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.Id == productVariant.Product.ShopId);

            if (shop == null || shop.Account == null)
            {
                await transaction.RollbackAsync();
                return new PurchaseResponse
                {
                    Success = false,
                    Message = "Không tìm thấy chủ shop cho sản phẩm"
                };
            }

            // Tính phí sàn (nếu có) theo % lưu ở Product.Fee
            var feePercent = productVariant.Product.Fee ?? 0m;
            var feeAmount = Math.Round(totalAmount * (feePercent / 100m), 2, MidpointRounding.AwayFromZero);
            var sellerIncome = totalAmount - feeAmount;


            // KHÔNG cộng số dư ngay: giữ tiền (escrow) cho đến khi giải ngân
            var sellerAccount = shop.Account;

            // Tạo giao dịch PaymentTransaction cho Seller (BanHang) trạng thái PENDING, mô tả kèm OrderId để tra soát
            var sellerTransaction = new PaymentTransaction
            {
                UserId = sellerAccount.Id,
                Type = "BanHang",
                Amount = sellerIncome,
                PaymentDescription = $"Bán sản phẩm {productVariant.Product.Name} (Order #{orderProduct.Id})",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };
            _context.PaymentTransactions.Add(sellerTransaction);

            // Thông báo cho Seller: doanh thu đang tạm giữ, sẽ giải ngân sau SellerPayoutHoldMinutes phút
            await _context.Set<Notification>().AddAsync(new Notification
            {
                UserId = sellerAccount.Id,
                Type = "Payment",
                Title = "Doanh thu đang tạm giữ",
                Content = $"{sellerIncome:N0} VNĐ từ đơn hàng #{orderProduct.Id} đang được tạm giữ và sẽ giải ngân sau {SellerPayoutHoldMinutes} phút nếu không có khiếu nại.",
                RelatedEntityType = "PaymentTransaction",
                RelatedEntityId = sellerTransaction.Id, // sẽ có Id sau SaveChanges
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });


            await _context.SaveChangesAsync();

            // Notify buyer: order confirmed and payment completed
            await _notificationService.CreateOrderNotificationAsync(userId, orderProduct.Id, "CONFIRMED");
            await _notificationService.CreatePaymentNotificationAsync(userId, paymentTransaction.Id, "MuaHang", totalAmount);

            await transaction.CommitAsync();

          
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

    public async Task<bool> ReleaseSellerPayoutAsync(long orderId)
    {
        // Find order and related seller payout transaction
        var order = await _context.OrderProducts
            .Include(o => o.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Shop)
                        .ThenInclude(s => s.Account)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return false;

        // Ensure order is confirmed/completed before payout
        var allowed = (order.Status?.ToUpper() ?? "") == "CONFIRMED" || (order.Status?.ToUpper() ?? "") == "COMPLETED";
        if (!allowed) return false;

        var product = order.ProductVariant?.Product;
        var shop = product?.Shop;
        var seller = shop?.Account;
        if (seller == null) return false;

        // Find pending seller transaction for this order (by description pattern)
        var descriptionPrefix = $"Bán sản phẩm {product!.Name} (Order #{order.Id})";
        var sellerTx = await _context.PaymentTransactions
            .Where(pt => pt.UserId == seller.Id && pt.Type == "BanHang" && pt.Status == "PENDING" && pt.PaymentDescription!.StartsWith(descriptionPrefix))
            .OrderByDescending(pt => pt.CreatedAt)
            .FirstOrDefaultAsync();

        if (sellerTx == null) return false;

        // Credit seller now
        seller.Balance ??= 0;
        seller.Balance += sellerTx.Amount;
        sellerTx.Status = "COMPLETED";

        await _context.SaveChangesAsync();

        // Notify seller payout released
        await _context.Set<Notification>().AddAsync(new Notification
        {
            UserId = seller.Id,
            Type = "Payment",
            Title = "Giải ngân doanh thu",
            Content = $"{sellerTx.Amount:N0} VNĐ từ đơn hàng #{order.Id} đã được giải ngân vào số dư của bạn.",
            RelatedEntityType = "PaymentTransaction",
            RelatedEntityId = sellerTx.Id,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<OrderHistoryListResponse> GetSellerOrdersAsync(long sellerId, string? status = null)
    {
        // Lấy đơn hàng từ các shop của seller
        var query = _context.OrderProducts
            .Include(o => o.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Shop)
                        .ThenInclude(s => s.Account)
            .Include(o => o.Account) // Buyer account
            .Where(o => o.ProductVariant.Product.Shop.AccountId == sellerId)
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
            var buyer = order.Account; // Người mua
            
            // Get ProductStore codes for this order
            var productStores = await _context.ProductStores
                .Where(ps => ps.OrderProducts.Any(op => op.Id == order.Id))
                .ToListAsync();

            var productCodes = productStores.Select(ps => new ProductStoreInfo
            {
                Content = ps.Content,
                Value = ps.Value,
                Status = ps.Status,
                StatusDisplay = ps.Status switch
                {
                    "AVAILABLE" => "Có sẵn",
                    "SOLD" => "Đã bán",
                    "RESERVED" => "Đã đặt",
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
                    "HOLDING" => "Đang tạm giữ",
                    "CONFIRMED" or "CONFIRM" => "Đã xác nhận",
                    "RELEASED" => "Đã giải ngân",
                    "COMPLETED" => "Hoàn thành",
                    "CANCELLED" or "CANCELED" => "Đã hủy",
                    "REFUNDED" => "Đã hoàn tiền",
                    "FAILED" => "Thất bại",
                    _ => order.Status ?? "PENDING"
                },
                StatusClass = (order.Status?.ToUpper() ?? "PENDING") switch
                {
                    "PENDING" or "HOLDING" => "warning",
                    "CONFIRMED" or "CONFIRM" => "success",
                    "RELEASED" => "info",
                    "COMPLETED" => "success",
                    "CANCELLED" or "CANCELED" or "REFUNDED" => "secondary",
                    "FAILED" => "danger",
                    _ => "secondary"
                },
                Quantity = order.Quantity,
                TotalAmount = order.TotalAmount,
                TotalAmountDisplay = $"{order.TotalAmount:N0} VNĐ",
                CreatedAt = DateTime.Now, // OrderProduct không có CreatedAt, dùng thời gian hiện tại
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
                
                // Shop Info (shop của seller)
                ShopId = shop?.Id ?? 0,
                ShopName = shop?.Name ?? "N/A",
                ShopDescription = shop?.Description,
                
                // Buyer Info (thông tin người mua)
                ShopEmail = buyer?.Email, // Tạm dùng ShopEmail để lưu buyer email
                ShopPhone = buyer?.Phone, // Tạm dùng ShopPhone để lưu buyer phone
                
                // Product Codes
                ProductCodes = productCodes,
                HasCodes = productCodes.Any(),
                CanCancel = (order.Status?.ToUpper() ?? "PENDING") == "PENDING" || 
                           (order.Status?.ToUpper() ?? "PENDING") == "HOLDING"
            };

            orderResponses.Add(orderResponse);
        }

        // Calculate summary statistics
        var allSellerOrders = await _context.OrderProducts
            .Include(o => o.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Shop)
            .Where(o => o.ProductVariant.Product.Shop.AccountId == sellerId)
            .ToListAsync();
            
        var totalRevenue = allSellerOrders.Sum(o => o.TotalAmount);
        var totalOrders = allSellerOrders.Count;
        var completedOrders = allSellerOrders.Count(o => 
            (o.Status?.ToUpper() ?? "") == "COMPLETED" || 
            (o.Status?.ToUpper() ?? "") == "RELEASED");
        var pendingOrders = allSellerOrders.Count(o => 
            (o.Status?.ToUpper() ?? "") == "PENDING" || 
            (o.Status?.ToUpper() ?? "") == "HOLDING");
        var confirmedOrders = allSellerOrders.Count(o => 
            (o.Status?.ToUpper() ?? "") == "CONFIRMED" || 
            (o.Status?.ToUpper() ?? "") == "CONFIRM");
        var cancelledOrders = allSellerOrders.Count(o => 
            (o.Status?.ToUpper() ?? "") == "CANCELLED" || 
            (o.Status?.ToUpper() ?? "") == "CANCELED" ||
            (o.Status?.ToUpper() ?? "") == "REFUNDED");

        return new OrderHistoryListResponse
        {
            Orders = orderResponses,
            TotalCount = totalCount,
            TotalSpent = totalRevenue, // Doanh thu của seller
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            ConfirmedOrders = confirmedOrders,
            CancelledOrders = cancelledOrders
        };
    }

    public async Task<OrderHistoryResponse?> GetSellerOrderDetailAsync(long orderId, long sellerId)
    {
        var order = await _context.OrderProducts
            .Include(o => o.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Shop)
                        .ThenInclude(s => s.Account)
            .Include(o => o.Account) // Buyer
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ProductVariant.Product.Shop.AccountId == sellerId);

        if (order == null)
            return null;

        var product = order.ProductVariant?.Product;
        var shop = product?.Shop;
        var buyer = order.Account;

        // Get ProductStore codes
        var productStores = await _context.ProductStores
            .Where(ps => ps.OrderProducts.Any(op => op.Id == order.Id))
            .ToListAsync();

        var productCodes = productStores.Select(ps => new ProductStoreInfo
        {
            Content = ps.Content,
            Value = ps.Value,
            Status = ps.Status,
            StatusDisplay = ps.Status switch
            {
                "AVAILABLE" => "Có sẵn",
                "SOLD" => "Đã bán",
                "RESERVED" => "Đã đặt",
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
                "HOLDING" => "Đang tạm giữ",
                "CONFIRMED" or "CONFIRM" => "Đã xác nhận",
                "RELEASED" => "Đã giải ngân",
                "COMPLETED" => "Hoàn thành",
                "CANCELLED" or "CANCELED" => "Đã hủy",
                "REFUNDED" => "Đã hoàn tiền",
                "FAILED" => "Thất bại",
                _ => order.Status ?? "PENDING"
            },
            StatusClass = (order.Status?.ToUpper() ?? "PENDING") switch
            {
                "PENDING" or "HOLDING" => "warning",
                "CONFIRMED" or "CONFIRM" => "success",
                "RELEASED" => "info",
                "COMPLETED" => "success",
                "CANCELLED" or "CANCELED" or "REFUNDED" => "secondary",
                "FAILED" => "danger",
                _ => "secondary"
            },
            Quantity = order.Quantity,
            TotalAmount = order.TotalAmount,
            TotalAmountDisplay = $"{order.TotalAmount:N0} VNĐ",
            CreatedAt = DateTime.Now, // OrderProduct không có CreatedAt, dùng thời gian hiện tại
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
            ShopEmail = buyer?.Email,
            ShopPhone = buyer?.Phone,
            
            ProductCodes = productCodes,
            HasCodes = productCodes.Any(),
            CanCancel = (order.Status?.ToUpper() ?? "PENDING") == "PENDING" || 
                       (order.Status?.ToUpper() ?? "PENDING") == "HOLDING"
        };
    }
}