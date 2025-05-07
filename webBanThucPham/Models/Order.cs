using System;
using System.Collections.Generic;

namespace webBanThucPham.Models;

public partial class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public int? TransactStatusId { get; set; }
    public bool Deleted { get; set; }
    public bool Paid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentId { get; set; }
    public string? Note { get; set; }
    public int? PaymentMethodId { get; set; }
    public int? DeliveryAddressId { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Deliveryaddress? DeliveryAddress { get; set; }
    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();
    public virtual PaymentMethod? PaymentMethod { get; set; }

    // ✅ Thêm dòng này để fix lỗi
    public virtual Transactstatuss? TransactStatus { get; set; }
}
