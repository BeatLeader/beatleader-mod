using UnityEngine;
using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using HMUI;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using static BeatLeader.Models.AbstractReplay.NoteEvent.NoteEventType;
using MathUtils = Reactive.MathUtils;

namespace BeatLeader.UI.Replayer {
    internal class Timeline : ReactiveComponent, IReplayTimeline {
        #region Setup

        private IReplayPauseController? _pauseController;
        private IReplayTimeController? _timeController;
        private IVirtualPlayersManager? _playersManager;
        private ReplayerUISettings? _uiSettings;

        private bool _allowTimeUpdate;
        private bool _wasPausedBeforeRewind;

        public void Setup(
            IVirtualPlayersManager playersManager,
            IReplayPauseController pauseController,
            IReplayTimeController timeController,
            ReplayerUISettings replayerSettings
        ) {
            if (_playersManager != null) {
                _playersManager.PrimaryPlayerWasChangedEvent -= HandlePrimaryPlayerChangedEvent;
            }
            _playersManager = playersManager;
            _pauseController = pauseController;
            _timeController = timeController;
            _uiSettings = replayerSettings;
            _playersManager.PrimaryPlayerWasChangedEvent += HandlePrimaryPlayerChangedEvent;

            SetValueRange(_timeController.SongStartTime, _timeController.ReplayEndTime);
            ReloadMarkers();
            _allowTimeUpdate = true;
        }

        protected override void OnDestroy() {
            if (_playersManager != null) {
                _playersManager.PrimaryPlayerWasChangedEvent -= HandlePrimaryPlayerChangedEvent;
            }
        }

        protected override void OnUpdate() {
            if (_allowTimeUpdate) {
                SetValueSilent(_timeController!.SongTime);
            }
        }

        #endregion

        #region Value

        private float _minValue;
        private float _maxValue = 1f;
        private float _value;
        private bool _isPressed;

        private float MaxHandlePosition => _slidingArea.rect.width - _handle.rect.width;

        private void SetValueRange(float minValue, float maxValue) {
            _minValue = minValue;
            _maxValue = maxValue;
            SetValueSilent(_value);
        }

        private void SetValueSilent(float value) {
            if (_isPressed) {
                return;
            }
            _value = Mathf.Clamp(value, _minValue, _maxValue);
            PlaceHandle();
        }

        private void PlaceHandle() {
            var pos = MathUtils.Map(_value, _minValue, _maxValue, 0f, MaxHandlePosition);
            _handle.localPosition = new(pos, 0f, 0f);
        }

        protected override void OnRectDimensionsChanged() {
            PlaceHandle();
        }

        protected override void OnLayoutApply() {
            PlaceHandle();
        }

        #endregion

        #region Markers

        private class MarkerGroup : ReactiveComponent {
            public float MarkerScale {
                set {
                    foreach (var marker in _markersPool.SpawnedComponents) {
                        marker.ContentTransform.localScale = new(value, 1f, 1f);
                    }
                }
            }

            private readonly ReactivePool<Image> _markersPool = new();
            private readonly List<float> _markerTimes = new();
            private float _totalTime;
            private Sprite? _markerSprite;
            private Color _markerColor;

            public void Setup(float totalTime, IEnumerable<float> markerTimes, Sprite markerSprite, Color markerColor) {
                _totalTime = totalTime;
                _markerSprite = markerSprite;
                _markerColor = markerColor;
                _markerTimes.Clear();
                _markerTimes.AddRange(markerTimes);
                ReloadMarkers();
            }

            private void ReloadMarkers() {
                var maxSize = ContentTransform.rect.width;
                _markersPool.DespawnAll();
                foreach (var time in _markerTimes) {
                    //creating marker
                    var marker = _markersPool.Spawn();
                    marker.Material = GameResources.UINoGlowMaterial;
                    marker.Sprite = _markerSprite;
                    marker.Color = _markerColor;
                    marker.Use(ContentTransform);
                    marker.WithSizeDelta(2f, 2f);
                    //placing marker
                    var pos = MathUtils.Map(time, 0f, _totalTime, 0f, maxSize);
                    marker.ContentTransform.localPosition = new(pos, 0f, 0f);
                }
            }

            protected override void OnRectDimensionsChanged() {
                ReloadMarkers();
            }

            protected override void OnInitialize() {
                ContentTransform.pivot = new(0f, 0.5f);
            }
        }

        private struct MarkerData {
            public string name;
            public Color color;
            public Sprite sprite;
            public Func<IReplay, IEnumerable<float>> getTimesDelegate;
        }

        public IReadOnlyCollection<string> AvailableMarkers => markerNames;

        private static readonly string[] markerNames = {
            "Miss", "Bomb", "Pause", "Wall"
        };

        private static readonly MarkerData[] markers = {
            new() {
                name = "Miss",
                color = Color.red,
                sprite = BundleLoader.CrossIcon,
                getTimesDelegate = x => x.NoteEvents
                    .Where(y => y.eventType is Miss or BadCut)
                    .Select(y => y.CutTime)
            },
            new() {
                name = "Bomb",
                color = Color.yellow,
                sprite = BundleLoader.CrossIcon,
                getTimesDelegate = x => x.NoteEvents
                    .Where(y => y.eventType is BombCut)
                    .Select(y => y.CutTime)
            },
            new() {
                name = "Pause",
                color = Color.blue,
                sprite = BundleLoader.PauseIcon,
                getTimesDelegate = x => x.PauseEvents.Select(y => y.time)
            },
            new() {
                name = "Wall",
                color = Color.magenta,
                sprite = BundleLoader.Sprites.background,
                getTimesDelegate = x => x.WallEvents.Select(y => y.time)
            }
        };

        private readonly Dictionary<string, MarkerGroup> _markerGroups = new();
        private readonly ReactivePool<MarkerGroup> _markerGroupsPool = new();

        public void SetMarkersEnabled(string name, bool enable = true) {
            if (!_markerGroups.TryGetValue(name, out var group)) return;
            group.Enabled = enable;
            var flag = name switch {
                "Miss" => TimelineMarkersMask.Miss,
                "Bomb" => TimelineMarkersMask.Bomb,
                "Pause" => TimelineMarkersMask.Pause,
                "Wall" => TimelineMarkersMask.Wall,
                _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
            };
            var mask = _uiSettings!.MarkersMask;
            _uiSettings.MarkersMask = enable ? mask | flag : mask & ~flag;
        }

        public bool GetMarkersEnabled(string name) {
            var mask = _uiSettings!.MarkersMask;
            return name switch {
                "Miss" => (TimelineMarkersMask.Miss & mask) != 0,
                "Bomb" => (TimelineMarkersMask.Bomb & mask) != 0,
                "Pause" => (TimelineMarkersMask.Pause & mask) != 0,
                "Wall" => (TimelineMarkersMask.Wall & mask) != 0,
                _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
            };
        }

        private void ReloadMarkers() {
            _markerGroupsPool.DespawnAll();
            _markerGroups.Clear();
            var replay = _playersManager!.PrimaryPlayer.Replay;
            foreach (var marker in markers) {
                var group = _markerGroupsPool.Spawn();
                group.Use(_groupsArea);
                group.WithRectExpand();
                //setting the group up
                group.Setup(
                    _timeController!.ReplayEndTime,
                    marker.getTimesDelegate(replay),
                    marker.sprite,
                    marker.color
                );
                group.Enabled = GetMarkersEnabled(marker.name);
                //adding to the dict
                _markerGroups[marker.name] = group;
            }
        }

        #endregion

        #region Construct

        private RectTransform _slidingArea = null!;
        private RectTransform _handle = null!;
        private RectTransform _groupsArea = null!;

        private State<GraphicState> _graphicState = null!;
        private AnimatedState<float> _backgroundScale = null!;
        private AnimatedState<Color> _backgroundColor = null!;

        protected override GameObject Construct() {
            _graphicState = Remember(GraphicState.None);
            _backgroundScale = RememberAnimated(1f, 15.fact);
            _backgroundColor = RememberAnimated(UIStyle.InputColorSet.Color, 15.fact);

            _graphicState.AddCallback(
                x => {
                    var focused = x.IsHovered || x.IsActive;

                    _backgroundScale.TargetValue = focused ? 1.4f : 1f;
                    _backgroundColor.TargetValue = UIStyle.InputColorSet.GetColor(focused ? GraphicState.Hovered : GraphicState.None);
                },
                lazy: true
            );

            _backgroundScale.AddCallback(
                x => {
                    foreach (var group in _markerGroupsPool.SpawnedComponents) {
                        group.MarkerScale = x;
                    }
                },
                lazy: true
            );

            return new Background {
                FlexController = {
                    Padding = new() { left = 1f, right = 1f },
                    Overflow = Overflow.Visible,
                    AlignItems = Align.Center
                },

                ContentTransform = {
                    slocalScale = _backgroundScale.Map(static x => new Vector3(1f, x))
                },
                Sprite = BundleLoader.Sprites.background,
                PixelsPerUnit = 12f,
                Material = GameResources.UINoGlowMaterial,
                sColor = _backgroundColor,
                Children = {
                    //sliding area
                    new Background {
                        FlexItem = {
                            FlexGrow = 1f,
                            Size = new() { y = 120.pct }
                        },

                        ContentTransform = {
                            pivot = new(0f, 0.5f)
                        },
                        Sprite = BundleLoader.TransparentPixel,
                        Children = {
                            new Layout {
                                ContentTransform = {
                                    pivot = Vector2.zero
                                }
                            }.Bind(ref _groupsArea).WithRectExpand(),
                            //handle
                            new Image {
                                ContentTransform = {
                                    anchorMin = new(0.5f, 0f),
                                    anchorMax = new(0.5f, 1f),
                                    sizeDelta = new(1f, 0f),
                                    pivot = new(0f, 0.5f)
                                },
                                Sprite = BundleLoader.Sprites.background,
                                PixelsPerUnit = 30f,
                                Color = Color.white.ColorWithAlpha(0.8f)
                            }.Bind(ref _handle)
                        }
                    }.WithPointerEvents(
                        onDown: HandlePointerDown,
                        onUp: HandlePointerUp,
                        onUpdate: HandlePointerUpdated
                    ).Bind(ref _slidingArea)
                }
            }.Use();
        }

        protected override void OnInitialize() {
            this.FlexItem.Size = new() { y = 4f };
        }

        #endregion

        #region Callbacks

        private void HandlePrimaryPlayerChangedEvent(IVirtualPlayer player) {
            ReloadMarkers();
        }

        private void HandlePointerUpdated(PointerEventsHandler handler) {
            _graphicState.IsHovered = handler.IsHovered;
            _graphicState.IsActive = handler.IsPressed;
            _isPressed = handler.IsPressed;

            if (!handler.IsPressed || handler.EventData == null) {
                return;
            }

            var canvasSettings = Content.GetComponentInParent<CurvedCanvasSettings>();
            var pos = handler.EventData.TranslateToLocalPoint(_slidingArea, Canvas!, canvasSettings).x;
            pos = Mathf.Clamp(pos, 0f, MaxHandlePosition);

            _value = MathUtils.Map(pos, 0f, MaxHandlePosition, _minValue, _maxValue);
            PlaceHandle();
            _timeController?.Rewind(_value, false);
        }

        private void HandlePointerDown(PointerEventsHandler handler) {
            _wasPausedBeforeRewind = _pauseController?.IsPaused ?? false;
            _allowTimeUpdate = false;
        }

        private void HandlePointerUp(PointerEventsHandler handler) {
            if (!_wasPausedBeforeRewind) {
                _pauseController?.Resume();
            }
            _allowTimeUpdate = true;
        }

        #endregion
    }
}