namespace BeatLeader.Models {
    public class PlayerScoreStats {
        public long totalScore;
        public long totalRankedScore;
        public float averageAccuracy;
        public float medianAccuracy;
        public float averageRankedAccuracy;
        public float medianRankedAccuracy;
        public float averageWeightedRankedAccuracy;
        public float averageRank;
        public float topAccuracy;
        public float topPp;
        public int peakRank;
        public int rankedPlayCount;
        public int totalPlayCount;
        public int top1Count;
        public float averageWeightedRankedRank;
        public int sspPlays;
        public int ssPlays;
        public int spPlays;
        public int sPlays;
        public int aPlays;
        public int topHMD;
        public string? topPlatform;
        public int replaysWatched;
        public int watchedReplays;
        public long firstScoreTime;
        public long lastScoreTime;
    }
    
    public class PlayerBadge {
        public int id;
        public string description = "";
        public string image = "";
        public string? link;
    }
    
    public class PlayerFollowersInfo {
        public int followingCount;
        public int followersCount;
        public string[] following = [];
        public string[] followers = [];
    }
    
    public class PlayerFollower {
        public string id = "";
        public string name = "";
        public string? avatar;
        public int count;
        public bool mutual;
    }
}
