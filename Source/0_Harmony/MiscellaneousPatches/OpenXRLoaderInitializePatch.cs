using System;
using BeatLeader.EyeTracking;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine.XR.OpenXR;

namespace BeatLeader {
    // Features can only be enabled while the loader is down, which after startup only happens when SiraUtil restarts it on SteamVR
    [HarmonyPatch(typeof(OpenXRLoaderBase), nameof(OpenXRLoaderBase.Initialize))]
    internal static class OpenXRLoaderInitializePatch {
        [UsedImplicitly]
        private static void Prefix() {
            try {
                OpenXREyeGaze.EnableFeatureBeforeLoaderInitialize();
            } catch (Exception ex) {
                Plugin.Log.Error($"[EyeTracking] Failed to enable OpenXR eye gaze: {ex}");
            }
        }

        [UsedImplicitly]
        private static void Postfix() {
            try {
                OpenXREyeGaze.UpdateIsActive();
            } catch (Exception ex) {
                Plugin.Log.Error($"[EyeTracking] Failed to check OpenXR eye gaze: {ex}");
            }
        }
    }
}
