using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using System.Text.Json;

namespace Mo_DataAccess.Services;

public class VnpayTransactionServices : GenericRepository<VnpayTransaction>, IVnpayTransactionServices
{
    public VnpayTransactionServices(SwpGroup6Context context) : base(context)
    {
    }
}
