using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ARFurniture.AdminWeb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ARFurniture.AdminWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        // Tiêm IConfiguration để tự động đọc appsettings.json
        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var payload = new { Username = model.Username, Password = model.Password };
            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // Lấy BaseUrl từ appsettings.json (VD: http://trannamkhanh.../api/)
            string baseUrl = _config.GetSection("ApiSettings:BaseUrl").Value;

            // Nối thêm Auth/login vào cuối link
            string fullApiUrl = baseUrl + "Auth/login";

            using (var client = _httpClientFactory.CreateClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, fullApiUrl);
                request.Content = content;

                // ========================================================
                // 3 LỆNH BẮT BUỘC ĐỂ VƯỢT TƯỜNG LỬA SMARTERASP
                // ========================================================
                request.Headers.ExpectContinue = false; // Chặn lỗi 401 HTML
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0");
                request.Headers.Add("Accept", "*/*");
                // ========================================================

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<LoginResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (user.Role != "Admin")
                    {
                        ViewBag.Error = "Tài khoản của bạn không có quyền truy cập trang Quản trị!";
                        return View(model);
                    }

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    return RedirectToAction("Index", "Home");
                }

                var errorDetail = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"Lỗi từ API ({response.StatusCode}): {errorDetail}";
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}