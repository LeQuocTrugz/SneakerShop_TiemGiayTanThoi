using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Web_StoreGiay.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; } // Giá sau khi giảm (nếu có)

        public string? Description { get; set; }

        public string? ImageUrl { get; set; } // Ảnh chính của sản phẩm

        public int CategoryId { get; set; }
        public  Category? Category { get; set; } // Liên kết đến danh mục
        public GenderType Gender { get; set; }

        public  List<ProductImage>? Images { get; set; } // Danh sách ảnh phụ

        public ICollection<ProductSize>? ProductSizes { get; set; } // Quản lý size & tồn kho

        public ICollection<Review>? Reviews { get; set; } // Đánh giá sản phẩm
    }
}
