using Mo_DataAccess.Repo;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

public interface ICategoryServices:IGenericRepository<Category>
{
    // Interface cho Category, kế thừa từ GenericRepository
    public interface ICategoryServices : IGenericRepository<Category>
    {
        // Nếu sau này bạn muốn viết thêm hàm riêng cho Category thì thêm ở đây
        // ví dụ: IEnumerable<Category> GetActiveCategories();
    }

}