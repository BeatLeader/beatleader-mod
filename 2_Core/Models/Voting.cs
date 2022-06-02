namespace BeatLeader.Models {
    public enum VoteStatus {
        CantVote = 1,
        CanVote = 2,
        Voted = 3,
    }

    public readonly struct Vote {
        public readonly float Rankability;
        public readonly float StarRating;
        public readonly MapType MapType;

        public Vote(float rankability, float starRating, MapType mapType) {
            Rankability = rankability;
            StarRating = starRating;
            MapType = mapType;
        }
    }

    public enum MapType {
        Unknown = 0,
        Acc = 1,
        Tech = 2,
        Midspeed = 3,
        Stamina = 4
    }
}