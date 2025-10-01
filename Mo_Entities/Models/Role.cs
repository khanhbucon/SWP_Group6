using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Role
{
    public long Id { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
