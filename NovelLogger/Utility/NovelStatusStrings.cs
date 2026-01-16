using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NovelLogger.Utility
{
    public static class NovelStatusStrings
    {
        public const string UpToDate = "Up to Date";
        public const string DidNotFinish = "Didn't Finish";
        public const string Completed = "Completed";
        public const string DidNotLike = "Didn't Like";

        public static readonly string[] All =
        {
            UpToDate,
            DidNotFinish,
            Completed,
            DidNotLike
        };
    }
}
