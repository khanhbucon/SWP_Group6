using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_Api.Extensions;
using Hangfire;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;

namespace Mo_Api.ApiController
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderProductServices _orderService;
        private readonly IBackgroundJobClient _jobs;

        public OrderController(IOrderProductServices orderService, IBackgroundJobClient jobs)
        {
            _orderService = orderService;
            _jobs = jobs;
        }

        [HttpGet("my-orders")]
        public async Task<ActionResult<OrderHistoryListResponse>> GetMyOrders(string? status = null)
        {
            try
            {
                // Lấy userId từ JWT token
                var userId = User.GetUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin user" });
                }

                var result = await _orderService.GetUserOrdersAsync(userId.Value, status);
                
                if (result == null || result.Orders.Count == 0)
                {
                    return Ok(new OrderHistoryListResponse
                    {
                        Orders = new List<OrderHistoryResponse>(),
                        TotalCount = 0,
                        TotalSpent = 0,
                        TotalOrders = 0,
                        CompletedOrders = 0,
                        PendingOrders = 0,
                        CancelledOrders = 0
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đơn hàng", error = ex.Message });
            }
        }

   
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderHistoryResponse>> GetOrderDetail(long orderId)
        {
            try
            {
                // Lấy userId từ JWT token
                var userId = User.GetUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin user" });
                }

                var order = await _orderService.GetOrderDetailAsync(orderId, userId.Value);
                
                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy chi tiết đơn hàng", error = ex.Message });
            }
        }

      
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetMyOrderStats()
        {
            try
            {
                // Lấy userId từ JWT token
                var userId = User.GetUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin user" });
                }

                var result = await _orderService.GetUserOrdersAsync(userId.Value, null);
                
                return Ok(new
                {
                    totalSpent = result.TotalSpent,
                    totalOrders = result.TotalOrders,
                    completedOrders = result.CompletedOrders,
                    pendingOrders = result.PendingOrders,
                    cancelledOrders = result.CancelledOrders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê", error = ex.Message });
            }
        }
        [HttpPost("purchase")]
        public async Task<ActionResult<object>> Purchase([FromBody] PurchaseRequest request)
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin user" });
                }

                var prep = await _orderService.PreparePurchaseAsync(userId.Value, request);
                if (!prep.Success)
                {
                    return BadRequest(new { message = prep.Message });
                }
                _jobs.Enqueue(() => _orderService.ProcessPurchaseJobAsync(userId.Value, prep.OrderId, request, prep.IdempotencyKey));
                return Ok(new { success = true, message = prep.Message, orderId = prep.OrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi mua hàng", error = ex.Message });
            }
        }

        [HttpGet("seller-orders")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<OrderHistoryListResponse>> GetSellerOrders(string? status = null)
        {
            try
            {
                var sellerId = User.GetUserId();
                if (sellerId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin seller" });
                }

                var result = await _orderService.GetSellerOrdersAsync(sellerId.Value, status);
                
                if (result == null || result.Orders.Count == 0)
                {
                    return Ok(new OrderHistoryListResponse
                    {
                        Orders = new List<OrderHistoryResponse>(),
                        TotalCount = 0,
                        TotalSpent = 0,
                        TotalOrders = 0,
                        CompletedOrders = 0,
                        PendingOrders = 0,
                        ConfirmedOrders = 0,
                        CancelledOrders = 0
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đơn hàng của seller", error = ex.Message });
            }
        }

        [HttpGet("seller-orders/{orderId}")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<OrderHistoryResponse>> GetSellerOrderDetail(long orderId)
        {
            try
            {
                var sellerId = User.GetUserId();
                if (sellerId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin seller" });
                }

                var order = await _orderService.GetSellerOrderDetailAsync(orderId, sellerId.Value);
                
                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy chi tiết đơn hàng", error = ex.Message });
            }
        }
    }
}

