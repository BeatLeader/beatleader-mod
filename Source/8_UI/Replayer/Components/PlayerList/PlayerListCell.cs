using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BeatLeader.UI.Replayer {
    internal class PlayerListCell : ReactiveComponent {
        public const float CELL_SIZE = 16f;

        #region Construct

        private QuickReplayOverview _replayOverview = null!;
        private QuickMiniProfile _miniProfile = null!;
        private Image _scoreBackground = null!;
        private Label _scoreText = null!;
        private ImageButton _backgroundImage = null!;
        private RectTransform _actualContent = null!;

        protected override GameObject Construct() {
            return new BackgroundButton {
                    ContentTransform = {
                        pivot = new(1f, 0f)
                    },
                    OnClick = () => CellSelectedEvent?.Invoke(this),
                    Image = {
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 5f
                    },
                    Colors = null,
                    Children = {
                        new QuickMiniProfile {
                                UseAlternativeBlur = true
                            }
                            .Bind(ref _miniProfile)
                            .AsFlexItem(flexGrow: 1f, minSize: new() { x = 38f }),
                        //overview
                        new Layout {
                            Children = {
                                //score background
                                new Background {
                                    Sprite = BundleLoader.Sprites.background,
                                    Children = {
                                        //score label
                                        new Label {
                                            Text = "0",
                                            FontSize = 4f
                                        }.Bind(ref _scoreText).AsFlexItem(size: "auto")
                                    }
                                }.AsFlexGroup(
                                    direction: FlexDirection.Column,
                                    alignItems: Align.Center,
                                    padding: new() {
                                        left = 2f,
                                        top = 1f,
                                        right = 2f,
                                        bottom = 1f
                                    }
                                ).Bind(ref _scoreBackground).AsFlexItem(flexGrow: 1f),
                                //replay overview
                                new QuickReplayOverview()
                                    .Bind(ref _replayOverview)
                                    .AsFlexItem(size: new() { y = 5f, x = 12f })
                            }
                        }.AsFlexGroup(
                            direction: FlexDirection.Column,
                            alignItems: Align.Center,
                            padding: 1f,
                            gap: 1f
                        ).AsFlexItem(
                            minSize: new() { x = 22f },
                            margin: new() { right = 2f }
                        )
                    }
                }.AsFlexGroup()
                .Bind(ref _backgroundImage)
                .Bind(ref _actualContent)
                .AsFlexItem(size: new() { y = CELL_SIZE })
                .Use();
        }

        #endregion

        #region Setup

        public IVirtualPlayer Player => _player ?? throw new UninitializedComponentException();

        public event Action<PlayerListCell>? CellSelectedEvent;

        private IBeatmapTimeController _timeController = null!;
        private IReplayScoreEventsProcessor? _scoreEventsProcessor;
        private IReplayBeatmapEventsProcessor? _beatmapEventsProcessor;
        private PlayerList? _playerList;
        private IReplay? _replay;

        public void Setup(IVirtualPlayer player, IBeatmapTimeController timeController, PlayerList playerList) {
            _timeController = timeController;
            _playerList = playerList;
            //score events
            if (_scoreEventsProcessor is not null) {
                _scoreEventsProcessor.ScoreEventDequeuedEvent -= HandleScoreEventDequeued;
                _scoreEventsProcessor.EventQueueAdjustFinishedEvent -= HandleScoreEventQueueAdjustFinished;
            }
            _player = player;
            _replay = player.Replay;
            _scoreEventsProcessor = player.ReplayScoreEventsProcessor;
            _scoreEventsProcessor.ScoreEventDequeuedEvent += HandleScoreEventDequeued;
            _scoreEventsProcessor.EventQueueAdjustFinishedEvent += HandleScoreEventQueueAdjustFinished;
            //beatmap events
            if (_beatmapEventsProcessor is not null) {
                _beatmapEventsProcessor.NoteEventDequeuedEvent -= HandleNoteEventDequeued;
                _beatmapEventsProcessor.EventQueueAdjustStartedEvent -= HandleNoteEventQueueAdjustStarted;
            }
            _beatmapEventsProcessor = player.ReplayBeatmapEventsProcessor;
            _beatmapEventsProcessor.NoteEventDequeuedEvent += HandleNoteEventDequeued;
            _beatmapEventsProcessor.EventQueueAdjustStartedEvent += HandleNoteEventQueueAdjustStarted;
            //initialization
            RefreshPlayer();
            RefreshScoreWithoutNotice();
            RefreshBackgroundMaterial();
        }

        private void RefreshPlayer() {
            var replayData = _replay!.ReplayData;
            _miniProfile.SetPlayer(replayData.Player!);
            _replayOverview.SetReplay(_replay);
            Enabled = true;
        }

        protected override void OnInitialize() {
            LoadBackgroundMaterial();
            LoadScoreBackgroundMaterial();
        }

        protected override void OnDestroy() {
            if (_scoreEventsProcessor is not null) {
                _scoreEventsProcessor.ScoreEventDequeuedEvent -= HandleScoreEventDequeued;
            }
            Object.Destroy(_backgroundFillMaterial);
            Object.Destroy(_scoreBackgroundMaterial);
        }

        protected override void OnUpdate() {
            UpdateMoveAnimation();
            UpdateFillAnimation();
        }

        protected override void OnLateUpdate() {
            RefreshScoreFullComboLines();
        }

        #endregion

        #region Fill Animation

        private static readonly int fillAspectProperty = Shader.PropertyToID("_FillAspect");
        private static readonly int colorProperty = Shader.PropertyToID("_Color");
        private static readonly int backgroundColorProperty = Shader.PropertyToID("_BackgroundColor");

        private Material _backgroundFillMaterial = null!;

        private void UpdateFillAnimation() {
            var songTime = _timeController.SongTime;
            var replayFinishTime = _replay!.ReplayData.FinishTime;
            var aspect = MathUtils.Map(songTime, 0, replayFinishTime, 0, 1);
            _backgroundFillMaterial.SetFloat(fillAspectProperty, aspect);
        }

        private void RefreshBackgroundMaterial() {
            var color = _replay!.OptionalReplayData?.AccentColor ?? Color.clear;
            _backgroundFillMaterial.SetColor(colorProperty, color);
            var backgroundColor = color.ColorWithAlpha(0.4f);
            _backgroundFillMaterial.SetColor(backgroundColorProperty, backgroundColor);
        }

        private void LoadBackgroundMaterial() {
            _backgroundFillMaterial = Object.Instantiate(BundleLoader.OpponentBackgroundMaterial);
            _backgroundImage.Image.Material = _backgroundFillMaterial;
        }

        #endregion

        #region Move Animation

        private float _targetPos;

        public void MoveTo(float pos) {
            _targetPos = pos;
        }

        private void UpdateMoveAnimation() {
            _actualContent.localPosition = Vector3.Lerp(
                _actualContent.localPosition,
                new Vector3(0f, _targetPos),
                Time.deltaTime * 4f
            );
        }

        #endregion

        #region Score Background

        private static readonly int displayLinesProperty = Shader.PropertyToID("_DisplayLines");

        private Material _scoreBackgroundMaterial = null!;

        private void RefreshScoreFullComboLines() {
            _scoreBackgroundMaterial.SetInt(displayLinesProperty, _fullCombo ? 1 : 0);
        }

        private void LoadScoreBackgroundMaterial() {
            _scoreBackgroundMaterial = Object.Instantiate(BundleLoader.OpponentScoreBackgroundMaterial);
            _scoreBackground.Material = _scoreBackgroundMaterial;
        }

        #endregion

        #region Score

        private bool _fullCombo = true;

        private void RefreshScore(LinkedListNode<ScoreEvent>? node = null) {
            if (_scoreEventsProcessor!.QueueIsBeingAdjusted) {
                return;
            }
            
            RefreshScoreWithoutNotice(node);
            _playerList!.NotifyCellUpdateRequired();
        }

        private void RefreshScoreWithoutNotice(LinkedListNode<ScoreEvent>? node = null) {
            node ??= _scoreEventsProcessor!.CurrentScoreEvent;
            _scoreText.Text = $"{node?.Value.score ?? 0}";
        }

        #endregion

        #region Callbacks

        private IVirtualPlayer? _player;

        private void HandleScoreEventDequeued(LinkedListNode<ScoreEvent> node) {
            RefreshScore(node);
        }

        private void HandleScoreEventQueueAdjustFinished() {
            RefreshScore(_scoreEventsProcessor!.CurrentScoreEvent);
        }

        private void HandleNoteEventDequeued(LinkedListNode<NoteEvent> node) {
            if (node.Value.eventType is NoteEvent.NoteEventType.Miss) {
                _fullCombo = false;
            }
        }

        private void HandleNoteEventQueueAdjustStarted() {
            _fullCombo = true;
        }

        #endregion
    }
}