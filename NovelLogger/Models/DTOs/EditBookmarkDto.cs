using NovelLogger.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovelLogger.Models.DTOs
{
    public class EditBookmarkDto
    {
        public int BookmarkId { get; set; }
        public string NovelTitle { get; set; } = null!;
        public string NovelStatus { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string? Notes { get; set; }
        public bool IsSaved { get; set; }
    }
}
