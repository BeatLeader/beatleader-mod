using BeatLeader.Replayer.Camera;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class FlyingViewParamsContentView : ParamsContentView<FlyingCameraPose> {
        #region UI Values

        [UIValue("fly-speed")]
        private int FlySpeed {
            get => FlyingViewConfig.Instance.FlySpeed;
            set {
                if (!IsParsed) return;
                Pose.flySpeed = value;
                FlyingViewConfig.Instance.FlySpeed = value;
            }
        }

        [UIValue("disable-input-on-unlocked-cursor")]
        private bool DisableInputOnUnlockedCursor {
            get => FlyingViewConfig.Instance.DisableOnUnCur;
            set {
                if (!IsParsed) return;
                Pose.disableInputOnUnlockedCursor = value;
                FlyingViewConfig.Instance.DisableOnUnCur = value;
            }
        }

        #endregion

        #region UI Components

        [UIValue("sensitivity-menu-button")] private NavigationButton _sensitivityMenuButton;

        private VectorControlsContentView _sensitivityVectorControls;

        #endregion

        #region Setup

        public override int Id => 5;

        protected override void OnInstantiate() {
            _sensitivityVectorControls = InstantiateOnSceneRoot<VectorControlsContentView>();
            _sensitivityMenuButton = Instantiate<NavigationButton>(transform);

            _sensitivityVectorControls.ManualInit(null);
            _sensitivityMenuButton.Setup(this, _sensitivityVectorControls, "Sensitivity");
            SetupSensitivityControls();
        }

        protected override void OnInitialize() {
            var config = FlyingViewConfig.Instance;
            _sensitivityVectorControls.MultipliedVector = config.Sensitivity;
            FlySpeed = config.FlySpeed;
            DisableInputOnUnlockedCursor = config.DisableOnUnCur;
            NotifyPropertyChanged();
        }

        private void SetupSensitivityControls() {
            _sensitivityVectorControls.Min = 1;
            _sensitivityVectorControls.Max = 10;
            _sensitivityVectorControls.Increment = 1;
            _sensitivityVectorControls.Dimensions = 2;
            _sensitivityVectorControls.Multiplier = 0.1f;
            _sensitivityVectorControls.VectorChangedEvent += HandleVectorChanged;
        }

        #endregion

        #region UI Callbacks

        private void HandleVectorChanged(Vector3 vector) {
            Pose.mouseSensitivity = vector;
            FlyingViewConfig.Instance.Sensitivity = (Vector2)vector;
        }

        [UIAction("reset-position")]
        private void HandlePositionResetButtonClicked() {
            Pose.Reset();
        }

        #endregion
    }
}
