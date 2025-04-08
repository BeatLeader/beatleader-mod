using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal interface IBeatmapReplayLaunchPanel {
        IReadOnlyCollection<IReplayHeader> SelectedReplays { get; }

        event Action? SelectedReplaysUpdatedEvent;

        void AddSelectedReplay(IReplayHeader header);
        void RemoveSelectedReplay(IReplayHeader header);
    }

    internal class BeatmapReplayLaunchPanel : ReactiveComponent, IBeatmapReplayLaunchPanel {
        #region DetailPanel

        public interface IDetailPanel : IReactiveComponent {
            void Setup(IBeatmapReplayLaunchPanel? launchPanel);
            void SetData(IReplayHeader? header);
        }

        #endregion

        #region BeatmapReplayLaunchPanel

        IReadOnlyCollection<IReplayHeader> IBeatmapReplayLaunchPanel.SelectedReplays => ReplaysList.HighlightedItems;
        public ReplaysList ReplaysList => _replaysListPanel.ReplaysList;

        public event Action? SelectedReplaysUpdatedEvent;

        public event Action<IReplayHeader>? ReplaySelectedEvent;
        public event Action<IReplayHeader>? ReplayDeselectedEvent;

        void IBeatmapReplayLaunchPanel.AddSelectedReplay(IReplayHeader header) => AddSelectedReplay(header, true);
        void IBeatmapReplayLaunchPanel.RemoveSelectedReplay(IReplayHeader header) => RemoveSelectedReplay(header, true);

        public void AddSelectedReplay(IReplayHeader header, bool notify) {
            ReplaysList.HighlightedItems.Add(header);
            ReplaysList.Refresh(false);
            SelectedReplaysUpdatedEvent?.Invoke();
            ReplaySelectedEvent?.Invoke(header);
        }

        public void RemoveSelectedReplay(IReplayHeader header, bool notify) {
            ReplaysList.HighlightedItems.Remove(header);
            ReplaysList.Refresh(false);
            SelectedReplaysUpdatedEvent?.Invoke();
            ReplayDeselectedEvent?.Invoke(header);
        }

        public void ClearSelectedReplays() {
            foreach (var item in ReplaysList.HighlightedItems.ToArray()) {
                RemoveSelectedReplay(item, true);
            }
        }

        #endregion

        #region Init

        private ReplayPreviewLoader? _previewLoader;

        public void Setup(ReplayPreviewLoader? previewLoader) {
            _previewLoader = previewLoader;
        }

        protected override void OnDisable() {
            _previewLoader?.StopPreview();
        }

        #endregion

        #region ShowLoadingScreen

        private void ShowLoadingScreen(bool show) {
            _loadingContainer.Enabled = show;
            _mainContainerCanvasGroup.alpha = show ? 0.2f : 1;
        }

        #endregion

        #region Construct

        private Image _loadingContainer = null!;
        private Dummy _detailPanelContainer = null!;
        private CanvasGroup _mainContainerCanvasGroup = null!;
        private Label _detailMissingLabel = null!;
        private ReplaysListPanel _replaysListPanel = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    //main container
                    new Dummy {
                        Children = {
                            //replays list
                            new ReplaysListPanel()
                                .With(
                                    x => x.ReplaysList.WithListener(
                                        y => y.SelectedIndexes,
                                        HandleItemsSelected
                                    )
                                )
                                .AsFlexItem(size: new() { x = 60f })
                                .Bind(ref _replaysListPanel),
                            //scrollbar
                            new Scrollbar()
                                .With(x => _replaysListPanel.ReplaysList.Scrollbar = x)
                                .AsFlexItem(margin: new() { left = 1f, right = 2f }),
                            //detail panel container
                            new Dummy {
                                Children = {
                                    new Label {
                                        Text = "Monke didn't find anything to show",
                                        EnableWrapping = true
                                    }.WithRectExpand().Bind(ref _detailMissingLabel)
                                }
                            }.AsFlexItem(size: new() { x = 50f }).Bind(ref _detailPanelContainer)
                        }
                    }.WithNativeComponent(out _mainContainerCanvasGroup).AsFlexGroup(gap: 2f).AsFlexItem(),
                    //loading container
                    new Image {
                        Enabled = false,
                        Sprite = BundleLoader.Sprites.transparentPixel,
                        Children = {
                            new Spinner().AsFlexItem(size: 8f),
                            //
                            new BsButton {
                                OnClick = () => ReplayManager.CancelLoading()
                            }.WithLabel("Cancel").AsFlexItem()
                        }
                    }.AsFlexGroup(
                        direction: FlexDirection.Column,
                        justifyContent: Justify.Center,
                        alignItems: Align.Center,
                        gap: 2f
                    ).WithRectExpand().Bind(ref _loadingContainer)
                }
            }.AsFlexGroup().Use();
        }

        protected override void OnInitialize() {
            ReplayManager.ReplayDeletedEvent += HandleReplayDeleted;
            ReplayManager.LoadingStartedEvent += HandleReplaysLoadStarted;
            ReplayManager.LoadingFinishedEvent += HandleReplaysLoadFinished;
        }

        protected override void OnDestroy() {
            ReplayManager.ReplayDeletedEvent -= HandleReplayDeleted;
            ReplayManager.LoadingStartedEvent -= HandleReplaysLoadStarted;
            ReplayManager.LoadingFinishedEvent -= HandleReplaysLoadFinished;
        }

        #endregion

        #region DetailPanel

        public IDetailPanel? DetailPanel {
            get => _detailPanel;
            set {
                _detailPanel?.Setup(null);
                _detailPanel?.Use(null);
                _detailPanel = value;
                _detailPanel?.Setup(this);
                _detailPanel?.WithRectExpand().Use(_detailPanelContainer.ContentTransform);
                _detailMissingLabel.Enabled = _detailPanel == null;
            }
        }

        private IDetailPanel? _detailPanel;

        #endregion

        #region Callbacks

        private void HandleReplayDeleted(IReplayHeader header) {
            _detailPanel?.SetData(null);
        }

        private void HandleItemsSelected(IReadOnlyCollection<int> items) {
            if (items.Count is 0) {
                _detailPanel?.SetData(null);
                _previewLoader?.StopPreview();
                return;
            }
            var index = items.First();
            var header = _replaysListPanel.ReplaysList.FilteredItems[index];
            _previewLoader?.LoadPreview(header);
            _detailPanel?.SetData(header);
        }

        private void HandleReplaysLoadStarted() {
            ShowLoadingScreen(true);
        }

        private void HandleReplaysLoadFinished(bool finished) {
            ShowLoadingScreen(false);
        }

        #endregion
    }
}