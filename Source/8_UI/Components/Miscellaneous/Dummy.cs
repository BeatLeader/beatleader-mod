using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class Dummy : ReeUIComponentV3<Dummy> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public float Size {
            set {
                Width = value;
                Height = value;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Width {
            get => _layoutElement.preferredWidth;
            set {
                _layoutElement.preferredWidth = value;
                NotifyPropertyChanged();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Height {
            get => _layoutElement.preferredHeight;
            set => _layoutElement.preferredHeight = value;
        }

        #endregion

        #region Construct

        private LayoutElement _layoutElement = null!;

        protected override GameObject Construct(Transform parent) {
            var go = parent.gameObject.CreateChild("Dummy");
            _layoutElement = go.AddComponent<LayoutElement>();
            var sizeFitter = go.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return go;
        }

        #endregion
    }
}