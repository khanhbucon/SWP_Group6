using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class SystemsConfig
{
    public string Email { get; set; } = null!;

    public decimal? Fee { get; set; }

    public string GoogleAppPassword { get; set; } = null!;
}
