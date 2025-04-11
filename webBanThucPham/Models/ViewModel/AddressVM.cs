using System.ComponentModel.DataAnnotations;

public class AddressVM
{
    // Có thể để trống
    public string? Street { get; set; }

    // Bắt buộc nhập
    [Required(ErrorMessage = "Phường/Xã không được để trống")]
    public string Ward { get; set; } = null!;

    [Required(ErrorMessage = "Quận/Huyện không được để trống")]
    public string District { get; set; } = null!;

    [Required(ErrorMessage = "Tỉnh/Thành phố không được để trống")]
    public string Province { get; set; } = null!;
}
