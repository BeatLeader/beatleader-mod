using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;

namespace BeatLeader {
    internal static class BeatSaberExtensions {
        public static void DismissSelf(this FlowCoordinator flowCoordinator) {
            //TODO: asm pub
            var parent = flowCoordinator.GetField<FlowCoordinator, FlowCoordinator>("_parentFlowCoordinator");
            parent.DismissFlowCoordinator(flowCoordinator);
        }
    }
}