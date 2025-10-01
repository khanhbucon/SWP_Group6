using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class VnpayTransaction
{
    public long Id { get; set; }

    public long PaymentTransactionId { get; set; }

    public DateTime? Date { get; set; }

    public string? Content { get; set; }

    public string? BankName { get; set; }

    public string PaymentAccount { get; set; } = null!;

    public string PaymentNumber { get; set; } = null!;

    public decimal Value { get; set; }

    public virtual PaymentTransaction PaymentTransaction { get; set; } = null!;
}
