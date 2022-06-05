namespace BeatLeader.Models {
    internal enum VoteStatus {
        CantVote = 1,
        CanVote = 2,
        Voted = 3,
    }

    internal readonly struct Vote {
        public readonly float Rankability;
        public readonly float StarRating;
        public readonly MapType MapType;

        public bool HasStarRating => StarRating > 0;
        public bool HasMapType => MapType != MapType.Unknown;
        
        public Vote(float rankability, float starRating = 0, MapType mapType = MapType.Unknown) {
            Rankability = rankability;
            StarRating = starRating;
            MapType = mapType;
        }
    }

    internal enum MapType {
        Unknown = 0,
        Acc = 1,
        Tech = 2,
        Midspeed = 3,
        Speed = 4
    }
}