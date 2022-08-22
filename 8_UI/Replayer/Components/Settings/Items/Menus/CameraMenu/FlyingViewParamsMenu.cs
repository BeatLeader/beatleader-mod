using BeatLeader.Replayer.Poses;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.FlyingViewParamsMenu.bsml")]
    internal class FlyingViewParamsMenu : CameraParamsMenu
    {
        [SerializeAutomatically] private static int _flySpeed;
        [SerializeAutomatically] private static bool _followOrigin;
        [SerializeAutomatically] private static bool _disableOnUnCur;
        [SerializeAutomatically] private static float _sensitivityX = 0.5f;
        [SerializeAutomatically] private static float _sensitivityY = 0.5f;

        [UIValue("fly-speed")] private int flySpeed
        {
            get => _flySpeed;
            set
            {
                _cameraPose.flySpeed = value;
                _flySpeed = value;
                NotifyPropertyChanged(nameof(flySpeed));
            }
        }
        [UIValue("follow-origin")] private bool followOrigin
        {
            get => _followOrigin;
            set
            {
                _cameraPose.followOrigin = value;
                _followOrigin = value;
                NotifyPropertyChanged(nameof(followOrigin));
            }
        }
        [UIValue("disable-input-on-unlocked-cursor")] private bool disableInputOnUnlockedCursor
        {
            get => _disableOnUnCur;
            set
            {
                _cameraPose.disableInputOnUnlockedCursor = value;
                _disableOnUnCur = value;
                NotifyPropertyChanged(nameof(disableInputOnUnlockedCursor));
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
            var matrix = Instantiate<VectorMatrixMenu>();
            matrix.min = 1;
            matrix.max = 10;
            matrix.increment = 1;
            matrix.bitDepth = 2;
            matrix.multiplier = 0.1f;
            matrix.OnVectorChanged += NotifyVectorChanged;
            matrix.vector = new Vector3(_sensitivityX, _sensitivityY);
            _sensitivityMenuButton = CreateButtonForMenu(this, matrix, "Sensitivity");
        }
        private void NotifyVectorChanged(Vector3 vector)
        {
            _cameraPose.mouseSensitivity = vector;
            _sensitivityX = vector.x;
            _sensitivityY = vector.y;
        }
    }
}
