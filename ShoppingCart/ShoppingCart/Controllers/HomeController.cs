using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCart_DataAccess;
using ShoppingCart_DataAccess.Repository.IRepository;
using ShoppingCart_Models;
using ShoppingCart_Models.ViewModels;
using ShoppingCart_Utility;
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
        private readonly IProductRepository _prodRepo;
        private readonly ICategoryRepository _catRepo;

        public HomeController(ILogger<HomeController> logger, IProductRepository prodRepo,
            ICategoryRepository catRepo)
        {
            _logger = logger;
            _prodRepo = prodRepo;
            _catRepo = catRepo;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Products = _prodRepo.GetAll(includeProperties: "Category,ApplicationType"),
                Categories = _catRepo.GetAll()
            };
            return View(homeVM);
        }
        public IActionResult Details(int id)
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }
            DetailsVM detailsVM = new DetailsVM()
            {
                Product = _prodRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Category,ApplicationType"),
                ExistsInCart = false
            };
            foreach (var item in shoppingCartList)
            {
                if (item.ProductId == id)
                {
                    detailsVM.ExistsInCart = true;
                }
            }
            return View(detailsVM);
        }
        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id, DetailsVM detailsVM)
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }
            shoppingCartList.Add(new ShoppingCartModel { ProductId = id, SqFt = detailsVM.Product.TempSqFt });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            TempData[WC.Success] = "Item added to cart successfully";
            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);
            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            TempData[WC.Success] = "Item removed from cart successfully";
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
