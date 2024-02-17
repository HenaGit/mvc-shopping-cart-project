﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart_DataAccess;
using ShoppingCart_DataAccess.Repository.IRepository;
using ShoppingCart_Models;
using ShoppingCart_Models.ViewModels;
using ShoppingCart_Utility;
using ShoppingCart_Utility.BrainTree;

namespace ShoppingCart.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IProductRepository _prodRepo;
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;
        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(IWebHostEnvironment webHostEnvironment, IEmailSender emailSender,
             IApplicationUserRepository userRepo, IProductRepository prodRepo,
             IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo,
             IOrderHeaderRepository orderHRepo,
             IOrderDetailRepository orderDRepo, IBrainTreeGate brain)
        {
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _brain = brain;
            _userRepo = userRepo;
            _prodRepo = prodRepo;
            _inqDRepo = inqDRepo;
            _inqHRepo = inqHRepo;
            _orderDRepo = orderDRepo;
            _orderHRepo = orderHRepo;
        }
        public IActionResult Index()
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (
                HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count()
                    > 0
            )
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> prodList = new List<Product>();

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                prodList.Add(prodTemp);
            }

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCartModel { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }
        public IActionResult Summary()
        {
            ApplicationUser applicationUser;

            if (User.IsInRole(WC.AdminRole))
            {
                if (HttpContext.Session.Get<int>(WC.SessionInquiryId) != 0)
                {
                    //cart has been loaded using an inquiry
                    InquiryHeader inquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhoneNumber
                    };
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }
                var gateway = _brain.GetGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                applicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value);
            }
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (
                HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count()
                    > 0
            )
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser,
            };

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = _prodRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                ProductUserVM.ProductList.Add(prodTemp);
            }

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
           
            if (User.IsInRole(WC.AdminRole))
            {
                //we need to create an order
                //var orderTotal = 0.0;
                //foreach (Product prod in ProductUserVM.ProductList)
                //{
                //    orderTotal += prod.Price * prod.TempSqFt;
                //}
                OrderHeader orderHeader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = ProductUserVM.ProductList.Sum(x => x.TempSqFt * x.Price),
                    City = ProductUserVM.ApplicationUser.City,
                    StreetAddress = ProductUserVM.ApplicationUser.StreetAddress,
                    State = ProductUserVM.ApplicationUser.State,
                    PostalCode = ProductUserVM.ApplicationUser.PostalCode,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = WC.StatusPending
                };
                _orderHRepo.Add(orderHeader);
                _orderHRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerSqFt = prod.Price,
                        Sqft = prod.TempSqFt,
                        ProductId = prod.Id
                    };
                    _orderDRepo.Add(orderDetail);

                }
                _orderDRepo.Save();
                return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });
            }
            else
            {
                //var PathToTemplate =
                //    _webHostEnvironment.WebRootPath
                //    + Path.DirectorySeparatorChar.ToString()
                //    + "templates"
                //    + Path.DirectorySeparatorChar.ToString()
                //    + "Inquiry.html";

                //var subject = "New Inquiry";
                //string HtmlBody = "";
                //using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                //{
                //    HtmlBody = sr.ReadToEnd();
                //}

                //StringBuilder productListSB = new StringBuilder();
                //foreach (var prod in ProductUserVM.ProductList)
                //{
                //    productListSB.Append(
                //        $" - Name: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />"
                //    );
                //}

                //string messageBody = string.Format(
                //    HtmlBody,
                //    ProductUserVM.ApplicationUser.FullName,
                //    ProductUserVM.ApplicationUser.Email,
                //    ProductUserVM.ApplicationUser.PhoneNumber,
                //    productListSB.ToString()
                //);
                //await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

                //we need to create an inquiry
                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claim.Value,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.Now

                };

                _inqHRepo.Add(inquiryHeader);
                _inqHRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    InquiryDetail inquiryDetail = new InquiryDetail()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id,
                    };
                    _inqDRepo.Add(inquiryDetail);

                }
                _inqDRepo.Save();
                TempData[WC.Success] = "Inquiry submitted successfully";
            }
            
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation(int id = 0)
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            if (
                HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartModel>>(WC.SessionCart).Count()
                    > 0
            )
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartModel>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProdList)
        {
            List<ShoppingCartModel> shoppingCartList = new List<ShoppingCartModel>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCartModel { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
