using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTM_SHOP.Models;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTM_SHOP.Controllers
{
    public class BlogController : Controller
    {
        private readonly OnlineShopContext _context;

        public BlogController(OnlineShopContext context)
        {
            _context = context;
      
        }

        [Route("blogs.html", Name ="Blog")]
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 6;
            var lsPosts = _context.Posts.AsNoTracking().OrderByDescending(x => x.PostId);
            PagedList<Post> models = new PagedList<Post>(lsPosts, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        [Route("/tin-tuc/{Alias}-{id}.html",Name ="tinDetails")]
        public IActionResult Details(int id)
        {
            var post = _context.Posts.AsNoTracking().SingleOrDefault(x => x.PostId == id);
            if (post == null)
            {
                return RedirectToAction("Index");
            }
            var lsLienQuan = _context.Posts.AsNoTracking()
                .Where(x => x.Published && x.PostId != id)
                .Take(3)
                .OrderByDescending(x => x.CreateDate)
                .ToList();
            ViewBag.BaiVietLienQuan = lsLienQuan;
            return View(post);
        }

    }
}
