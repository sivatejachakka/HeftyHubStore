using HeftyHub.DataAccess.Data;
using HeftyHub.DataAccess.Repository.IRepository;
using HeftyHub.Models;
using HeftyHub.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HeftyHubWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constants.ROLE_USER_ADMIN)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWorkRepository;

        public CategoryController(IUnitOfWork unitOfWorkRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWorkRepository._CategoryRepository.GetAll().ToList();
            return View(objCategoryList);
        }

        public IActionResult CategoryCreateUpdate(int? id)
        {
            Category? category = null;
            if (id != null)
            {
                category = _unitOfWorkRepository._CategoryRepository.Get(u => u.CategoryId == id); ;

                if (category == null)
                {
                    return NotFound();
                }
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult CategoryCreateUpdate(Category category)
        {
            if (!category.Name.IsNullOrEmpty() && category.Name.Any(c => char.IsDigit(c)))
            {
                if (category.Name.All(char.IsDigit))
                {
                    ModelState.AddModelError("Name", "The Category Name field should not be a number.");
                }
                ModelState.AddModelError("Name", "The Category Name field should not contain digits.");
            }
            if (ModelState.IsValid)
            {
                if (category.CategoryId == 0)
                {
                    _unitOfWorkRepository._CategoryRepository.Add(category);
                    TempData["success"] = "Category Created Successfully.";
                }
                else
                {
                    _unitOfWorkRepository._CategoryRepository.Update(category);
                    TempData["success"] = "Category Updated Successfully.";
                }
                _unitOfWorkRepository.Save();

                // to redirect it to another controller we have to pass the controller name as 2nd parameter RedirectToAction("ActionName", "ControllerName);
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult DeleteCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? category = _unitOfWorkRepository._CategoryRepository.Get(u => u.CategoryId == id); ;

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // On Click of delete it will search with the name of the action method with name same as the view, here as we already have that method, so the same
        // page will be displayed. So to delete we added a method with signature 'Post' and its ActionName defined as the View name which is 'DeleteCategory'.
        [HttpPost, ActionName("DeleteCategory")]
        public IActionResult Delete(int? id)
        {
            Category category = _unitOfWorkRepository._CategoryRepository.Get(u => u.CategoryId == id); ;

            if (category == null)
            {
                return NotFound(); ;
            }

            _unitOfWorkRepository._CategoryRepository.Remove(category);
            _unitOfWorkRepository.Save();

            TempData["success"] = "Category Deleted Successfully.";

            return RedirectToAction("Index");
        }
    }
}
