namespace Web_StoreGiay.Models
{
    public class Size
    {
        public int Id { get; set; }
        public int SizeNumber { get; set; } // Ví dụ: 36, 37, 38...
        public ICollection<ProductSize> ProductSizes { get; set; }
    }
}
