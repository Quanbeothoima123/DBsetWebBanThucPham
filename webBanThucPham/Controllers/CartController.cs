using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using webBanThucPham.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace webBanThucPham.Controllers
{
    public class CartController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;

        public CartController(INotyfService notyf, DbBanThucPhamContext context)
        {
            _notyf = notyf;
            _context = context;
        }

        // Hiển thị giỏ hàng của người dùng
        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");
            var userCustomID = HttpContext.Session.GetInt32("CustomerId");


            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (string.IsNullOrEmpty(userEmail) || !userCustomID.HasValue)
            {
                TempData["Message"] = "Bạn cần đăng nhập để xem giỏ hàng!";
                return RedirectToAction("Login", "CustomAccount");
            }

            // Lấy danh sách sản phẩm trong giỏ hàng của người dùng
            var cartItems = _context.Cartitems
                                    .Where(ci => ci.Cart.CustomerId == userCustomID)
                                    .Include(ci => ci.Product)
                                    .ToList();

            var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Price);

            // Truyền dữ liệu vào View
            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = totalAmount;

            return View();
        }
        [HttpGet]
        public IActionResult AddToCart(int productId)
        {
            // Kiểm tra đăng nhập
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["AddToCartError"] = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng!";
                return RedirectToAction("Login", "CustomAccount"); // Controller đăng nhập
            }

            // Tìm khách hàng dựa theo email
            var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
            if (customer == null)
            {
                TempData["AddToCartError"] = "Không tìm thấy tài khoản khách hàng!";
                return RedirectToAction("Login", "CustomAccount");
            }

            // Kiểm tra xem đã có giỏ hàng chưa
            var cart = _context.Carts.FirstOrDefault(c => c.CustomerId == customer.CustomerId);
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customer.CustomerId,
                    CreatedDate = DateTime.Now
                };
                _context.Carts.Add(cart);
                _context.SaveChanges(); // Đảm bảo cart có CartId mới
            }

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var cartItem = _context.Cartitems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity = (cartItem.Quantity ?? 0) + 1;
            }
            else
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
                if (product == null)
                {
                    TempData["AddToCartError"] = "Sản phẩm không tồn tại!";
                    return RedirectToAction("Index", "Home");
                }

                cartItem = new Cartitem
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    Quantity = 1,
                    Price = product.Price
                };
                _context.Cartitems.Add(cartItem);
            }

            _context.SaveChanges();
            TempData["AddToCartSuccess"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult CartView()
        {
            // Lấy CustomerId từ Session
            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                // Nếu chưa đăng nhập, chuyển về trang đăng nhập hoặc thông báo giỏ hàng trống
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "CustomAccount");
            }

            // Tìm Cart theo CustomerId
            var cart = _context.Carts
                .Include(c => c.Cartitems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerId == customerId);

            // Nếu chưa có cart hoặc cart rỗng
            if (cart == null || cart.Cartitems == null || !cart.Cartitems.Any())
            {
                ViewBag.CartCount = 0;
                return View(new List<Cartitem>());
            }

            ViewBag.CartCount = cart.Cartitems.Count;

            return View(cart.Cartitems.ToList());
        }

        [HttpGet]
        public IActionResult RemoveItem(int id)
        {
            // Lấy CustomerId từ session để đảm bảo người dùng có quyền với cart item này
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                return RedirectToAction("Login", "CustomAccount");
            }

            // Tìm cart item theo id và kiểm tra xem có thuộc cart của người dùng không
            var cartItem = _context.Cartitems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.CartItemId == id && ci.Cart.CustomerId == customerId);

            if (cartItem != null)
            {
                _context.Cartitems.Remove(cartItem);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm hoặc bạn không có quyền xóa.";
            }

            return RedirectToAction("CartView");
        }

        [HttpPost]
        public IActionResult IncreaseQuantityAjax(int id)
        {
            var item = _context.Cartitems.FirstOrDefault(x => x.CartItemId == id);
            if (item != null)
            {
                item.Quantity++;
                _context.SaveChanges();
                return Json(new { success = true, quantity = item.Quantity });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult DecreaseQuantityAjax(int id)
        {
            var item = _context.Cartitems.FirstOrDefault(x => x.CartItemId == id);
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                _context.SaveChanges();
                return Json(new { success = true, quantity = item.Quantity });
            }
            return Json(new { success = false });
        }

        public IActionResult CheckoutView(int[] selectedItems)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "CustomAccount");

            var lastOrder = _context.Orders.OrderByDescending(o => o.OrderId).FirstOrDefault();
            ViewBag.LastOrderId = lastOrder?.OrderId ?? 0;
            var cartItems = _context.Cartitems
                .Include(c => c.Product)
                .Where(c => selectedItems.Contains(c.CartItemId) && c.Cart.CustomerId == customerId)
                .ToList();

            var customer = _context.Customers
                .Include(c => c.Deliveryaddresses)
                .FirstOrDefault(c => c.CustomerId == customerId);

            ViewBag.Customer = customer;

            return View(cartItems);
        }



        [HttpPost]
        public IActionResult Checkout(int[] selectedItems, int[] quantities, string paymentMethod, string? note, int selectedAddressId)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thanh toán.";
                return RedirectToAction("Login", "CustomAccount");
            }

            var cart = _context.Carts.Include(c => c.Cartitems).ThenInclude(ci => ci.Product)
                                      .FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null)
            {
                TempData["ErrorMessage"] = "Giỏ hàng không tồn tại.";
                return RedirectToAction("CartView", "Cart");
            }

            var removedItems = new List<string>();
            var outOfStockItems = new List<string>();
            var orderDetails = new List<Orderdetail>();
            int totalAmount = 0;

            for (int i = 0; i < selectedItems.Length; i++)
            {
                var cartItemId = selectedItems[i];
                var quantity = quantities[i];

                var cartItem = cart.Cartitems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
                if (cartItem == null || cartItem.Quantity == null)
                    continue;

                if (quantity > cartItem.Quantity)
                {
                    removedItems.Add(cartItem.Product.ProductName);
                    _context.Cartitems.Remove(cartItem);
                    continue;
                }

                var product = cartItem.Product;
                int stock = product.UnitsInStock ?? 0;

                if (quantity > stock)
                {
                    outOfStockItems.Add(product.ProductName);
                    continue;
                }

                product.UnitsInStock = stock - quantity;

                int price = product.Price ?? 0;
                int discount = product.Discount ?? 0;
                int discountedPrice = price * (100 - discount) / 100;

                int itemTotal = discountedPrice * quantity;
                totalAmount += itemTotal;

                orderDetails.Add(new Orderdetail
                {
                    ProductId = product.ProductId,
                    Quantity = quantity,
                    OrderNumber = i + 1,
                    Total = itemTotal,
                    Discount = discount,
                    ShipDate = null
                });

                _context.Cartitems.Remove(cartItem);
            }

            if (outOfStockItems.Any())
            {
                TempData["ErrorMessage"] = $"Sản phẩm sau đã vượt quá số lượng tồn kho: {string.Join(", ", outOfStockItems)}.";
                return RedirectToAction("CartView", "Cart");
            }

            if (!orderDetails.Any())
            {
                TempData["ErrorMessage"] = "Không có sản phẩm nào hợp lệ để thanh toán.";
                return RedirectToAction("CartView", "Cart");
            }

            int paymentMethodId = 0;
            if (paymentMethod == "COD") paymentMethodId = 1;

            int? deliveryAddressId = selectedAddressId == 0 ? null : selectedAddressId;


            var newOrder = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                ShipDate = null,
                TransactStatusId = 1,
                Deleted = false,
                Paid = false,
                PaymentId = null,
                PaymentDate = null,
                PaymentMethodId = paymentMethodId,
                DeliveryAddressId = deliveryAddressId, // <- bây giờ có thể là null
                Note = note,
                Orderdetails = orderDetails
            };

            _context.Orders.Add(newOrder);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thanh toán thành công.";
            return RedirectToAction("CheckoutSuccess");
        }
        public IActionResult CheckoutSuccess()
        {
            return View();
        }

    }
}
