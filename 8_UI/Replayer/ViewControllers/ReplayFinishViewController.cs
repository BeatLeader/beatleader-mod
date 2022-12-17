using BeatLeader.Components;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayFinishView")]
    internal class ReplayFinishViewController : StandaloneViewController<CanvasScreen> {
        [Inject] private readonly ReplayFinishController _finishController;

        [UIValue("replay-finish-panel")]
        private ReplayFinishPanel _replayFinishPanel;

        protected override void OnPreParse() {
            _replayFinishPanel = ReeUIComponentV2.Instantiate<ReplayFinishPanel>(transform);
            _finishController.ReplayWasFinishedEvent += HandleReplayFinish;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _finishController.ReplayWasFinishedEvent -= HandleReplayFinish;
        }

        [UIAction("#post-parse")]
        private void HandlePostParse() {
            //IsVisible = false;
        }

        private void HandleReplayFinish() {
            if (_finishController.ExitAutomatically) return;
            //IsVisible = true;
        }
    }
}
