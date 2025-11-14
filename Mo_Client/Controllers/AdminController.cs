using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models;
using Mo_Client.Models.Admin;
using Mo_Client.Services;
using Mo_Entities.ModelResponse;
using System.Linq;
using System;

namespace Mo_Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly CategoryService _categoryService;

        public AdminController(AdminService adminService, CategoryService categoryService)
        {
            _adminService = adminService;
            _categoryService = categoryService;
       }

        /// <summary>
        /// Kiểm tra quyền Admin
        /// </summary>
        private bool IsAdmin()
        {
            var token = Request.Cookies["accessToken"];
            var roles = Request.Cookies["roles"];
            
            return !string.IsNullOrEmpty(token) && 
                   !string.IsNullOrEmpty(roles) && 
                   roles.Contains("Admin");
        }

        /// <summary>
        /// Redirect về login nếu không có quyền Admin
        /// </summary>
        private IActionResult RedirectToLogin()
        {
            if (string.IsNullOrEmpty(Request.Cookies["accessToken"]))
            {
                return RedirectToAction("Login", "Account");
            }
            
            ViewBag.Error = "Bạn không có quyền truy cập trang này. Chỉ Admin mới được phép.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                if (!IsAdmin())
                    return RedirectToLogin();

                var token = Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    _adminService.SetToken(token);
                }
                var stats = await _adminService.GetDashboardStatsAsync();
                
                var dashboardVm = new DashboardVm
                {
                    TotalUsers = stats?.TotalUsers ?? 0,
                    TotalShops = stats?.TotalShops ?? 0,
                    TotalProducts = stats?.TotalProducts ?? 0,
                    PendingShops = stats?.PendingShops ?? 0,
                    PendingProducts = stats?.PendingProducts ?? 0,
                    BannedUsers = stats?.BannedUsers ?? 0,
                    RecentUsers = stats?.RecentUsers?.Select(u => new RecentUserVm
                    {
                        Username = u.Username,
                        Email = u.Email,
                        CreatedAt = u.CreatedAt,
                        IsActive = u.IsActive
                    }).ToList() ?? new List<RecentUserVm>()
                };

                return View(dashboardVm);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new DashboardVm());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Shops(string? search = null, int page = 1)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"] ?? string.Empty);
                var items = await _adminService.GetShopsAsync(search);

                if (items == null)
                {
                    TempData["Error"] = "Không thể tải danh sách cửa hàng";
                    return View(new ShopManagementVm());
                }

                var vm = new ShopManagementVm
                {
                    SearchTerm = search,
                    PageNumber = page,
                    TotalCount = items.Count,
                    Shops = items.Select(s => new ShopVm
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Owner = s.Owner,
                        Status = s.Status,
                        CreatedAt = s.CreatedAt ?? DateTime.UtcNow,
                        ProductCount = s.ProductCount,
                        ReportCount = s.ReportCount,
                        Description = null
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new ShopManagementVm());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveShop(long shopId)
        {
            if (!IsAdmin()) return RedirectToLogin();
            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"] ?? string.Empty);
                var ok = await _adminService.ApproveShopAsync(shopId);
                TempData[ok ? "Success" : "Error"] = ok ? "Duyệt cửa hàng thành công" : "Không thể duyệt cửa hàng";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Shops");
        }

        [HttpPost]
        public async Task<IActionResult> SuspendShop(long shopId)
        {
            if (!IsAdmin()) return RedirectToLogin();
            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"] ?? string.Empty);
                var ok = await _adminService.SuspendShopAsync(shopId);
                TempData[ok ? "Success" : "Error"] = ok ? "Tạm dừng cửa hàng thành công" : "Không thể tạm dừng cửa hàng";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Shops");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateShop(long shopId)
        {
            if (!IsAdmin()) return RedirectToLogin();
            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"] ?? string.Empty);
                var ok = await _adminService.ActivateShopAsync(shopId);
                TempData[ok ? "Success" : "Error"] = ok ? "Kích hoạt cửa hàng thành công" : "Không thể kích hoạt cửa hàng";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Shops");
        }

        [HttpGet]
        public async Task<IActionResult> Products(string? search = null, int page = 1)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"] ?? string.Empty);
                var items = await _adminService.GetProductsAsync(search);
                if (items == null)
                {
                    TempData["Error"] = "Không thể tải danh sách sản phẩm";
                    return View(new ProductManagementVm());
                }

                var vm = new ProductManagementVm
                {
                    SearchTerm = search,
                    PageNumber = page,
                    TotalCount = items.Count,
                    Products = items.Select(p => new ProductVm
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ShopName = p.ShopName,
                        Category = p.Category,
                        Price = p.Price,
                        SoldCount = p.SoldCount,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt ?? DateTime.UtcNow,
                        Description = p.Description
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new ProductManagementVm());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveProduct(long productId)
        {
            if (!IsAdmin()) return RedirectToLogin();
            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"] ?? string.Empty);
                var ok = await _adminService.ApproveProductAsync(productId);
                TempData[ok ? "Success" : "Error"] = ok ? "Duyệt sản phẩm thành công" : "Không thể duyệt sản phẩm";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> SuspendProduct(long productId)
        {
            if (!IsAdmin()) return RedirectToLogin();
            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"] ?? string.Empty);
                var ok = await _adminService.SuspendProductAsync(productId);
                TempData[ok ? "Success" : "Error"] = ok ? "Tạm dừng sản phẩm thành công" : "Không thể tạm dừng sản phẩm";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateProduct(long productId)
        {
            if (!IsAdmin()) return RedirectToLogin();
            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"] ?? string.Empty);
                var ok = await _adminService.ActivateProductAsync(productId);
                TempData[ok ? "Success" : "Error"] = ok ? "Kích hoạt sản phẩm thành công" : "Không thể kích hoạt sản phẩm";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> ManagerUser(UserSearchVm? searchModel = null)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"]);
                
                // Set default values
                if (searchModel == null)
                {
                    searchModel = new UserSearchVm
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }
                
                // Truyền search parameters vào ViewBag để giữ lại giá trị trong form
                ViewBag.UserId = searchModel.UserId;
                ViewBag.Email = searchModel.Email;
                ViewBag.Phone = searchModel.Phone;
                ViewBag.Role = searchModel.Role;
                ViewBag.IsActive = searchModel.IsActive?.ToString();
                ViewBag.IsEKYCVerified = searchModel.IsEKYCVerified?.ToString();
                ViewBag.PageNumber = searchModel.PageNumber;
                ViewBag.PageSize = searchModel.PageSize;
                
                // Lấy tất cả users từ API
                var allUsers = await _adminService.GetAllUsersAsync();
                if (allUsers == null)
                {
                    ViewBag.Error = "Không thể tải danh sách người dùng";
                    return View(new UserSearchResultVm { Users = new List<ListAccountVm>() });
                }
                
                // Thực hiện search/filter ở phía frontend
                var filteredUsers = FilterUsers(allUsers, searchModel);
                
                // Thực hiện phân trang
                var paginatedUsers = PaginateUsers(filteredUsers, searchModel.PageNumber, searchModel.PageSize);
                
                var searchResult = new UserSearchResultVm
                {
                    Users = paginatedUsers,
                    TotalCount = filteredUsers.Count,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize
                };
                
                return View(searchResult);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new UserSearchResultVm { Users = new List<ListAccountVm>() });
            }
        }

        /// <summary>
        /// Filter users based on search criteria
        /// </summary>
        private List<ListAccountVm> FilterUsers(List<ListAccountVm> users, UserSearchVm searchModel)
        {
            var filteredUsers = users.AsQueryable();

            // Filter by UserId
            if (searchModel.UserId.HasValue)
            {
                filteredUsers = filteredUsers.Where(u => u.UserId == searchModel.UserId.Value);
            }

            // Filter by Email
            if (!string.IsNullOrEmpty(searchModel.Email))
            {
                filteredUsers = filteredUsers.Where(u => u.Email != null && 
                    u.Email.Contains(searchModel.Email, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Phone
            if (!string.IsNullOrEmpty(searchModel.Phone))
            {
                filteredUsers = filteredUsers.Where(u => u.Phone != null && 
                    u.Phone.Contains(searchModel.Phone, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Role
            if (!string.IsNullOrEmpty(searchModel.Role))
            {
                filteredUsers = filteredUsers.Where(u => u.Roles != null && 
                    u.Roles.Contains(searchModel.Role, StringComparer.OrdinalIgnoreCase));
            }


            // Filter by IsActive
            if (searchModel.IsActive.HasValue)
            {
                filteredUsers = filteredUsers.Where(u => u.IsActive == searchModel.IsActive.Value);
            }

            // Filter by IsEKYCVerified
            if (searchModel.IsEKYCVerified.HasValue)
            {
                filteredUsers = filteredUsers.Where(u => u.IsEKYCVerified == searchModel.IsEKYCVerified.Value);
            }

            return filteredUsers.ToList();
        }

        /// <summary>
        /// Paginate users based on page number and page size
        /// </summary>
        private List<ListAccountVm> PaginateUsers(List<ListAccountVm> users, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var skip = (pageNumber - 1) * pageSize;
            return users.Skip(skip).Take(pageSize).ToList();
        }

        /// <summary>
        /// Paginate transactions based on page number and page size
        /// </summary>
        private List<TransactionHistoryResponse> PaginateTransactions(List<TransactionHistoryResponse> transactions, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var skip = (pageNumber - 1) * pageSize;
            return transactions.Skip(skip).Take(pageSize).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> BanUser(long userId)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"]);
                var success = await _adminService.BanUserAsync(userId);
                if (success)
                {
                    TempData["Success"] = "Thay đổi trạng thái người dùng thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể thay đổi trạng thái người dùng";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("ManagerUser");
        }

        [HttpPost]
        public async Task<IActionResult> GrantSellerRole(long userId)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"]);
                
                // Kiểm tra xác minh danh tính trước khi cấp quyền Seller
                var users = await _adminService.GetAllUsersAsync();
                if (users != null)
                {
                    var targetUser = users.FirstOrDefault(u => u.UserId == userId);
                    if (targetUser != null)
                    {
                        if (!targetUser.IsEKYCVerified)
                        {
                            TempData["Error"] = $"Không thể cấp quyền Seller cho người dùng '{targetUser.Username}'. Người dùng chưa xác minh danh tính (eKYC).";
                            return RedirectToAction("ManagerUser");
                        }
                    }
                }

                var success = await _adminService.GrantSellerRoleAsync(userId);
                if (success)
                {
                    TempData["Success"] = "Cấp phép Seller thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể cấp phép Seller";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("ManagerUser");
        }

        [HttpGet]
        public async Task<IActionResult> Transactions(string? search = null, long? userId = null, string? type = null, 
            string? status = null, string? dateFrom = null, string? dateTo = null, int page = 1, int pageSize = 10)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"]);
                var allTransactions = await _adminService.GetAllTransactionsAsync();
                
                if (allTransactions == null)
                {
                    ViewBag.Error = "Không thể tải danh sách giao dịch";
                    return View(new TransactionHistoryListResponse());
                }

                // Set ViewBag for form values
                ViewBag.Search = search;
                ViewBag.UserId = userId;
                ViewBag.Type = type;
                ViewBag.Status = status;
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                ViewBag.PageNumber = page;
                ViewBag.PageSize = pageSize;

                // Filter transactions
                var filteredTransactions = FilterTransactions(allTransactions.Transactions, search, userId, type, status, dateFrom, dateTo);
                var totalCount = filteredTransactions.Count;

                // Paginate transactions
                var paginatedTransactions = PaginateTransactions(filteredTransactions, page, pageSize);

                // Calculate total pages
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var result = new TransactionHistoryListResponse
                {
                    Transactions = paginatedTransactions,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalIncome = allTransactions.TotalIncome,
                    TotalExpense = allTransactions.TotalExpense,
                    NetAmount = allTransactions.NetAmount
                };

                // Add TotalPages to ViewBag for pagination UI
                ViewBag.TotalPages = totalPages;

                return View(result);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new TransactionHistoryListResponse());
            }
        }

        /// <summary>
        /// Filter transactions based on search criteria
        /// </summary>
        private List<TransactionHistoryResponse> FilterTransactions(
            List<TransactionHistoryResponse> transactions, 
            string? search, 
            long? userId, 
            string? type, 
            string? status, 
            string? dateFrom, 
            string? dateTo)
        {
            var filtered = transactions.AsQueryable();

            // Filter by search (ID or Description)
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                filtered = filtered.Where(t => 
                    t.Id.ToString().Contains(searchLower) ||
                    (t.Description != null && t.Description.ToLower().Contains(searchLower))
                );
            }

            // Filter by User ID
            if (userId.HasValue)
            {
                filtered = filtered.Where(t => t.UserId == userId.Value);
            }

            // Filter by Type
            if (!string.IsNullOrEmpty(type))
            {
                filtered = filtered.Where(t => t.Type == type);
            }

            // Filter by Status
            if (!string.IsNullOrEmpty(status))
            {
                filtered = filtered.Where(t => t.Status == status);
            }

            // Filter by Date From
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
            {
                filtered = filtered.Where(t => t.CreatedAt.HasValue && t.CreatedAt.Value.Date >= fromDate.Date);
            }

            // Filter by Date To
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var toDate))
            {
                filtered = filtered.Where(t => t.CreatedAt.HasValue && t.CreatedAt.Value.Date <= toDate.Date);
            }

            return filtered.ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTransactionStatus(long transactionId, string status, 
            string? search = null, long? userId = null, string? type = null, 
            string? statusFilter = null, string? dateFrom = null, string? dateTo = null,
            int page = 1, int pageSize = 10)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"]);
                var (success, message) = await _adminService.UpdateTransactionStatusAsync(transactionId, status);
                
                if (success)
                {
                    TempData["Success"] = message;
                }
                else
                {
                    TempData["Error"] = message;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            
            // Giữ lại các filter parameters và pagination
            return RedirectToAction("Transactions", new { 
                search = search,
                userId = userId,
                type = type,
                status = statusFilter,
                dateFrom = dateFrom,
                dateTo = dateTo,
                page = page,
                pageSize = pageSize
            });
        }
    }
}