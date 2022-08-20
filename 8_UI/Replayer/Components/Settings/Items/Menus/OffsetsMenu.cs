using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.OffsetsMenu.bsml")]
    internal class OffsetsMenu : MenuWithContainer
    {
        [Inject] private readonly ReplayerCameraController _cameraController;

        [SerializeAutomatically] private static float offsetX;
        [SerializeAutomatically] private static float offsetY;
        [SerializeAutomatically] private static float offsetZ = -1;

        [UIValue("offset-x")] private int _offsetX
        {
            get => (int)(_cameraController.Offset.x * 10);
            set
            {
                var vector = _cameraController.Offset;
                vector.x = offsetX = value * 0.1f;
                _cameraController.Offset = vector;
                UpdateText();
                NotifyPropertyChanged(nameof(_offsetX));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }
        [UIValue("offset-y")] private int _offsetY
        {
            get => (int)(_cameraController.Offset.y * 10);
            set
            {
                var vector = _cameraController.Offset;
                vector.y = offsetY = value * 0.1f;
                _cameraController.Offset = vector;
                UpdateText();
                NotifyPropertyChanged(nameof(_offsetY));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }
        [UIValue("offset-z")] private int _offsetZ
        {
            get => (int)(_cameraController.Offset.z * 10);
            set
            {
                var vector = _cameraController.Offset;
                vector.z = offsetZ = value * 0.1f;
                _cameraController.Offset = vector;
                UpdateText();
                NotifyPropertyChanged(nameof(_offsetZ));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }

        [UIComponent("offsets-text")] private TextMeshProUGUI _offsetsText;

        protected override void OnAfterParse()
        {
            _offsetX = (int)(offsetX * 10);
            _offsetY = (int)(offsetY * 10);
            _offsetZ = (int)(offsetZ * 10);
        }
        private void UpdateText()
        {
            _offsetsText.text = $"<color=\"green\">X:{_offsetX * 0.1} <color=\"red\">Y:{_offsetY * 0.1} <color=\"blue\">Z:{_offsetZ * 0.1}";
        }
    }
}
