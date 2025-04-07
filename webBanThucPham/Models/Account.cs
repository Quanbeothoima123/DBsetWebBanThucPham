using System;
using System.Collections.Generic;

namespace webBanThucPham.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string Password { get; set; } = null!;

    public string? Salt { get; set; }

    public bool? Active { get; set; }

    public string? FullName { get; set; }

    public int? RoleId { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? CreateDate { get; set; }

    public virtual Role? Role { get; set; }

    public virtual ICollection<Tintuc> Tintucs { get; set; } = new List<Tintuc>();
}
