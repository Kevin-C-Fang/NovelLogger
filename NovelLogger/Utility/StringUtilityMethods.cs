namespace NovelLogger.Utility
{
    public static class StringUtilityMethods
    {
        public static string NormalizeTitle(string title)
        {
            return string.Join(' ', title.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
