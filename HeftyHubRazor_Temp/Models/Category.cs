using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HeftyHubRazor_Temp.Models
{
    public class Category
    {
        // this data annotation tells that this Id property will be the primary key for the vategory table.
        // also entiry framework treats the property as primary key even without [Key] annotation, if the property name is Id or Modelname followed by Id (CategoryId)
        [Key]
        public int CategoryId { get; set; }

        [Required] // to make it as not null setting
        [MaxLength(30)]
        [DisplayName("Category Name")] // the name that we want in client side or UI when it is being displayed
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1-100")]
        public int DisplayOrder { get; set; }
    }
}
