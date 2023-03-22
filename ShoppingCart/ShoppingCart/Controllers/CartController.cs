using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Data;
using ShoppingCart.Models;
using ShoppingCart.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _db.Product.Where(u => prodInCart.Contains(u.Id));

            return View(prodList);
        }
        public IActionResult Remove(int id)
        {

            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
