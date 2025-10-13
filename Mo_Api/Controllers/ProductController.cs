using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Repositories;

namespace Mo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductRepository _repo = new ProductRepository();

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _repo.GetAllProducts();
            return Ok(products);
        }
    }
}
