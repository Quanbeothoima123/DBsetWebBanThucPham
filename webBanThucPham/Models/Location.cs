﻿using System;
using System.Collections.Generic;

namespace webBanThucPham.Models;

public partial class Location
{
    public int LocationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public string? Slug { get; set; }

    public string? NameWithType { get; set; }

    public string? PathWithType { get; set; }

    public int? ParentCode { get; set; }

    public int? Levels { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
