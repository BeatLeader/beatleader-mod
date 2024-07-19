using System.Linq;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Utils;
using UnityEngine.UI;
using UnityEngine;
using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Models.AbstractReplay;
using System;

namespace BeatLeader.Components {
    internal class Timeline : ReeUIComponentV2, IReplayTimeline {
        #region UI Components

        [UIComponent("container")]
        private readonly RectTransform _container = null!;

        [UIComponent("fill-area")]
        private readonly RectTransform _fillArea = null!;

        [UIComponent("marks-area")]
        private readonly RectTransform _marksArea = null!;

        [UIComponent("marks-area-container")]
        private readonly RectTransform _marksAreaContainer = null!;

        [UIComponent("background")]
        private readonly Image _background = null!;

        [UIComponent("handle")]
        private readonly Image _handle = null!;

        [UIComponent("fill")]
        private readonly Image _fill = null!;

        private TimelineAnimator _timelineAnimator = null!;
        private Slider _slider = null!;

        #endregion

        #region Setup

        private IReplayPauseController _pauseController = null!;
        private IReplayTimeController _timeController = null!;
        private IVirtualPlayersManager _playersManager = null!;

        private bool _allowTimeUpdate = true;
        private bool _allowRewind;
        private bool _wasPausedBeforeRewind;

        public void Setup(
            IVirtualPlayersManager playersManager,
            IReplayPauseController pauseController,
            IReplayTimeController timeController) {
            OnDispose();
            _playersManager = playersManager;
            _pauseController = pauseController;
            _timeController = timeController;
            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChangedEvent;

            SetupSlider();
            SetupMarkers();
            _timelineAnimator.Setup(_background.rectTransform,
                _handle.rectTransform, _marksAreaContainer, _fillArea);

            HandlePriorityPlayerChangedEvent(playersManager.PriorityPlayer!);
            _allowRewind = true;
        }

        protected override void OnDispose() {
            if (_playersManager != null)
                _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChangedEvent;
        }

        protected override void OnInitialize() {
            _background.isMaskingGraphic = false;
            _handle.transform.localScale = Vector2.zero;
            _marksArea.sizeDelta = new Vector2(50, 2);

            _slider = _container.gameObject.AddComponent<Slider>();
            _slider.targetGraphic = _handle;
            _slider.handleRect = _handle.rectTransform;
            _slider.fillRect = _fill.rectTransform;
            _slider.navigation = new() { mode = Navigation.Mode.None };
            _slider.onValueChanged.AddListener(HandleSliderValueChanged);

            _timelineAnimator = _slider.gameObject.AddComponent<TimelineAnimator>();
            _timelineAnimator.HandlePressedEvent += HandleSliderPressed;
            _timelineAnimator.HandleReleasedEvent += HandleSliderReleased;
        }


        private void SetupSlider() {
            _slider.minValue = _timeController.SongStartTime;
            _slider.maxValue = _timeController.ReplayEndTime;
        }

        #endregion

        #region Callbacks

        private void HandlePriorityPlayerChangedEvent(VirtualPlayer player) {
            GenerateDefaultMarkersFromReplay(player.Replay!);
        }

        private void HandleSliderValueChanged(float value) {
            if (_allowRewind)
                _timeController?.Rewind(value, false);
        }

        private void HandleSliderReleased() {
            if (!_wasPausedBeforeRewind)
                _pauseController?.Resume(true);
            _allowTimeUpdate = true;
        }

        private void HandleSliderPressed() {
            _wasPausedBeforeRewind = _pauseController?.IsPaused ?? false;
            _allowTimeUpdate = false;
        }

        #endregion

        #region UpdateTime

        private void Update() {
            if (_allowTimeUpdate) {
                _slider.SetValueWithoutNotify(_timeController.SongTime);
            }
        }

        #endregion

        #region Default Marks

        private Image _missPrefab = null!;
        private Image _bombPrefab = null!;
        private Image _pausePrefab = null!;

        private void SetupMarkers() {
            _missPrefab = new GameObject("MissMark").AddComponent<Image>();
            _missPrefab.sprite = BundleLoader.CrossIcon;
            _missPrefab.color = Color.red;
            _missPrefab.material = BundleLoader.UIAdditiveGlowMaterial;

            _bombPrefab = new GameObject("BombMark").AddComponent<Image>();
            _bombPrefab.sprite = BundleLoader.CrossIcon;
            _bombPrefab.color = Color.yellow;
            _bombPrefab.material = BundleLoader.UIAdditiveGlowMaterial;

            _pausePrefab = new GameObject("PauseMark").AddComponent<Image>();
            _pausePrefab.sprite = BundleLoader.PauseIcon;
            _pausePrefab.color = Color.blue;
            _pausePrefab.material = BundleLoader.UIAdditiveGlowMaterial;
        }

        private void GenerateDefaultMarkersFromReplay(IReplay replay) {
            float firstNoteTime = replay.NoteEvents.FirstOrDefault().eventTime;
            float lastNoteTime = replay.NoteEvents.LastOrDefault().eventTime;

            GenerateMarkers(replay.NoteEvents
                .Where(x => x.eventType is NoteEvent.NoteEventType.Miss
                    or NoteEvent.NoteEventType.BadCut)
                .Select(x => x.eventTime), _missPrefab.gameObject);

            GenerateMarkers(replay.NoteEvents
                .Where(x => x.eventType == NoteEvent.NoteEventType.BombCut)
                .Select(x => x.eventTime), _bombPrefab.gameObject);

            GenerateMarkers(replay.PauseEvents
                .Where(x => x.time >= firstNoteTime && x.time <= lastNoteTime)
                .Select(x => x.time), _pausePrefab.gameObject);
        }

        #endregion

        #region Marks

        public event Action? MarkersWereGeneratedEvent;

        private readonly Dictionary<string, List<GameObject>> _marks = new();

        private void GenerateMarkers(IEnumerable<float> times, GameObject prefab) {
            if (_marks.TryGetValue(prefab.name, out var marks)) {
                marks.ForEach(static x => DestroyImmediate(x));
                marks.Clear();
            } else marks = new();
            foreach (var item in times) {
                var instance = Instantiate(prefab, _marksArea, false);
                var instanceRect = instance.GetComponent<RectTransform>();
                instanceRect.localPosition = new(MapTimelineMarker(item), 0);
                instanceRect.sizeDelta = CalculateMarkerSize();
                marks.Add(instance);
            }
            _marks[prefab.name] = marks;
            MarkersWereGeneratedEvent?.Invoke();
        }

        public void ShowMarkers(string name, bool show) {
            if (!_marks.TryGetValue(name, out var marks)) return;
            marks.ForEach(x => x.SetActive(show));
        }

        private Vector2 CalculateMarkerSize() {
            return new(_marksArea.sizeDelta.y, _marksArea.sizeDelta.y);
        }

        private float MapTimelineMarker(float time) {
            var marksArXDiv2 = _marksArea.sizeDelta.x / 2;
            var markXDiv2 = CalculateMarkerSize().x / 2;
            var val = MathUtils.Map(time, _timeController.SongStartTime,
                _timeController.ReplayEndTime, -marksArXDiv2, marksArXDiv2);
            if (marksArXDiv2 - Mathf.Abs(val) < markXDiv2) {
                var pos = marksArXDiv2 - markXDiv2;
                val = val < 0 ? (-pos) : pos;
            }
            return val;
        }

        #endregion
    }
}