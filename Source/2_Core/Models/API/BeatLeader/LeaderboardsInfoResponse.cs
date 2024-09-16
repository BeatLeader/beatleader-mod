using BeatLeader.Utils;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Runtime.InteropServices;

namespace BeatLeader.Models {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public struct MassLeaderboardsInfoResponse {
        public string id;
        public SongInfo song;
        public DiffInfo difficulty;
        public QualificationInfo qualification;
        public Clan clan;
        public bool clanRankingContested;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public struct HashLeaderboardsInfoResponse {
        public SongInfo song;
        public HashLeaderboardInfo[] leaderboards;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public struct HashLeaderboardInfo {
        public string id;
        public DiffInfo difficulty;
        public QualificationInfo qualification;
        public Clan clan;
        public bool clanRankingContested;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public struct SongInfo {
        public string id;
        public string hash;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public struct DiffInfo {
        public int id;
        public int value;
        public int mode;
        public int status;
        public string modeName;
        public string difficultyName;
        public int nominatedTime;
        public int qualifiedTime;
        public int rankedTime;
        public float stars;
        public float accRating;
        public float passRating;
        public float techRating;
        public int maxScore;
        public int type;
        public Dictionary<string, float>? modifierValues;
        public Dictionary<string, float>? modifiersRating;
    }

    public struct QualificationInfo {
        public int id;
        public int timeset;
        public string rtMember;
        public int criteriaMet;
        public int criteriaTimeset;
        public string criteriaChecker;
        public string? criteriaCommentary;
        public string mapperId;
        public bool mapperQualification;
        public int approvalTimeset;
        public bool approved;
        public string approvers;
    }
}