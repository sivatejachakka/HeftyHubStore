using HeftyHub.DataAccess.Repository.IRepository;
using HeftyHub.Models;
using HeftyHub.Models.ViewModels;
using HeftyHub.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace HeftyHubWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private IUnitOfWork _unitOfWork;

        [BindProperty]
        public List<OrderHeader> OrderHeaderList { get; set; }

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string status = "all")
        {
            if (User.IsInRole(Constants.ROLE_USER_ADMIN) || User.IsInRole(Constants.ROLE_USER_EMPLOYEE))
            {
                OrderHeaderList = _unitOfWork._OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                OrderHeaderList = _unitOfWork._OrderHeaderRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
            }

                switch (status)
            { 
                case "pending":
                    OrderHeaderList = OrderHeaderList.Where(u => u.PaymentStatus == Constants.PAYMENT_STATUS_DELAYED_PAYMENT).ToList();
                    break;
                case "inprocess":
                    OrderHeaderList = OrderHeaderList.Where(u => u.OrderStatus == Constants.STATUS_INPROCESS).ToList();
                    break;
                case "completed":
                    OrderHeaderList = OrderHeaderList.Where(u => u.OrderStatus == Constants.STATUS_SHIPPED).ToList();
                    break;
                case "approved":
                    OrderHeaderList = OrderHeaderList.Where(u => u.OrderStatus == Constants.STATUS_APPROVED).ToList();
                    break;
                default:
                    break;
            }

            return View(OrderHeaderList);
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork._OrderHeaderRepository.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork._OrderDetailRepository.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        [ActionName("Details")]
        public IActionResult Details_Pay_Now()
        {
            OrderVM.OrderHeader = _unitOfWork._OrderHeaderRepository.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork._OrderDetailRepository.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            // stripe logic
            var domain = "https://localhost:7001/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
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
            _unitOfWork._OrderHeaderRepository.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303); // redirect to strip page for payment
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork._OrderHeaderRepository.Get(u => u.Id == orderHeaderId);
            if (orderHeader != null && orderHeader.PaymentStatus == Constants.PAYMENT_STATUS_DELAYED_PAYMENT)
            {
                //this is an order by company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session != null && session.PaymentStatus.ToLower() == "paid")
                {
                    // as the payment is done now we will have the PaymentIntentId
                    _unitOfWork._OrderHeaderRepository.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork._OrderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, Constants.PAYMENT_STATUS_APPROVED);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

        [HttpPost]
        [Authorize(Roles = Constants.ROLE_USER_ADMIN + "," + Constants.ROLE_USER_EMPLOYEE)]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDb = _unitOfWork._OrderHeaderRepository.Get(u => u.Id == OrderVM.OrderHeader.Id);

            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork._OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = Constants.ROLE_USER_ADMIN + "," + Constants.ROLE_USER_EMPLOYEE)]
        public IActionResult StartProcessing()
        {
            _unitOfWork._OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, Constants.STATUS_INPROCESS);
            _unitOfWork.Save();

            TempData["success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = Constants.ROLE_USER_ADMIN + "," + Constants.ROLE_USER_EMPLOYEE)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork._OrderHeaderRepository.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.OrderStatus = Constants.STATUS_SHIPPED;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            if(orderHeaderFromDb.PaymentStatus == Constants.PAYMENT_STATUS_DELAYED_PAYMENT)
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork._OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Shipped Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = Constants.ROLE_USER_ADMIN + "," + Constants.ROLE_USER_EMPLOYEE)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork._OrderHeaderRepository.Get(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb.PaymentStatus == Constants.PAYMENT_STATUS_APPROVED)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId,
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork._OrderHeaderRepository.UpdateStatus(orderHeaderFromDb.Id, Constants.STATUS_CANCELLED, Constants.STATUS_REFUNDED);
            }
            else
            {
                _unitOfWork._OrderHeaderRepository.UpdateStatus(orderHeaderFromDb.Id, Constants.STATUS_CANCELLED, Constants.STATUS_CANCELLED);
            }
            _unitOfWork.Save();

            TempData["success"] = "Order Cancelled Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
    }
}
