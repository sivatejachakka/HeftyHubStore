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
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWorkRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWorkRepository, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWorkRepository._ProductRepository.GetAll(includeProperties:"Category").ToList();
            return View(objProductList);
        }

        public IActionResult ProductCreateUpdate(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWorkRepository._CategoryRepository
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString()
                });

            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList;

            ProductVM productVM = new()
            {
                CategoryList = CategoryList
            };

            if (id != null)
            {
                productVM.Product = _unitOfWorkRepository._ProductRepository.Get(u => u.ProductId == id); ;

                if (productVM.Product == null)
                {
                    return NotFound();
                }
            }

            return View(productVM);
        }

        // As we are using ProjectVM so the combined model will be returned here where we have POST request, as the view has ProductVM model.
        [HttpPost]
        public IActionResult ProductCreateUpdate(ProductVM obj, IFormFile? imgFile)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (imgFile != null) {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imgFile.FileName);
                    string productPath = Path.Combine(wwwRootPath, "images\\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create)) {
                        imgFile.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (obj.Product.ProductId == 0)
                {
                    _unitOfWorkRepository._ProductRepository.Add(obj.Product);
                    TempData["success"] = "Product Created Successfully.";
                }
                else
                {
                    _unitOfWorkRepository._ProductRepository.Update(obj.Product);
                    TempData["success"] = "Product Updated Successfully.";
                }
                _unitOfWorkRepository.Save();

                // to redirect it to another controller we have to pass the controller name as 2nd parameter RedirectToAction("ActionName", "ControllerName);
                return RedirectToAction("Index");
            }
            else
            {
                obj.CategoryList = _unitOfWorkRepository._CategoryRepository
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString()
                });
                return View(obj);
            }
        }

        public IActionResult DeleteProduct(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? product = _unitOfWorkRepository._ProductRepository.Get(u => u.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            IEnumerable<SelectListItem> CategoryList = _unitOfWorkRepository._CategoryRepository
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString()
                });

            ProductVM productVM = new()
            {
                Product = product,
                CategoryList = CategoryList,
            };

            return View(productVM);
        }

        // On Click of delete it will search with the name of the action method with name same as the view, here as we already have that method, so the same
        // page will be displayed. So to delete we added a method with signature 'Post' and its ActionName defined as the View name which is 'DeleteProduct'.
        [HttpPost, ActionName("DeleteProduct")]
        public IActionResult Delete(int? id)
        {
            Product product = _unitOfWorkRepository._ProductRepository.Get(u => u.ProductId == id); ;

            if (product == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            // delete the old image
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWorkRepository._ProductRepository.Remove(product);
            _unitOfWorkRepository.Save();

            TempData["success"] = "Product Deleted Successfully.";

            return RedirectToAction("Index");
        }

        //#region API CALLS

        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    List<Product> objProductList = _unitOfWorkRepository._ProductRepository.GetAll(includeProperties: "Category").ToList();
        //    return Json(new {data = objProductList});
        //}

        //#endregion
    }
}
