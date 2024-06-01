using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Scrollbar = BeatLeader.Components.Scrollbar;

namespace BeatLeader.UI.Hub {
    internal interface IBeatmapReplayLaunchPanel {
        IReadOnlyCollection<IReplayHeaderBase> SelectedReplays { get; }

        void AddSelectedReplay(IReplayHeaderBase header);
        void RemoveSelectedReplay(IReplayHeaderBase header);
    }

    internal class BeatmapReplayLaunchPanel : ReeUIComponentV3<BeatmapReplayLaunchPanel>, IBeatmapReplayLaunchPanel {
        #region DetailPanel

        public interface IDetailPanel {
            void Setup(IBeatmapReplayLaunchPanel? launchPanel, Transform? parent);
            void SetData(IReplayHeader? header);
        }

        #endregion

        #region UI Components

        [UIComponent("replays-list-panel"), UsedImplicitly]
        private ReplaysListPanel _replaysListPanel = null!;

        [UIComponent("replays-list-scrollbar"), UsedImplicitly]
        private Scrollbar _replaysListScrollbar = null!;

        [UIObject("loading-container"), UsedImplicitly]
        private GameObject _loadingContainerObject = null!;

        [UIObject("main-container"), UsedImplicitly]
        private GameObject _mainContainerObject = null!;

        [UIComponent("detail-panel-container"), UsedImplicitly]
        private Transform _detailPanelContainer = null!;

        [UIObject("detail-missing-text"), UsedImplicitly]
        private GameObject _detailMissingTextObject = null!;

        private CanvasGroup _mainContainerCanvasGroup = null!;

        #endregion

        #region BeatmapReplayLaunchPanel

        IReadOnlyCollection<IReplayHeaderBase> IBeatmapReplayLaunchPanel.SelectedReplays => ReplaysList.highlightedItems;

        public ReplaysList ReplaysList => _replaysListPanel.ReplaysList;

        public event Action<IReplayHeaderBase>? ReplaySelectedEvent;
        public event Action<IReplayHeaderBase>? ReplayDeselectedEvent;

        void IBeatmapReplayLaunchPanel.AddSelectedReplay(IReplayHeaderBase header) => AddSelectedReplay(header, true);
        void IBeatmapReplayLaunchPanel.RemoveSelectedReplay(IReplayHeaderBase header) => RemoveSelectedReplay(header, true);

        public void AddSelectedReplay(IReplayHeaderBase header, bool notify) {
            ReplaysList.highlightedItems.Add(header);
            ReplaysList.Refresh(false);
            ReplaySelectedEvent?.Invoke(header);
        }

        public void RemoveSelectedReplay(IReplayHeaderBase header, bool notify) {
            ReplaysList.highlightedItems.Remove(header);
            ReplaysList.Refresh(false);
            ReplayDeselectedEvent?.Invoke(header);
        }

        #endregion

        #region Init

        private IReplaysLoader? _replaysLoader;

        public void Setup(IReplaysLoader replaysLoader) {
            if (_replaysLoader is not null) {
                _replaysLoader.ReplayRemovedEvent -= HandleReplayDeleted;
                _replaysLoader.ReplaysLoadStartedEvent -= HandleReplaysLoadStarted;
                _replaysLoader.ReplaysLoadFinishedEvent -= HandleReplaysLoadFinished;
            }
            _replaysLoader = replaysLoader;
            _replaysListPanel.Setup(replaysLoader);
            _replaysLoader.ReplayRemovedEvent += HandleReplayDeleted;
            _replaysLoader.ReplaysLoadStartedEvent += HandleReplaysLoadStarted;
            _replaysLoader.ReplaysLoadFinishedEvent += HandleReplaysLoadFinished;
        }

        protected override void OnInitialize() {
            var image = _loadingContainerObject.AddComponent<Image>();
            image.sprite = BundleLoader.TransparentPixel;
            image.color = Color.clear;
            _mainContainerCanvasGroup = _mainContainerObject.AddComponent<CanvasGroup>();
            //ReplaysList.Scrollbar = _replaysListScrollbar;
            ReplaysList.ItemsWithIndexesSelectedEvent += HandleItemsSelected;
        }

        protected override void OnDispose() {
            if (_replaysLoader is not null) {
                _replaysLoader.ReplayRemovedEvent -= HandleReplayDeleted;
                _replaysLoader.ReplaysLoadStartedEvent -= HandleReplaysLoadStarted;
                _replaysLoader.ReplaysLoadFinishedEvent -= HandleReplaysLoadFinished;
            }
        }

        protected override bool OnValidation() {
            return _replaysLoader is not null;
        }

        #endregion

        #region ShowLoadingScreen

        private void ShowLoadingScreen(bool show) {
            _loadingContainerObject.SetActive(show);
            _mainContainerCanvasGroup.alpha = show ? 0.2f : 1;
        }

        #endregion

        #region ReplayFilter

        public IReplayFilter? ReplayFilter {
            get => null;
            set { }
        }

        #endregion

        #region DetailPanel

        public IDetailPanel? DetailPanel {
            get => _detailPanel;
            set {
                _detailPanel?.Setup(null, null);
                _detailPanel = value;
                _detailPanel?.Setup(this, _detailPanelContainer);
                _detailMissingTextObject.SetActive(_detailPanel is null);
            }
        }

        private IDetailPanel? _detailPanel;

        #endregion

        #region Callbacks

        private void HandleReplayDeleted(IReplayHeader header) {
            _detailPanel?.SetData(null);
        }

        private void HandleItemsSelected(ICollection<int> items) {
            if (items.Count is 0) {
                _detailPanel?.SetData(null);
                return;
            }
            var index = items.First();
            _detailPanel?.SetData(_replaysListPanel.ReplaysList.VisibleItems[index]);
        }

        private void HandleReplaysLoadStarted() {
            ShowLoadingScreen(true);
        }

        private void HandleReplaysLoadFinished() {
            ShowLoadingScreen(false);
        }

        [UIAction("cancel-loading-button-click"), UsedImplicitly]
        private void HandleCancelLoadingButtonClicked() {
            _replaysLoader!.CancelReplaysLoad();
        }

        #endregion
    }
}