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
        [SerializeAutomatically] private static Models.Vector3 _positionOffset = new Vector3(0, 0, -1);
        [SerializeAutomatically] private static Models.Vector3 _rotationOffset = Vector3.zero;
        [SerializeAutomatically] private static int _movementSmoothness = 8;

        [UIValue("movement-smoothness")] private int _Smoothness
        {
            get => _movementSmoothness;
            set
            {
                var val = (int)MathUtils.Map(value, 1, 10, 10, 1);
                _cameraPose.smoothness = val;
                _movementSmoothness = value;
                NotifyPropertyChanged(nameof(_Smoothness));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }

        public override int Id => 4;
        public override Type Type => typeof(PlayerViewCameraPose);

        [UIValue("position-menu-button")] private SubMenuButton _positionMenuButton;
        [UIValue("rotation-menu-button")] private SubMenuButton _rotationMenuButton;
        private PlayerViewCameraPose _cameraPose;

        protected override void OnBeforeParse()
        {
            _cameraPose = (PlayerViewCameraPose)PoseProvider;

            SetupPositionOffsetMenu();
            SetupRotationOffsetMenu();

            _Smoothness = _movementSmoothness;
        }

        private void SetupPositionOffsetMenu()
        {
            var posVectorControls = Instantiate<VectorControlsMenu>();

            posVectorControls.xSlider.min = -100;
            posVectorControls.xSlider.max = 100;

            posVectorControls.ySlider.min = -100;
            posVectorControls.ySlider.max = 100;

            posVectorControls.zSlider.min = 0;
            posVectorControls.zSlider.max = 150;

            posVectorControls.multiplier = 0.01f;
            posVectorControls.increment = 5;
            posVectorControls.zSlider.multiplier = -0.01f;

            posVectorControls.VectorChangedEvent += HandlePositionChanged;
            posVectorControls.multipliedVector = _positionOffset;

            _positionMenuButton = CreateButtonForMenu(this, posVectorControls, "Position");
        }
        private void SetupRotationOffsetMenu()
        {
            var rotVectorControls = Instantiate<VectorControlsMenu>();

            rotVectorControls.min = -180;
            rotVectorControls.max = 180;

            rotVectorControls.increment = 5;
            rotVectorControls.vector = _rotationOffset;
            rotVectorControls.VectorChangedEvent += HandleRotationChanged;

            _rotationMenuButton = CreateButtonForMenu(this, rotVectorControls, "Rotation");
        }

        private void HandlePositionChanged(Vector3 vector)
        {
            _cameraPose.positionOffset = vector;
            _positionOffset = vector;
            AutomaticConfigTool.NotifyTypeChanged(GetType());
        }
        private void HandleRotationChanged(Vector3 vector)
        {
            _cameraPose.rotationOffset = Quaternion.Euler(vector);
            _rotationOffset = vector;
            AutomaticConfigTool.NotifyTypeChanged(GetType());
        }
    }
}
