using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NTM_SHOP.Extension;
using NTM_SHOP.Helper;
using NTM_SHOP.Models;
using NTM_SHOP.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NTM_SHOP.Controllers
{

    public class AccountsController : Controller
    {
        private readonly OnlineShopContext _context;
        public INotyfService _notyfService { get; }
        public AccountsController(OnlineShopContext context, INotyfService notifyfService)
        {
            _context = context;
            _notyfService = notifyfService;

        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if(khachhang != null)
                {
                    return Json(data: "Số điện thoại : " + Phone + " Đã được sử dụng");
                }
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang != null)
                {
                    return Json(data: "Số điện thoại : " + Email + " Đã được sử dụng");
                }
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }
        [Authorize]
        [Route("tai-khoan-cua-toi.html", Name = "Dashboard")]
        public IActionResult Dashboard()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));

                if (khachhang != null)
                {
                    // Lấy ID Tỉnh/Thành hiện tại (giá trị có thể là null)
                    object selectedTinhThanh = khachhang.LocationId;

                    // 1. LẤY DỮ LIỆU ĐỊA ĐIỂM VÀ TẠO SELECTLIST CHO TỈNH/THÀNH
                    ViewData["lsTinhThanh"] = new SelectList(
                        _context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(),
                        "LocationId",
                        "NameWithType",
                        selectedTinhThanh // TRUYỀN GIÁ TRỊ ĐƯỢC CHỌN VÀO ĐÂY
                    );

                    // 2. LƯU DỮ LIỆU KHÁCH HÀNG VÀO VIEWDATAG
                    // Rất quan trọng: Truyền Customer Model gốc vào ViewData cho Partial View
                    ViewData["CustomerInfo"] = khachhang;

                    // 3. LẤY DANH SÁCH ĐƠN HÀNG (đã có)
                    var lsOrder = _context.Orders
                        .AsNoTracking()
                        .Include(x => x.TransactStatus)
                        .Where(x => x.CustomerId == khachhang.CustomerId)
                        .OrderByDescending(x => x.OrderDate)
                        .ToList();

                    ViewBag.DonHang = lsOrder;

                    return View(khachhang);
                }
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html",Name ="DangKy")]
        public IActionResult DangKyTaiKhoan()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public async Task<IActionResult> DangKyTaiKhoan(RegisterVM taikhoan)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    string salt = Utilities.GetRandomKey();
                    Customer khachhang = new Customer
                    {
                        FullName = taikhoan.FullName,
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = (taikhoan.Password + salt.Trim()).ToMD5(),
                        Active = true,
                        Salt = salt.Trim(),
                        CreateDate = DateTime.Now

                    };
                    try
                    {
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();
                        // Lưu Session MaKH
                        HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("CustomerId");
                        // Identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, khachhang.FullName),
                            new Claim("CustomerId", khachhang.CustomerId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        _notyfService.Success("Đăng ký tài khoản thành công ");
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("DangKyTaiKhoan", "Accounts");
                    }
                }
                else
                {
                    return View(taikhoan);
                }
            }
            catch
            {
                return View(taikhoan);
            }

        }

        [AllowAnonymous]
        [Route("dang-nhap.html", Name ="DangNhap")]
        public IActionResult Login (string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Dashboard", "Accounts");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public async Task<IActionResult> Login (LoginViewModel customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail)
                    {
                        return View(customer);
                    }

                    var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);
                    var admin = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName && x.RoleId == 1);
                    if (admin != null)
                    {
                        _notyfService.Success("Chào mừng admin");
                        return RedirectToAction("Index","AdminOrders", new { area = "Admin" });
                    }

                    if (khachhang == null)
                    {
                        return RedirectToAction("DangKyTaiKhoan");
                    }
                    string saltValue = khachhang.Salt != null ? khachhang.Salt.Trim() : "";
                    string pass = (customer.Password + khachhang.Salt.Trim()).ToMD5();
                    if(khachhang.Password != pass)
                    {
                        _notyfService.Error("Thông tin đăng nhập không chính xác ");
                        return View(customer);
                    }

                    //Kiểm tra tài khoản có bị disable không?
                    if (khachhang.Active == false)
                    {
                        return RedirectToAction("ThongBao", "Accounts");
                    }

                    //Lưu session vào MaKH
                    HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");
                    //Identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,khachhang.FullName),
                        new Claim("CustomerId", khachhang.CustomerId.ToString())
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "customer");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    _notyfService.Success("Đăng nhập thành công");
                    if (!string.IsNullOrEmpty(customer.ReturnUrl))
                    {
                        return Redirect(customer.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                }
            }
            catch
            {
                return RedirectToAction("DangKyTaiKhoan", "Accounts");
            }
            return View(customer);
        }


        [HttpGet]
        [Route("dang-xuat.html",Name ="Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");

            if (taikhoanID == null)
            {
                _notyfService.Error("Vui lòng đăng nhập lại.");
                return RedirectToAction("Login", "Accounts");
            }

            // Nếu ModelState.IsValid thất bại, cần thông báo lỗi cụ thể
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Vui lòng nhập mật khẩu mới và xác nhận chính xác.");
                return RedirectToAction("Dashboard", "Accounts"); // Quay lại Dashboard để hiển thị form/lỗi
            }

            try
            {
                var taikhoan = _context.Customers.Find(Convert.ToInt32(taikhoanID));

                if (taikhoan == null)
                {
                    _notyfService.Error("Tài khoản không tồn tại.");
                    return RedirectToAction("Login", "Accounts");
                }

                // 1. Xử lý Salt: Đảm bảo Salt không NULL
                string saltValue = taikhoan.Salt != null ? taikhoan.Salt.Trim() : "";

                // 2. Xác thực Mật khẩu Hiện tại
                var pass = (model.PasswordNow.Trim() + saltValue).ToMD5();

                if (pass == taikhoan.Password)
                {
                    // 3. Tạo và Cập nhật Mật khẩu Mới
                    string passnew = (model.Password.Trim() + saltValue).ToMD5();
                    taikhoan.Password = passnew;

                    _context.Update(taikhoan);
                    _context.SaveChanges();

                    _notyfService.Success("Đổi mật khẩu thành công!");
                    return RedirectToAction("Dashboard", "Accounts");
                }
                else
                {
                    // Mật khẩu hiện tại không đúng
                    _notyfService.Error("Mật khẩu hiện tại không chính xác.");
                    return RedirectToAction("Dashboard", "Accounts");
                }
            }
            catch (Exception ex)
            {
                // Ghi log ex nếu cần
                _notyfService.Error("Cập nhật mật khẩu không thành công do lỗi hệ thống.");
                return RedirectToAction("Dashboard", "Accounts");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeInfomation(ChangeInfoViewModel model)
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");

            // Nếu không tìm thấy Session ID, chuyển hướng về Login
            if (string.IsNullOrEmpty(customerIdString))
            {
                _notyfService.Error("Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.");
                return RedirectToAction("Login", "Accounts");
            }

            // 1. Validation Model
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Vui lòng nhập đầy đủ hoặc đúng định dạng thông tin.");
                // Chú ý: Nếu dùng RedirectToAction, lỗi Validation sẽ không hiển thị trên form.
                return RedirectToAction("Dashboard");
            }

            // 2. Tìm Customer
            int customerId = Convert.ToInt32(customerIdString);
            var khachhang = await _context.Customers.FindAsync(customerId);

            if (khachhang == null)
            {
                _notyfService.Error("Tài khoản không tồn tại.");
                return RedirectToAction("Dashboard");
            }

            // 3. Cập nhật thông tin từ ViewModel vào đối tượng khachhang
            khachhang.FullName = model.FullName;
            khachhang.Phone = model.Phone;
            khachhang.Address = model.Address;

            // THÊM LOGIC CẬP NHẬT LOCATION TỪ VIEWMODEL
            // Giả định các trường LocationId, District, Ward trong Customer Model là kiểu int? hoặc int
            khachhang.LocationId = model.TinhThanh;
            khachhang.District = model.QuanHuyen;
            khachhang.Ward = model.PhuongXa;


            try
            {
                // 4. Lưu thay đổi vào Database
                _context.Update(khachhang);
                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật thông tin thành công!");
            }
            catch (Exception ex)
            {
                // Ghi log ex nếu cần thiết
                _notyfService.Error("Cập nhật thất bại. Vui lòng thử lại.");
            }

            return RedirectToAction("Dashboard"); // Quay lại trang Dashboard
        }
        [HttpPost]
        public IActionResult GetDistrictList(int TinhThanhId)
        {
            // Tìm danh sách Location cấp 2 (Quận/Huyện) thuộc Tỉnh/Thành được chọn
            var lsquanhuyen = _context.Locations
                .AsNoTracking()
                // ✅ ĐÃ SỬA: So sánh int? (ParentCode) với int (TinhThanhId)
                // Dùng .Value để so sánh int với int an toàn (hoặc dùng toán tử == trực tiếp)
                .Where(x => x.ParentCode.Value == TinhThanhId)
                .OrderBy(x => x.Name)
                .ToList();

            if (lsquanhuyen.Any())
            {
                return Json(new SelectList(lsquanhuyen, "LocationId", "NameWithType"));
            }
            return Json(null);
        }

        [HttpPost]
        public IActionResult GetWardList(int QuanHuyenId)
        {
            // Tìm danh sách Location cấp 3 (Phường/Xã) thuộc Quận/Huyện được chọn
            var lsPhuongXa = _context.Locations
                .AsNoTracking()
                // ✅ ĐÃ SỬA: So sánh int? (ParentCode) với int (QuanHuyenId)
                .Where(x => x.ParentCode.Value == QuanHuyenId)
                .OrderBy(x => x.Name)
                .ToList();

            if (lsPhuongXa.Any())
            {
                return Json(new SelectList(lsPhuongXa, "LocationId", "NameWithType"));
            }
            return Json(null);
        }
    }
}
