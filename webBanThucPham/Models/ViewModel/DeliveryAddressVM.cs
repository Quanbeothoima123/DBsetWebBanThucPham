using webBanThucPham.Models.ViewModel;

public class DeliveryAddressVM
{
    public int DeliveryAddressID { get; set; }
    public AddressVM Address { get; set; } = new();
    public string PhoneNumber { get; set; } = null!;

    // Dùng để xác định địa chỉ nào đang được chỉnh sửa
    public bool IsEditing { get; set; } = false;
}