using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Token
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public string RefreshToken { get; set; } = null!;

    public string AccessToken { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
