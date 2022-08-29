namespace BeatLeader.Models {
    internal struct MassLeaderboardsInfoResponse {
        public string id;
        public SongInfo song;
        public DiffInfo difficulty;
        public QualificationInfo qualification;
    }

    internal struct HashLeaderboardsInfoResponse {
        public SongInfo song;
        public HashLeaderboardInfo[] leaderboards;
    }

    internal struct HashLeaderboardInfo {
        public string id;
        public DiffInfo difficulty;
        public QualificationInfo qualification;
    }

    internal struct SongInfo {
        public string id;
        public string hash;
    }

    internal struct DiffInfo {
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
        public int type;
        public ModifiersMap modifierValues;
    }

    internal struct QualificationInfo {
        public int id;
        public int timeset;
        public string rtMember;
        public int criteriaMet;
        public int criteriaTimeset;
        public string criteriaChecker;
        public string criteriaCommentary;
        public bool mapperAllowed;
        public string mapperId;
        public bool mapperQualification;
        public int approvalTimeset;
        public bool approved;
        public string approvers;
    }

    internal struct ModifiersMap {
        public int modifierId;
        public float da;
        public float fs;
        public float ss;
        public float sf;
        public float gn;
        public float na;
        public float nb;
        public float nf;
        public float no;
        public float pm;
        public float sc;
    }
}