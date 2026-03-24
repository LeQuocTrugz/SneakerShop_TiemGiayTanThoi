using Microsoft.AspNetCore.Mvc;
using Web_StoreGiay.Models;
using Web_StoreGiay.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace Week3_2280603443.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISizeRepository _sizeRepository;
        private IProductSizeRepository _productSizeRepository;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, ISizeRepository sizeRepository, IProductSizeRepository productSizeRepository, ApplicationDbContext context, EmailService emailService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _sizeRepository = sizeRepository;
            _productSizeRepository = productSizeRepository;
            _context = context;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
        // Hiển thị form thêm sản phẩm mới 
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            ViewBag.Sizes = await _sizeRepository.GetAllAsync();
            return View();
        }

        // Xử lý thêm sản phẩm mới 
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrlFile, List<IFormFile> imageFiles, List<int> selectedSizes, List<int> stockQuantities)
        {
            if (ModelState.IsValid)
            {
                // Xử lý ảnh đại diện (imageUrl)
                if (imageUrlFile != null && imageUrlFile.Length > 0)
                {
                    var imagePath = await SaveImage(imageUrlFile); 
                    product.ImageUrl = imagePath; 
                }

                // Xử lý ảnh phụ
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    product.Images = new List<ProductImage>(); // Khởi tạo danh sách ảnh nếu chưa có
                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var imagePath = await SaveImage(file); // Lưu ảnh phụ
                            product.Images.Add(new ProductImage { Url = imagePath });
                        }
                    }
                }
                if (product.DiscountPrice.HasValue && product.DiscountPrice.Value >= product.Price)
                {
                    ModelState.AddModelError("DiscountPrice", "Giá giảm phải nhỏ hơn giá gốc.");
                    return View(product);
                }

                await _productRepository.AddAsync(product); // Thêm sản phẩm vào cơ sở dữ liệu
                if (selectedSizes != null && stockQuantities != null && selectedSizes.Count == stockQuantities.Count)
                {
                    for (int i = 0; i < selectedSizes.Count; i++)
                    {
                        var productSize = new ProductSize
                        {
                            ProductId = product.Id,
                            SizeId = selectedSizes[i],
                            Stock = stockQuantities[i]
                        };
                        await _productSizeRepository.AddAsync(productSize);
                    }
                }
                return RedirectToAction(nameof(Index)); // Quay lại trang danh sách sản phẩm
            }

            // Trả về lại form nếu có lỗi
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            ViewBag.Sizes = await _sizeRepository.GetAllAsync();
            return View(product);
        }
        private async Task<string> SaveImage(IFormFile image)
        {
            //Thay đổi đường dẫn theo cấu hình của bạn 
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối 
        }
        // Hiển thị thông tin chi tiết sản phẩm 
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Hiển thị form cập nhật sản phẩm 
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            ViewBag.Sizes = await _sizeRepository.GetAllAsync();
            var productSizes = await _productSizeRepository.GetByProductIdAsync(id);
            ViewBag.ProductSizes = productSizes;
            return View(product);
        }
        // Xử lý cập nhật sản phẩm 
        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile? imageUrlFile, List<IFormFile>? imageFiles, List<int> selectedSizes, List<int> stockQuantities)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin sản phẩm
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.DiscountPrice = product.DiscountPrice;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                // Cập nhật ảnh đại diện nếu có ảnh mới
                if (imageUrlFile != null && imageUrlFile.Length > 0)
                {
                    var imagePath = await SaveImage(imageUrlFile);
                    existingProduct.ImageUrl = imagePath;
                }

                existingProduct.Images.Clear();
                // Cập nhật danh sách ảnh phụ nếu có ảnh mới
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    existingProduct.Images ??= new List<ProductImage>(); // Đảm bảo danh sách không null

                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var imagePath = await SaveImage(file);
                            existingProduct.Images.Add(new ProductImage { Url = imagePath });
                        }
                    }
                }

                // Lấy danh sách size hiện tại đã liên kết với sản phẩm
                var existingProductSizes = await _productSizeRepository.GetByProductIdAsync(id);

                // Xóa các size không còn được chọn nữa
                var sizesToRemove = existingProductSizes.Where(ps => !selectedSizes.Contains(ps.SizeId)).ToList();

                foreach (var sizeToRemove in sizesToRemove)
                {
                    await _productSizeRepository.DeleteAsync(sizeToRemove.SizeId); // Xóa size đã chọn trước đó nhưng không còn được chọn
                }

                // Cập nhật hoặc thêm các size mới và số lượng tồn kho
                if (selectedSizes != null && stockQuantities != null && selectedSizes.Count == stockQuantities.Count)
                {
                    for (int i = 0; i < selectedSizes.Count; i++)
                    {
                        var sizeId = selectedSizes[i];
                        var stock = stockQuantities[i];

                        var existingSize = existingProductSizes.FirstOrDefault(ps => ps.SizeId == sizeId);

                        if (existingSize != null)
                        {
                            existingSize.Stock = stock;
                            await _productSizeRepository.UpdateAsync(existingSize);
                        }
                        else
                        {
                            var productSize = new ProductSize
                            {
                                ProductId = id,
                                SizeId = sizeId,
                                Stock = stock
                            };
                            await _productSizeRepository.AddAsync(productSize);
                        }
                    }
                }
                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }
        // Hiển thị form xác nhận xóa sản phẩm 
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Xử lý xóa sản phẩm 
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> OrderMail()
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
            return RedirectToAction("OrderMail1");
        }
        public async Task<IActionResult> OrderMail1()
        {
            // Lấy tất cả các đơn hàng có trạng thái "Đã xác nhận"
            var orders = await _context.Orders
                .Where(o => o.Status == "Đã xác nhận")
            .ToListAsync();

            return View(orders);
        }
    }
}
