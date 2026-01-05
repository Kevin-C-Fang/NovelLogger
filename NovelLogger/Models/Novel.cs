using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovelLogger.Models
{
    public class Novel
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(450)]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required, MaxLength(200)]
        public string TitleNormalized { get; set; } = null!;
    }
}
