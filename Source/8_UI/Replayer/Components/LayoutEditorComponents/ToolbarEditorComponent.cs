using System.Collections;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Replayer {
    internal class ToolbarEditorComponent : LayoutEditorComponent<ToolbarEditorComponent>, Toolbar.ISettingsPanel {
        #region Setup

        private Toolbar _toolbar = null!;
        private ReplayerSettingsPanel _settingsPanel = null!;

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IReplayTimeController timeController,
            IVirtualPlayersManager playersManager,
            ICameraController cameraController,
            IVirtualPlayerBodySpawner bodySpawner,
            ReplayLaunchData launchData
        ) {
            _toolbar.Setup(
                pauseController,
                finishController,
                timeController,
                playersManager,
                this
            );
            _settingsPanel.Setup(
                launchData.Settings,
                cameraController,
                bodySpawner
            );
        }

        #endregion

        #region LayoutEditorComponent

        public override string ComponentName => "Toolbar";
        protected override Vector2 MinSize => new(90, 66);

        protected override GameObject ConstructInternal(Transform parent) {
            var container = parent.gameObject.CreateChild("Container");
            var containerTransform = container.AddComponent<RectTransform>();
            containerTransform.anchorMin = Vector2.zero;
            containerTransform.anchorMax = Vector2.one;
            containerTransform.sizeDelta = Vector2.zero;

            var flexContainer = container.AddComponent<FlexContainer>();
            flexContainer.FlexDirection = FlexDirection.Column;
            flexContainer.AlignItems = AlignItems.Stretch;
            flexContainer.Gap = new(0f, 1f);

            var settingsPanelContainer = container.CreateChild("SettingsPanelContainer");
            var settingsPanelContainerTransform = settingsPanelContainer.AddComponent<RectTransform>();
            settingsPanelContainer.AddComponent<RectMask2D>();
            var settingsFlexItem = settingsPanelContainer.AddComponent<FlexItem>();
            settingsFlexItem.FlexGrow = 1;

            _settingsPanel = ReplayerSettingsPanel.Instantiate(settingsPanelContainerTransform);
            _settingsPanel.Content.GetComponent<ContentSizeFitter>().enabled = false;
            _settingsPanelTransform = _settingsPanel.ContentTransform;
            _settingsPanelTransform.anchorMin = Vector2.zero;
            _settingsPanelTransform.anchorMax = Vector2.one;
            _settingsPanelTransform.sizeDelta = Vector2.zero;
            _settingsPanelCanvasGroup = _settingsPanel.Content.AddComponent<CanvasGroup>();
            ApplyInitialAnimationValues();

            _toolbar = Toolbar.Instantiate(containerTransform);
            var toolbarFlexItem = _toolbar.Content.AddComponent<FlexItem>();
            toolbarFlexItem.FlexBasis = new(-1f, 10f);

            return container;
        }

        #endregion

        #region SettingsPanel

        [SerializeField] private float settingsSizeOffset = 0.2f;
        [SerializeField] private float animationTime = 0.35f;
        [SerializeField] private float animationFramerate = 120f;

        private CanvasGroup _settingsPanelCanvasGroup = null!;
        private RectTransform _settingsPanelTransform = null!;

        private IEnumerator SettingsAnimationCoroutine(bool appear) {
            var totalFrames = animationTime * animationFramerate;
            var timePerFrame = animationTime / animationFramerate;

            var startPos = Vector3.zero;
            var deltaPos = _settingsPanelTransform.rect.size;
            var endPos = startPos - new Vector3(0, deltaPos.y, 0);
            var startScale = Vector3.one;
            var endScale = startScale - settingsSizeOffset * Vector3.one;

            for (var i = 0; i < totalFrames; i++) {
                yield return new WaitForSeconds(timePerFrame);
                var t = Mathf.Sin(i / totalFrames * Mathf.PI * 0.5f);
                if (appear) t = 1 - t;
                _settingsPanelCanvasGroup.alpha = Mathf.Lerp(0, 1, 1 - t);
                _settingsPanelTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
                _settingsPanelTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            }

            _settingsPanelCanvasGroup.alpha = appear ? 1 : 0;
            _settingsPanelCanvasGroup.interactable = appear;
        }

        private void ApplyInitialAnimationValues() {
            var deltaPos = _settingsPanelTransform.rect.size;
            _settingsPanelTransform.localPosition = Vector3.zero - new Vector3(0, deltaPos.y, 0);
            _settingsPanelTransform.localScale = Vector3.one - settingsSizeOffset * Vector3.one;
            _settingsPanelCanvasGroup.alpha = 0;
            _settingsPanelCanvasGroup.interactable = false;
        }

        public void Present() {
            StartCoroutine(SettingsAnimationCoroutine(true));
        }

        public void Dismiss() {
            StartCoroutine(SettingsAnimationCoroutine(false));
        }

        #endregion
    }
}