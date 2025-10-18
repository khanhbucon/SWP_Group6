using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess;

namespace Mo_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryApiController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepo;

        public CategoryApiController(CategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // GET: api/category
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _categoryRepo.GetAllCategories();
            return Ok(categories);
        }
    }
}
