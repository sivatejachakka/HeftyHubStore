using HeftyHub.DataAccess.Repository.IRepository;
using HeftyHub.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeftyHubWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        // for a file we want to create we have to appened ViewComponent to that, as that is a keyword that is needed for view component and it must
        // Inherit the ViewComponent class

        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // the name of the function will be InvokeAsync(), an d that is what will be used when we invoke the view component.
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if(HttpContext.Session.GetInt32(Constants.SESSION_CART) == null)
                {
                    HttpContext.Session.SetInt32(Constants.SESSION_CART, _unitOfWork._ShoppingCartRepository.GetAll(u => u.ApplicationUserId == claim.Value).Count());
                }
                return View(HttpContext.Session.GetInt32(Constants.SESSION_CART));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
