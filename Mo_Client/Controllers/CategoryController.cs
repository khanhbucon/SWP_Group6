using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models;
using Mo_Client.Services;
using System.Threading.Tasks;

namespace Mo_Client.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }
    }
}
