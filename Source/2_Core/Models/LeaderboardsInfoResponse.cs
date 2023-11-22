using BeatLeader.Utils;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct MassLeaderboardsInfoResponse {
        public string id;
        public SongInfo song;
        public DiffInfo difficulty;
        public QualificationInfo qualification;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct HashLeaderboardsInfoResponse {
        public SongInfo song;
        public HashLeaderboardInfo[] leaderboards;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct HashLeaderboardInfo {
        public string id;
        public DiffInfo difficulty;
        public QualificationInfo qualification;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct SongInfo {
        public string id;
        public string hash;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
        public float accRating;
        public float passRating;
        public float techRating;
        public int maxScore;
        public int type;
        public ModifiersMap modifierValues;
        public ModifiersRating? modifiersRating;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal class ModifiersRating {
        public float fsPassRating;
        public float fsAccRating;
        public float fsTechRating;
        public float fsStars;

        public float ssPassRating;
        public float ssAccRating;
        public float ssTechRating;
        public float ssStars;

        public float sfPassRating;
        public float sfAccRating;
        public float sfTechRating;
        public float sfStars;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct QualificationInfo {
        public int id;
        public int timeset;
        public string rtMember;
        public int criteriaMet;
        public int criteriaTimeset;
        public string criteriaChecker;
        public string criteriaCommentary;
        public string mapperId;
        public bool mapperQualification;
        public int approvalTimeset;
        public bool approved;
        public string approvers;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public struct ModifiersMap {
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

        //TODO: rework
        public float GetModifierValueByModifierServerName(string name) {
            return (float)(typeof(ModifiersMap).GetField(name.ToLower(), ReflectionUtils.DefaultFlags)?.GetValue(this) ?? -1f);
        }

        public void LoadFromGameModifiersParams(IEnumerable<GameplayModifierParamsSO> modifiersParams) {
            foreach (var item in modifiersParams) {
                var modifierServerName = ModifiersMapManager
                    .ParseModifierLocalizationKeyToServerName(item.modifierNameLocalizationKey);

                typeof(ModifiersMap).GetField(modifierServerName.ToLower(),
                    ReflectionUtils.DefaultFlags)?.SetValueDirect(__makeref(this), item.multiplier);
            }
        }
    }
}