using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BE_25DH190065.Models;
using BE_25DH190065.Models.ViewModel;

namespace BE_25DH190065.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            var stats = db.Categories.Select(c => new StatisticsVM
            {
                CategoryName = c.CategoryName,
                ProductCount = c.Products.Count(),
                HighestPrice = c.Products.Any() ? c.Products.Max(p => p.ProductPrice) : 0,
                LowestPrice = c.Products.Any() ? c.Products.Min(p => p.ProductPrice) : 0,
                AveragePrice = c.Products.Any() ? c.Products.Average(p => p.ProductPrice) : 0
            }).ToList();

            return View(stats);
        }
    }
}