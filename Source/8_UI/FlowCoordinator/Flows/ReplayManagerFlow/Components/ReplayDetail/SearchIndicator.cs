using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class SearchIndicator : ReactiveComponent {
        #region Construct
        
        protected override GameObject Construct() {
            return new Image {
                Sprite = GameResources.Sprites.SearchIcon
            }.WithRectExpand().Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: 6f);
        }

        #endregion

        #region Animation
        
        public float Radius = 1f;
        public float RotationSpeed = 3f;
        
        private float _angle;

        protected override void OnUpdate() {
            ContentTransform.localPosition = new Vector3(
                Mathf.Cos(_angle),
                Mathf.Sin(_angle)
            ) * Radius;
            ContentTransform.localEulerAngles = new Vector3(
                0,
                0,
                Mathf.Sin(_angle) * 20f
            );
            _angle -= RotationSpeed * Time.deltaTime;
        }

        #endregion
    }
}