using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal partial class FlyingViewParamsContentView : ParamsContentView<FlyingCameraView> {
        [UIValue("setup-fpfc")]
        private bool SetupFPFC => InputUtils.IsInFPFC;

        public override void Setup(Models.ICameraView poseProvider) {
            if (View != null) View.ResetEvent -= HandleViewResetted;
            base.Setup(poseProvider);
            View!.manualMove = !SetupFPFC;
            View.ResetEvent += HandleViewResetted;
            View.mouseSensitivity = _mouseSensitivity;
            View.flySpeed = _flySpeed;
            View.manualPosition = _position;
            View.manualRotation = _rotation;
        }

        protected override void OnInstantiate() {
            var conf = FlyingViewConfig.Instance;
            _position = conf.VRPosition;
            _rotation = conf.VRRotation;
            _flySpeed = conf.FlySpeed;
            _mouseSensitivity = conf.Sensitivity;
            if (SetupFPFC) OnInstantiateFPFC();
        }

        protected override void OnInitialize() {
            if (SetupFPFC) OnInitializeFPFC();
            else OnInitializeVR();
        }

        protected override void OnDispose() {
            if (View == null) return;
            var conf = FlyingViewConfig.Instance;
            var view = View!;
            conf.VRPosition = view.manualPosition;
            conf.VRRotation = view.manualRotation;
            conf.FlySpeed = (int)view.flySpeed;
            conf.Sensitivity = view.mouseSensitivity;
        }

        [UIAction("reset-position")]
        private void HandleResetButtonClicked() {
            View?.Reset();
        }
    }

    //VR part
    partial class FlyingViewParamsContentView {
        [UIValue("x-pos")]
        private float XPos {
            get => View?.manualPosition.x ?? _position.x;
            set {
                _position.x = value;
                UpdateLocationText();
                if (View == null) return;
                View.manualPosition.x = value;
            }
        }

        [UIValue("y-pos")]
        private float YPos {
            get => View?.manualPosition.y ?? _position.y;
            set {
                _position.y = value;
                UpdateLocationText();
                if (View == null) return;
                View.manualPosition.y = value;
            }
        }

        [UIValue("z-pos")]
        private float ZPos {
            get => View?.manualPosition.z ?? _position.z;
            set {
                _position.z = value;
                UpdateLocationText();
                if (View == null) return;
                View.manualPosition.z = value;
            }
        }

        [UIValue("rot")]
        private float Rot {
            get => _rotation;
            set {
                _rotation = value;
                UpdateLocationText();
                if (View == null) return;
                View.manualRotation = value;
            }
        }

        [UIComponent("location-text")]
        private readonly TextMeshProUGUI _locationText = null!;
        private Vector3 _position;
        private float _rotation;

        private void OnInitializeVR() {
            UpdateLocationText();
        }

        private void UpdateLocationText() {
            _locationText.text = FormatUtils.FormatLocation(_position, _rotation);
        }

        private void HandleViewResetted() {
            if (SetupFPFC) return;
            _position = View!.manualPosition;
            _rotation = View.manualRotation;
            NotifyPropertyChanged(nameof(XPos));
            NotifyPropertyChanged(nameof(YPos));
            NotifyPropertyChanged(nameof(ZPos));
            NotifyPropertyChanged(nameof(Rot));
            UpdateLocationText();
        }
    }

    //FPFC part
    partial class FlyingViewParamsContentView {
        [UIValue("fly-speed")]
        private int FlySpeed {
            get => (int)(View?.flySpeed ?? _flySpeed);
            set {
                _flySpeed = value;
                if (View == null) return;
                View.flySpeed = value;
            }
        }

        [UIValue("sensitivity-menu-button")]
        private NavigationButton _sensitivityMenuButton = null!;
        private VectorControlsContentView _sensitivityVectorControls = null!;
        private Vector2 _mouseSensitivity;
        private int _flySpeed;

        private void OnInstantiateFPFC() {
            _sensitivityVectorControls = InstantiateOnSceneRoot<VectorControlsContentView>();
            _sensitivityMenuButton = Instantiate<NavigationButton>(transform);
            _sensitivityVectorControls.ManualInit(null!);
            _sensitivityMenuButton.Setup(this, _sensitivityVectorControls, "Sensitivity");

            _sensitivityVectorControls.Min = 1;
            _sensitivityVectorControls.Max = 10;
            _sensitivityVectorControls.Increment = 1;
            _sensitivityVectorControls.Dimensions = 2;
            _sensitivityVectorControls.Multiplier = 0.1f;
            _sensitivityVectorControls.VectorChangedEvent += HandleVectorChanged;
        }

        private void OnInitializeFPFC() {
            NotifyPropertyChanged(nameof(FlySpeed));
            _sensitivityVectorControls.MultipliedVector = _mouseSensitivity;
        }

        private void HandleVectorChanged(Vector3 vector) {
            if (View == null) return;
            View!.mouseSensitivity = vector;
        }
    }
}
