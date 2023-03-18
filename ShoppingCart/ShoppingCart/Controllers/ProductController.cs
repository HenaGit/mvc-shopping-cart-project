﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoppingCart.Data;
using ShoppingCart.Models;
using ShoppingCart.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductController(ApplicationDbContext db)
        {
            _db = db;
           
        }
        public IActionResult Index()
        {
            IEnumerable<Product> objList = _db.Product;

            foreach (var obj in objList)
            {
                obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
                //obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            };

            return View(objList);
        }
        //GET -UPSERT
        public IActionResult Upsert(int? id)
        {
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});
            //ViewBag.CategoryDropDown = CategoryDropDown;
            //ViewData["CategoryDropDown"] = CategoryDropDown;
            //Product product = new Product();
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            if (id == null)
            {
                //this is for create
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Product.Find(id);
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
            
        }
        //GET -UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Category.Add(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);

        }
    }
}
