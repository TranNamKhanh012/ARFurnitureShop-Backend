var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Cấu hình HttpClient gọi sang Project API (Cổng 5103)
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
