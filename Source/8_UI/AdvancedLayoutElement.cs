using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader {
    [RequireComponent(typeof(RectTransform))]
    public class AdvancedLayoutElement : LayoutElement {
        public float maxHeight = -1;
        public float maxWidth = -1;

        private readonly List<ILayoutElement> _children = new();

        public override float preferredHeight {
            get {
                if (maxHeight is -1) return base.preferredHeight;
                //for unknown reasons LayoutUtility.GetPreferredHeight() causes deadlock
                var baseValue = Mathf.Max(GetLayoutElements()
                    .Select(x => x.preferredHeight).ToArray());
                return baseValue > maxHeight ? maxHeight : baseValue;
            }
            set => base.preferredHeight = value;
        }

        public override float preferredWidth {
            get {
                if (maxWidth is -1) return base.preferredWidth;
                //same as for height
                var baseValue = Mathf.Max(GetLayoutElements()
                    .Select(x => x.preferredWidth).ToArray());
                return baseValue > maxWidth ? maxWidth : baseValue;
            }
            set => base.preferredWidth = value;
        }

        private IEnumerable<ILayoutElement> GetLayoutElements() {
            _children.Clear();
            transform.GetComponentsInChildren(true, _children);
            _children.Remove(this);
            return _children;
        }
    }
}