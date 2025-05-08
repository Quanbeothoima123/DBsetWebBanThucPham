using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using webBanThucPham.Models;
using webBanThucPham.Models.ViewModel;
using webBanThucPham.Services.Momo;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace webBanThucPham.Controllers
{

    public class PaymentController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;
        private IMomoService _momoService;

        public PaymentController(INotyfService notyf, DbBanThucPhamContext context, IMomoService momoService)
        {
            _momoService = momoService;
            _notyf = notyf;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfo info, List<int> selectedItems, List<int> quantities, int selectedAddressId, string note)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thanh toán.";
                return RedirectToAction("Login", "CustomAccount");
            }

            var customer = await _context.Customers
                .Include(c => c.Deliveryaddresses)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null || (string.IsNullOrEmpty(customer.Address) && !customer.Deliveryaddresses.Any() && selectedAddressId == 0))
            {
                TempData["ErrorMessage"] = "Bạn cần thêm địa chỉ giao hàng trước khi thanh toán.";
                return RedirectToAction("EditInfo", "CustomAccount");
            }

            // Lưu TempData và tạo yêu cầu MoMo
            TempData["SelectedItems"] = JsonConvert.SerializeObject(selectedItems);
            TempData["Quantities"] = JsonConvert.SerializeObject(quantities);
            TempData["SelectedAddressId"] = selectedAddressId;
            TempData["Note"] = note;
            TempData["PaymentMethodId"] = 2;

            var response = await _momoService.CreatePaymentAsyc(info);
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback(string orderId, string requestId, string errorCode, string message, string payType, string extraData)
        {
            //var queryString = HttpContext.Request.Query;
            //Console.WriteLine("MoMo Callback Query String:");
            //foreach (var param in queryString)
            //{
            //    Console.WriteLine($"{param.Key}={param.Value}");
            //}
            // Kiểm tra nếu thanh toán MoMo không thành công
            if (errorCode != "0")
            {
                TempData["ErrorMessage"] = $"Thanh toán MoMo không thành công!";
                return RedirectToAction("CartView", "Cart");
            }

            // Lấy CustomerId từ session
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thanh toán.";
                return RedirectToAction("Login", "CustomAccount");
            }

            // Lấy dữ liệu từ TempData
            var selectedItemsJson = TempData["SelectedItems"]?.ToString();
            var quantitiesJson = TempData["Quantities"]?.ToString();
            var selectedAddressId = Convert.ToInt32(TempData["SelectedAddressId"]);
            var note = TempData["Note"]?.ToString();
            var paymentMethodId = Convert.ToInt32(TempData["PaymentMethodId"]); // Giả sử 2 là MoMo

            // Kiểm tra dữ liệu TempData
            if (string.IsNullOrEmpty(selectedItemsJson) || string.IsNullOrEmpty(quantitiesJson))
            {
                TempData["ErrorMessage"] = "Dữ liệu đơn hàng không hợp lệ.";
                return RedirectToAction("CartView", "Cart");
            }

            var selectedItems = JsonConvert.DeserializeObject<List<int>>(selectedItemsJson);
            var quantities = JsonConvert.DeserializeObject<List<int>>(quantitiesJson);

            // Lấy giỏ hàng của khách hàng
            var cart = await _context.Carts
                .Include(c => c.Cartitems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                TempData["ErrorMessage"] = "Giỏ hàng không tồn tại.";
                return RedirectToAction("CartView", "Cart");
            }

            // Khởi tạo danh sách để lưu chi tiết đơn hàng và các lỗi
            var removedItems = new List<string>();
            var outOfStockItems = new List<string>();
            var orderDetails = new List<Orderdetail>();
            int totalAmount = 0;

            // Xử lý từng sản phẩm được chọn
            for (int i = 0; i < selectedItems.Count; i++)
            {
                var cartItemId = selectedItems[i];
                var quantity = quantities[i];

                var cartItem = cart.Cartitems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
                if (cartItem == null || cartItem.Quantity == null)
                {
                    removedItems.Add(cartItem?.Product?.ProductName ?? "Sản phẩm không xác định");
                    continue;
                }

                // Kiểm tra số lượng yêu cầu có vượt quá số lượng trong giỏ hàng không
                if (quantity > cartItem.Quantity)
                {
                    removedItems.Add(cartItem.Product.ProductName);
                    _context.Cartitems.Remove(cartItem);
                    continue;
                }

                // Kiểm tra số lượng tồn kho
                var product = cartItem.Product;
                int stock = product.UnitsInStock ?? 0;
                if (quantity > stock)
                {
                    outOfStockItems.Add(product.ProductName);
                    continue;
                }

                // Cập nhật số lượng tồn kho
                product.UnitsInStock = stock - quantity;

                // Tính giá sau giảm giá
                int price = product.Price ?? 0;
                int discount = product.Discount ?? 0;
                int discountedPrice = price * (100 - discount) / 100;
                int itemTotal = discountedPrice * quantity;
                totalAmount += itemTotal;

                // Thêm chi tiết đơn hàng
                orderDetails.Add(new Orderdetail
                {
                    ProductId = product.ProductId,
                    Quantity = quantity,
                    OrderNumber = i + 1,
                    Total = itemTotal,
                    Discount = discount,
                    ShipDate = null
                });
            }

            // Kiểm tra lỗi tồn kho
            if (outOfStockItems.Any())
            {
                TempData["ErrorMessage"] = $"Sản phẩm sau đã vượt quá số lượng tồn kho: {string.Join(", ", outOfStockItems)}.";
                return RedirectToAction("CartView", "Cart");
            }

            // Kiểm tra nếu không có sản phẩm hợp lệ
            if (!orderDetails.Any())
            {
                TempData["ErrorMessage"] = "Không có sản phẩm nào hợp lệ để thanh toán.";
                return RedirectToAction("CartView", "Cart");
            }

            // Tạo đơn hàng mới
            var newOrder = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                ShipDate = null,
                TransactStatusId = 2, // Giả sử 2 là trạng thái "Đã thanh toán"
                Deleted = false,
                Paid = true, // MoMo thanh toán thành công
                PaymentId = orderId, // Lưu orderId từ MoMo
                PaymentDate = DateTime.Now,
                PaymentMethodId = paymentMethodId, // 2 cho MoMo
                DeliveryAddressId = selectedAddressId == 0 ? null : selectedAddressId,
                Note = note,
                Orderdetails = orderDetails
            };

            // Thêm đơn hàng và xóa các sản phẩm trong giỏ hàng
            _context.Orders.Add(newOrder);
            foreach (var cartItem in cart.Cartitems.Where(ci => selectedItems.Contains(ci.CartItemId)))
            {
                _context.Cartitems.Remove(cartItem);
            }

            // Lưu vào database
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thanh toán MoMo thành công.";
            return RedirectToAction("CheckoutSuccess", "Cart");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
