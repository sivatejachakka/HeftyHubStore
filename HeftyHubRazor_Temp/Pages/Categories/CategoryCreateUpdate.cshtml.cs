using HeftyHubRazor_Temp.Data;
using HeftyHubRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace HeftyHubRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class CategoryCreateUpdateModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        // we need this to make sure that the property is available for posting it. So it will bind the property and when we post that, it will automatically bind it.
        // or we can use BindProperties at PageModel level
        //[BindProperty]
        public Category Category { get; set; }

        public CategoryCreateUpdateModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult OnGet(int? id)
        {
            if (id != null)
            {
                Category = _db.tblCategory.Find(id);

                if (Category == null)
                {
                    return NotFound();
                }
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (Category != null && !Category.Name.IsNullOrEmpty() && Category.Name.Any(c => char.IsDigit(c)))
            {
                if (Category.Name.All(char.IsDigit))
                {
                    ModelState.AddModelError("Category.Name", "The Category Name field should not be a number.");
                }
                ModelState.AddModelError("Category.Name", "The Category Name field should not contain digits.");
            }
            if (ModelState.IsValid)
            {
                if (Category.CategoryId == 0)
                {
                    _db.tblCategory.Add(Category);
                    TempData["success"] = "Category Created Successfully.";
                }
                else
                {
                    _db.tblCategory.Update(Category);
                    TempData["success"] = "Category Updated Successfully.";
                }
                _db.SaveChanges();

                // to redirect it to another controller we have to pass the controller name as 2nd parameter RedirectToAction("ActionName", "ControllerName);
                // in case of MVC we use RedirectToAction() but for Razor pages we use RedirectToPage
                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
