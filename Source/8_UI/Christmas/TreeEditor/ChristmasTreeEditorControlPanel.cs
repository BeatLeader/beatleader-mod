using System;
using BeatLeader.API.Methods;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasTreeEditorControlPanel : ReeUIComponentV2 {
        #region Events

        public event Action? CancelButtonClickedEvent;
        public event Action? OkButtonClickedEvent;

        #endregion

        #region Components

        [UIObject("loading-indicator"), UsedImplicitly]
        private GameObject _loadingIndicator = null!;

        [UIObject("bg"), UsedImplicitly]
        private GameObject _bg = null!;

        private CanvasGroup _bgGroup = null!;

        #endregion

        #region Setup

        private StaticScreen _screen = null!;
        private ChristmasTree _tree = null!;
        
        public void Setup(ChristmasTree tree) {
            _tree = tree;
        }
        
        public void SetLoading(bool loading) {
            _bgGroup.alpha = loading ? 0.3f : 1f;
            _bgGroup.interactable = !loading;
            _loadingIndicator.SetActive(loading);
        }

        public void Present() {
            _screen.Present();
        }

        public void Dismiss() {
            _screen.Dismiss();
        }

        protected override void OnInitialize() {
            _screen = Content.gameObject.AddComponent<StaticScreen>();
            _bgGroup = _bg.GetOrAddComponent<CanvasGroup>();
            var indicatorGroup = _loadingIndicator.AddComponent<CanvasGroup>();
            indicatorGroup.ignoreParentGroups = true;
            SetLoading(false);
        }

        #endregion

        #region Callbacks

        [UIAction("clear-click"), UsedImplicitly]
        private void HandleClearClicked() {
            _tree.ClearOrnaments();
        }
        
        [UIAction("cancel-click"), UsedImplicitly]
        private void HandleCancelClicked() {
            ChristmasTreeRequest.SendRequest();
            CancelButtonClickedEvent?.Invoke();
        }

        [UIAction("save-click"), UsedImplicitly]
        private void HandleSaveClicked() {
            OkButtonClickedEvent?.Invoke();
        }

        #endregion
    }
}