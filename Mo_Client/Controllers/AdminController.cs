using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models;
using Mo_Client.Models.Admin;
using Mo_Client.Services;

namespace Mo_Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
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

                // TODO: Gọi API để lấy thống kê thực tế
                var dashboardVm = new DashboardVm
                {
                    TotalUsers = 1250, // Demo data
                    TotalShops = 45,   // Demo data
                    TotalProducts = 320, // Demo data
                    PendingShops = 3,  // Demo data
                    PendingProducts = 8, // Demo data
                    BannedUsers = 12,  // Demo data
                    RecentUsers = new List<RecentUserVm>
                    {
                        new RecentUserVm { Username = "user1", Email = "user1@example.com", CreatedAt = DateTime.Now.AddDays(-1) },
                        new RecentUserVm { Username = "user2", Email = "user2@example.com", CreatedAt = DateTime.Now.AddDays(-2) },
                        new RecentUserVm { Username = "user3", Email = "user3@example.com", CreatedAt = DateTime.Now.AddDays(-3) }
                    }
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
        public IActionResult Shops(string? search = null, int page = 1)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                // TODO: Gọi API để lấy danh sách cửa hàng
                var shopsVm = new ShopManagementVm
                {
                    SearchTerm = search,
                    PageNumber = page,
                    TotalCount = 3, // Demo data
                    Shops = new List<ShopVm>
                    {
                        new ShopVm 
                        { 
                            Id = 1, 
                            Name = "Cửa hàng demo 1", 
                            Owner = "user1", 
                            Status = "Active", 
                            CreatedAt = DateTime.Now.AddDays(-10), 
                            ProductCount = 15,
                            ReportCount = 0,
                            Description = "Cửa hàng chuyên bán điện tử"
                        },
                        new ShopVm 
                        { 
                            Id = 2, 
                            Name = "Cửa hàng demo 2", 
                            Owner = "user2", 
                            Status = "Pending", 
                            CreatedAt = DateTime.Now.AddDays(-5), 
                            ProductCount = 8,
                            ReportCount = 2,
                            Description = "Cửa hàng thời trang"
                        },
                        new ShopVm 
                        { 
                            Id = 3, 
                            Name = "Cửa hàng demo 3", 
                            Owner = "user3", 
                            Status = "Inactive", 
                            CreatedAt = DateTime.Now.AddDays(-15), 
                            ProductCount = 0,
                            ReportCount = 5,
                            Description = "Cửa hàng gia dụng"
                        }
                    }
                };

                return View(shopsVm);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new ShopManagementVm());
            }
        }

        [HttpGet]
        public IActionResult Products(string? search = null, int page = 1)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                // TODO: Gọi API để lấy danh sách sản phẩm
                var productsVm = new ProductManagementVm
                {
                    SearchTerm = search,
                    PageNumber = page,
                    TotalCount = 3, // Demo data
                    Products = new List<ProductVm>
                    {
                        new ProductVm 
                        { 
                            Id = 1, 
                            Name = "Sản phẩm demo 1", 
                            ShopName = "Cửa hàng demo 1", 
                            Category = "Điện tử", 
                            Status = "Active", 
                            Price = 500000, 
                            CreatedAt = DateTime.Now.AddDays(-5),
                            SoldCount = 25,
                            Description = "Sản phẩm điện tử chất lượng cao"
                        },
                        new ProductVm 
                        { 
                            Id = 2, 
                            Name = "Sản phẩm demo 2", 
                            ShopName = "Cửa hàng demo 2", 
                            Category = "Thời trang", 
                            Status = "Pending", 
                            Price = 300000, 
                            CreatedAt = DateTime.Now.AddDays(-3),
                            SoldCount = 0,
                            Description = "Quần áo thời trang"
                        },
                        new ProductVm 
                        { 
                            Id = 3, 
                            Name = "Sản phẩm demo 3", 
                            ShopName = "Cửa hàng demo 3", 
                            Category = "Gia dụng", 
                            Status = "Inactive", 
                            Price = 200000, 
                            CreatedAt = DateTime.Now.AddDays(-7),
                            SoldCount = 8,
                            Description = "Đồ dùng gia đình"
                        }
                    }
                };

                return View(productsVm);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new ProductManagementVm());
            }
        }

        [HttpGet]
        public IActionResult Categories()
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                // TODO: Gọi API để lấy danh sách danh mục
                var categoriesVm = new CategoryManagementVm
                {
                    TotalCount = 3, // Demo data
                    Categories = new List<CategoryVm>
                    {
                        new CategoryVm 
                        { 
                            Id = 1, 
                            Name = "Điện tử", 
                            Description = "Thiết bị điện tử", 
                            ProductCount = 45, 
                            IsActive = true,
                            CreatedAt = DateTime.Now.AddDays(-30),
                            SubCategories = new List<SubCategoryVm>
                            {
                                new SubCategoryVm { Id = 1, Name = "Điện thoại", Description = "Smartphone", ProductCount = 20, IsActive = true, CategoryId = 1 },
                                new SubCategoryVm { Id = 2, Name = "Laptop", Description = "Máy tính xách tay", ProductCount = 25, IsActive = true, CategoryId = 1 }
                            }
                        },
                        new CategoryVm 
                        { 
                            Id = 2, 
                            Name = "Thời trang", 
                            Description = "Quần áo, giày dép", 
                            ProductCount = 120, 
                            IsActive = true,
                            CreatedAt = DateTime.Now.AddDays(-25)
                        },
                        new CategoryVm 
                        { 
                            Id = 3, 
                            Name = "Gia dụng", 
                            Description = "Đồ dùng gia đình", 
                            ProductCount = 80, 
                            IsActive = true,
                            CreatedAt = DateTime.Now.AddDays(-20)
                        }
                    }
                };

                return View(categoriesVm);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new CategoryManagementVm());
            }
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
                    return View(new List<ListAccountResponse>());
                }
                return View(users);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new List<ListAccountResponse>());
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
        // [HttpPost]
        // public async Task<IActionResult> ApproveShop(long shopId)
        // {
        //     // Implement approve shop logic
        // }
        
        // [HttpPost]
        // public async Task<IActionResult> SuspendShop(long shopId)
        // {
        //     // Implement suspend shop logic
        // }
    }
}