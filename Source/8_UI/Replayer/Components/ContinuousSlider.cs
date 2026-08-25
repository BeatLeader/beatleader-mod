using System;
using BeatLeader.Utils;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.UI.Replayer {
    internal class ContinuousSlider : Slider {
        private ContinuousSliderPointerUpdater? _pointerUpdater;

        protected override void OnInitialize() {
            base.OnInitialize();
            if (!InputUtils.UsesFPFC) {
                _pointerUpdater = new(SlidingAreaEventsHandler);
            }
        }

        protected override void OnUpdate() {
            base.OnUpdate();
            _pointerUpdater?.Tick();
        }

        protected override void OnDestroy() {
            _pointerUpdater?.Dispose();
            base.OnDestroy();
        }
    }

    internal sealed class ContinuousSliderPointerUpdater : IDisposable {
        private const float SmoothingSpeed = 18f;
        private const float MinAppliedMovementSqr = 0.00000001f;

        private readonly PointerEventsHandler _eventsHandler;
        private PointerEventData? _pointerEventData;
        private Vector3 _smoothedWorldPosition;
        private Vector3 _lastAppliedWorldPosition;

        public ContinuousSliderPointerUpdater(PointerEventsHandler eventsHandler) {
            _eventsHandler = eventsHandler;
            _eventsHandler.PointerDownEvent += HandlePointerDown;
            _eventsHandler.PointerUpEvent += HandlePointerUp;
        }

        public void Tick() {
            if (!_eventsHandler.IsPressed || _pointerEventData == null) return;

            var originalRaycast = _pointerEventData.pointerCurrentRaycast;
            var smoothing = 1f - Mathf.Exp(-SmoothingSpeed * Time.unscaledDeltaTime);
            _smoothedWorldPosition = Vector3.Lerp(
                _smoothedWorldPosition,
                originalRaycast.worldPosition,
                smoothing
            );
            if ((_smoothedWorldPosition - _lastAppliedWorldPosition).sqrMagnitude < MinAppliedMovementSqr) return;

            _lastAppliedWorldPosition = _smoothedWorldPosition;
            var smoothedRaycast = originalRaycast;
            smoothedRaycast.worldPosition = _smoothedWorldPosition;
            _pointerEventData.pointerCurrentRaycast = smoothedRaycast;
            try {
                ((IDragHandler)_eventsHandler).OnDrag(_pointerEventData);
            } finally {
                _pointerEventData.pointerCurrentRaycast = originalRaycast;
            }
        }

        public void Dispose() {
            _eventsHandler.PointerDownEvent -= HandlePointerDown;
            _eventsHandler.PointerUpEvent -= HandlePointerUp;
        }

        private void HandlePointerDown(PointerEventsHandler handler, PointerEventData eventData) {
            _pointerEventData = eventData;
            _smoothedWorldPosition = eventData.pointerCurrentRaycast.worldPosition;
            _lastAppliedWorldPosition = _smoothedWorldPosition;
        }

        private void HandlePointerUp(PointerEventsHandler handler, PointerEventData eventData) {
            _pointerEventData = null;
        }
    }
}
