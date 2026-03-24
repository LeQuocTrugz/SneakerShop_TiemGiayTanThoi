namespace Web_StoreGiay.Models
{
    public class ProductSize
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int SizeId { get; set; }
        public Size Size { get; set; }
        public int Stock { get; set; } // Số lượng tồn kho theo từng size
    }
}
