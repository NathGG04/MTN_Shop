using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NTM_SHOP.Extension;
using NTM_SHOP.Models;
using NTM_SHOP.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTM_SHOP.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly OnlineShopContext _context;
        public INotyfService _notyfService { get; }
        public ShoppingCartController(OnlineShopContext context, INotyfService notifyfService)
        {
            _context = context;
            _notyfService = notifyfService;

        }

        public List<CartItem> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if ( gh == default(List<CartItem>))
                {
                    gh = new List<CartItem>();
                }
                return gh;
            }
        }

        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(int productID, int? quantity)
        {
            List<CartItem> cart = GioHang;
            // thêm sản phảm vào giỏ hàng
            try
            {
                CartItem item = cart.SingleOrDefault(p => p.product.ProductId == productID);
                if (item != null)
                {
                    item.quantity = item.quantity + quantity.Value;
                    //Lưu lại session
                    HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                }
                else
                {
                    Product hh = _context.Products.SingleOrDefault(p => p.ProductId == productID);
                    item = new CartItem
                    {
                        quantity = quantity.HasValue ? quantity.Value : 1,
                        product = hh
                    };
                    cart.Add(item); // thêm vào giỏ
                }
                // lưu lại session
                HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                _notyfService.Success("Sản phẩm đã được thêm vào giỏ hàng");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
          
        }

        [HttpPost]
        [Route("api/cart/update")]
        public IActionResult UpdateCart (int productID, int? quantity)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            try
            {
                if(cart != null )
                {
                    CartItem item = cart.SingleOrDefault(p => p.product.ProductId == productID);
                    if(item != null && quantity.HasValue)
                    {
                        item.quantity = quantity.Value;
                    }
                    // Lưu lại session
                    HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }



        [HttpPost]
        [Route("api/cart/remove")]
        public IActionResult Remove(int productID)
        {
            try
            {
                List<CartItem> gioHang = GioHang;
                CartItem item = gioHang.SingleOrDefault(p => p.product.ProductId == productID);
                if (item != null)
                {
                    gioHang.Remove(item);
                }
                // lưu lại session
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
            
        }


        [Route("cart.html",Name = "Cart")]
        public IActionResult Index()
        {          
            return View(GioHang);
        }
    }
}
