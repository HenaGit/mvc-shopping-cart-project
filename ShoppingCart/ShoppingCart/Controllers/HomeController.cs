using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCart.Data;
using ShoppingCart.Models;
using ShoppingCart.Models.ViewModels;
using ShoppingCart.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Products = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType),
                Categories = _db.Category
            };
            return View(homeVM);
        }
        public IActionResult Details(int id)
        {
            DetailsVM detailsVM = new DetailsVM()
            {
                Product = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType)
                .Where(u => u.Id == id).FirstOrDefault(),
                ExistsInCart = false
            };
            return View(detailsVM);
        }
        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }
            shoppingCartList.Add(new ShoppingCartModel { ProductId = id });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
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
