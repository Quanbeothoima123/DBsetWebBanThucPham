using webBanThucPham.Models.ViewModel;

public class EditInfoViewModel
{
    public string? Avatar { get; set; }
    public string? FullName { get; set; }
    public DateTime? Birthday { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public DateTime? LastLogin { get; set; }

    public AddressVM DefaultAddress { get; set; } = new AddressVM();

    public List<DeliveryAddressVM> DeliveryAddresses { get; set; } = new List<DeliveryAddressVM>();

    // Cho khối thêm địa chỉ mới
    public bool AddingNewAddress { get; set; } = false;
}
