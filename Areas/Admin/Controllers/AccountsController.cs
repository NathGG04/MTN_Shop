using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTM_SHOP.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class AccountsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
