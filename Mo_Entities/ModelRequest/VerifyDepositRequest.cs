using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelRequest
{
    public class VerifyDepositRequest
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [Range(1000, 100000000)]
        public decimal Amount { get; set; }
    }
}
