using BeatLeader.Replayer.Camera;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class FlyingViewParamsContentView : ParamsContentView<FlyingCameraPose> {
        #region UI Values

        [UIValue("fly-speed")]
        private int _FlySpeed {
            get => FlyingViewConfig.FlySpeed;
            set {
                if (!IsParsed) return;
                Pose.flySpeed = value;
                FlyingViewConfig.FlySpeed = value;
                AutomaticConfigTool.NotifyTypeChanged(typeof(FlyingViewConfig));
            }
        }
        [UIValue("disable-input-on-unlocked-cursor")]
        private bool _DisableInputOnUnlockedCursor {
            get => FlyingViewConfig.DisableOnUnCur;
            set {
                if (!IsParsed) return;
                Pose.disableInputOnUnlockedCursor = value;
                FlyingViewConfig.DisableOnUnCur = value;
                AutomaticConfigTool.NotifyTypeChanged(typeof(FlyingViewConfig));
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
                 FlyingViewConfig.SensitivityX, FlyingViewConfig.SensitivityY);
            _FlySpeed = FlyingViewConfig.FlySpeed;
            _DisableInputOnUnlockedCursor = FlyingViewConfig.DisableOnUnCur;
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
            FlyingViewConfig.SensitivityX = vector.x;
            FlyingViewConfig.SensitivityY = vector.y;
            AutomaticConfigTool.NotifyTypeChanged(typeof(FlyingViewConfig));
        }

        [UIAction("reset-position")]
        private void HandlePositionResetClicked() {
            Pose.Reset();
        }

        #endregion
    }
}
