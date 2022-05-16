using System.ComponentModel.DataAnnotations;

namespace VeryBigShoeCompany.Models
{
    public class UploadFileModel : ResponseModel
    {
        [Required(ErrorMessage = "Please enter file name")]
        public string FileName { get; set; }
        [Required(ErrorMessage = "Please select file")]
        public IFormFile File { get; set; }

        public string ValidationMessage { get; set; }

        public BigShoeDataImport Orders { get; set; }
    }
}
