using IPA.Utilities;
using System.Collections.Generic;

namespace BeatLeader.Core.Managers.NoteEnhancer
{
    class PreSwingRatingContainer
    {
        public bool AlreadyCut { get; set; }
        public float Value { get; set; }
    }

    class PostSwingRatingContainer 
    {
        public bool AlreadyCut { get; set; }
        public float Value { get; set; }
    }

    class SwingRatingEnhancer
    {
        public static Dictionary<SaberMovementData, PreSwingRatingContainer> preSwingMap = new();
        public static Dictionary<SaberSwingRatingCounter, PostSwingRatingContainer> postSwingMap = new();

        public static void Enhance(Models.NoteCutInfo cutInfo, SaberSwingRatingCounter counter) {
            SaberMovementData _saberMovementData = counter.GetField<SaberMovementData, SaberSwingRatingCounter>("_saberMovementData");

            if (preSwingMap.ContainsKey(_saberMovementData)) {
                cutInfo.beforeCutRating = preSwingMap[_saberMovementData].Value;
            } else {
                cutInfo.beforeCutRating = counter.beforeCutRating;
            }
            if (postSwingMap.ContainsKey(counter))
            {
                cutInfo.afterCutRating = postSwingMap[counter].Value;
            }
            else
            {
                cutInfo.afterCutRating = counter.afterCutRating;
            }
        }
    }
}
