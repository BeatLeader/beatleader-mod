using System.Collections.Generic;
using BeatLeader.Utils;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class EventTimeline : LayoutComponentBase<EventTimeline> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        private IEnumerable<Event> Events {
            set => value.ForEach(x => events.Add(x));
        }

        [ExternalProperty, UsedImplicitly]
        public Color BackgroundColor {
            get => _backgroundImage.color;
            set => _backgroundImage.color = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Color FillColor {
            get => _fillImage.color;
            set => _fillImage.color = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Vector2 Range {
            get => _range;
            set {
                _range = value;
                RecalculateLayout();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Value {
            get => _value;
            set {
                _value = Mathf.Clamp(value, Range.x, Range.y);
                RecalculateLayout();
            }
        }

        private float _value;
        private Vector2 _range;

        #endregion

        #region Events

        public readonly struct Event {
            public Event(string id, float time, Sprite icon) {
                this.time = time;
                this.icon = icon;
                _id = id;
            }

            public readonly float time;
            public readonly Sprite icon;

            private readonly string _id;

            public override bool Equals(object? obj) {
                if (obj is not Event e) return false;
                return e._id == _id;
            }

            public override int GetHashCode() {
                return _id.GetHashCode();
            }
        }

        public readonly HashSet<Event> events = new();

        #endregion

        #region Markers

        private readonly Dictionary<Event, GameObject> _markers = new();

        private void ReloadMarkers() {
            DestroyMarkers();
            foreach (var e in events) _markers.Add(e, CreateMarker(e.icon));
        }

        private void RecalculateMarkersLayout() {
            var areaSize = _markerAreaTransform.rect.size.x;
            foreach (var (e, marker) in _markers) {
                var pos = MathUtils.Map(
                    e.time, Range.x, Range.y,
                    -areaSize, areaSize
                );
                marker.transform.localPosition = new(pos, 0);
            }
        }

        private GameObject CreateMarker(Sprite icon) {
            var go = _markerAreaGo.CreateChild("Marker");
            var image = go.AddComponent<AdvancedImageView>();
            image.sprite = icon;
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new(4, 4);
            return go;
        }

        private void DestroyMarkers() {
            foreach (var (e, marker) in _markers) marker.TryDestroy();
        }

        #endregion

        #region Layout

        private void RecalculateLayout() {
            RecalculateMarkersLayout();
            var ratio = MathUtils.Map(Value, Range.x, Range.y, 0, 1);
            var rect = _backgroundRectTransform.rect;
            _fillRectTransform.sizeDelta = new(rect.width * ratio, rect.height);
        }

        #endregion

        #region Setup

        private RectTransform _markerAreaTransform = null!;
        private RectTransform _backgroundRectTransform = null!;
        private RectTransform _fillRectTransform = null!;
        private GameObject _markerAreaGo = null!;
        private Image _backgroundImage = null!;
        private Image _fillImage = null!;

        protected override void OnInitialize() {
            var container = Content!;
            //markers area
            _markerAreaGo = container.CreateChild("Area");
            _markerAreaTransform = _markerAreaGo.AddComponent<RectTransform>();
            //background
            _backgroundImage = CreateImage("Background");
            _backgroundRectTransform = (RectTransform)_backgroundImage.transform;
            //background fill
            _fillImage = CreateImage("Fill");
            _fillRectTransform = (RectTransform)_fillImage.transform;
            _fillRectTransform.anchorMin = Vector2.zero;
            _fillRectTransform.anchorMax = Vector2.zero;
            _fillRectTransform.pivot = Vector2.zero;
            _fillRectTransform.SetParent(_backgroundRectTransform, false);

            Image CreateImage(string gameObjectName) {
                var image = container.CreateChild(gameObjectName).AddComponent<Image>();
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 10f;
                image.sprite = BundleLoader.WhiteBG;
                return image;
            }
        }

        #endregion
    }
}