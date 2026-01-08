using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace NovelLogger.Models
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

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Display(Name = "Save this bookmark")]
        public bool IsSaved { get; set; }

        public int BookmarkId { get; set; }
    }
}
