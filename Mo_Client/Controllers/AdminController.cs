using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models;
using Mo_Client.Models.Admin;
using Mo_Client.Services;
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

                _adminService.SetToken(Request.Cookies["accessToken"]);
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
        public async Task<IActionResult> ManagerUser()
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                _adminService.SetToken(Request.Cookies["accessToken"]);
                var users = await _adminService.GetAllUsersAsync();
                if (users == null)
                {
                    ViewBag.Error = "Không thể tải danh sách người dùng";
                    return View(new List<ListAccountVm>());
                }
                return View(users);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new List<ListAccountVm>());
            }
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

        // TODO: Thêm các action khác khi có API
    }
}