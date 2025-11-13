
using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NTM_SHOP.Extension; // Namespace của các Extension methods
using NTM_SHOP.Helper;
using NTM_SHOP.Models;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database Context (Thay thế ConfigureServices - Start)
builder.Services.AddDbContext<OnlineShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbNTMShop")));

// 2. Thêm các Services của bạn (Thay thế ConfigureServices - Body)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Thời gian chờ phiên
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(p =>
    {
        p.LoginPath = "/dang-nhap.html";
        p.AccessDeniedPath = "/";
    });

// 1. Cấu hình Schemes
builder.Services.AddAuthentication(options =>
{
    // Đặt Scheme mặc định (Default Scheme) là User/Shop
    options.DefaultAuthenticateScheme = "UserScheme";
    options.DefaultChallengeScheme = "UserScheme";
    options.DefaultSignInScheme = "UserScheme";
})
// Scheme cho Khách hàng (Shop)
.AddCookie("UserScheme", options =>
{
    options.LoginPath = "/dang-nhap.html"; // Trang đăng nhập của Shop
    options.AccessDeniedPath = "/home/accessdenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(10);
})
// Scheme cho Admin
.AddCookie("AdminScheme", options =>
{
    options.LoginPath = "/Admin/AdminLogin"; // Trang đăng nhập của Admin
    options.AccessDeniedPath = "/Admin/AdminLogin";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

// 2. Cấu hình Authorization Policies (Rất quan trọng)
builder.Services.AddAuthorization(options =>
{
    // Policy cho khu vực Admin
    options.AddPolicy("AdminAreaPolicy", policy =>
    {
        // Yêu cầu sử dụng Scheme Admin khi vào khu vực Admin
        policy.AddAuthenticationSchemes("AdminScheme");
        policy.RequireRole("Admin"); // Chỉ cho phép Role là "Admin"
    });

    // Policy cho khu vực Shop (nếu cần)
    options.AddPolicy("UserPolicy", policy =>
    {
        // Yêu cầu sử dụng Scheme User
        policy.AddAuthenticationSchemes("UserScheme");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddNotyf(options => { options.DurationInSeconds = 10; options.IsDismissable = true; options.Position = NotyfPosition.TopRight; });
// Kết thúc Add Services

var app = builder.Build();

// 3. Cấu hình Middleware (Thay thế Configure)

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
app.UseSession(); // Thêm Session
app.UseAuthentication(); // Thêm Authentication
app.UseAuthorization();


// 4. Cấu hình Routing (Map Controllers)

// Area Route cho Admin (Luôn đặt Area trước các Route thông thường)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Custom Route cho các trang Page
app.MapControllerRoute(
    name: "Page",
    pattern: "/page/{Alias}.html",
    defaults: new { controller = "Page", action = "Details" }
);

// Custom Route cho Product List
app.MapControllerRoute(
    name: "List",
    pattern: "/{Alias}.html",
    defaults: new { controller = "Product", action = "List" }
);

// Custom Route cho Product Details
app.MapControllerRoute(
    name: "Details",
    pattern: "/{Title}-{id}.html",
    defaults: new { controller = "Product", action = "Details" }
);

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();