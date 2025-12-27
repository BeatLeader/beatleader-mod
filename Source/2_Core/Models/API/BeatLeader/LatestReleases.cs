using System.Runtime.InteropServices;

namespace BeatLeader.Models {
    [StructLayout(LayoutKind.Auto)]
    internal struct LatestReleases {
        public ReleaseInfo pc;
        public ReleaseInfo quest;
    }

    internal struct ReleaseInfo {
        public string version;
        public string link;
    }
}