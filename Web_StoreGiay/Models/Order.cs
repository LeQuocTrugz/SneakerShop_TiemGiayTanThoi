namespace Web_StoreGiay.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } // Chờ xác nhận, Đang giao, Đã nhận...

        public string Name { get; set; }  // Tên người nhận
        public string Email { get; set; }  // Email người nhận
        public string PhoneNumber { get; set; }  // Số điện thoại
        public string Address { get; set; }  // Địa chỉ giao hàng
        public decimal ShippingFee { get; set; } = 45000;  // Phí ship (COD) cố định
        public ICollection<OrderDetail> OrderDetails { get; set; }

    }
}
