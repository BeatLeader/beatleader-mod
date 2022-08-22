using BeatLeader.Replayer.Poses;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.PlayerViewParamsMenu.bsml")]
    internal class PlayerViewParamsMenu : CameraParamsMenu
    {
        [SerializeAutomatically] private static Vector3Serializable offset = new Vector3(0, 0, -1);
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
            var matrix = Instantiate<VectorMatrixMenu>();
            matrix.multiplier = -0.01f;
            matrix.min = 0;
            matrix.max = 150;
            matrix.increment = 5;
            matrix.OnVectorChanged += NotifyVectorChanged;
            matrix.vector = offset;
            _offsetsMenuButton = CreateButtonForMenu(this, matrix, "Offsets");
        }
        private void NotifyVectorChanged(Vector3 vector)
        {
            _cameraPose.offset = vector;
            offset = vector;
        }
    }
}
