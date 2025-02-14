using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal abstract class BasicReplayDetailPanel : ReactiveComponent, BeatmapReplayLaunchPanel.IDetailPanel {
        #region Construct

        private ReplayStatisticsPanel _replayStatisticsPanel = null!;
        private LoadingContainer _statsContainer = null!;
        private QuickMiniProfile _miniProfile = null!;
        private TagsStrip _tagsStrip = null!;
        private CanvasGroup _contentGroup = null!;
        private Label _emptyLabel = null!;

        protected sealed override GameObject Construct() {
            return new Dummy {
                Children = {
                    //actual content
                    new Dummy {
                        Children = {
                            new QuickMiniProfile()
                                .AsFlexItem(size: new() { y = 16f })
                                .Bind(ref _miniProfile),
                            //
                            new TagsStrip()
                                .AsFlexItem(size: new() { y = 5f })
                                .Bind(ref _tagsStrip),
                            //
                            new ReeWrapperV2<ReplayStatisticsPanel>()
                                .AsFlexItem(size: new() { y = 37f })
                                .BindRee(ref _replayStatisticsPanel)
                                .InLoadingContainer()
                                .Bind(ref _statsContainer),
                            //
                            ConstructButtons()
                        }
                    }.AsFlexGroup(direction: FlexDirection.Column).WithNativeComponent(out _contentGroup).WithRectExpand(),
                    //empty label
                    new Label {
                        Text = "Select a replay to let Monke think on it",
                        EnableWrapping = true
                    }.WithRectExpand().Bind(ref _emptyLabel)
                }
            }.Use();
        }

        protected abstract ILayoutItem ConstructButtons();

        #endregion

        #region Setup

        protected abstract bool AllowTagsEdit { get; }
        protected abstract string EmptyText { get; }

        public virtual void Setup(IBeatmapReplayLaunchPanel? launchPanel) { }

        protected void SetupInternal(IReplayTagManager tagManager) {
            _tagsStrip.Setup(tagManager, _statsContainer);
        }

        protected override void OnInitialize() {
            _miniProfile.SetPlayer(null);
            _tagsStrip.AllowEdit = AllowTagsEdit;
            _emptyLabel.Text = EmptyText;
        }

        #endregion

        #region Animation

        private readonly ValueAnimator _valueAnimator = new() { LerpCoefficient = 15f };

        private void SetDetailsVisible(bool visible) {
            _valueAnimator.SetTarget(visible ? 1f : 0f);
        }

        private void RefreshDetailsAnimation(float progress) {
            _contentGroup.alpha = progress;
            _emptyLabel.Color = Color.white.ColorWithAlpha(1f - progress);
        }

        protected override void OnUpdate() {
            _valueAnimator.Update();
            RefreshDetailsAnimation(_valueAnimator.Progress);
        }

        #endregion

        #region Data

        protected IReplayHeader? Header { get; private set; }
        protected CancellationToken CancellationToken => _tokenSource.Token;
        
        private CancellationTokenSource _tokenSource = new();
        private string? _lastPlayerId;

        public void SetData(IReplayHeader? header) {
            if (header == Header) return;
            _tokenSource.Cancel();
            _tokenSource = new();
            SetDataAsync(header, _tokenSource.Token).RunCatching();
        }

        private async Task SetDataAsync(IReplayHeader? header, CancellationToken token) {
            Header = header;
            if (header == null) {
                SetDetailsVisible(false);
                _miniProfile.SetPlayer(null);
                _lastPlayerId = null;
            } else {
                SetDetailsVisible(true);
                //starting tasks
                _tagsStrip.SetMetadata(header.ReplayMetadata);
                var playerTask = RefreshPlayerAsync(header, token);
                var statsTask = RefreshStatsAsync(header, token);
                var setDataTask = SetDataInternalAsync(header, token);
                //awaiting tasks
                await playerTask;
                await statsTask;
                await setDataTask;
            }
        }

        protected virtual Task SetDataInternalAsync(IReplayHeader header, CancellationToken token) {
            return Task.CompletedTask;
        }

        private async Task RefreshStatsAsync(IReplayHeader header, CancellationToken token) {
            _statsContainer.Loading = true;
            await _replayStatisticsPanel.SetDataByHeaderAsync(header, token);
            _statsContainer.Loading = false;
        }

        private async Task RefreshPlayerAsync(IReplayHeader header, CancellationToken token) {
            if (_lastPlayerId == header.ReplayInfo.PlayerID) return;
            _miniProfile.SetLoading();
            var player = await header.LoadPlayerAsync(false, token);
            _miniProfile.SetPlayer(player);
            _lastPlayerId = player.Id;
        }

        #endregion
    }
}