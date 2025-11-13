using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NTM_SHOP.Models;
using PagedList.Core;
using PagedList.Core.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTM_SHOP.Controllers
{
    public class ProductController : Controller
    {
        private readonly OnlineShopContext _context;
        public ProductController(OnlineShopContext context)
        {
            _context = context;
        }

        [Route("shop.html", Name = "ShopProduct")]
        public IActionResult Index(int? page,int CatID=0)
        {
            try
            {

                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var pageSize = 8;
                var lsProducts = _context.Products.AsNoTracking().OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsProducts, pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;
                return View(models);
                ViewBag.CurrentCateID = CatID;
                ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", CatID);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CurrentCateID = CatID;
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", CatID);

        }

        [Route("/{Alias}", Name = "ListProduct")]
        public IActionResult List(string Alias,int page = 1, int CatID=0)
        {
            try
            {
                var pageSize = 10;
                var categories = _context.Categories.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);
                var lsProducts = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CatId == categories.CatId)
                    .OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsProducts, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.CurrentCategory = categories;
                ViewBag.CurrentCateID = CatID;
                ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", CatID);
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CurrentCateID = CatID;
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", CatID);

        }

        [Route("/{Alias}-{id}", Name = "ProductDetails")]
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
        public IActionResult FindProduct(string keyword)
        {
            List<Product> ls = new List<Product>();

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                return PartialView("ListProductsSearchPartial", null);
            }
            ls = _context.Products
                .AsNoTracking()
                .Include(a => a.Cat)
                .Where(x => x.ProductName.Contains(keyword))
                .OrderByDescending(x => x.ProductName)
                .Take(10)
                .ToList();
            if (ls == null)
            {
                return PartialView("ListProductsSearchPartial", null);
            }
            else
            {
                return PartialView("ListProductsSearchPartial", ls);
            }
        }
        public IActionResult Filtter(int CatID = 0)
        {
            var url = $"/Product?CatID={CatID}";
            if (CatID == 0)
            {
                url = $"/Product";
            }

            return Json(new { status = "success", redirectUrl = url });
        }
    }
}
