using BeatLeader.Replayer.Poses;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.PlayerViewParamsMenu.bsml")]
    internal class PlayerViewParamsMenu : CameraPoseParamsMenu
    {
        [SerializeAutomatically] private static float offsetX;
        [SerializeAutomatically] private static float offsetY;
        [SerializeAutomatically] private static float offsetZ = -1;
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
            var matrix = Instantiate<Vector3MatrixMenu>();
            matrix.multiplier = -0.01f;
            matrix.min = 0;
            matrix.max = 150;
            matrix.increment = 5;
            matrix.OnVectorChanged += x => _cameraPose.offset = x;
            matrix.vector = new UnityEngine.Vector3(offsetX, offsetY, offsetZ);
            _offsetsMenuButton = CreateButtonForMenu(this, matrix, "Offsets");
        }
    }
}
