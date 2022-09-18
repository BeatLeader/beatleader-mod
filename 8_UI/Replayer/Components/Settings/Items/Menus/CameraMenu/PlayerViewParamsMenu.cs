using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using UnityEngine;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically("PlayerViewConfig")]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.PlayerViewParamsMenu.bsml")]
    internal class PlayerViewParamsMenu : CameraParamsMenu
    {
        [SerializeAutomatically] private static Vector3Serializable _offset = new Vector3(0, 0, -1);
        [SerializeAutomatically] private static int _movementSmoothness = 8;

        [UIValue("movement-smoothness")] private int _Smoothness
        {
            get => _movementSmoothness;
            set
            {
                var val = (int)MathUtils.Map(value, 1, 10, 10, 1);
                _cameraPose.smoothness = val;
                _movementSmoothness = value;
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }

        public override int Id => 4;
        public override Type Type => typeof(PlayerViewCameraPose);

        [UIValue("offsets-menu-button")]
        private SubMenuButton _offsetsMenuButton;
        private PlayerViewCameraPose _cameraPose;

        protected override void OnBeforeParse()
        {
            _cameraPose = (PlayerViewCameraPose)PoseProvider;
            var vectorControls = Instantiate<VectorControlsMenu>();

            vectorControls.xSlider.min = -100;
            vectorControls.xSlider.max = 100;

            vectorControls.ySlider.min = -100;
            vectorControls.ySlider.max = 100;

            vectorControls.zSlider.min = 0;
            vectorControls.zSlider.max = 150;

            vectorControls.multiplier = 0.01f;
            vectorControls.increment = 5;
            vectorControls.zSlider.multiplier = -0.01f;

            vectorControls.OnVectorChanged += NotifyVectorChanged;
            vectorControls.multipliedVector = _offset;
            _offsetsMenuButton = CreateButtonForMenu(this, vectorControls, "Offsets");

            _Smoothness = _movementSmoothness;
        }
        private void NotifyVectorChanged(Vector3 vector)
        {
            _cameraPose.offset = vector;
            _offset = vector;
            AutomaticConfigTool.NotifyTypeChanged(GetType());
        }
    }
}
