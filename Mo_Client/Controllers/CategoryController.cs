using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models.Admin;
using Mo_Client.Models;
using Mo_Client.Services;

namespace Mo_Client.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AdminService _adminService;
        private readonly CategoryService _categoryService;

        public CategoryController(AdminService adminService, CategoryService categoryService)
        {
            _adminService = adminService;
            _categoryService = categoryService;
        }

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
        public async Task<IActionResult> Categories(string? search = null)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync(search);
                var categoriesVm = new CategoryManagementVm
                {
                    TotalCount = categories.Count,
                    SearchTerm = search,
                    Categories = categories.Select(c => new CategoryVm
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CreatedAt = DateTime.UtcNow, // Sử dụng thời gian hiện tại
                        SubCategories = c.SubCategories?.Select(sc => new SubCategoryVm
                        {
                            Id = sc.Id,
                            Name = sc.Name,
                            IsActive = sc.IsActive ?? true,
                            CategoryId = sc.CategoryId
                        }).ToList() ?? new List<SubCategoryVm>()
                    }).ToList()
                };

                return View(categoriesVm);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Category Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Category StackTrace: {ex.StackTrace}");

                var emptyVm = new CategoryManagementVm
                {
                    SearchTerm = search,
                    TotalCount = 0,
                    Categories = new List<CategoryVm>()
                };
                return View(emptyVm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(string Name)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                if (string.IsNullOrEmpty(Name))
                {
                    ViewBag.Error = "Tên danh mục không được để trống";
                    return RedirectToAction("Categories");
                }

                // Gọi API để tạo category
                var category = await _categoryService.CreateCategoryAsync(Name);

                if (category != null)
                {
                    TempData["Success"] = "Thêm danh mục thành công: " + Name;
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi thêm danh mục";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Categories");
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(long id)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                var request = new UpdateCategoryVm
                {
                    Name = category.Name
                };
                return View(request);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Categories");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(long id, UpdateCategoryVm request)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                await _categoryService.UpdateCategoryAsync(id, request);
                TempData["Success"] = "Cập nhật danh mục thành công";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            if (!IsAdmin())
                return RedirectToLogin();

            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                TempData["Success"] = "Xóa danh mục thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Categories");
        }



        // Helper methods for SubCategory views
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Json(categories?.Select(c => new { id = c.Id, name = c.Name }).ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryById(long id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Json(new { id = category?.Id, name = category?.Name });
            }
            catch (Exception ex)
            {
                return Json(new { id = 0, name = "Không xác định" });
            }
        }

    }
}
