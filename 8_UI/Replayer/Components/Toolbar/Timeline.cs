using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using BeatLeader.Models;

namespace BeatLeader.Components
{
    internal class Timeline : ReeUIComponentV2
    {
        #region UI Components 

        [UIComponent("container")] private readonly RectTransform _container;
        [UIComponent("fill-area")] private readonly RectTransform _fillArea;
        [UIComponent("marks-area")] private readonly RectTransform _marksArea;
        [UIComponent("marks-area-container")] private readonly RectTransform _marksAreaContainer;
        [UIComponent("background")] private readonly Image _background;
        [UIComponent("handle")] private readonly Image _handle;
        [UIComponent("fill")] private readonly Image _fill;

        private TimelineAnimator _timelineAnimator;
        private Slider _slider;

        #endregion

        #region Setup

        private IReplayPauseController _pauseController;
        private IBeatmapTimeController _beatmapTimeController;
        private Replay _replay;
        private bool _wasPausedBeforeRewind;

        public void Setup(
            Replay replay,
            IReplayPauseController pauseController,
            IBeatmapTimeController beatmapTimeController)
        {
            _replay = replay;
            _pauseController = pauseController;
            _beatmapTimeController = beatmapTimeController;

            SetupSlider();
            SetupMarkers();
        }

        protected override void OnInitialize()
        {
            _background.isMaskingGraphic = false;
            _handle.transform.localScale = Vector2.zero;
            _marksArea.sizeDelta = new Vector2(50, 2);

            _slider = _container.gameObject.AddComponent<Slider>();
            _slider.targetGraphic = _handle;
            _slider.handleRect = _handle.rectTransform;
            _slider.fillRect = _fill.rectTransform;
            _slider.minValue = 0;
            _slider.onValueChanged.AddListener(HandleSliderValueChanged);

            _timelineAnimator = _slider.gameObject.AddComponent<TimelineAnimator>();
            _timelineAnimator.HandlePressedEvent += HandleSliderPressed;
            _timelineAnimator.HandleReleasedEvent += HandleSliderReleased;
            _timelineAnimator.Setup(_background.rectTransform,
                _handle.rectTransform, _marksAreaContainer, _fillArea);

            _missPrefab = new GameObject("MissIcon").AddComponent<Image>();
            _missPrefab.sprite = BSMLUtility.LoadSprite("#bad-cut-icon");
            _missPrefab.color = Color.red;

            _bombPrefab = new GameObject("BombIcon").AddComponent<Image>();
            _bombPrefab.sprite = BSMLUtility.LoadSprite("#bad-cut-icon");
            _bombPrefab.color = Color.yellow;
        }

        private void SetupMarkers()
        {
            GenerateMarkers(_replay.notes
                .Where(x => x.eventType == NoteEventType.miss || x.eventType == NoteEventType.bad)
                .Select(x => x.eventTime), _missPrefab.gameObject);

            GenerateMarkers(_replay.notes
                .Where(x => x.eventType == NoteEventType.bomb)
                .Select(x => x.eventTime), _bombPrefab.gameObject);
        }
        private void SetupSlider()
        {
            _slider.minValue = _beatmapTimeController.SongStartTime;
            _slider.maxValue = _beatmapTimeController.SongEndTime;
        }

        #endregion

        #region Event Handlers

        private void HandleSliderValueChanged(float value)
        {
            _beatmapTimeController?.Rewind(value, false);
        }
        private void HandleSliderReleased()
        {
            if (!_wasPausedBeforeRewind)
                _pauseController?.Resume(true);
        }
        private void HandleSliderPressed()
        {
            _wasPausedBeforeRewind = _pauseController?.IsPaused ?? false;
        }

        #endregion

        #region Logic

        private void Update()
        {
            _slider.SetValueWithoutNotify(_beatmapTimeController.SongTime);
        }

        #endregion

        #region Marks

        private Image _missPrefab;
        private Image _bombPrefab;

        private Dictionary<GameObject, List<GameObject>> _marks = new();

        public void GenerateMarkers(IEnumerable<float> times, GameObject prefab)
        {
            ClearMarkers(prefab);
            List<GameObject> marks = new List<GameObject>();
            foreach (var item in times)
            {
                GameObject instance = Instantiate(prefab, _marksArea, false);
                RectTransform instanceRect = instance.GetComponent<RectTransform>();
                instanceRect.localPosition = new Vector2(MapTimelineMarker(item), 0);
                instanceRect.sizeDelta = CalculateMarkerSize();

                marks.Add(instance);
            }
            _marks.Add(prefab, marks);
        }
        private void ClearMarkers(GameObject prefab)
        {
            if (_marks.TryGetValue(prefab, out List<GameObject> marks)) marks.Clear();
        }

        private Vector2 CalculateMarkerSize()
        {
            return new Vector2(_marksArea.sizeDelta.y, _marksArea.sizeDelta.y);
        }
        private float MapTimelineMarker(float time)
        {
            float marksArXDiv2 = _marksArea.sizeDelta.x / 2;
            float markXDiv2 = CalculateMarkerSize().x / 2;

            float val = MathUtils.Map(time, _beatmapTimeController.SongStartTime
                , _beatmapTimeController.SongEndTime, -marksArXDiv2, marksArXDiv2);

            if (marksArXDiv2 - Mathf.Abs(val) < markXDiv2)
            {
                float pos = marksArXDiv2 - markXDiv2;
                val = val < 0 ? (-pos) : pos;
            }
            return val;
        }

        #endregion
    }
}
