using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NovelLogger.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public DateTime DateAdded { get; set; }
        public string Notes { get; set; }
        public bool IsSaved { get; set; }
    }
}
