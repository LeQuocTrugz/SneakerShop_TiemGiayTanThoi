using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Web_StoreGiay.Models;

public class OrderManagerController : Controller
{
    private readonly EmailService _emailService;
    private readonly ApplicationDbContext _context;

    // Tiêm EmailService vào controller
    public OrderManagerController(ApplicationDbContext context,EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<IActionResult> Index()
    {
        // Lấy tất cả các đơn hàng có trạng thái "Chờ xác nhận"
        var orders = await _context.Orders
            .Where(o => o.Status == "Chờ xác nhận")
        .ToListAsync();

        return View(orders);
    }
    // Action để xác nhận đơn hàng và gửi email xác nhận
    [HttpPost]
    public async Task<IActionResult> ConfirmOrder(int orderId)
    {
        // Tìm đơn hàng trong cơ sở dữ liệu
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        // Cập nhật trạng thái đơn hàng thành "Đã xác nhận"
        order.Status = "Đã xác nhận";
        _context.Update(order);
        await _context.SaveChangesAsync();

        // Tạo nội dung email xác nhận
        var subject = "Đơn hàng của bạn đã được xác nhận!";
        var body = $"<p>Đơn hàng của bạn đã được xác nhận và đang được xử lý.</p>" +
                   $"<p><strong>Tên người nhận:</strong> {order.Name}</p>" +
                   $"<p><strong>Email:</strong> {order.Email}</p>" +
                   $"<p><strong>Số điện thoại:</strong> {order.PhoneNumber}</p>" +
                   $"<p><strong>Địa chỉ:</strong> {order.Address}</p>" +
                   $"<p><strong>Phí ship:</strong> {order.ShippingFee.ToString("N0")} VND</p>" +
                   $"<h3>Thông tin chi tiết đơn hàng:</h3>";

        // Gửi email xác nhận cho khách hàng
        await _emailService.SendEmailAsync(order.Email, subject, body);

        // Trả về thông báo thành công hoặc chuyển hướng đến trang danh sách đơn hàng
        return RedirectToAction("ManageOrders");
    }
}
