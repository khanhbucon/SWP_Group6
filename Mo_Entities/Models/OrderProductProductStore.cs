using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class OrderProductProductStore
{
    public long OrderProductId { get; set; }

    public long ProductStoreId { get; set; }

    public virtual OrderProduct OrderProduct { get; set; } = null!;

    public virtual ProductStore ProductStore { get; set; } = null!;
}
