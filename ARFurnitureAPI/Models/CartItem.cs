namespace ARFurnitureAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } // Liên kết lấy thông tin sản phẩm
        public int Quantity { get; set; }    // Số lượng sản phẩm
    }
}