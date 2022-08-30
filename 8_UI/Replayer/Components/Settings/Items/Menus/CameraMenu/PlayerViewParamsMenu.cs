using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using UnityEngine;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.PlayerViewParamsMenu.bsml")]
    internal class PlayerViewParamsMenu : CameraParamsMenu
    {
        private static Vector3Serializable offset = new Vector3(0, 0, -1);
        [SerializeAutomatically] private static int movementSmoothness = 8;

        [UIValue("movement-smoothness")] private int smoothness
        {
            get => movementSmoothness;
            set
            {
                var val = (int)MathUtils.Map(value, 1, 10, 10, 1);
                _cameraPose.smoothness = val;
                movementSmoothness = value;
                NotifyPropertyChanged(nameof(smoothness));
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
            var matrix = Instantiate<VectorMatrixMenu>();

            matrix.xSlider.min = -100;
            matrix.xSlider.max = 100;

            matrix.ySlider.min = -100;
            matrix.ySlider.max = 100;

            matrix.zSlider.min = 0;
            matrix.zSlider.max = 150;

            matrix.multiplier = 0.01f;
            matrix.increment = 5;
            matrix.zSlider.multiplier = -0.01f;

            matrix.OnVectorChanged += NotifyVectorChanged;
            matrix.multipliedVector = offset;
            _offsetsMenuButton = CreateButtonForMenu(this, matrix, "Offsets");
        }
        private void NotifyVectorChanged(Vector3 vector)
        {
            _cameraPose.offset = vector;
            offset = vector;
            AutomaticConfigTool.NotifyTypeChanged(GetType());
        }
    }
}
