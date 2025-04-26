using System;
using System.Collections.Generic;

namespace webBanThucPham.Models;

public partial class Tintuc
{
    public int PostId { get; set; }

    public string? Title { get; set; }

    public string? Scontents { get; set; }

    public string? Contents { get; set; }

    public string? Thum { get; set; }

    public bool Published { get; set; }

    public string? Alias { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Author { get; set; }

    public int? AccountId { get; set; }

    public string? Tags { get; set; }

    public int? CatId { get; set; }

    public bool IsHot { get; set; } = false;
    public bool IsNewfeed { get; set; } = false;


    public string? MetaKey { get; set; }

    public string? MetaDesc { get; set; }

    public int? Views { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Category? Cat { get; set; }
}
