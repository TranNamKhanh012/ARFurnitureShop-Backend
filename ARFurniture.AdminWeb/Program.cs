using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.AddControllersWithViews();


// 1. CẤU HÌNH COOKIE ĐĂNG NHẬP (THÊM VÀO ĐÂY)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Đẩy về trang này nếu chưa đăng nhập
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(1); // Giữ đăng nhập 1 ngày
    });

// Cấu hình HttpClient gọi sang Project API (Cổng 5103)
builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName, client =>
{
    var baseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value;
    // Nếu quên chưa cấu hình thì báo lỗi rõ ràng luôn cho dễ sửa
    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new Exception("Chưa cấu hình ApiSettings:BaseUrl trong appsettings.json!");
    }

    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient("ApiClient", client =>
{
    // ĐÃ SỬA: Điền chính xác cổng của API vào đây (Nhớ kiểm tra kỹ là http hay https nhé)
    client.BaseAddress = new Uri("http://localhost:5103/");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Dòng này giúp bỏ qua lỗi chứng chỉ bảo mật khi chạy máy ảo ở nhà
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
