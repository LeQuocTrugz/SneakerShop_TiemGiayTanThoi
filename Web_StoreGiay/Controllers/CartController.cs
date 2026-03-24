using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_StoreGiay.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Web_StoreGiay.Repositories;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IProductRepository _productRepository;
    private readonly ISizeRepository _sizeRepository;

    public CartController(ApplicationDbContext context, IProductRepository productRepository, ISizeRepository sizeRepository)
    {
        _context = context;
        _productRepository = productRepository;
        _sizeRepository = sizeRepository;
    }

    // Thêm sản phẩm vào giỏ hàng
    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int sizeId, int quantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Lấy UserId của người dùng từ đăng nhập
        var product = await _productRepository.GetByIdAsync(productId);
        var size = await _sizeRepository.GetByIdAsync(sizeId);

        if (product == null || size == null)
        {
            return NotFound();
        }

        // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
        var existingCartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId && c.SizeId == sizeId);

        if (existingCartItem != null)
        {
            // Nếu có rồi thì cập nhật số lượng
            existingCartItem.Quantity += quantity;
            _context.Update(existingCartItem);
        }
        else
        {
            // Nếu chưa có thì thêm mới
            var newCartItem = new CartItem
            {
                UserId = userId,
                ProductId = productId,
                SizeId = sizeId,
                Quantity = quantity,
                Price = product.Price  // Hoặc tính toán giá giảm tại đây nếu có
            };

            _context.CartItems.Add(newCartItem);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Cart");  // Sau khi thêm sản phẩm, chuyển đến trang giỏ hàng
    }

    // Hiển thị giỏ hàng
    public async Task<IActionResult> Cart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Include(c => c.Size)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return View(cartItems);  // Trả về View hiển thị giỏ hàng
    }

    // Cập nhật giỏ hàng
    [HttpPost]
    public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
    {
        var cartItem = await _context.CartItems
                                     .Include(ci => ci.Product)
                                     .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

        if (cartItem == null)
        {
            return NotFound();
        }

        // Cập nhật số lượng và giá tiền mới vào giỏ hàng
        cartItem.Quantity = quantity;
        cartItem.Price = cartItem.Product.Price * quantity;  // Cập nhật lại giá tiền cho sản phẩm trong giỏ

        _context.Update(cartItem);
        await _context.SaveChangesAsync();

        return RedirectToAction("Cart");  // Quay lại giỏ hàng sau khi cập nhật
    }

    // Xóa sản phẩm khỏi giỏ hàng
    [HttpGet]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var cartItem = await _context.CartItems.FindAsync(id);

        if (cartItem == null)
        {
            return NotFound();
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return RedirectToAction("Cart"); // Sau khi xóa, quay lại giỏ hàng
    }

    // Hiển thị trang nhập thông tin giao hàng
    public IActionResult ShippingInfo()
    {
        return View();  // Trả về trang điền thông tin giao hàng
    }

    // Xác nhận đơn hàng
    [HttpPost]
    public async Task<IActionResult> ConfirmOrder(Order order)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Lấy giỏ hàng từ cơ sở dữ liệu
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
            .Include(c => c.Size)
            .ToListAsync();

        // Kiểm tra nếu giỏ hàng trống
        if (cartItems.Count == 0)
        {
            return RedirectToAction("Cart");
        }

        // Tính tổng tiền đơn hàng (không bao gồm phí ship)
        var totalPrice = cartItems.Sum(c => c.Price * c.Quantity);

        // Lưu thông tin đơn hàng vào đối tượng Order
        var newOrder = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            TotalPrice = totalPrice + order.ShippingFee,  // Tổng giá trị bao gồm phí ship
            Status = "Chờ xác nhận",
            Name = order.Name,
            Email = order.Email,
            PhoneNumber = order.PhoneNumber,
            Address = order.Address,
            ShippingFee = order.ShippingFee
        };

        // Lưu đơn hàng vào cơ sở dữ liệu
        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        // Lưu OrderDetails cho đơn hàng
        foreach (var item in cartItems)
        {
            var orderDetail = new OrderDetail
            {
                OrderId = newOrder.Id,
                ProductId = item.ProductId,
                SizeId = item.SizeId,
                Quantity = item.Quantity,
                Price = item.Price
            };
            _context.OrderDetails.Add(orderDetail);
        }

        // Xóa giỏ hàng của người dùng
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        // Chuyển đến trang xác nhận đơn hàng
        return RedirectToAction("OrderConfirmation", new { orderId = newOrder.Id });
    }

    // Xem xác nhận đơn hàng
    public async Task<IActionResult> OrderConfirmation(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);  // Trả về view xác nhận đơn hàng
    }

    // Xem chi tiết đơn hàng
    public async Task<IActionResult> OrderDetails(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.Id == id)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound();
        }

        return View(order);  // Hiển thị chi tiết đơn hàng
    }

    // Xem lịch sử đơn hàng
    public async Task<IActionResult> OrderHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .ToListAsync();

        return View(orders);  // Trả về view lịch sử đơn hàng
    }
    [HttpPost]
    public IActionResult OrderSuccess()
    {
        // Chuyển đến trang thông báo thành công sau khi đơn hàng được xác nhận
        return View();  // Trả về trang thông báo thành công
    }
}
