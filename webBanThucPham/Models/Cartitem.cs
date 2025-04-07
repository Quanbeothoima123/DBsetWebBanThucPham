using System;
using System.Collections.Generic;

namespace webBanThucPham.Models;

public partial class Cartitem
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public int? Quantity { get; set; }

    public int? Price { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
