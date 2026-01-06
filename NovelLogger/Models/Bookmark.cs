using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovelLogger.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(450)]
        public string UserId { get; set; } = null!;

        public int NovelId { get; set; }

        [ForeignKey(nameof(NovelId))]
        public Novel Novel { get; set; } = null!;

        [Required, MaxLength(2048)]
        public string Url { get; set; } = null!;

        [MaxLength(2000)]
        public string? Notes { get; set; }
        public bool IsSaved { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}
