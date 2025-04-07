using System;
using System.Collections.Generic;

namespace webBanThucPham.Models;

public partial class Attribute
{
    public int AttributeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Attributesprice> Attributesprices { get; set; } = new List<Attributesprice>();
}
