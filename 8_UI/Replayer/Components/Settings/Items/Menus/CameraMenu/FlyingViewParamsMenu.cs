using BeatLeader.Replayer.Camera;
using BeatSaberMarkupLanguage.Attributes;
using System;
using UnityEngine;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically("FlyingViewConfig")]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.FlyingViewParamsMenu.bsml")]
    internal class FlyingViewParamsMenu : CameraParamsMenu
    {
        [SerializeAutomatically] private static int _flySpeed = 4;
        [SerializeAutomatically] private static bool _followOrigin = true;
        [SerializeAutomatically] private static bool _disableOnUnCur = true;
        [SerializeAutomatically] private static float _sensitivityX = 0.5f;
        [SerializeAutomatically] private static float _sensitivityY = 0.5f;

        [UIValue("fly-speed")] private int _FlySpeed
        {
            get => _flySpeed;
            set
            {
                _cameraPose.flySpeed = value;
                _flySpeed = value;
                NotifyPropertyChanged(nameof(_FlySpeed));
            }
        }
        [UIValue("follow-origin")] private bool _FollowOrigin
        {
            get => _followOrigin;
            set
            {
                _cameraPose.followOrigin = value;
                _followOrigin = value;
                NotifyPropertyChanged(nameof(_FollowOrigin));
            }
        }
        [UIValue("disable-input-on-unlocked-cursor")] private bool _DisableInputOnUnlockedCursor
        {
            get => _disableOnUnCur;
            set
            {
                _cameraPose.disableInputOnUnlockedCursor = value;
                _disableOnUnCur = value;
                NotifyPropertyChanged(nameof(_DisableInputOnUnlockedCursor));
            }
        }

        public override int Id => 5;
        public override Type Type => typeof(FlyingCameraPose);

        [UIValue("sensitivity-menu-button")]
        private SubMenuButton _sensitivityMenuButton;
        private FlyingCameraPose _cameraPose;

        protected override void OnBeforeParse()
        {
            _cameraPose = (FlyingCameraPose)PoseProvider;
            var vectorControls = Instantiate<VectorControlsMenu>();
            vectorControls.min = 1;
            vectorControls.max = 10;
            vectorControls.increment = 1;
            vectorControls.dimensions = 2;
            vectorControls.multiplier = 0.1f;
            vectorControls.VectorChangedEvent += HandleVectorChanged;
            vectorControls.multipliedVector = new Vector3(_sensitivityX, _sensitivityY);
            _sensitivityMenuButton = CreateButtonForMenu(this, vectorControls, "Sensitivity");

            _FlySpeed = _flySpeed;
            _FollowOrigin = _followOrigin;
            _DisableInputOnUnlockedCursor = _disableOnUnCur;
        }
        private void HandleVectorChanged(Vector3 vector)
        {
            _cameraPose.mouseSensitivity = vector;
            _sensitivityX = vector.x;
            _sensitivityY = vector.y;
            AutomaticConfigTool.NotifyTypeChanged(GetType());
        }
    }
}
