using System.Collections.Generic;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class DanceEventStatus {
        public PlatformEvent eventDescription = null!;
        public DanceEventRules? rules;
        public int playersCount;
        public DanceEventPlayer? player;
        public DanceEventMap? score;
    }

    [PublicAPI]
    public class DanceEventRules {
        public int countedMaps;
        public float dancingLine;
        public float minCountedFraction;
    }

    [PublicAPI]
    public class DanceEventPlayer {
        public string playerId = "";
        public float points;
        public int rank;
        public int countryRank;
        public int dancingMaps;
        public IReadOnlyList<DanceEventMap> maps = new List<DanceEventMap>();
    }

    [PublicAPI]
    public class DanceEventMap {
        public int scoreId;
        public string leaderboardId = "";
        public string hash = "";
        public string difficulty = "";
        public float danceScore;
        public float points;
        public bool eligible;
        public bool dancing;
        public bool counted;
        public long timepost;
    }
}
