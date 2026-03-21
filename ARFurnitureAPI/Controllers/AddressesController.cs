using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARFurnitureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AddressesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH ĐỊA CHỈ CỦA 1 USER
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserAddress>>> GetUserAddresses(int userId)
        {
            var addresses = await _context.UserAddresses
                                          .Where(a => a.UserId == userId)
                                          .OrderByDescending(a => a.IsDefault) // Mặc định luôn xếp lên đầu
                                          .ToListAsync();
            return Ok(addresses);
        }

        // 2. THÊM ĐỊA CHỈ MỚI
        [HttpPost]
        public async Task<ActionResult<UserAddress>> AddAddress(UserAddress address)
        {
            // Kiểm tra xem user này đã có địa chỉ nào chưa
            var existingAddresses = await _context.UserAddresses.Where(a => a.UserId == address.UserId).ToListAsync();

            // Nếu đây là địa chỉ đầu tiên, hoặc user tick chọn làm Mặc định
            if (!existingAddresses.Any() || address.IsDefault)
            {
                address.IsDefault = true;
                // Tắt mặc định của các địa chỉ cũ đi
                foreach (var addr in existingAddresses) addr.IsDefault = false;
            }

            _context.UserAddresses.Add(address);
            await _context.SaveChangesAsync();

            return Ok(address);
        }

        // 3. SỬA ĐỊA CHỈ
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, UserAddress address)
        {
            if (id != address.Id) return BadRequest();

            if (address.IsDefault)
            {
                // Tắt mặc định của các địa chỉ khác
                var otherAddresses = await _context.UserAddresses.Where(a => a.UserId == address.UserId && a.Id != id).ToListAsync();
                foreach (var addr in otherAddresses) addr.IsDefault = false;
            }

            _context.Entry(address).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 4. XÓA ĐỊA CHỈ
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await _context.UserAddresses.FindAsync(id);
            if (address == null) return NotFound();

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}