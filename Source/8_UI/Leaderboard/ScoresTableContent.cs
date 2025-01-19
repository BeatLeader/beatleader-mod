using System.Collections.Generic;

namespace BeatLeader {
    public class ScoresTableContent {
        public readonly IScoreRowContent? ExtraRowContent;
        public readonly IReadOnlyList<IScoreRowContent> MainRowContents;
        public readonly int CurrentPage;
        public readonly int PagesCount;
        public readonly bool ForceClanTags;
        public readonly bool SeekAvailable;
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < PagesCount;

        public ScoresTableContent(IScoreRowContent? extraRowContent, IReadOnlyList<IScoreRowContent> mainRowContents, int currentPage, int pagesCount, bool forceClanTags, bool seekAvailable) {
            ExtraRowContent = extraRowContent;
            MainRowContents = mainRowContents;
            CurrentPage = currentPage;
            PagesCount = pagesCount;
            ForceClanTags = forceClanTags;
            SeekAvailable = seekAvailable;
        }
    }
}