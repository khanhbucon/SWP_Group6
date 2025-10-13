using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Mo_Entities.Models;

namespace Mo_DataAccess.Repositories
{
    public class ProductRepository : DBContext
    {
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            using (var conn = GetConnection())
            {
                string sql = @"
                    SELECT p.id, p.name, p.description, p.details, p.isActive, s.name AS shopName
                    FROM Products p
                    INNER JOIN Shops s ON p.shopId = s.id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var p = new Product
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("id")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description")),
                                Details = reader.IsDBNull(reader.GetOrdinal("details")) ? "" : reader.GetString(reader.GetOrdinal("details")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("isActive")),
                                ShopName = reader.IsDBNull(reader.GetOrdinal("shopName")) ? "" : reader.GetString(reader.GetOrdinal("shopName"))
                            };
                            products.Add(p);
                        }
                    }
                }
            }

            return products;
        }
    }
}
