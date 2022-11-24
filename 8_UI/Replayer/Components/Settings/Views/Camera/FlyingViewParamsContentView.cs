using BeatLeader.Replayer.Camera;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class FlyingViewParamsContentView : ParamsContentView<FlyingCameraPose> {
        #region UI Values

        [UIValue("fly-speed")]
        private int _FlySpeed {
            get => FlyingViewConfig.Instance.FlySpeed;
            set {
                if (!IsParsed) return;
                Pose.flySpeed = value;
                FlyingViewConfig.Instance.FlySpeed = value;
                FlyingViewConfig.Instance.Save();
            }
        }
        [UIValue("disable-input-on-unlocked-cursor")]
        private bool _DisableInputOnUnlockedCursor {
            get => FlyingViewConfig.Instance.DisableOnUnCur;
            set {
                if (!IsParsed) return;
                Pose.disableInputOnUnlockedCursor = value;
                FlyingViewConfig.Instance.DisableOnUnCur = value;
                FlyingViewConfig.Instance.Save();
            }
        }
        public override int Id => 5;

        #endregion

        #region UI Components

        [UIValue("sensitivity-menu-button")] private NavigationButton _sensitivityMenuButton;

        private VectorControlsContentView _sensitivityVectorControls;

        #endregion

        #region Setup

        protected override void OnInstantiate() {
            _sensitivityVectorControls = InstantiateOnSceneRoot<VectorControlsContentView>();
            _sensitivityMenuButton = Instantiate<NavigationButton>(transform);

            _sensitivityVectorControls.ManualInit(null);
            _sensitivityMenuButton.Setup(this, _sensitivityVectorControls, "Sensitivity");
            SetupSensitivityControls();
        }

        protected override void OnInitialize() {
            _sensitivityVectorControls.MultipliedVector = new(
                 FlyingViewConfig.Instance.SensitivityX, FlyingViewConfig.Instance.SensitivityY);
            _FlySpeed = FlyingViewConfig.Instance.FlySpeed;
            _DisableInputOnUnlockedCursor = FlyingViewConfig.Instance.DisableOnUnCur;
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
            FlyingViewConfig.Instance.SensitivityX = vector.x;
            FlyingViewConfig.Instance.SensitivityY = vector.y;
            FlyingViewConfig.Instance.Save();
        }

        [UIAction("reset-position")]
        private void HandlePositionResetClicked() {
            Pose.Reset();
        }

        #endregion
    }
}
