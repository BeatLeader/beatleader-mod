using BeatLeader.Core.Managers.NoteEnhancer;
using HarmonyLib;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader
{
    [HarmonyPatch(typeof(SaberSwingRatingCounter), nameof(SaberSwingRatingCounter.ProcessNewData))]
	public static class PostSwingRatingEnhancerPatch
	{
        static void Prefix(SaberSwingRatingCounter __instance, BladeMovementDataElement newData, BladeMovementDataElement prevData, bool prevDataAreValid) {
            PostSwingRatingContainer container;
            if (!SwingRatingEnhancer.postSwingMap.ContainsKey(__instance))
            {
                container = new PostSwingRatingContainer();
                SwingRatingEnhancer.postSwingMap[__instance] = container;
            }
            else
            {
                container = SwingRatingEnhancer.postSwingMap[__instance];
            }

            container.AlreadyCut = __instance.GetField<bool, SaberSwingRatingCounter>("_notePlaneWasCut");
        }

		static void Postfix(SaberSwingRatingCounter __instance, BladeMovementDataElement newData, BladeMovementDataElement prevData, bool prevDataAreValid) {
            PostSwingRatingContainer container = SwingRatingEnhancer.postSwingMap[__instance];
            bool _rateAfterCut = __instance.GetField<bool, SaberSwingRatingCounter>("_rateAfterCut");
            Plane _notePlane = __instance.GetField<Plane, SaberSwingRatingCounter>("_notePlane");
            if (!container.AlreadyCut && !_notePlane.SameSide(newData.topPos, prevData.topPos))
            {
                Vector3 _cutTopPos = __instance.GetField<Vector3, SaberSwingRatingCounter>("_cutTopPos");
                Vector3 _cutBottomPos = __instance.GetField<Vector3, SaberSwingRatingCounter>("_cutBottomPos");
                Vector3 _afterCutTopPos = __instance.GetField<Vector3, SaberSwingRatingCounter>("_afterCutTopPos");
                Vector3 _afterCutBottomPos = __instance.GetField<Vector3, SaberSwingRatingCounter>("_afterCutBottomPos");

                float angleDiff = Vector3.Angle(_cutTopPos - _cutBottomPos, _afterCutTopPos - _afterCutBottomPos);

                if (_rateAfterCut)
                {
                    container.Value = SaberSwingRating.AfterCutStepRating(angleDiff, 0.0f);
                }
            }
            else
            {
                Vector3 _cutPlaneNormal = __instance.GetField<Vector3, SaberSwingRatingCounter>("_cutPlaneNormal");
                float normalDiff = Vector3.Angle(newData.segmentNormal, _cutPlaneNormal);
                if (_rateAfterCut)
                {
                    container.Value += SaberSwingRating.AfterCutStepRating(newData.segmentAngle, normalDiff);
                }
            }
        }
	}
}
