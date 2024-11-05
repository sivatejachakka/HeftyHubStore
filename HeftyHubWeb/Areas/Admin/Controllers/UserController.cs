using HeftyHub.DataAccess.Data;
using HeftyHub.DataAccess.Repository;
using HeftyHub.DataAccess.Repository.IRepository;
using HeftyHub.Models;
using HeftyHub.Models.ViewModels;
using HeftyHub.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HeftyHubWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constants.ROLE_USER_ADMIN)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            List<ApplicationUser> objUserList = _unitOfWork._ApplicationUserRepository.GetAll(includeProperties: "Company").ToList(); ;

            foreach (var user in objUserList)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

                if (user.Company == null)
                {
                    user.Company = new Company() {
                        Name = ""
                    };
                }
            }

            return View(objUserList);
        }

        public IActionResult RoleManagement(string userId)
        {

            RoleManagementVM RoleVM = new RoleManagementVM()
            {
                ApplicationUser = _unitOfWork._ApplicationUserRepository.Get(u => u.Id == userId, includeProperties: "Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork._CompanyRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.CompanyId.ToString()
                }),
            };
            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork._ApplicationUserRepository.Get(u => u.Id == userId))
                    .GetAwaiter().GetResult().FirstOrDefault();
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            string oldRole = _userManager.GetRolesAsync(_unitOfWork._ApplicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id))
            .GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork._ApplicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);


            if (!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                //a role was updated
                if (roleManagementVM.ApplicationUser.Role == Constants.ROLE_USER_COMPANY)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if (oldRole == Constants.ROLE_USER_COMPANY)
                {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork._ApplicationUserRepository.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }
            else
            {
                if (oldRole == Constants.ROLE_USER_COMPANY && applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    _unitOfWork._ApplicationUserRepository.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult LockUnlock(string id)
        {
            bool isLocked;
            var userFromDb = _unitOfWork._ApplicationUserRepository.Get(u => u.Id == id);

            if(userFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if(userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
            {
                // user is currently locked and we need to unlock them
                userFromDb.LockoutEnd = DateTime.Now;
                isLocked = false;
            }
            else
            {
                userFromDb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _unitOfWork._ApplicationUserRepository.Update(userFromDb);
            _unitOfWork.Save();

            TempData["success"] = "User " + (isLocked ? "Locked" : "Unlocked") + " Successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
