using BeatLeader.Core.Managers.NoteEnhancer;
using HarmonyLib;
using IPA.Utilities;
using System;
using UnityEngine;

namespace BeatLeader
{
    [HarmonyPatch(typeof(SaberMovementData), nameof(SaberMovementData.ComputeSwingRating), new Type[] { typeof(bool), typeof(float) })]
    public static class PreSwingRatingEnhancerPatch
    {
        static void Postfix(SaberMovementData __instance, bool overrideSegmenAngle, float overrideValue)
        {
            BladeMovementDataElement[] _data = __instance.GetField<BladeMovementDataElement[], SaberMovementData>("_data");

            int _nextAddIndex = __instance.GetField<int, SaberMovementData>("_nextAddIndex");
            int _validCount = __instance.GetField<int, SaberMovementData>("_validCount");

            int length = _data.Length;

            int index = _nextAddIndex - 1;
            if (index < 0) index += length;

            float startTime = _data[index].time;
            float time = startTime;

            Vector3 segmentNormal1 = _data[index].segmentNormal;
            float angleDiff = overrideSegmenAngle ? overrideValue : _data[index].segmentAngle;
            float swingRating = SaberSwingRating.BeforeCutStepRating(angleDiff, 0.0f);
            for (int i = 2; (double)startTime - (double)time < 0.4 && i < _validCount; ++i)
            {
                --index;
                if (index < 0) index += length;

                Vector3 segmentNormal2 = _data[index].segmentNormal;
                float segmentAngle = _data[index].segmentAngle;

                float normalDiff = Vector3.Angle(segmentNormal2, segmentNormal1);
                if ((double)normalDiff <= 90.0)
                {
                    swingRating += SaberSwingRating.BeforeCutStepRating(segmentAngle, normalDiff);
                    time = _data[index].time;
                }
                else {
                    break;
                }
            }

            PreSwingRatingContainer container;
            if (!SwingRatingEnhancer.preSwingMap.ContainsKey(__instance))
            {
                container = new PreSwingRatingContainer();
                SwingRatingEnhancer.preSwingMap[__instance] = container;
            }
            else
            {
                container = SwingRatingEnhancer.preSwingMap[__instance];
            }

            container.Value = swingRating;
        }
    }
}
