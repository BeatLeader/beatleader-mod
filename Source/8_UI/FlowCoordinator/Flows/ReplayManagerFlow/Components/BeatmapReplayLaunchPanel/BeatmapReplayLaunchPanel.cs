using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
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
        public ReplaysListPanel ReplaysListPanel => _replaysListPanel;

        public event Action? SelectedReplaysUpdatedEvent;

        public event Action<IReplayHeader>? ReplaySelectedEvent;
        public event Action<IReplayHeader>? ReplayDeselectedEvent;

        void IBeatmapReplayLaunchPanel.AddSelectedReplay(IReplayHeader header) => AddSelectedReplay(header, true);
        void IBeatmapReplayLaunchPanel.RemoveSelectedReplay(IReplayHeader header) => RemoveSelectedReplay(header, true);

        public void AddSelectedReplay(IReplayHeader header, bool notify) {
            ReplaysList.SetItemHighlighted(header, true);
            SelectedReplaysUpdatedEvent?.Invoke();
            ReplaySelectedEvent?.Invoke(header);
        }

        public void RemoveSelectedReplay(IReplayHeader header, bool notify) {
            ReplaysList.SetItemHighlighted(header, false);
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

        #region Construct

        private Layout _detailPanelContainer = null!;
        private Label _detailMissingLabel = null!;
        private ReplaysListPanel _replaysListPanel = null!;
        private State<bool> _detailPanelCovered = null!;
        private State<bool> _loading = null!;

        protected override GameObject Construct() {
            _detailPanelCovered = Remember(false);
            _loading = Remember(false);

            var detailPanelAlpha = RememberAnimated(_detailPanelCovered.Map(static x => x ? 0f : 1f), 200.ms);

            return new Layout {
                FlexController = {
                    JustifyContent = Justify.SpaceAround
                },

                Children = {
                    //main container
                    new Layout {
                        FlexController = {
                            Gap = 2f
                        },

                        Children = {
                            new ReplaysListPanel {
                                FlexItem = {
                                    Size = new() { x = 60f }
                                },

                                ReplaysList = {
                                    OnSelectedItemsChanged = HandleItemsSelected
                                }
                            }.Bind(ref _replaysListPanel),
                            
                            new Scrollbar {
                                FlexItem = {
                                    Margin = new() { left = 1f, right = 2f }
                                },

                                ScrollContext = _replaysListPanel.ReplaysList.ScrollContext,
                                HideIfNothingToScroll = true
                            },
                            
                            // Detail panel container
                            new Layout {
                                FlexItem = {
                                    Size = new() { x = 50f }
                                },

                                Children = {
                                    new Label {
                                        Text = "Monke didn't find anything to show",
                                        EnableWrapping = true
                                    }.WithRectExpand().Bind(ref _detailMissingLabel)
                                }
                            }.WithAlpha(detailPanelAlpha).Bind(ref _detailPanelContainer)
                        }
                    }.WithAlpha(_loading.Map(static x => x ? 0.2f : 1f)),
                    
                    // Loading container
                    new Background {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.Center,
                            AlignItems = Align.Center,
                            Gap = 2f
                        },

                        IgnoreLayout = true,
                        sEnabled = _loading,
                        Sprite = BundleLoader.Sprites.transparentPixel,
                        Children = {
                            new Spinner {
                                FlexItem = {
                                    Size = 8f
                                }
                            },
                            //
                            new BsTextButton {
                                Text = "Cancel",
                                OnClick = () => ReplayManager.CancelLoading()
                            }
                        }
                    }.WithRectExpand()
                }
            }.Use();
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

        public bool DetailPanelCovered {
            get => _detailPanelCovered.Value;
            set => _detailPanelCovered.Value = value;
        }

        #endregion

        #region Callbacks

        private void HandleReplayDeleted(IReplayHeader header) {
            _detailPanel?.SetData(null);
        }

        private void HandleItemsSelected(IReadOnlyCollection<IReplayHeader> items) {
            if (items.Count is 0) {
                _detailPanel?.SetData(null);
                _previewLoader?.StopPreview();
                return;
            }
            var header = items.First();
            _previewLoader?.LoadPreview(header);
            _detailPanel?.SetData(header);
        }

        private void HandleReplaysLoadStarted() {
            _loading.Value = true;
        }

        private void HandleReplaysLoadFinished(bool finished) {
            _loading.Value = false;
        }

        #endregion
    }
}