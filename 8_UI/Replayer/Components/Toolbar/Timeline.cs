using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.UI.BSML_Addons;
using BeatLeader.Replays.Managers;
using BeatLeader.Replays;
using BeatLeader.Utils;
using BeatLeader.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader.Components
{
    internal class Timeline : ReeUIComponentV2WithContainer
    {
        #region Components 

        [UIComponent("container")]
        private RectTransform _container;

        [UIComponent("clickable-area")]
        private RectTransform _clickableArea;

        [UIComponent("fill-area")]
        private RectTransform _fillArea;

        [UIComponent("marks-area")]
        private RectTransform _marksArea;

        [UIComponent("marks-area-container")]
        private RectTransform _marksAreaContainer;

        [UIComponent("background")]
        private Image _background;

        [UIComponent("handle")]
        private Image _handle;

        [UIComponent("fill")]
        private Image _fill;

        private Slider _slider;

        #endregion

        #region Configuration

        private const float _handleExpandSize = 0.8f;
        private const float _backgroundExpandSize = 1f;
        private const float _animationFrameRate = 60f;
        private const float _expandTransitionDuration = 0.15f;
        private const float _shrinkTransitionDuration = 0.1f;

        #endregion

        #region Setup

        protected override void OnInitialize()
        {
            _background.isMaskingGraphic = false;
            _handle.transform.localScale = Vector3.zero;
            _marksArea.sizeDelta = new Vector2(50, 2);

            _slider = _container.gameObject.AddComponent<Slider>();
            _slider.targetGraphic = _handle;
            _slider.handleRect = _handle.rectTransform;
            _slider.fillRect = _fill.rectTransform;
            _slider.minValue = 0;
            _slider.maxValue = _playbackController.TotalSongTime;
            _slider.onValueChanged.AddListener(x => _playbackController.Rewind(x));

            _uiManager.onRaycast += OnRaycast;

            _missPrefab = new GameObject("MissIcon").AddComponent<Image>();
            _missPrefab.sprite = BSMLUtility.LoadSprite("#bad-cut-icon");
            _missPrefab.color = Color.red;

            _bombPrefab = new GameObject("BombIcon").AddComponent<Image>();
            _bombPrefab.sprite = BSMLUtility.LoadSprite("#bad-cut-icon");
            _bombPrefab.color = Color.yellow;

            GenerateMarkers(_replay.notes.Where(x => x.eventType == Models.NoteEventType.miss || x.eventType == Models.NoteEventType.bad)
                .Select(x => x.eventTime), _missPrefab.gameObject);
            GenerateMarkers(_replay.notes.Where(x => x.eventType == Models.NoteEventType.bomb)
                .Select(x => x.eventTime), _bombPrefab.gameObject);
        }

        #endregion

        #region Logic

        [Inject] private readonly PlaybackController _playbackController;
        [Inject] private readonly UI2DManager _uiManager;
        [Inject] private readonly Models.Replay _replay;

        private bool _focusWasLost;
        private bool _lastRaycastWasValid;

        private void Update()
        {
            _slider.value = _playbackController.CurrentSongTime;

            if (_focusWasLost && !Input.GetMouseButton(0))
            {
                _normalTrigger = true;
                _focusWasLost = false;
            }

            if (_inProcess) return;
            if (_highlightedTrigger)
                SetState(true, x => _highlightedTrigger = false);
            else if (_normalTrigger)
                SetState(false, x => _normalTrigger = false);
        }
        private void OnRaycast(List<RaycastResult> results)
        {
            bool flag = false;
            foreach (var raycast in results)
            {
                if (raycast.gameObject == _clickableArea.gameObject)
                {
                    flag = true;
                    if (!_lastRaycastWasValid)
                    {
                        _highlightedTrigger = true;
                        _focusWasLost = false;
                        _lastRaycastWasValid = true;
                    }
                }
            }
            if (!flag && _lastRaycastWasValid)
            {
                _focusWasLost = true;
                _lastRaycastWasValid = false;
            }
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
            float val = MathUtils.Map(time, 0 /* song start offset */, _playbackController.TotalSongTime,
                -(_marksArea.sizeDelta.x / 2), _marksArea.sizeDelta.x / 2);
            if ((_marksArea.sizeDelta.x / 2) - val.ToPositive() < (CalculateMarkerSize().x / 2))
            {
                float pos = (_marksArea.sizeDelta.x / 2) - (CalculateMarkerSize().x / 2);
                val = val < 0 ? -pos : pos;
            }
            return val;
        }

        #endregion

        #region Animation

        private bool _normalTrigger;
        private bool _highlightedTrigger;
        private bool _expanded;
        private bool _inProcess;

        private void SetState(bool highlighted = true, Action<bool> callback = null)
        {
            if (_inProcess) return;
            _inProcess = true;
            StartCoroutine(AnimationCoroutine(
                highlighted ? _expandTransitionDuration : _shrinkTransitionDuration,
                highlighted, callback));
        }
        private IEnumerator AnimationCoroutine(float duration, bool expand, Action<bool> callback = null) //pain
        {
            if ((_expanded && expand) || (!_expanded && !expand))
            {
                _inProcess = false;
                callback?.Invoke(false);
                yield break;
            }
            float totalFramesCount = Mathf.FloorToInt(duration * _animationFrameRate);
            float frameDuration = duration / totalFramesCount;
            float sizeStepBG = _backgroundExpandSize / totalFramesCount;
            float sizeStepHandle = _handleExpandSize / totalFramesCount;
            for (int frame = 0; frame < totalFramesCount; frame++)
            {
                Vector2 nextSizeBG = new Vector2(_background.rectTransform.sizeDelta.x,
                    expand ? _background.rectTransform.sizeDelta.y + sizeStepBG : _background.rectTransform.sizeDelta.y - sizeStepBG);
                Vector2 nextSizeFillArea = new Vector2(_fillArea.sizeDelta.x,
                    expand ? _fillArea.sizeDelta.y + sizeStepBG : _fillArea.sizeDelta.y - sizeStepBG);
                Vector2 nextSizeHandle = new Vector2(
                    expand ? _handle.rectTransform.localScale.x + sizeStepHandle : _handle.rectTransform.localScale.x - sizeStepHandle,
                    expand ? _handle.rectTransform.localScale.y + sizeStepHandle : _handle.rectTransform.localScale.y - sizeStepHandle);
                Vector2 nextPosMarksArea = new Vector2(_marksAreaContainer.localPosition.x,
                expand ? _marksAreaContainer.localPosition.y - (sizeStepBG / 2) : _marksAreaContainer.localPosition.y + (sizeStepBG / 2));

                _marksAreaContainer.localPosition = nextPosMarksArea;
                _background.rectTransform.sizeDelta = nextSizeBG;
                _fillArea.sizeDelta = nextSizeFillArea;
                _handle.rectTransform.localScale = nextSizeHandle;

                yield return new WaitForSeconds(frameDuration);
            }

            _expanded = expand;
            _inProcess = false;
            callback?.Invoke(true);
        }

        #endregion
    }
}
