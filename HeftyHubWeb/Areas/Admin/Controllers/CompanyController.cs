using HeftyHub.DataAccess.Data;
using HeftyHub.DataAccess.Repository.IRepository;
using HeftyHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using HeftyHub.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using HeftyHub.Utility;

namespace HeftyHubWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constants.ROLE_USER_ADMIN)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWorkRepository;
        public CompanyController(IUnitOfWork unitOfWorkRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWorkRepository._CompanyRepository.GetAll().ToList();
            return View(objCompanyList);
        }

        public IActionResult CompanyCreateUpdate(int? id)
        {
            Company? company = null;
            if (id != null)
            {
                company = _unitOfWorkRepository._CompanyRepository.Get(u => u.CompanyId == id); ;

                if (company == null)
                {
                    return NotFound();
                }
            }
            return View(company);
        }

        // As we are using ProjectVM so the combined model will be returned here where we have POST request, as the view has CompanyVM model.
        [HttpPost]
        public IActionResult CompanyCreateUpdate(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.CompanyId == 0)
                {
                    _unitOfWorkRepository._CompanyRepository.Add(company);
                    TempData["success"] = "Company Created Successfully.";
                }
                else
                {
                    _unitOfWorkRepository._CompanyRepository.Update(company);
                    TempData["success"] = "Company Updated Successfully.";
                }
                _unitOfWorkRepository.Save();

                // to redirect it to another controller we have to pass the controller name as 2nd parameter RedirectToAction("ActionName", "ControllerName);
                return RedirectToAction("Index");
            }
            return View(company);
        }

        public IActionResult DeleteCompany(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Company? company = _unitOfWorkRepository._CompanyRepository.Get(u => u.CompanyId == id);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // On Click of delete it will search with the name of the action method with name same as the view, here as we already have that method, so the same
        // page will be displayed. So to delete we added a method with signature 'Post' and its ActionName defined as the View name which is 'DeleteCompany'.
        [HttpPost, ActionName("DeleteCompany")]
        public IActionResult Delete(int? id)
        {
            Company company = _unitOfWorkRepository._CompanyRepository.Get(u => u.CompanyId == id); ;

            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWorkRepository._CompanyRepository.Remove(company);
            _unitOfWorkRepository.Save();

            TempData["success"] = "Company Deleted Successfully.";

            return RedirectToAction("Index");
        }

        //#region API CALLS

        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    List<Company> objCompanyList = _unitOfWorkRepository._CompanyRepository.GetAll(includeProperties: "Category").ToList();
        //    return Json(new {data = objCompanyList});
        //}

        //#endregion
    }
}
