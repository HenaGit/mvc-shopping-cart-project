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
    public class ApplicationTypeController : Controller
    {
        private readonly IApplicationTypeRepository _appTypeRepo;

        public ApplicationTypeController(IApplicationTypeRepository appTypeRepo)
        {
            _appTypeRepo = appTypeRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<ApplicationType> applicationTypeList = _appTypeRepo.GetAll();
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
            _appTypeRepo.Add(applicationType);
            _appTypeRepo.Save();
            return RedirectToAction("Index");
        }

        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var applicationType = _appTypeRepo.Find(id.GetValueOrDefault());
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
                _appTypeRepo.Update(applicationType);
                _appTypeRepo.Save();
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
            var applicationType = _appTypeRepo.Find(id.GetValueOrDefault());
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
            var applicationType = _appTypeRepo.Find(id.GetValueOrDefault());
            if (applicationType == null)
            {
                return NotFound();
            }
            _appTypeRepo.Remove(applicationType);
            _appTypeRepo.Save();
            return RedirectToAction("Index");
        }
    }
}
