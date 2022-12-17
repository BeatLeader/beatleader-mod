using BeatLeader.Models;

namespace BeatLeader.Components {
    internal class ReplayFinishPanel : ReeUIComponentV2 {
        private IReplayFinishController _finishController;

        public void Setup(IReplayFinishController finishController) {
            _finishController = finishController;
        }
    }
}
