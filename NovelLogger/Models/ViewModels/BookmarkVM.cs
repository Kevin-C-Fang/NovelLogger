using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using NovelLogger.Utility;

namespace NovelLogger.Models.ViewModels
{
    public class BookmarkVM
    {
        [Required]
        [Display(Name = "Novel Title")]
        [MaxLength(200)]
        public string NovelTitle { get; set; } = null!;

        [Required]
        [Url]
        [MaxLength(2048)]
        public string Url { get; set; } = null!;

        [MaxLength(2000, ErrorMessage = "The field Notes has a maximum length of 2000 characters.")]
        public string? Notes { get; set; }

        [Display(Name = "Save this bookmark")]
        public bool IsSaved { get; set; }

        public int BookmarkId { get; set; }

        [Required]
        [Display(Name = "Novel Status")]
        public string NovelStatus { get; set; } = null!;

        [ValidateNever]
        public IEnumerable<SelectListItem> NovelStatusList { get; set; } = null!;
    }
}
