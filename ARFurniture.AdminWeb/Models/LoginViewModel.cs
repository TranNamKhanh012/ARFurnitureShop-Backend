namespace ARFurniture.AdminWeb.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // Class phụ để hứng dữ liệu từ API Backend trả về
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}