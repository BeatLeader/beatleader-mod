using System.Collections.Generic;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;
using ImageButton = BeatLeader.UI.Reactive.Components.ImageButton;

namespace BeatLeader.UI.Replayer {
    internal class PlayerList : Table<IVirtualPlayer, PlayerList.Cell> {
        #region Cell

        public class Cell : TableComponentCell<IVirtualPlayer> {
            #region Construct

            private QuickReplayOverview _replayOverview = null!;
            private QuickMiniProfile _miniProfile = null!;
            private Image _scoreBackground = null!;
            private Label _scoreText = null!;
            private ImageButton _backgroundImage = null!;
            private RectTransform _actualContent = null!;

            protected override GameObject Construct() {
                return new ImageButton {
                    ContentTransform = {
                        pivot = new(1f, 0f)
                    },
                    Image = {
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 5f,
                        //required to fill the TEXCOORD1 with non-sliced UV
                        UseGradient = true
                    },
                    Colors = null,
                    GrowOnHover = false,
                    HoverLerpMul = float.MaxValue,
                    Children = {
                        new QuickMiniProfile()
                            .Bind(ref _miniProfile)
                            .AsFlexItem(grow: 1f, minSize: new() { x = 38f }),
                        //overview
                        new Dummy {
                            Children = {
                                //score background
                                new Image {
                                    Sprite = BundleLoader.Sprites.background,
                                    Children = {
                                        //score label
                                        new Label {
                                            Text = "Score",
                                            FontSize = 3f
                                        }.AsFlexItem(size: "auto"),
                                        //actual score label
                                        new Label {
                                            Text = "0",
                                            FontSize = 4f
                                        }.Bind(ref _scoreText).AsFlexItem(size: "auto")
                                    }
                                }.AsFlexGroup(
                                    direction: Reactive.Yoga.FlexDirection.Column,
                                    alignItems: Align.Center,
                                    padding: new() {
                                        left = 2f,
                                        top = 1f,
                                        right = 2f,
                                        bottom = 1f
                                    }
                                ).Bind(ref _scoreBackground).AsFlexItem(grow: 1f),
                                //replay overview
                                new QuickReplayOverview()
                                    .Bind(ref _replayOverview)
                                    .AsFlexItem(size: new() { y = 5f, x = 12f })
                            }
                        }.AsFlexGroup(
                            direction: Reactive.Yoga.FlexDirection.Column,
                            alignItems: Align.Center,
                            padding: 1f,
                            gap: 1f
                        ).AsFlexItem(minSize: new() { x = 22f })
                    }
                }.With(
                    x => x
                        .AsFlexGroup()
                        .WithClickListener(() => SelectSelf(true))
                        .Bind(ref _backgroundImage)
                        .Bind(ref _actualContent)
                        .WithRectExpand()
                ).In<Dummy>().WithSizeDelta(0f, 20f).Use();
            }

            #endregion

            #region Setup

            private IBeatmapTimeController _timeController = null!;
            private IReplayScoreEventsProcessor? _scoreEventsProcessor;
            private IReplayBeatmapEventsProcessor? _beatmapEventsProcessor;
            private IVirtualPlayer? _previousVirtualPlayer;
            private PlayerList _playerList = null!;

            public void Setup(IBeatmapTimeController timeController, PlayerList playerList) {
                _timeController = timeController;
                _playerList = playerList;
                if (_previousVirtualPlayer != null && _previousVirtualPlayer != Item) {
                    _playerList.NotifyCellForceUpdateRequired();
                }
                _previousVirtualPlayer = Item;
            }

            protected override void OnInit(IVirtualPlayer player) {
                //score events
                if (_scoreEventsProcessor is not null) {
                    _scoreEventsProcessor.ScoreEventDequeuedEvent -= HandleScoreEventDequeued;
                    _scoreEventsProcessor.EventQueueAdjustFinishedEvent -= HandleScoreEventQueueAdjustFinished;
                }
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
                if (_previousVirtualPlayer != null && _previousVirtualPlayer != player) {
                    _playerList.NotifyCellForceUpdateRequired();
                }
                _previousVirtualPlayer = player;
            }

            private void RefreshPlayer() {
                var replay = Item.Replay;
                var replayData = replay.ReplayData;
                _miniProfile.SetPlayer(replayData.Player!);
                _replayOverview.SetReplay(replay);
                Enabled = true;
            }

            protected override void OnInitialize() {
                Enabled = false;
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
                var replayFinishTime = Item!.Replay.ReplayData.FinishTime;
                var aspect = MathUtils.Map(songTime, 0, replayFinishTime, 0, 1);
                _backgroundFillMaterial.SetFloat(fillAspectProperty, aspect);
            }

            private void RefreshBackgroundMaterial() {
                var color = Item.Replay.OptionalReplayData?.AccentColor ?? Color.clear;
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

            public Vector2 GetPosition() {
                var world = _actualContent.position;
                return ContentTransform.parent.InverseTransformPoint(world);
            }

            public void StartAnimatedMove(Vector2 sourcePosition) {
                var world = ContentTransform.parent.TransformPoint(sourcePosition);
                _actualContent.position = world;
            }

            private void UpdateMoveAnimation() {
                _actualContent.localPosition = Vector3.Lerp(
                    _actualContent.localPosition,
                    Vector3.zero,
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
                if (_scoreEventsProcessor!.QueueIsBeingAdjusted) return;
                RefreshScoreWithoutNotice(node);
                _playerList.NotifyCellUpdateRequired(Item!, _currentNoteEvent);
            }

            private void RefreshScoreWithoutNotice(LinkedListNode<ScoreEvent>? node = null) {
                node ??= _scoreEventsProcessor!.CurrentScoreEvent;
                _scoreText.Text = $"{node?.Value.score ?? 0}";
            }

            #endregion

            #region Callbacks

            private NoteEvent _currentNoteEvent;

            private void HandleScoreEventDequeued(LinkedListNode<ScoreEvent> node) {
                RefreshScore(node);
            }

            private void HandleScoreEventQueueAdjustFinished() {
                RefreshScore(_scoreEventsProcessor!.CurrentScoreEvent);
            }

            private void HandleNoteEventDequeued(LinkedListNode<NoteEvent> node) {
                _currentNoteEvent = node.Value;
                if (node.Value.eventType is NoteEvent.NoteEventType.Miss) {
                    _fullCombo = false;
                }
            }

            private void HandleNoteEventQueueAdjustStarted() {
                _fullCombo = true;
            }

            #endregion
        }

        #endregion

        #region Sorting

        private class VirtualPlayerComparator : IComparer<IVirtualPlayer> {
            public int Compare(IVirtualPlayer x, IVirtualPlayer y) {
                var xScore = GetScore(x);
                var yScore = GetScore(y);
                return Comparer<int>.Default.Compare(yScore, xScore);
            }

            private static int GetScore(IVirtualPlayer player) {
                return player.ReplayScoreEventsProcessor.CurrentScoreEvent?.Value.score ?? 0;
            }
        }

        private readonly VirtualPlayerComparator _virtualPlayerComparator = new();

        private void RefreshSorting() {
            Items.Sort(_virtualPlayerComparator);
        }

        #endregion

        #region Setup

        private IBeatmapTimeController? _timeController;
        private IVirtualPlayersManager? _playersManager;

        public void Setup(IBeatmapTimeController? timeController, IVirtualPlayersManager? playersManager) {
            if (_playersManager != null) {
                _playersManager.PrimaryPlayerWasChangedEvent -= HandlePrimaryPlayerChanged;
            }
            _timeController = timeController;
            _playersManager = playersManager;
            if (_playersManager != null) {
                _playersManager.PrimaryPlayerWasChangedEvent += HandlePrimaryPlayerChanged;
                _primaryPlayer = _playersManager.PrimaryPlayer;
            }
        }

        protected override void OnCellConstruct(Cell cell) {
            if (_timeController == null) throw new UninitializedComponentException();
            cell.Setup(_timeController, this);
        }

        protected override void OnUpdate() {
            UpdateHandleAnimation();
            UpdateCellAnimation();
        }

        protected override void OnInitialize() {
            CreateHandle();
        }

        protected override void OnStart() {
            Refresh();
        }

        #endregion

        #region Cell Handling

        private readonly Dictionary<IVirtualPlayer, NoteEvent> _noteEvents = new();
        private float _lastReportedTime;
        private bool _cellUpdateRequired;
        private bool _forceUpdateRequired;

        private void NotifyCellForceUpdateRequired() {
            _forceUpdateRequired = true;
            _cellUpdateRequired = true;
        }

        private void NotifyCellUpdateRequired(IVirtualPlayer player, NoteEvent noteEvent) {
            _noteEvents[player] = noteEvent;
            _cellUpdateRequired = true;
        }

        private bool CompareNoteEvents(NoteEvent x, NoteEvent y) {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return x.spawnTime == y.spawnTime && x.noteId == y.noteId;
        }

        private bool CanUpdate() {
            NoteEvent? previousNoteEvent = null;
            foreach (var (_, noteEvent) in _noteEvents) {
                if (previousNoteEvent != null && !CompareNoteEvents(noteEvent, previousNoteEvent.Value)) return false;
                previousNoteEvent = noteEvent;
            }
            return true;
        }

        private void UpdateCellAnimation() {
            var time = Time.time;
            if (_cellUpdateRequired && time - _lastReportedTime > 0.1f && (_forceUpdateRequired || CanUpdate())) {
                Refresh();
                _lastReportedTime = time;
                _cellUpdateRequired = false;
                _forceUpdateRequired = false;
            }
        }

        #endregion

        #region Cell Animation

        private readonly Dictionary<IVirtualPlayer, Vector2> _cellPositions = new();

        private void SaveCellsPosition() {
            foreach (var (cell, item) in SpawnedCells) {
                _cellPositions[item] = cell.GetPosition();
            }
        }

        private void StartCellsAnimation() {
            foreach (var (cell, item) in SpawnedCells) {
                if (!_cellPositions.TryGetValue(item, out var oldPosition)) continue;
                var actualCellPosition = cell.GetPosition();
                if (actualCellPosition == oldPosition) continue;
                cell.StartAnimatedMove(oldPosition);
            }
        }

        #endregion

        #region Handle Animation

        private IVirtualPlayer _primaryPlayer = null!;
        private RectTransform _handleTransform = null!;

        private void RefreshHandle() {
            _handleTransform.gameObject.SetActive(Items.Count > 1);
            foreach (var (cell, item) in SpawnedCells) {
                if (item != _primaryPlayer) continue;
                _handleTransform.SetParent(cell.ContentTransform, true);
            }
        }

        private void CreateHandle() {
            var handle = new Image {
                ContentTransform = {
                    pivot = new(1f, 0.5f)
                },
                Sprite = BundleLoader.Sprites.triangleIcon,
                Material = BundleLoader.Materials.uiNoDepthMaterial,
                Color = new(0.1f, 0.1f, 0.1f)
            }.WithSizeDelta(4f, 6f).Bind(ref _handleTransform);
            handle.Use(ScrollContent);
        }

        private void UpdateHandleAnimation() {
            _handleTransform.localPosition = Vector3.Lerp(
                _handleTransform.localPosition,
                new(0f, CellSize / 2f, 0f),
                Time.deltaTime * 4f
            );
        }

        #endregion

        #region Callbacks

        protected override void OnEarlyRefresh() {
            SaveCellsPosition();
            RefreshSorting();
        }

        protected override void OnRefresh() {
            RefreshHandle();
            StartCellsAnimation();
        }

        private void HandlePrimaryPlayerChanged(IVirtualPlayer player) {
            _primaryPlayer = player;
            RefreshHandle();
        }

        #endregion
    }
}