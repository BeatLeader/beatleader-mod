namespace BeatLeader.Utils {
    internal static class VRPlatformHelperExtensions {
        // IVRPlatformHelper.vrPlatformSDK was removed in 1.45, UnityXRHelper (OpenXR)
        // is the only real implementation now, the rest are deviceless/mock helpers
        public static bool IsOpenXR(this IVRPlatformHelper helper) {
            return helper is UnityXRHelper;
        }
    }
}
