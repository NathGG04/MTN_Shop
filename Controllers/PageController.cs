using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTM_SHOP.Models;
using System.Linq;

namespace NTM_SHOP.Controllers
{
    public class PageController : Controller
    {
        private readonly OnlineShopContext _context;

        public PageController(OnlineShopContext context)
        {
            _context = context;

        }


        [Route("/page/{Alias}", Name = "PageDetails")]
        public IActionResult Details(string Alias)
        {
            if (string.IsNullOrEmpty(Alias))
            {
                return RedirectToAction("Index", "Home");
            }


            var page = _context.Pages.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);
            if (page == null)
            {
                return RedirectToAction("Index", "Home");
            }
           
            return View(page);
        }
    }
}
