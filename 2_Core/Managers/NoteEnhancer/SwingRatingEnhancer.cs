using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;

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

        public static void Reset(SaberSwingRatingCounter counter) {
            var saberMovementData = counter.GetField<SaberMovementData, SaberSwingRatingCounter>("_saberMovementData");
            preSwingMap.Remove(saberMovementData);
            postSwingMap.Remove(counter);
        }

        public static void Enhance(Models.NoteCutInfo cutInfo, SaberSwingRatingCounter counter) {
            cutInfo.beforeCutRating = GetBeforeCutSwingRating(counter);
            cutInfo.afterCutRating = GetAfterCutSwingRating(counter);
        }

        private static float GetBeforeCutSwingRating(SaberSwingRatingCounter counter) {
            var saberMovementData = counter.GetField<SaberMovementData, SaberSwingRatingCounter>("_saberMovementData");
            var realBeforeCutRating = counter.beforeCutRating;
            if (!preSwingMap.ContainsKey(saberMovementData)) return realBeforeCutRating;
            var unclampedBeforeCutRating = preSwingMap[saberMovementData].Value;
            return ChooseSwingRating(realBeforeCutRating, unclampedBeforeCutRating);
        }

        private static float GetAfterCutSwingRating(SaberSwingRatingCounter counter) {
            var realAfterCutRating = counter.afterCutRating;
            if (!postSwingMap.ContainsKey(counter)) return realAfterCutRating;
            var unclampedAfterCutRating = postSwingMap[counter].Value;
            return ChooseSwingRating(realAfterCutRating, unclampedAfterCutRating);
        }

        private static float ChooseSwingRating(float real, float unclamped) {
            return real < 1 ? real : Mathf.Max(real, unclamped);
        }
    }
}
