using System.Collections.Generic;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class EventTimeline : LayoutComponentBase<EventTimeline> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        private ISet<Event> Events { get; set; } = new HashSet<Event>();

        [ExternalProperty, UsedImplicitly]
        public Color BackgroundColor {
            get => _backgroundImage.Color;
            set => _backgroundImage.Color = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Vector2 Range {
            get => _range;
            set {
                _range = value;
                Recalculate();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Value {
            get => _value;
            set {
                _value = value;
                Recalculate();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float StartValue {
            get => _startValue;
            set {
                _startValue = value;
                Recalculate();
            }
        }

        private float _value;
        private float _startValue;
        private Vector2 _range;

        #endregion

        #region Event Management

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

        public bool AddEvent(Event e) => Events.Add(e);

        public bool RemoveEvent(Event e) => Events.Remove(e);

        public bool RemoveEvent(string id) => Events.Remove(new(id, 0, null!));

        #endregion

        #region Markers

        private readonly List<GameObject> _markers = new();

        private void Recalculate() {
            DestroyMarkers();
            foreach (var e in Events) {
                var marker = CreateMarker(e.icon);
                var areaSize = _markerAreaTransform.rect.size.x;
                var pos = MathUtils.Map(
                    e.time, Range.x, Range.y,
                    -areaSize, areaSize
                );
                marker.localPosition = new(pos, 0);
                _markers.Add(marker.gameObject);
            }
        }

        private RectTransform CreateMarker(Sprite icon) {
            var go = _markerAreaGo.CreateChild("Marker");
            var image = go.AddComponent<AdvancedImageView>();
            image.sprite = icon;
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new(4, 4);
            return rect;
        }

        private void DestroyMarkers() {
            foreach (var marker in _markers) marker.TryDestroy();
        }

        #endregion

        #region Setup

        protected override LayoutGroupType LayoutGroup => LayoutGroupType.Horizontal;

        private GameObject _markerAreaGo = null!;
        private RectTransform _markerAreaTransform = null!;
        private AdvancedImage _backgroundImage = null!;

        protected override void OnInitialize() {
            var container = Content!;
            var background = AdvancedImage.Instantiate(container.transform);
            background.IgnoreLayout = true;
            background.InheritSize = true;
            background.ImageType = Image.Type.Sliced;
            background.PixelsPerUnit = 10f;
            background.Sprite = BundleLoader.WhiteBG;
            _backgroundImage = background;
            
            var area = container.CreateChild("Area");
            area.AddComponent<LayoutElement>().ignoreLayout = true;
            _markerAreaGo = area;
            _markerAreaTransform = area.GetComponent<RectTransform>();
        }

        #endregion
    }
}