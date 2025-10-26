using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_Api.Extensions;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelResponse;

namespace Mo_Api.ApiController
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderProductServices _orderService;

        public OrderController(IOrderProductServices orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của user đang đăng nhập
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (PENDING, COMPLETED, CANCELLED, etc.)</param>
        /// <returns>Danh sách đơn hàng với thông tin chi tiết</returns>
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

        /// <summary>
        /// Lấy chi tiết một đơn hàng của user đang đăng nhập
        /// </summary>
        /// <param name="orderId">ID của đơn hàng</param>
        /// <returns>Chi tiết đơn hàng</returns>
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

        /// <summary>
        /// Lấy thống kê đơn hàng của user đang đăng nhập
        /// </summary>
        /// <returns>Thống kê tổng hợp về đơn hàng</returns>
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
    }
}

