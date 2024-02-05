using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Data;
using ShoppingCart.Models;
using ShoppingCart_Utility;

namespace ShoppingCart.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ApplicationTypeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<ApplicationType> applicationTypeList = _db.ApplicationType;
            return View(applicationTypeList);
        }

        //GET -CREATE
        public IActionResult Create()
        {
            return View();
        }

        //GET -POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationType applicationType)
        {
            _db.ApplicationType.Add(applicationType);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var applicationType = _db.ApplicationType.Find(id);
            if (applicationType == null)
            {
                return NotFound();
            }

            return View(applicationType);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationType applicationType)
        {
            if (ModelState.IsValid)
            {
                _db.ApplicationType.Update(applicationType);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(applicationType);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var applicationType = _db.ApplicationType.Find(id);
            if (applicationType == null)
            {
                return NotFound();
            }

            return View(applicationType);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var applicationType = _db.ApplicationType.Find(id);
            if (applicationType == null)
            {
                return NotFound();
            }
            _db.ApplicationType.Remove(applicationType);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
