using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;

namespace Mo_Api.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryServices _subCategoryServices;

        public SubCategoryController(ISubCategoryServices subCategoryServices)
        {
            _subCategoryServices = subCategoryServices;
        }

        [HttpGet]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _subCategoryServices.GetAll()
                          .OrderBy(x => x.Id) // sắp xếp theo Id tăng dần
                          .ToList();

            var result = list.Select(x => new SubCategoryResponse
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.CategoryId,
                IsActive = x.IsActive,
                CategoryName = x.Category?.Name
            }).ToList();

            return Ok(result);
        }


        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            var sub = _subCategoryServices.GetById(id);
            if (sub == null) return NotFound();

            return Ok(new SubCategoryResponse
            {
                Id = sub.Id,
                Name = sub.Name,
                CategoryId = sub.CategoryId,
                IsActive = sub.IsActive,
                CategoryName = sub.Category?.Name
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] SubCategory sub)
        {
            _subCategoryServices.Add(sub);
            return Ok(sub);
        }

        [HttpPut("{id}")]
        public IActionResult Update(long id, [FromBody] SubCategory sub)
        {
            var existing = _subCategoryServices.GetById(id);
            if (existing == null) return NotFound();

            existing.Name = sub.Name;
            existing.CategoryId = sub.CategoryId;
            existing.IsActive = sub.IsActive;
            _subCategoryServices.Update(existing);

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            _subCategoryServices.Delete(id);
            return NoContent();
        }
    }
}
