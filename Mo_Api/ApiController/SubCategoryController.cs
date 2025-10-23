using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
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

        /// <summary>
        /// Tạo danh mục con mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSubCategory([FromBody] CreateSubCategoryRequest request)
        {
            try
            {
            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { Success = false, Message = "Tên danh mục con không được để trống" });
            }

            if (request.CategoryId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Danh mục cha không hợp lệ" });
            }

            // Kiểm tra trùng tên trong cùng category
            var nameExists = await _subCategoryServices.SubCategoryNameExistsAsync(request.Name, request.CategoryId);
            if (nameExists)
            {
                return BadRequest(new { Success = false, Message = "Tên danh mục con đã tồn tại trong danh mục này" });
            }

                var subCategory = new SubCategory
                {
                    CategoryId = request.CategoryId,
                    Name = request.Name,
                    IsActive = true
                };

                var result = await _subCategoryServices.AddAsync(subCategory);

                return Ok(new { 
                    Success = true, 
                    Message = "Tạo danh mục con thành công",
                    Data = new {
                        Id = result.Id,
                        CategoryId = result.CategoryId,
                        Name = result.Name,
                        IsActive = result.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi tạo danh mục con" });
            }
        }

        /// <summary>
        /// Cập nhật danh mục con
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubCategory(long id, [FromBody] UpdateSubCategoryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest(new { Success = false, Message = "Tên danh mục con không được để trống" });
                }

                var subCategory = await _subCategoryServices.GetByIdAsync(id);
                if (subCategory == null)
                {
                    return NotFound(new { Success = false, Message = "Không tìm thấy danh mục con" });
                }

                // Kiểm tra trùng tên trong cùng category (loại trừ chính nó)
                var nameExists = await _subCategoryServices.SubCategoryNameExistsAsync(request.Name, subCategory.CategoryId, id);
                if (nameExists)
                {
                    return BadRequest(new { Success = false, Message = "Tên danh mục con đã tồn tại trong danh mục này" });
                }

                subCategory.Name = request.Name;
                subCategory.IsActive = request.IsActive;
                var result = await _subCategoryServices.UpdateAsync(subCategory);

                return Ok(new { 
                    Success = true, 
                    Message = "Cập nhật danh mục con thành công",
                    Data = new {
                        Id = result.Id,
                        CategoryId = result.CategoryId,
                        Name = result.Name,
                        IsActive = result.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi cập nhật danh mục con" });
            }
        }
    }

    public class CreateSubCategoryRequest
    {
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateSubCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
