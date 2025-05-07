using webBanThucPham.Models.Momo;
using webBanThucPham.Models.ViewModel;

namespace webBanThucPham.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsyc(OrderInfo model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
