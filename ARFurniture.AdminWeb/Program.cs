using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 1. CẤU HÌNH COOKIE ĐĂNG NHẬP
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    });

// 2. CẤU HÌNH LIÊN KẾT LINK API (CHỈ CẦN DUY NHẤT 1 KHỐI NÀY LÀ ĐỦ)
builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName, client =>
{
    // Căn cứ vào đây, nó sẽ tự tìm link trong file appsettings.json để gắn vào
    var baseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value;

    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new Exception("Chưa cấu hình ApiSettings:BaseUrl trong appsettings.json!");
    }

    client.BaseAddress = new Uri(baseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Bỏ qua lỗi chứng chỉ bảo mật (nếu API chạy https mà chưa có SSL chuẩn)
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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