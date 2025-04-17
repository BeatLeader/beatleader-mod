#nullable disable

namespace BeatLeader.Models {
    public class PlatformEvent {
        public string image;
        public string name;
        public long endDate;
        public bool downloadable;
        public int playerCount;

        public int playlistId;
        public string? description;
    }
}