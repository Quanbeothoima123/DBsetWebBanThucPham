using webBanThucPham.Models.ViewModel;

public class EditInfoViewModel
{
    public string FullName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Avatar { get; set; }

    public AddressVM DefaultAddress { get; set; } = new(); // từ Customer.Address
    public string Email { get; set; }
    public string Phone { get; set; } // Phone mặc định

    public DateTime? LastLogin { get; set; }

    public List<DeliveryAddressVM> DeliveryAddresses { get; set; } = new(); // địa chỉ giao hàng
}