using System;
using System.Runtime.InteropServices;

namespace BeatLeader.Models {
    internal enum VoteStatus {
        CantVote = 1,
        CanVote = 2,
        Voted = 3,
    }

    [StructLayout(LayoutKind.Auto)]
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

    [Flags]
    internal enum MapType {
        Unknown = 0,
        Acc = 1,
        Tech = 2,
        Midspeed = 4,
        Speed = 8
    }
}