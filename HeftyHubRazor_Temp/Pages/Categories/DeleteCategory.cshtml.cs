using HeftyHubRazor_Temp.Data;
using HeftyHubRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HeftyHubRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class DeleteCategoryModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public Category Category { get; set; }

        public DeleteCategoryModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult OnGet(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category = _db.tblCategory.Find(id);

            if (Category == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            Category = _db.tblCategory.Find(id);

            if (Category == null)
            {
                return NotFound(); ;
            }

            _db.tblCategory.Remove(Category);
            _db.SaveChanges();

            TempData["success"] = "Category Deleted Successfully.";

            return RedirectToPage("Index");
        }
    }
}
