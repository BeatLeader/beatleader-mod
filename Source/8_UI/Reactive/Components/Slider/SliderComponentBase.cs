using System;
using BeatLeader.Components;
using BeatLeader.Utils;
using HMUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.UI.Reactive.Components {
    internal abstract class SliderComponentBase : ReactiveComponent {
        #region Props

        public Range ValueRange {
            get => _range;
            set {
                _range = value;
                Refresh();
                NotifyPropertyChanged();
            }
        }

        public float Value {
            get => _value;
            set {
                if (!_canReceiveValues) return;
                _value = value;
                Refresh();
                NotifyPropertyChanged();
            }
        }

        public float ValueStep {
            get => _step;
            set {
                _step = value;
                Refresh();
                NotifyPropertyChanged();
            }
        }


        private Range _range = new(0f, 1f);
        private float _value;
        private float _step;
        private bool _canReceiveValues = true;

        public void SetValueSilent(float value) {
            if (!_canReceiveValues) return;
            _value = value;
            Refresh(true);
        }

        #endregion

        #region Input

        protected float MaxHandlePosition => SlidingAreaTransform.rect.width - HandleTransform.rect.width;

        protected virtual void Refresh(bool silent = false, bool forceRefreshValue = false) {
            RefreshValue(silent, forceRefreshValue);
            RefreshHandle();
        }

        private void RefreshValue(bool silent, bool forceRefreshValue) {
            var value = _value;
            _value = MathUtils.RoundStepped(_value, _step, ValueRange.Start);
            _value = Mathf.Clamp(_value, ValueRange.Start, ValueRange.End);
            if (!silent && (forceRefreshValue || Math.Abs(_value - value) > 0.001)) {
                NotifyPropertyChanged(nameof(Value));
            }
        }

        private void RefreshHandle() {
            var pos = MathUtils.Map(_value, ValueRange.Start, ValueRange.End, 0f, MaxHandlePosition);
            PlaceHandle(pos);
        }

        protected virtual void PlaceHandle(float pos) {
            HandleTransform.localPosition = new(pos, 0f, 0f);
        }

        #endregion

        #region Setup

        protected abstract PointerEventsHandler SlidingAreaEventsHandler { get; }
        protected abstract RectTransform SlidingAreaTransform { get; }
        protected abstract RectTransform HandleTransform { get; }

        protected override void OnRectDimensionsChanged() {
            Refresh();
        }

        protected override void OnLayoutApply() {
            Refresh();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 40f, y = 6f });
            SlidingAreaEventsHandler.PointerUpdatedEvent += HandlePointerUpdated;
        }

        #endregion

        #region Callbacks

        private void HandlePointerUpdated(PointerEventsHandler handler, PointerEventData eventData) {
            _canReceiveValues = !handler.IsPressed;
            if (!handler.IsPressed) return;
            var canvasSettings = Content.GetComponentInParent<CurvedCanvasSettings>();
            var pos = eventData.TranslateToLocalPoint(SlidingAreaTransform, Canvas!, canvasSettings).x;
            pos = Mathf.Clamp(pos, 0f, MaxHandlePosition);
            _value = MathUtils.Map(pos, 0f, MaxHandlePosition, ValueRange.Start, ValueRange.End);
            Refresh(forceRefreshValue: true);
        }

        #endregion
    }
}