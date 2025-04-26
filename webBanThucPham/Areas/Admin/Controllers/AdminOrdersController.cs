using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using webBanThucPham.Helper;
using webBanThucPham.Models;
using X.PagedList;
using System.IO;
using DinkToPdf;
using System.Drawing.Printing;
using DinkToPdf.Contracts;


namespace webBanThucPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;
        private readonly IConverter _converter;
        public AdminOrdersController(INotyfService notyf, DbBanThucPhamContext context, IWebHostEnvironment webHostEnvironment, IConverter converter)
        {
            _notyf = notyf;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _converter = converter;
        }
        // Controller Action
        public async Task<IActionResult> Index(int? orderId, string? customerEmail, int? statusId, DateTime? fromDate, DateTime? toDate, int? minTotal, int? maxTotal, bool? deletedOnly, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.TransactStatus)
                .Include(o => o.Orderdetails)
                .AsQueryable();

            // Nếu chọn checkbox "Hủy bởi khách hàng" => chỉ lấy đơn bị hủy
            if (deletedOnly.HasValue && deletedOnly.Value)
            {
                orders = orders.Where(o => o.Deleted);
            }
            else
            {
                // Ngược lại: chỉ lấy đơn chưa bị hủy
                orders = orders.Where(o => !o.Deleted);
            }

            if (orderId.HasValue)
            {
                orders = orders.Where(o => o.OrderId == orderId.Value);
            }

            if (!string.IsNullOrEmpty(customerEmail))
            {
                orders = orders.Where(o => o.Customer.Email.Contains(customerEmail));
            }

            if (statusId.HasValue)
            {
                orders = orders.Where(o => o.TransactStatusId == statusId);
            }

            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= fromDate);
            }

            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= toDate);
            }

            if (minTotal.HasValue)
            {
                orders = orders.Where(o => o.Orderdetails.Sum(od => od.Total) >= minTotal);
            }

            if (maxTotal.HasValue)
            {
                orders = orders.Where(o => o.Orderdetails.Sum(od => od.Total) <= maxTotal);
            }

            ViewBag.StatusList = new SelectList(await _context.Transactstatusses.ToListAsync(), "TracsactStatusId", "Status");

            return View(await orders.OrderByDescending(o => o.OrderDate).ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: AdminOrders/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.TransactStatus)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return NotFound();
            }

            ViewBag.StatusList = new SelectList(_context.Transactstatusses, "TracsactStatusId", "Status");
            return View(order);
        }

        // POST: AdminOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,TransactStatusId,ShipDate,PaymentDate")] Order updatedOrder)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return NotFound();
            }

            order.TransactStatusId = updatedOrder.TransactStatusId;
            order.ShipDate = updatedOrder.ShipDate;
            order.PaymentDate = updatedOrder.PaymentDate;

            await _context.SaveChangesAsync();

            _notyf.Success("✅ Cập nhật đơn hàng thành công!");
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.Customer)
                .Include(o => o.PaymentMethod)
                .Include(o => o.TransactStatus)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return NotFound();
            }

            return View(order);
        }

        public IActionResult ExportToExcel(int id)
        {
            var order = _context.Orders
                .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .Include(o => o.PaymentMethod)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return NotFound();
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("ChiTietDonHang");

                int row = 1;

                // Thông tin đơn hàng
                worksheet.Cells[row, 1].Value = $"Chi tiết đơn hàng #{order.OrderId}";
                worksheet.Cells[row, 1, row, 4].Merge = true;
                worksheet.Cells[row, 1].Style.Font.Size = 16;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                row += 2;

                worksheet.Cells[row, 1].Value = "Khách hàng:";
                worksheet.Cells[row, 2].Value = order.Customer.FullName;
                row++;

                worksheet.Cells[row, 1].Value = "Ngày đặt:";
                worksheet.Cells[row, 2].Value = order.OrderDate?.ToString("dd/MM/yyyy");
                row++;

                worksheet.Cells[row, 1].Value = "Phương thức thanh toán:";
                worksheet.Cells[row, 2].Value = order.PaymentMethod?.MethodName;
                row += 2;

                // Header chi tiết sản phẩm
                worksheet.Cells[row, 1].Value = "Tên sản phẩm";
                worksheet.Cells[row, 2].Value = "Số lượng";
                worksheet.Cells[row, 3].Value = "Giá";
                worksheet.Cells[row, 4].Value = "Thành tiền";

                using (var range = worksheet.Cells[row, 1, row, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                row++;

                // Dữ liệu sản phẩm
                foreach (var item in order.Orderdetails)
                {
                    worksheet.Cells[row, 1].Value = item.Product.ProductName;
                    worksheet.Cells[row, 2].Value = item.Quantity;
                    worksheet.Cells[row, 3].Value = item.Product.Price;
                    worksheet.Cells[row, 4].Value = item.Total;

                    worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0 đ";
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0 đ";
                    row++;
                }

                // Tổng tiền
                worksheet.Cells[row, 3].Value = "Tổng cộng:";
                worksheet.Cells[row, 3].Style.Font.Bold = true;
                worksheet.Cells[row, 4].Value = order.Orderdetails.Sum(od => od.Total);
                worksheet.Cells[row, 4].Style.Font.Bold = true;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0 đ";

                // Auto fit all columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Xuất ra file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Order_{order.OrderId}.xlsx");
            }
        }
        public IActionResult ExportToPdf(int id)
        {
            var order = _context.Orders
                .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .Include(o => o.PaymentMethod)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return NotFound();
            }

            // Tạo nội dung HTML chi tiết
            string html = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 20px;
            font-size: 14px;
        }}
        h1, h2, h3 {{
            color: #333;
        }}
        .info {{
            margin-bottom: 20px;
        }}
        .info p {{
            margin: 5px 0;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }}
        table, th, td {{
            border: 1px solid #000;
        }}
        th, td {{
            padding: 8px;
            text-align: left;
        }}
        th {{
            background-color: #f2f2f2;
        }}
        .total {{
            margin-top: 20px;
            font-size: 16px;
            font-weight: bold;
            text-align: right;
        }}
    </style>
</head>
<body>
    <h1>HÓA ĐƠN BÁN HÀNG</h1>

    <div class='info'>
        <p><strong>Mã đơn hàng:</strong> {order.OrderId}</p>
        <p><strong>Khách hàng:</strong> {order.Customer.FullName}</p>
        <p><strong>Email:</strong> {order.Customer.Email}</p>
        <p><strong>Ngày đặt hàng:</strong> {order.OrderDate:dd/MM/yyyy}</p>
        <p><strong>Phương thức thanh toán:</strong> {order.PaymentMethod?.MethodName}</p>
    </div>

    <table>
        <thead>
            <tr>
                <th>#</th>
                <th>Tên sản phẩm</th>
                <th>Số lượng</th>
                <th>Giá (₫)</th>
                <th>Thành tiền (₫)</th>
            </tr>
        </thead>
        <tbody>";

            int index = 1;
            foreach (var item in order.Orderdetails)
            {
                html += $@"
            <tr>
                <td>{index++}</td>
                <td>{item.Product.ProductName}</td>
                <td>{item.Quantity}</td>
                <td>{item.Product.Price:N0}</td>
                <td>{item.Total:N0}</td>
            </tr>";
            }

            html += @"
        </tbody>
    </table>";

            html += $@"
    <div class='total'>
        Tổng tiền: {order.Orderdetails.Sum(od => od.Total):N0} ₫
    </div>

</body>
</html>";

            // Tạo file PDF từ HTML
            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = DinkToPdf.PaperKind.A4,
                    Margins = new MarginSettings { Top = 20, Bottom = 20, Left = 20, Right = 20 }
                },
                Objects = {
            new ObjectSettings
            {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            byte[] pdfBytes = _converter.Convert(pdfDoc);

            return File(pdfBytes, "application/pdf", $"Order_{order.OrderId}.pdf");
        }



    }
}
