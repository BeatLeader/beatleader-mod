using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal interface IBeatmapReplayLaunchPanel {
        IReadOnlyCollection<IReplayHeaderBase> SelectedReplays { get; }

        event Action? SelectedReplaysUpdatedEvent;

        void AddSelectedReplay(IReplayHeaderBase header);
        void RemoveSelectedReplay(IReplayHeaderBase header);
    }

    internal class BeatmapReplayLaunchPanel : ReactiveComponent, IBeatmapReplayLaunchPanel {
        #region DetailPanel

        public interface IDetailPanel : IReactiveComponent {
            void Setup(IBeatmapReplayLaunchPanel? launchPanel);
            void SetData(IReplayHeader? header);
        }

        #endregion

        #region BeatmapReplayLaunchPanel

        IReadOnlyCollection<IReplayHeaderBase> IBeatmapReplayLaunchPanel.SelectedReplays => ReplaysList.HighlightedItems;
        public ReplaysList ReplaysList => _replaysListPanel.ReplaysList;
        
        public event Action? SelectedReplaysUpdatedEvent;
        
        public event Action<IReplayHeaderBase>? ReplaySelectedEvent;
        public event Action<IReplayHeaderBase>? ReplayDeselectedEvent;

        void IBeatmapReplayLaunchPanel.AddSelectedReplay(IReplayHeaderBase header) => AddSelectedReplay(header, true);
        void IBeatmapReplayLaunchPanel.RemoveSelectedReplay(IReplayHeaderBase header) => RemoveSelectedReplay(header, true);

        public void AddSelectedReplay(IReplayHeaderBase header, bool notify) {
            ReplaysList.HighlightedItems.Add(header);
            ReplaysList.Refresh(false);
            SelectedReplaysUpdatedEvent?.Invoke();
            ReplaySelectedEvent?.Invoke(header);
        }

        public void RemoveSelectedReplay(IReplayHeaderBase header, bool notify) {
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

        protected override bool Validate() {
            return _replaysLoader is not null;
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
                            new BsButton()
                                .WithClickListener(HandleCancelLoadingButtonClicked)
                                .WithLabel("Cancel")
                                .AsFlexItem()
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

        protected override void OnDestroy() {
            if (_replaysLoader is not null) {
                _replaysLoader.ReplayRemovedEvent -= HandleReplayDeleted;
                _replaysLoader.ReplaysLoadStartedEvent -= HandleReplaysLoadStarted;
                _replaysLoader.ReplaysLoadFinishedEvent -= HandleReplaysLoadFinished;
            }
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
                return;
            }
            var index = items.First();
            _detailPanel?.SetData(_replaysListPanel.ReplaysList.FilteredItems[index] as IReplayHeader);
        }

        private void HandleReplaysLoadStarted() {
            ShowLoadingScreen(true);
        }

        private void HandleReplaysLoadFinished() {
            ShowLoadingScreen(false);
        }

        private void HandleCancelLoadingButtonClicked() {
            _replaysLoader!.CancelReplaysLoad();
        }

        #endregion
    }
}