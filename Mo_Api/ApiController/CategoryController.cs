using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Mo_Api.ApiController;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryServices _categoryServices;
    private readonly SwpGroup6Context _context;

    public CategoryController(ICategoryServices categoryServices, SwpGroup6Context context)
    {
        _categoryServices = categoryServices;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllCategories([FromQuery] string? search = null)
    {
        try
        {
            IEnumerable<Category> categories;
            
            if (string.IsNullOrEmpty(search))
            {
                categories = await _context.Categories
                    .Include(c => c.SubCategories)
                    .ToListAsync();
            }
            else
            {
                categories = await _context.Categories
                    .Include(c => c.SubCategories)
                    .Where(c => c.Name.Contains(search) || c.Id.ToString().Contains(search))
                    .ToListAsync();
            }

            var categoryResponses = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SubCategories = c.SubCategories?.Select(sc => new SubCategoryResponse
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    CategoryId = sc.CategoryId,
                    IsActive = sc.IsActive
                }).ToList() ?? new List<SubCategoryResponse>()
            }).ToList();

            return Ok(new { Success = true, Data = categoryResponses, TotalCount = categoryResponses.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy danh sách danh mục" });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCategoryById(long id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (category == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy danh mục" });
            }

            var categoryResponse = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SubCategories = category.SubCategories?.Select(sc => new SubCategoryResponse
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    CategoryId = sc.CategoryId,
                    IsActive = sc.IsActive
                }).ToList() ?? new List<SubCategoryResponse>()
            };

            return Ok(new { Success = true, Data = categoryResponse });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy thông tin danh mục" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] Mo_Entities.ModelRequest.CreateCategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Kiểm tra xem tên danh mục đã tồn tại chưa
            var existingCategory = await _categoryServices.GetAllAsync();
            if (existingCategory.Any(c => c.Name.ToLower() == request.Name.ToLower()))
            {
                return BadRequest(new { Success = false, Message = "Tên danh mục đã tồn tại" });
            }

            var category = new Category
            {
                Name = request.Name
            };

            await _categoryServices.AddAsync(category);

            var categoryResponse = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Ok(new { Success = true, Message = "Tạo danh mục thành công", Data = categoryResponse });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi tạo danh mục" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(long id, [FromBody] Mo_Entities.ModelRequest.UpdateCategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var category = await _categoryServices.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy danh mục" });
            }

            // Kiểm tra xem tên danh mục đã tồn tại chưa (trừ danh mục hiện tại)
            var existingCategories = await _categoryServices.GetAllAsync();
            if (existingCategories.Any(c => c.Id != id && c.Name.ToLower() == request.Name.ToLower()))
            {
                return BadRequest(new { Success = false, Message = "Tên danh mục đã tồn tại" });
            }

            category.Name = request.Name;

            await _categoryServices.UpdateAsync(category);

            var categoryResponse = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Ok(new { Success = true, Message = "Cập nhật danh mục thành công", Data = categoryResponse });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi cập nhật danh mục" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        try
        {
            var category = await _categoryServices.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy danh mục" });
            }

            // Kiểm tra xem category có thể xóa được không
            var canDelete = await _categoryServices.CanDeleteCategoryAsync(id);
            if (!canDelete)
            {
                return BadRequest(new { 
                    Success = false, 
                    Message = "Không thể xóa danh mục này vì đang có danh mục con hoặc sản phẩm liên kết. Vui lòng xóa tất cả danh mục con và sản phẩm trước." 
                });
            }

            await _categoryServices.DeleteAsync(category);

            return Ok(new { Success = true, Message = "Xóa danh mục thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi xóa danh mục" });
        }
    }
}
