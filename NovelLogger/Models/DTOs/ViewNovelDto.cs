using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NovelLogger.Models.DTOs
{
    public class ViewNovelDto
    {
        public string NovelTitle { get; set; } = null!;
        public string NovelStatus { get; set; } = null!;
        public int NovelId { get; set; }
    }
}
