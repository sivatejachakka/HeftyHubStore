using HeftyHub.DataAccess.Repository.IRepository;
using HeftyHub.Models;
using HeftyHub.Models.ViewModels;
using HeftyHub.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Stripe.Checkout;

namespace HeftyHubWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // basically what will happen is when the details will be populated in the view, and when hitting the submit button this variable will
        // automatically be populated with those values, so on submitting we need to write the parameters in SummaryPOST Action method.
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork._ApplicationUserRepository.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost, ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (!ValidateOrderHeaderFieldsOnPlacingOrder())
            {
                return RedirectToAction(nameof(Summary));
            }

            ShoppingCartVM.ShoppingCartList = _unitOfWork._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork._ApplicationUserRepository.Get(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // it is a regular customer
                ShoppingCartVM.OrderHeader.PaymentStatus = Constants.PAYMENT_STATUS_PENDING;
                ShoppingCartVM.OrderHeader.OrderStatus = Constants.STATUS_PENDING;
            }
            else
            {
                // it is a company user
                ShoppingCartVM.OrderHeader.PaymentStatus = Constants.PAYMENT_STATUS_DELAYED_PAYMENT;
                ShoppingCartVM.OrderHeader.OrderStatus = Constants.STATUS_APPROVED;
            }

            _unitOfWork._OrderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Count = cart.Count,
                    Price = cart.Price
                };
                _unitOfWork._OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // it is a regular customer account and we need to capture payment
                // stripe logic
                var domain = "https://localhost:7001/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach(var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                // here in session the paymentIntentId is null as the payment is not done. On successful payment it will get populate the PaymentIntentId
                _unitOfWork._OrderHeaderRepository.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303); // redirect to strip page for payment
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork._OrderHeaderRepository.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader != null && orderHeader.PaymentStatus != Constants.PAYMENT_STATUS_DELAYED_PAYMENT)
            {
                //this is an order by customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session != null && session.PaymentStatus.ToLower() == "paid")
                {
                    // as the payment is done now we will have the PaymentIntentId
                    _unitOfWork._OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork._OrderHeaderRepository.UpdateStatus(id, Constants.STATUS_APPROVED, Constants.PAYMENT_STATUS_APPROVED);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCartItems = _unitOfWork._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            
            _unitOfWork._ShoppingCartRepository.RemoveRange(shoppingCartItems);
            _unitOfWork.Save();

            HttpContext.Session.Clear();

            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb =_unitOfWork._ShoppingCartRepository.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork._ShoppingCartRepository.Update(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork._ShoppingCartRepository.Get(u => u.Id == cartId, tracked: true);
            if (cartFromDb.Count <= 1)
            {
                //remove that from cart
                HttpContext.Session.SetInt32(Constants.SESSION_CART, _unitOfWork._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

                _unitOfWork._ShoppingCartRepository.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork._ShoppingCartRepository.Update(cartFromDb);
            }
            
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            // here when we are getting the cart from DB by default we assigned tracking as false, we are not tracking that entity, when we use remove below,
            // that entity is no longer tracked by EFCore  and because of that we will get an error message. To solve that we have to set the track property to be true.
            var cartFromDb = _unitOfWork._ShoppingCartRepository.Get(u => u.Id == cartId, tracked: true);

            HttpContext.Session.SetInt32(Constants.SESSION_CART, _unitOfWork._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

            _unitOfWork._ShoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if(shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                //shoppingCart.Count > 100
                return shoppingCart.Product.Price100;
            }
        }

        private bool ValidateOrderHeaderFieldsOnPlacingOrder()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.Name))
            {
                ModelState.AddModelError("ShoppingCartVM.OrderHeader.Name", "The Name field should not be empty.");
                isValid = false;
            }
            if (string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.PhoneNumber))
            {
                ModelState.AddModelError("ShoppingCartVM.OrderHeader.PhoneNumber", "The Phone Number field should not be empty.");
                isValid = false;
            }
            if (string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.StreetAddress))
            {
                ModelState.AddModelError("ShoppingCartVM.OrderHeader.StreetAddress", "The Address field should not be empty.");
                isValid = false;
            }
            if (string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.City))
            {
                ModelState.AddModelError("ShoppingCartVM.OrderHeader.City", "The City field should not be empty.");
                isValid = false;
            }
            if (string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.State))
            {
                ModelState.AddModelError("ShoppingCartVM.OrderHeader.State", "The State field should not be empty.");
                isValid = false;
            }
            if (string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.PostalCode))
            {
                ModelState.AddModelError("ShoppingCartVM.OrderHeader.PostalCode", "The Postal Code field should not be empty.");
                isValid = false;
            }

            return isValid;
        }
    }
}
