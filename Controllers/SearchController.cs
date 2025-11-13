using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTM_SHOP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTM_SHOP.Controllers
{
    public class SearchController : Controller
    {
        private readonly OnlineShopContext _context;
        public SearchController(OnlineShopContext context)
        {
            _context = context;
        }


        [HttpPost]
        public IActionResult FindProduct(string keyword)
        {
            List<Product> ls = new List<Product>();

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                return PartialView("_ListProductsSearchPartial", null);
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
                return PartialView("_ListProductsSearchPartial", null);
                return PartialView("ListProductsSearchPartial", null);
            }
            else
            {
                return PartialView("_ListProductsSearchPartial", ls);
                return PartialView("ListProductsSearchPartial", ls);
            }
        }
    }
}
