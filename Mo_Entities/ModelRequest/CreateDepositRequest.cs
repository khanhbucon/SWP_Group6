using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelRequest
{
    public class CreateDepositRequest
    {
        public long UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
