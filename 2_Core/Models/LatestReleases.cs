namespace BeatLeader.Models {
    internal struct LatestReleases {
        public ReleaseInfo pc;
        public ReleaseInfo quest;
    }

    internal struct ReleaseInfo {
        public string version;
        public string link;
    }
}