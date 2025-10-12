using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class SupportTicket
{
    public long Id { get; set; }

    public long? AccountId { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Account? Account { get; set; }
}
