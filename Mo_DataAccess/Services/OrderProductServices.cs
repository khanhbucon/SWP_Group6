using Microsoft.EntityFrameworkCore;
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
}