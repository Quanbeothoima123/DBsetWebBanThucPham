using System;
using System.Collections.Generic;

namespace webBanThucPham.Models;

public partial class Deliveryaddress
{
    public int DeliveryAddressId { get; set; }

    public int CustomerId { get; set; }

    public string? NameAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
