using System;
using System.Collections.Generic;

namespace webBanThucPham.Models;

public partial class Attributesprice
{
    public int AttributesPriceId { get; set; }

    public int AttributeId { get; set; }

    public int ProductId { get; set; }

    public int Price { get; set; }

    public bool? Active { get; set; }

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
