using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class PaymentTransaction
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Type { get; set; } = null!;

    public decimal Amount { get; set; }

    public string PaymentDescription { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual Account User { get; set; } = null!;

    public virtual ICollection<VnpayTransaction> VnpayTransactions { get; set; } = new List<VnpayTransaction>();
}
