using System.ComponentModel.DataAnnotations;

public class DeliveryAddressVM
{
    public int DeliveryAddressID { get; set; }

    public AddressVM Address { get; set; } = new AddressVM();

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải có 10 chữ số")]
    public string PhoneNumber { get; set; } = "";

    public bool IsEditing { get; set; } = false;
}
