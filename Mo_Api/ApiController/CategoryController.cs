using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Mo_Api.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryServices _categoryServices;

        public CategoryController(ICategoryServices categoryServices)
        {
            _categoryServices = categoryServices;
        }

        // ✅ Lấy toàn bộ danh sách Category (async)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _categoryServices.GetAllAsync();

            var result = list
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name
                })
                .ToList();

            return Ok(result);
        }

        // ✅ Lấy Category theo ID (async)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var cate = await _categoryServices.GetByIdAsync(id);
            if (cate == null)
                return NotFound();

            return Ok(new
            {
                id = cate.Id,
                name = cate.Name
            });
        }
    }
}
