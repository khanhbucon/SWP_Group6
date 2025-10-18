using Mo_Entities.Models;
using System.Collections.Generic;

namespace Mo_DataAccess.Services.Interface
{
    public interface ISubCategoryServices
    {
        List<SubCategory> GetAll();
        SubCategory? GetById(long id);
        void Add(SubCategory subCategory);
        void Update(SubCategory subCategory);
        void Delete(long id);
    }
}
