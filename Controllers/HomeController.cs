using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NTM_SHOP.Models;
using NTM_SHOP.ModelViews;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NTM_SHOP.Models;

namespace NTM_SHOP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly OnlineShopContext _context;


        public HomeController(ILogger<HomeController> logger, OnlineShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            HomeViewVM model = new HomeViewVM();

            var lsProducts = _context.Products
                .AsNoTracking()
                .Where(x => x.Active == true && x.HomeTag == true &&x.BestSeller==true)
                .OrderByDescending(x => x.DateCreated)
                .ToList();

            List<ProductHomeVM> lsProductViews = new List<ProductHomeVM>();

            var lsCats = _context.Categories
                .AsNoTracking()
                .Where(x => x.Published == true)
                .OrderByDescending(x => x.Ordering)
                .ToList();

            foreach(var item in lsCats)
            {
                ProductHomeVM productHome = new ProductHomeVM();
                productHome.category = item;
                productHome.lsProducts = lsProducts.Where(x => x.CatId == item.CatId).ToList();
                lsProductViews.Add(productHome);
            }

            var TinTuc = _context.Posts
                .AsNoTracking()
                .Where(x => x.Published == true && x.IsNewFeed == true)
                .OrderByDescending(x => x.CreateDate)
                .Take(3)
                .ToList();

            model.Products = lsProductViews;
            model.TinTucs = TinTuc;
            ViewBag.AllProducts = lsProducts;

            return View(model);
        }
        [Route("{Alias}/{id}.html", Name = "ProductDetailsBest")]
        public IActionResult Details(int id)
        {
            try
            {

                var product = _context.Products.Include(x => x.Cat).FirstOrDefault(x => x.ProductId == id);
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                var lsProduct = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CatId == product.CatId && x.ProductId != id && x.Active == true)
                    .OrderByDescending(x => x.DateCreated)
                    .Take(4)
                    .ToList();

                ViewBag.SanPham = lsProduct;


                return View(product);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

        }
        [Route("contact.html", Name = "Contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("about.html", Name = "About")]
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
