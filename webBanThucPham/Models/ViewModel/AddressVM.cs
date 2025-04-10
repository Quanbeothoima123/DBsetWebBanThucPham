namespace webBanThucPham.Models.ViewModel
{
    public class AddressVM
    {
        public string? Street { get; set; } // Số nhà, ngõ
        public string Ward { get; set; } = null!;
        public string District { get; set; } = null!;
        public string Province { get; set; } = null!;
    }
}
