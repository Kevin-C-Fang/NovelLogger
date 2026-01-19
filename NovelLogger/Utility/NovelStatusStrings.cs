using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using NovelLogger.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace NovelLogger.Utility
{
    public static class NovelStatusStrings
    {
        public const string UpToDate = "Up to Date";
        public const string DidNotFinish = "Didn't Finish";
        public const string Completed = "Completed";
        public const string DidNotLike = "Didn't Like";

        private static readonly IEnumerable<SelectListItem> _statusOptions =
            new List<SelectListItem>
            {
                new(UpToDate, UpToDate),
                new(DidNotFinish, DidNotFinish),
                new(Completed, Completed),
                new(DidNotLike, DidNotLike)
            };

        public static IEnumerable<SelectListItem> StatusOptions => _statusOptions;
    }
}
