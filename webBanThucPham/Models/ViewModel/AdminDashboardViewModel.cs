using webBanThucPham.Models;
using System.Collections.Generic;
namespace webBanThucPham.Models.ViewModel
{
    public class AdminDashboardViewModel
    {
        // Thông báo đơn hàng mới trong hôm nay
        public int NewOrdersToday { get; set; }

        // Thông tin người dùng
        public int TotalCustomers { get; set; }

        // Thông tin đơn hàng đã giao thành công trong ngày
        public int DeliveredOrdersToday { get; set; }

        // Doanh thu trong ngày
        public decimal TotalRevenueToday { get; set; }

        // Tổng số sản phẩm
        public int TotalProducts { get; set; }

        // Dữ liệu Pie Chart (Trạng thái đơn hàng)
        public List<OrderStatusData> OrderStatusData { get; set; }

        // Thanh thông báo đơn hàng mới
        public List<Order> RecentOrders { get; set; }

        // Top 5 sản phẩm bán chạy nhất
        public List<TopProduct> Top5BestSellingProducts { get; set; }

        // Top 5 sản phẩm ít tồn kho
        public List<Product> Top5LowStockProducts { get; set; }

        // Top 5 sản phẩm giảm giá nhiều nhất
        public List<Product> Top5DiscountedProducts { get; set; }
    }

    // Dữ liệu cho Pie Chart
    public class OrderStatusData
    {
        public int? StatusId { get; set; }
        public int Count { get; set; }
    }

    // Dữ liệu top sản phẩm bán chạy nhất
    public class TopProduct
    {
        public Product Product { get; set; }
        public int TotalQuantity { get; set; }
    }
}
