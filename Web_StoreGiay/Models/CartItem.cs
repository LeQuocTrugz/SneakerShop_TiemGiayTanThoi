namespace Web_StoreGiay.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Để liên kết giỏ hàng với người dùng
        public ApplicationUser User { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int SizeId { get; set; }
        public Size Size { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}
