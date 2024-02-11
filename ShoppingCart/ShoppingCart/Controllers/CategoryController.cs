using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart_DataAccess;
using ShoppingCart_DataAccess.Repository.IRepository;
using ShoppingCart_Models;
using ShoppingCart_Utility;

namespace ShoppingCart.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _catRepo;

        public CategoryController(ICategoryRepository catRepo)
        {
            _catRepo = catRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> catList = _catRepo.GetAll();
            return View(catList);
        }

        //GET -CREATE
        public IActionResult Create()
        {
            return View();
        }

        //GET -POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Add(category);
                _catRepo.Save();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _catRepo.Find(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Update(category);
                _catRepo.Save();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _catRepo.Find(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var category = _catRepo.Find(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }
            _catRepo.Remove(category);
            _catRepo.Save();
            return RedirectToAction("Index");
        }
    }
}
