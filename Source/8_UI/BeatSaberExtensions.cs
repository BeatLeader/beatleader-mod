using HMUI;
using IPA.Utilities;

namespace BeatLeader {
    internal static class BeatSaberExtensions {
        public static void ReplaceInitialViewControllers(
            this FlowCoordinator flowCoordinator,
            Optional<ViewController> mainViewController = default,
            Optional<ViewController> leftScreenViewController = default,
            Optional<ViewController> rightScreenViewController = default,
            Optional<ViewController> bottomScreenViewController = default,
            Optional<ViewController> topScreenViewController = default
        ) {
            Replace(flowCoordinator, "_providedMainViewController", mainViewController);
            Replace(flowCoordinator, "_providedLeftScreenViewController", leftScreenViewController);
            Replace(flowCoordinator, "_providedRightScreenViewController", rightScreenViewController);
            Replace(flowCoordinator, "_providedBottomScreenViewController", bottomScreenViewController);
            Replace(flowCoordinator, "_providedTopScreenViewController", topScreenViewController);
            return;

            static void Replace(FlowCoordinator coordinator, string name, Optional<ViewController> view) {
                if (!view.HasValue) return;
                coordinator.SetField(name, view.Value);
            }
        }
    }
}