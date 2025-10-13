using System.Collections.Generic;
using Mo_Entities.Models;

namespace Mo_DataAccess.Repositories
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAllProducts();
        ProductDetailModel? GetProductDetail(long id);
    }
}
