using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerViewParamsContentView : ParamsContentView<PlayerViewCameraView> {
        #region UI Values

        [UIValue("movement-smoothness")]
        private int Smoothness {
            get => PlayerViewConfig.Instance.MovementSmoothness;
            set {
                if (!IsParsed) return;
                View!.smoothness = MathUtils.Map(value, 1, 10, 10, 1);
                PlayerViewConfig.Instance.MovementSmoothness = value;
            }
        }

        #endregion

        #region UI Components

        [UIValue("position-menu-button")] private NavigationButton _posNavigationButton;
        [UIValue("rotation-menu-button")] private NavigationButton _rotNavigationButton;

        private VectorControlsContentView _posVectorControls;
        private VectorControlsContentView _rotVectorControls;

        #endregion

        #region Setup

        protected override void OnInstantiate() {
            _posVectorControls = InstantiateOnSceneRoot<VectorControlsContentView>();
            _rotVectorControls = InstantiateOnSceneRoot<VectorControlsContentView>();
            _posNavigationButton = Instantiate<NavigationButton>(transform);
            _rotNavigationButton = Instantiate<NavigationButton>(transform);

            _posVectorControls.ManualInit(null!);
            _rotVectorControls.ManualInit(null!);
            _posNavigationButton.Setup(this, _posVectorControls, "Position Offset");
            _rotNavigationButton.Setup(this, _rotVectorControls, "Rotation Offset");

            SetupPositionOffsetMenu();
            SetupRotationOffsetMenu();
        }

        protected override void OnInitialize() {
            var config = PlayerViewConfig.Instance;
            _posVectorControls.MultipliedVector = config.PositionOffset;
            _rotVectorControls.Vector = config.RotationOffset;
            Smoothness = config.MovementSmoothness;
            NotifyPropertyChanged();
        }

        private void SetupPositionOffsetMenu() {
            _posVectorControls.XSlider.min = -100;
            _posVectorControls.XSlider.max = 100;
            _posVectorControls.YSlider.min = -100;
            _posVectorControls.YSlider.max = 100;
            _posVectorControls.ZSlider.min = 0;
            _posVectorControls.ZSlider.max = 300;
            _posVectorControls.Multiplier = 0.01f;
            _posVectorControls.Increment = 5;
            _posVectorControls.ZSlider.multiplier = -0.01f;
            _posVectorControls.VectorChangedEvent += HandlePositionChanged;
        }
        private void SetupRotationOffsetMenu() {
            _rotVectorControls.Min = -180;
            _rotVectorControls.Max = 180;
            _rotVectorControls.Increment = 5;
            _rotVectorControls.VectorChangedEvent += HandleRotationChanged;
        }

        #endregion

        #region UI Callbacks

        private void HandlePositionChanged(Vector3 vector) {
            View!.positionOffset = vector;
            PlayerViewConfig.Instance.PositionOffset = vector;
        }

        private void HandleRotationChanged(Vector3 vector) {
            View!.rotationOffset = Quaternion.Euler(vector);
            PlayerViewConfig.Instance.RotationOffset = vector;
        }

        #endregion
    }
}
