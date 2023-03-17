using BeatLeader.Utils;
using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class AnchorButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler {
        #region Configuration

        private const float ExpandSize = 0.10f;
        private const float AnimationFrameRate = 60f;
        private const float ExpandTransitionDuration = 0.075f;
        private const float ShrinkTransitionDuration = 0.05f;
        private static readonly Color SelectedColor = Color.cyan;

        #endregion

        #region Events

        public event Action<AnchorButton> AnchorSelectedEvent;

        #endregion

        #region Setup

        public Vector2 size;
        public Vector2 anchor;
        public Sprite sprite;

        private TextureSplitter _splitter;
        private RectTransform _rect;
        private Image _image;

        private void Awake() {
            _rect = GetComponent<RectTransform>();
            _image = gameObject.AddComponent<Image>();
            _splitter = gameObject.AddComponent<TextureSplitter>();
            _splitter.image = _image;
        }
        public void Setup(Sprite sprite, Vector2 anchor, Vector2 size) {
            this.anchor = anchor;
            this.sprite = sprite;
            this.size = size;
            Refresh();
        }

        public void Refresh() {
            _splitter.point1 = anchor;
            _rect.pivot = anchor.Invert();
            _rect.sizeDelta = size;
            _image.sprite = sprite;
            _splitter.Refresh();
        }
        public void Select() {
            if (_anchorSelected) return;
            _splitter.color = SelectedColor;
            _splitter.Refresh();
            AnchorSelectedEvent?.Invoke(this);
            _anchorSelected = true;
        }
        public void RemoveSelection() {
            _splitter.color = Color.white;
            _splitter.Refresh();
            _anchorSelected = false;
        }

        #endregion

        #region Triggers

        private bool _highlightedStateTrigger;
        private bool _anchorSelected;

        private void Update() {
            StartAnimation(_highlightedStateTrigger);
        }

        public void OnPointerEnter(PointerEventData data) {
            _highlightedStateTrigger = true;
        }
        public void OnPointerExit(PointerEventData data) {
            _highlightedStateTrigger = false;
        }
        public void OnPointerDown(PointerEventData eventData) {
            Select();
        }

        #endregion

        #region Animation

        private bool _highlighted;
        private bool _isAnimating;

        private void StartAnimation(bool highlight) {
            if (_isAnimating || highlight == _highlighted) return;
            _isAnimating = true;
            StartCoroutine(AnimationCoroutine(highlight));
        }

        private IEnumerator AnimationCoroutine(bool highlight) {
            var duration = highlight ? ExpandTransitionDuration : ShrinkTransitionDuration;
            var totalFramesCount = Mathf.FloorToInt(duration * AnimationFrameRate);
            var frameDuration = duration / totalFramesCount;
            var sizeStep = ExpandSize / totalFramesCount;
            var vecStep = new Vector3(sizeStep, sizeStep);
            for (int frame = 0; frame < totalFramesCount; frame++) {
                _rect.localScale += highlight ? vecStep : -vecStep;
                yield return new WaitForSeconds(frameDuration);
            }
            _highlighted = highlight;
            _isAnimating = false;
        }

        #endregion
    }
}
