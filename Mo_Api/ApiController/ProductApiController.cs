using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess;

namespace Mo_Api.ApiController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductApiController : ControllerBase
    {
        private readonly ProductRepository _productRepo;

        public ProductApiController(ProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        // DELETE /api/product/{id}
        [HttpDelete("{id}")]
        public IActionResult SoftDelete(long id)
        {
            bool success = _productRepo.SoftDelete(id);

            if (!success)
            {
                return NotFound(new { message = "Product not found or already deleted" });
            }

            return Ok(new { message = "Product deleted successfully (soft delete)" });
        }
    }
}
