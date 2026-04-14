using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NovelLogger.Models.ViewModels
{
    public class NovelVM
    {
        [Required]
        [Display(Name = "Novel Title")]
        [MaxLength(200)]
        public string NovelTitle { get; set; } = null!;

        [Required]
        [Display(Name = "Novel Status")]
        public string NovelStatus { get; set; } = null!;

        public int NovelId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> NovelStatusList { get; set; } = null!;
    }
}
