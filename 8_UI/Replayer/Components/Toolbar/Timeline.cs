using System.Linq;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Utils;
using UnityEngine.UI;
using UnityEngine;
using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;

namespace BeatLeader.Components {
    internal class Timeline : ReeUIComponentV2 {
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
        private IBeatmapTimeController _beatmapTimeController = null!;
        private IVirtualPlayersManager _playersManager = null!;
        private ReplayLaunchData _launchData = null!;

        private bool _allowTimeUpdate = true;
        private bool _allowRewind;
        private bool _wasPausedBeforeRewind;

        public void Setup(
            IVirtualPlayersManager playersManager,
            IReplayPauseController pauseController,
            IBeatmapTimeController beatmapTimeController,
            ReplayLaunchData launchData) {
            OnDispose();
            _launchData = launchData;
            _playersManager = playersManager;
            _pauseController = pauseController;
            _beatmapTimeController = beatmapTimeController;
            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChangedEvent;

            SetupSlider();
            SetupMarkers();
            _timelineAnimator.Setup(_background.rectTransform,
                _handle.rectTransform, _marksAreaContainer, _fillArea);

            HandlePriorityPlayerChangedEvent(playersManager.PriorityPlayer);
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
            _slider.onValueChanged.AddListener(HandleSliderValueChanged);

            _timelineAnimator = _slider.gameObject.AddComponent<TimelineAnimator>();
            _timelineAnimator.HandlePressedEvent += HandleSliderPressed;
            _timelineAnimator.HandleReleasedEvent += HandleSliderReleased;
        }

        private void SetupMarkers() {
            _missPrefab = new GameObject("MissIcon").AddComponent<Image>();
            _missPrefab.sprite = BundleLoader.CrossIcon;
            _missPrefab.color = Color.red;

            _bombPrefab = new GameObject("BombIcon").AddComponent<Image>();
            _bombPrefab.sprite = BundleLoader.CrossIcon;
            _bombPrefab.color = Color.yellow;

            _pausePrefab = new GameObject("PauseIcon").AddComponent<Image>();
            _pausePrefab.sprite = BundleLoader.PauseIcon;
            _pausePrefab.color = Color.blue;
        }

        private void SetupSlider() {
            _slider.minValue = _beatmapTimeController.SongStartTime;
            _slider.maxValue = EndTime;
        }

        #endregion

        #region Callbacks

        private void HandlePriorityPlayerChangedEvent(VirtualPlayer player) {
            GenerateDefaultMarkersFromReplay(player.Replay);
        }

        private void HandleSliderValueChanged(float value) {
            if (_allowRewind)
                _beatmapTimeController?.Rewind(value, false);
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

        private float EndTime {
            get {
                var failTime = !_launchData.IsBattleRoyale ? _launchData.MainReplay.info.failTime : 0;
                return failTime <= 0 ? _beatmapTimeController.SongEndTime : failTime; 
            }
        }

        private void Update() {
            if (_allowTimeUpdate) {
                _slider.SetValueWithoutNotify(_beatmapTimeController.SongTime);
            }
        }

        #endregion

        #region Marks

        private Image _missPrefab = null!;
        private Image _bombPrefab = null!;
        private Image _pausePrefab = null!;

        private readonly Dictionary<GameObject, List<GameObject>> _marks = new();

        private void GenerateDefaultMarkersFromReplay(Replay replay) {
            GenerateMarkers(replay.notes
                .Where(x => x.eventType == NoteEventType.miss || x.eventType == NoteEventType.bad)
                .Select(x => x.eventTime), _missPrefab.gameObject);

            GenerateMarkers(replay.notes
                .Where(x => x.eventType == NoteEventType.bomb)
                .Select(x => x.eventTime), _bombPrefab.gameObject);

            GenerateMarkers(replay.pauses
                .Select(x => x.time), _pausePrefab.gameObject);
        }

        private void GenerateMarkers(IEnumerable<float> times, GameObject prefab) {
            ClearMarkers(prefab);
            var marks = new List<GameObject>();
            foreach (var item in times) {
                var instance = Instantiate(prefab, _marksArea, false);
                var instanceRect = instance.GetComponent<RectTransform>();
                instanceRect.localPosition = new(MapTimelineMarker(item), 0);
                instanceRect.sizeDelta = CalculateMarkerSize();
                marks.Add(instance);
            }
            _marks.Add(prefab, marks);
        }

        private void ClearMarkers(GameObject prefab) {
            if (_marks.TryGetValue(prefab, out var marks)) marks.Clear();
        }

        private Vector2 CalculateMarkerSize() {
            return new(_marksArea.sizeDelta.y, _marksArea.sizeDelta.y);
        }

        private float MapTimelineMarker(float time) {
            var marksArXDiv2 = _marksArea.sizeDelta.x / 2;
            var markXDiv2 = CalculateMarkerSize().x / 2;
            var val = MathUtils.Map(time, _beatmapTimeController.SongStartTime, EndTime, -marksArXDiv2, marksArXDiv2);
            if (marksArXDiv2 - Mathf.Abs(val) < markXDiv2) {
                var pos = marksArXDiv2 - markXDiv2;
                val = val < 0 ? (-pos) : pos;
            }
            return val;
        }

        #endregion
    }
}
