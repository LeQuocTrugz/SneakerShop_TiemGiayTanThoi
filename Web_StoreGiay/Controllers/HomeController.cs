using Microsoft.AspNetCore.Mvc;
using Web_StoreGiay.Models;
using Web_StoreGiay.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Web_StoreGiay.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISizeRepository _sizeRepository;
        private readonly ApplicationDbContext _context;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository, ISizeRepository sizeRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _sizeRepository = sizeRepository;
            _context = context;
        }

        // Trang ch? - L?y t?t c? s?n ph?m
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
        public async Task<IActionResult> AllProducts()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var sizes = await _sizeRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();

            ViewBag.Categories = categories;
            ViewBag.Sizes = sizes;
            ViewData["Title"] = "Giày";

            return View("ProductList", products); // Tr? v? view hi?n th? t?t c? s?n ph?m
        }

        // L?y s?n ph?m theo gi?i tính
        public async Task<IActionResult> ProductsByGender(int gender)
        {
            var products = await _productRepository.GetAllAsync();
            var filteredProducts = products.Where(p => (int)p.Gender == gender).ToList();
            var categories = await _categoryRepository.GetAllAsync();
            var sizes = await _sizeRepository.GetAllAsync();

            ViewBag.Categories = categories;
            ViewBag.Sizes = sizes;
            ViewData["Title"] = gender == 1 ? " Giày nam" : " Giày nữ";

            return View("ProductList", filteredProducts);
        }

        // L?y s?n ph?m theo danh m?c (Category)
        public async Task<IActionResult> ProductsByCategory(int categoryId)
        {
            var products = await _productRepository.GetAllAsync();
            var filteredProducts = products.Where(p => p.CategoryId == categoryId).ToList();
            var categories = await _categoryRepository.GetAllAsync();
            var sizes = await _sizeRepository.GetAllAsync();
            var category = categories.FirstOrDefault(c => c.Id == categoryId);

            ViewBag.Categories = categories;
            ViewBag.Sizes = sizes;
            ViewData["Title"] = $" {category?.Name}";

            return View("ProductList", filteredProducts);
        }

        // L?y s?n ph?m có gi?m giá
        public async Task<IActionResult> ProductsOnSale()
        {
            var products = await _productRepository.GetAllAsync();
            var filteredProducts = products.Where(p => p.DiscountPrice.HasValue).ToList();
            var categories = await _categoryRepository.GetAllAsync();
            var sizes = await _sizeRepository.GetAllAsync();

            ViewBag.Categories = categories;
            ViewBag.Sizes = sizes;
            ViewData["Title"] = " Danh Sách Giày giảm giá";

            return View("ProductList", filteredProducts);
        }
        // chi ti?t s?n ph?m
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            var sizes = await _sizeRepository.GetAllAsync();
            ViewBag.AllSizes = sizes;
            if (product == null)
            {
                return NotFound();
            }

             var relateds = (await _productRepository.GetAllAsync())
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToList();
            ViewBag.RelatedProducts = relateds;
            return View(product);
        }
        public IActionResult Search(string keyword)
        {
            // Lưu từ khóa để hiển thị lại trên View
            ViewBag.Keyword = keyword;

            // Kiểm tra nếu có từ khóa, tiến hành lọc sản phẩm theo tên
            var results = string.IsNullOrEmpty(keyword)
            ? _context.Products.ToList() // Nếu không có từ khóa, trả về tất cả sản phẩm
                : _context.Products
                    .Where(p => p.Name.Contains(keyword)) // Lọc sản phẩm theo tên
                    .ToList();

            // Truyền kết quả vào view
            return View(results);
        }
    }
}
