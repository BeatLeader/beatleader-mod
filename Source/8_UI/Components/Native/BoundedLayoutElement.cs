using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader {
    [RequireComponent(typeof(RectTransform))]
    public class BoundedLayoutElement : LayoutElement {
        public float maxHeight = -1;
        public float maxWidth = -1;

        public bool inheritMaxHeight;
        public bool inheritMaxWidth;

        private readonly List<ILayoutElement> _children = new();

        public override float preferredHeight {
            get {
                if (maxHeight is -1 && !inheritMaxHeight) return base.preferredHeight;
                var actualHeight = GetActualHeight();
                var potentialMaxHeight = maxHeight;
                if (inheritMaxHeight) {
                    potentialMaxHeight = GetParentHeight();
                }
                return Mathf.Clamp(actualHeight, 0, potentialMaxHeight);
            }
            set => base.preferredHeight = value;
        }

        public override float preferredWidth {
            get {
                if (maxWidth is -1 && !inheritMaxWidth) return base.preferredWidth;
                var actualWidth = GetActualWidth();
                var potentialMaxWidth = maxWidth;
                if (inheritMaxWidth) {
                    potentialMaxWidth = GetParentWidth();
                }
                return Mathf.Clamp(actualWidth, 0, potentialMaxWidth);
            }
            set => base.preferredWidth = value;
        }

        private float GetParentHeight() {
            return GetMaxLayoutValueInParent(static x => x.preferredHeight);
        }

        private float GetParentWidth() {
            return GetMaxLayoutValueInParent(static x => x.preferredWidth);
        }

        private float GetMaxLayoutValueInParent(Func<ILayoutElement, float> selector) {
            var parent = transform.parent;
            while (true) {
                if (parent is null) break;
                var go = parent.gameObject;
                var components = go
                    .GetComponents<ILayoutElement>()
                    .Where(static x => x is not LayoutGroup);
                try {
                    var value = components.Max(selector);
                    if (value is not -1) return value;
                } catch (InvalidOperationException) { }
                parent = parent.parent;
            }
            return -1;
        }

        private float GetActualHeight() {
            RefreshChildren();
            return _children.Select(static x => x.preferredHeight).Max();
        }

        private float GetActualWidth() {
            RefreshChildren();
            return _children.Select(static x => x.preferredWidth).Max();
        }

        private void RefreshChildren() {
            _children.Clear();
            transform.GetComponentsInChildren(true, _children);
            _children.Remove(this);
        }
    }
}