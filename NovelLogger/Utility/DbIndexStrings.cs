namespace NovelLogger.Utility
{
    public static class DbIndexStrings
    {
        public const string NovelUniqueIndex = "IX_Novels_UserId_TitleNormalized";
        public const string BookmarkUniqueIndex = "IX_Bookmarks_UserId_NovelId_Url";
    }
}
