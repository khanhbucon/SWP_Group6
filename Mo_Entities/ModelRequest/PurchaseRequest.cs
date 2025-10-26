using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelRequest
{
    public class PurchaseRequest
    {
        public long ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
