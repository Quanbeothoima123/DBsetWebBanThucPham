using Microsoft.AspNetCore.Mvc;

namespace webBanThucPham.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
