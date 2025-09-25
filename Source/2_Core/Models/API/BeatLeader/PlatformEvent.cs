using System;

namespace BeatLeader.Models {
    public class PlatformEvent {
        public required string id;
        public required string image;
        public required string name;
        public required long endDate;
        public required bool downloadable;
        public required int playerCount;

        public required int playlistId;
        public string? description;

        public bool IsHappening() {
            return DateTime.UtcNow.ToUnixTime() < endDate;
        }

        public TimeSpan ExpiresIn() {
            return endDate.AsUnixTime() - DateTime.UtcNow;
        }
    }
}