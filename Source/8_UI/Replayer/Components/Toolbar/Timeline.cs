using UnityEngine;
using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine.EventSystems;
using static BeatLeader.Models.AbstractReplay.NoteEvent.NoteEventType;
using AnimationCurve = Reactive.AnimationCurve;
using MathUtils = Reactive.MathUtils;

namespace BeatLeader.UI.Replayer {
    internal class Timeline : SliderBase, IReplayTimeline {
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

            ValueRange = new(_timeController.SongStartTime, _timeController.ReplayEndTime);
            HandlePrimaryPlayerChangedEvent(playersManager.PrimaryPlayer);
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
            var mask = _uiSettings!.MarkersMask;
            _uiSettings!.MarkersMask = name switch {
                "Miss" => mask | TimelineMarkersMask.Miss,
                "Bomb" => mask | TimelineMarkersMask.Bomb,
                "Pause" => mask | TimelineMarkersMask.Pause,
                "Wall" => mask | TimelineMarkersMask.Wall,
                _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
            };
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

        protected override PointerEventsHandler SlidingAreaEventsHandler => _pointerEventsHandler;
        protected override RectTransform SlidingAreaTransform => _slidingArea;
        protected override RectTransform HandleTransform => _handle;

        private RectTransform _slidingArea = null!;
        private RectTransform _handle = null!;
        private RectTransform _groupsArea = null!;
        private ImageButton _background = null!;
        private PointerEventsHandler _pointerEventsHandler = null!;

        private AnimatedValue<float> _backgroundScale = null!;
        private AnimatedValue<Color> _backgroundColor = null!;

        protected override GameObject Construct() {
            _backgroundScale = RememberAnimated(1f, 15.fact());
            _backgroundColor = RememberAnimated(UIStyle.InputColorSet.Color, 15.fact());

            _backgroundScale.ValueChangedEvent += x => {
                foreach (var group in _markerGroupsPool.SpawnedComponents) {
                    group.MarkerScale = x;
                }
            };

            return new BackgroundButton {
                    Image = {
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 12f,
                        Material = GameResources.UINoGlowMaterial
                    },
                    Colors = null,
                    Children = {
                        //sliding area
                        new Background {
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
                        }.WithNativeComponent(out _pointerEventsHandler).With(_ => {
                                _pointerEventsHandler.PointerUpdatedEvent += HandlePointerUpdated;
                                _pointerEventsHandler.PointerDownEvent += HandlePointerDown;
                                _pointerEventsHandler.PointerUpEvent += HandlePointerUp;
                            }
                        ).AsFlexItem(
                            flexGrow: 1f,
                            size: new() { y = 120.pct() }
                        ).Bind(ref _slidingArea)
                    }
                }
                .AsFlexGroup(
                    padding: new() { left = 1f, right = 1f },
                    overflow: Overflow.Visible,
                    alignItems: Align.Center
                )
                .Animate(_backgroundScale, (x, y) => x.ContentTransform.localScale = new(1f, y), true)
                .Animate(_backgroundColor, x => x.Image.Color, true)
                .Bind(ref _background)
                .Use();
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.AsFlexItem(size: new() { y = 4f });
            this.WithListener(x => x.Value, HandleSliderValueChanged);
        }

        #endregion

        #region Callbacks

        private void HandlePrimaryPlayerChangedEvent(IVirtualPlayer player) {
            ReloadMarkers();
        }

        private void HandleSliderValueChanged(float value) {
            _timeController?.Rewind(value, false);
        }

        private void HandlePointerUpdated(PointerEventsHandler handler, PointerEventData eventData) {
            var dragging = handler.IsPressed || handler.IsHovered;

            _backgroundScale.Value = dragging ? 1.4f : 1f;
            _backgroundColor.Value = dragging ? UIStyle.InputColorSet.HoveredColor : UIStyle.InputColorSet.Color;
        }

        private void HandlePointerDown(PointerEventsHandler handler, PointerEventData eventData) {
            _wasPausedBeforeRewind = _pauseController?.IsPaused ?? false;
            _allowTimeUpdate = false;
        }

        private void HandlePointerUp(PointerEventsHandler handler, PointerEventData eventData) {
            if (!_wasPausedBeforeRewind) {
                _pauseController?.Resume();
            }
            _allowTimeUpdate = true;
        }

        #endregion
    }
}