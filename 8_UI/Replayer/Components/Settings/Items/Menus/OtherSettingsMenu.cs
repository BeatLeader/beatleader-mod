using BeatLeader.Replayer.Movement;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.XR;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.OtherSettingsMenu.bsml")]
    internal class OtherSettingsMenu : MenuWithContainer
    {
        [Inject] private readonly VRControllersProvider _controllersManager;
        [Inject] private readonly ReplayWatermark _replayWatermark;
        [Inject] private readonly Models.ReplayLaunchData _replayData;

        [UIValue("show-head")] private bool _ShowHead
        {
            get => _replayData.actualSettings.ShowHead;
            set
            {
                _replayData.actualToWriteSettings.ShowHead = value;
                _controllersManager.ShowNode(XRNode.Head, value);
            }
        }
        [UIValue("show-left-saber")] private bool _ShowLeftSaber
        {
            get => _replayData.actualSettings.ShowLeftSaber;
            set
            {
                _replayData.actualToWriteSettings.ShowLeftSaber = value;
                _controllersManager.ShowNode(XRNode.LeftHand, value);
            }
        }
        [UIValue("show-right-saber")] private bool _ShowRightSaber
        {
            get => _replayData.actualSettings.ShowRightSaber;
            set
            {
                _replayData.actualToWriteSettings.ShowRightSaber = value;
                _controllersManager.ShowNode(XRNode.RightHand, value);
            }
        }
        [UIValue("show-watermark")] private bool _ShowWatermark
        {
            get => _replayWatermark.Enabled;
            set => _replayWatermark.Enabled = value;
        }
        [UIValue("watermark-can-be-disabled")] private bool _WatermarkCanBeDisabled
        {
            get => _replayWatermark.CanBeDisabled;
        }
    }
}
