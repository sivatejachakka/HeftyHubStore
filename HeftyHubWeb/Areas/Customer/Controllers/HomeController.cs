using HeftyHub.DataAccess.Repository.IRepository;
using HeftyHub.Models;
using HeftyHub.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace HeftyHubWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWorkRepository;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWorkRepository)
        {
            _logger = logger;
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public IActionResult Index()
        {
            // handled by ShoppingCartViewComponent
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //if(claim != null)
            //{
            //    HttpContext.Session.SetInt32(Constants.SESSION_CART, _unitOfWorkRepository._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == claim.Value).Count());
            //}

            IEnumerable<Product> productList = _unitOfWorkRepository._ProductRepository.GetAll(includeProperties: "Category");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWorkRepository._ProductRepository.Get(u => u.ProductId == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWorkRepository._ShoppingCartRepository.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                //cart already exists for the user with that product
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWorkRepository._ShoppingCartRepository.Update(cartFromDb);
                _unitOfWorkRepository.Save();
            }
            else
            {
                //add cart record
                _unitOfWorkRepository._ShoppingCartRepository.Add(shoppingCart);
                _unitOfWorkRepository.Save();

                HttpContext.Session.SetInt32(Constants.SESSION_CART, _unitOfWorkRepository._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId).Count());
            }

            TempData["success"] = "Cart updated successfully";

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
