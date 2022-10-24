using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerViewParamsContentView : ParamsContentView<PlayerViewCameraPose> {
        #region UI Values

        [UIValue("movement-smoothness")]
        private int _Smoothness {
            get => PlayerViewConfig.MovementSmoothness;
            set {
                if (!IsParsed) return;
                Pose.smoothness = MathUtils.Map(value, 1, 10, 10, 1);
                PlayerViewConfig.MovementSmoothness = value;
                AutomaticConfigTool.NotifyTypeChanged(typeof(PlayerViewConfig));
            }
        }

        public override int Id => 4;

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
            _posVectorControls.ManualInit(null);
            _rotVectorControls.ManualInit(null);

            _posNavigationButton = Instantiate<NavigationButton>(transform);
            _rotNavigationButton = Instantiate<NavigationButton>(transform);
            _posNavigationButton.Setup(this, _posVectorControls, "Position Offset");
            _rotNavigationButton.Setup(this, _rotVectorControls, "Rotation Offset");

            SetupPositionOffsetMenu();
            SetupRotationOffsetMenu();
        }

        protected override void OnInitialize() {
            _posVectorControls.MultipliedVector = PlayerViewConfig.PositionOffset;
            _rotVectorControls.Vector = PlayerViewConfig.RotationOffset;

            _Smoothness = PlayerViewConfig.MovementSmoothness;
            NotifyPropertyChanged();
        }

        private void SetupPositionOffsetMenu() {
            _posVectorControls.XSlider.min = -100;
            _posVectorControls.XSlider.max = 100;
            _posVectorControls.YSlider.min = -100;
            _posVectorControls.YSlider.max = 100;
            _posVectorControls.ZSlider.min = 0;
            _posVectorControls.ZSlider.max = 150;
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
            Pose.positionOffset = vector;
            PlayerViewConfig.PositionOffset = vector;
            AutomaticConfigTool.NotifyTypeChanged(typeof(PlayerViewConfig));
        }

        private void HandleRotationChanged(Vector3 vector) {
            Pose.rotationOffset = Quaternion.Euler(vector);
            PlayerViewConfig.RotationOffset = vector;
            AutomaticConfigTool.NotifyTypeChanged(typeof(PlayerViewConfig));
        }

        #endregion
    }
}
