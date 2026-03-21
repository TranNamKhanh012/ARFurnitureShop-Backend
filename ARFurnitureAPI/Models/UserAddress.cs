using System.ComponentModel.DataAnnotations;

namespace ARFurnitureAPI.Models
{
    public class UserAddress
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; } // Khóa ngoại móc với User đang đăng nhập
        public string ReceiverName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public bool IsDefault { get; set; } // true nếu là địa chỉ mặc định
    }
}