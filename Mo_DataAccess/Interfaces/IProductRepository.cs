using System.Threading.Tasks;
using System.Collections.Generic;

public interface IProductRepository
{
    // Lấy chi tiết sản phẩm và các thành phần liên quan (Variants, Shop, Feedbacks)
    Task<ProductDetailModel> GetProductDetailAsync(long productId);
}

// Model tổng hợp cho Data Access Layer
public class ProductDetailModel
{
    public Product Product { get; set; }
    public Shop Shop { get; set; }
    public Category Category { get; set; }
    public SubCategory SubCategory { get; set; }
    public IEnumerable<ProductVariant> Variants { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalFeedbacks { get; set; }
}