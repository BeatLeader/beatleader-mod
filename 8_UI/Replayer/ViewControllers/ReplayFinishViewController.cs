using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Utilities;
using UnityEngine.UI;
using VRUIControls;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayFinishView.bsml")]
    internal class ReplayFinishViewController : StandaloneViewController<CanvasScreen> {
        [Inject] private readonly IReplayFinishController _finishController = null!;
        [Inject] private readonly ReplayerUIBinder _uiBinder = null!;

        [UIValue("replay-finish-panel")]
        private ReplayFinishPanel _replayFinishPanel = null!;

        [Inject]
        private void OnInject() {
            Init();
        }

        protected override void OnPreParse() {
            _replayFinishPanel = ReeUIComponentV2.Instantiate<ReplayFinishPanel>(transform);
            _replayFinishPanel.ExitButtonClickedEvent += HandleReplayExitButtonClicked;
            _finishController.ReplayWasFinishedEvent += HandleReplayFinish;
        }

        protected override void OnInitialize() {
            if (InputUtils.IsInFPFC) {
                Screen.Apply2DTemplate();
                Screen.Canvas.sortingOrder = 2;
                gameObject.GetOrAddComponent<GraphicRaycaster>();
            } else {
                Screen.ApplyVRTemplate();
                screen.gameObject.GetOrAddComponent<VRGraphicRaycaster>()
                    .SetField("_physicsRaycaster", BeatSaberUI.PhysicsRaycasterWithCache);
                Container.transform.localScale = new(0.02f, 0.02f, 0.02f);
                Container.transform.position = new(0f, 1.5f, 3f);
            }
        }

        protected override void OnDispose() {
            _finishController.ReplayWasFinishedEvent -= HandleReplayFinish;
        }

        private void HandleReplayFinish() {
            if (_finishController.ExitAutomatically) return;
            Container.SetActive(true);
            _replayFinishPanel.SetEnabled(true);
            _uiBinder.AutoHideUI = false;
            _uiBinder.ViewController.IsVisible = false;
        }

        private void HandleReplayExitButtonClicked() {
            _finishController.Exit();
            StartCoroutine(BasicCoroutines.AnimateGroupCoroutine(Screen.CanvasGroup, 1f, 0f, 0.3f));
        }
    }
}
