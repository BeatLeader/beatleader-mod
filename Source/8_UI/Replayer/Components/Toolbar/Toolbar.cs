using BeatLeader.Components;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using BeatLeader.Models;
using BeatLeader.Utils;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.UI;

namespace BeatLeader.UI.Replayer {
    internal class Toolbar : ReeUIComponentV3<Toolbar> {
        #region UI Components

        [UIComponent("play-button"), UsedImplicitly]
        private ImageButton _playButton = null!;

        [UIComponent("song-time-text"), UsedImplicitly]
        private TMP_Text _songTimeText = null!;

        [UIValue("timeline"), UsedImplicitly]
        private Timeline _timeline = null!;

        [UIValue("exit-button"), UsedImplicitly]
        private ExitButton _exitButton = null!;

        [UIObject("exit-button-container"), UsedImplicitly]
        private GameObject _exitButtonContainer = null!;
        
        [UIObject("toolbar-container"), UsedImplicitly]
        private GameObject _toolbarContainer = null!;

        #endregion
        
        #region Setup

        public IReplayTimeline Timeline => _timeline;

        private IReplayPauseController? _pauseController;
        private IReplayFinishController? _finishController;
        private IReplayTimeController? _beatmapTimeController;
        private ISettingsPanel? _settingsPanel;

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IReplayTimeController timeController,
            IVirtualPlayersManager playersManager,
            ISettingsPanel settingsPanel
        ) {
            OnDispose();
            _pauseController = pauseController;
            _finishController = finishController;
            _beatmapTimeController = timeController;
            _settingsPanel = settingsPanel;

            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            _timeline.Setup(playersManager, pauseController, timeController);
            enabled = true;
        }

        private void Update() {
            UpdateSongTime();
        }

        protected override void OnInstantiate() {
            _timeline = ReeUIComponentV2.Instantiate<Timeline>(transform);
            _exitButton = ExitButton.Instantiate(transform);
            _exitButton.ClickEvent += HandleExitButtonClicked;
        }

        protected override void OnInitialize() {
            ApplyBackground(_exitButtonContainer);
            ApplyBackground(_toolbarContainer);
            enabled = false;
            SetSongTime(0, 0);
        }

        protected override void OnDispose() {
            if (_pauseController != null) {
                _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;
            }
        }

        #endregion

        #region Tools

        private static void ApplyBackground(GameObject gameObject) {
            var img = gameObject.AddComponent<AdvancedImageView>();
            img.sprite = BundleLoader.WhiteBG;
            img.material = GameResources.UIFogBackgroundMaterial;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 7f;
        }

        #endregion
        
        #region SongTime

        private void UpdateSongTime() {
            var timeController = _beatmapTimeController!;
            SetSongTime(timeController.SongTime, timeController.ReplayEndTime);
        }

        private void SetSongTime(float time, float totalTime) {
            _songTimeText.text = FormatUtils.FormatSongTime(time, totalTime);
        }

        #endregion

        #region PauseButton

        private static readonly Sprite playSprite = BundleLoader.PlayIcon;
        private static readonly Sprite pauseSprite = BundleLoader.PauseIcon;

        private void RefreshPauseButton(bool paused) {
            _playButton.Image.Sprite = paused ? playSprite : pauseSprite;
        }

        #endregion

        #region ExitButton
        
        private class ExitButton : ButtonComponentBase<ExitButton> {
            private static readonly Sprite openedDoorSprite = BundleLoader.OpenedDoorIcon;
            private static readonly Sprite closedDoorSprite = BundleLoader.ClosedDoorIcon;

            private AdvancedImage _image1 = null!;
            private AdvancedImage _image2 = null!;

            protected override void OnHoverProgressChange(float progress) {
                var alternativeProgress = MathUtils.Map(progress, 0f, 1f, 0.2f, 0f);
                _image1.Color = ImageButton.DefaultColor.ColorWithAlpha(alternativeProgress);
                _image2.Color = ImageButton.DefaultHoveredColor.ColorWithAlpha(progress);
            }

            protected override void OnContentConstruct(Transform parent) {
                _image1 = AdvancedImage.Instantiate(parent);
                _image1.InheritSize = true;
                _image1.Sprite = closedDoorSprite;
                _image1.Material = BundleLoader.UIAdditiveGlowMaterial;
                _image2 = AdvancedImage.Instantiate(parent);
                _image2.InheritSize = true;
                _image2.Sprite = openedDoorSprite;
                _image2.Material = BundleLoader.UIAdditiveGlowMaterial;
                Size = 6;
                BaseScale = Vector3.one;
                LayoutGroup.enabled = false;
            }
        }

        #endregion

        #region SettingsPanel

        public interface ISettingsPanel {
            void Present();
            void Dismiss();
        }

        private bool _settingsPanelPresented;

        private void RefreshSettingsPanel(bool state) {
            if (state == _settingsPanelPresented) return;
            if (state) {
                _settingsPanel!.Present();
            } else {
                _settingsPanel!.Dismiss();
            }
            _settingsPanelPresented = state;
        }

        #endregion

        #region Callbacks

        [UIAction("pause-button-click"), UsedImplicitly]
        private void HandlePauseButtonClicked() {
            if (!_pauseController!.IsPaused) {
                _pauseController.Pause();
            } else {
                _pauseController.Resume();
            }
        }

        [UIAction("settings-button-state-change"), UsedImplicitly]
        private void HandleSettingsButtonStateChanged(bool state) {
            RefreshSettingsPanel(state);
        }
        
        private void HandleExitButtonClicked() {
            _finishController?.Exit();
        }

        private void HandlePauseStateChanged(bool paused) {
            RefreshPauseButton(paused);
        }

        #endregion
    }
}