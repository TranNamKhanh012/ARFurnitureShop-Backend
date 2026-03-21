using ARFurnitureAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ARFurnitureAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Khai báo bảng Categories
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; } = null!;
        // Sau này chúng ta sẽ thêm các bảng Products, Users... vào đây
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
    }
}