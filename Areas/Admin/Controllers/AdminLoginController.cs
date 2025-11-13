using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTM_SHOP.Helper;
using NTM_SHOP.Models;
using NTM_SHOP.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NTM_SHOP.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminLoginController : Controller
    {
        private readonly OnlineShopContext _context;
        public INotyfService _notyfService { get; }

        public AdminLoginController(OnlineShopContext context, INotyfService notifyfService)
        {
            _context = context;
            _notyfService = notifyfService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {

            if (!ModelState.IsValid)
            {
                _notyfService.Warning("Vui lòng nhập đầy đủ thông tin");
                return View(model);
            }

            // 1. Tìm tài khoản bằng Email và Active
            var admin = await _context.Accounts
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == model.UserName && x.Active == true);

            if (admin == null)
            {
                _notyfService.Error("Tài khoản không tồn tại hoặc bị khóa!");
                return View(model);
            }

            // 2. Xác thực mật khẩu thô (DO DB CỦA BẠN ĐANG LƯU MẬT KHẨU THÔ)
            // ⚠️ CẦN CHUYỂN SANG HASHING TRƯỚC KHI ĐƯA VÀO PRODUCTION!
            bool isPasswordValid = (model.Password == admin.Password);

            if (isPasswordValid)
            {
                // 3. Kiểm tra Role phải là Admin
                // Dùng RoleID = 1 như trong script SQL bạn cung cấp.
                if (admin.RoleId != 1)
                {
                    _notyfService.Error("Bạn không có quyền truy cập quản trị!");
                    return View(model);
                }

                // 4. Lưu Claims và Đăng nhập thành công
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, admin.FullName),
                    new Claim("AdminID", admin.AccountId.ToString()),
                    // Sử dụng tên Role từ DB để đưa vào Claims
                    new Claim(ClaimTypes.Role, admin.Role != null ? admin.Role.RoleName : "Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "AdminLogin");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync("AdminScheme", // SỬ DỤNG ADMIN SCHEME
                 new ClaimsPrincipal(claimsIdentity),authProperties);

                _notyfService.Success("Đăng nhập thành công!");
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    // Chuyển hướng đến trang Admin mà người dùng cố gắng truy cập trước đó
                    return Redirect(model.ReturnUrl);
                }
                // 5. Chuyển hướng đến trang Admin Home
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else
            {
                // 6. Mật khẩu không đúng
                _notyfService.Error("Sai thông tin đăng nhập!");
                return View(model);
            }           
        }
    }
}