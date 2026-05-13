using System.ComponentModel.DataAnnotations;
using WRR.Admin.Extension;

namespace WRR.Admin.Models
{
    public class UploadImageViewModel
    {
        [Required]
        [AllowedExtensions(new string[] {".jpg", ".jpeg", ".png"})]
        public IFormFile Image { get; set; }

        public int Id { get; set; }
    }
}
