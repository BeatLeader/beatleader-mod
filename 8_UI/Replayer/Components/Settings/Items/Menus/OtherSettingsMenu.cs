using BeatLeader.Replayer.Emulation;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.OtherSettingsMenu.bsml")]
    internal class OtherSettingsMenu : MenuWithContainer
    {
        [Inject] private readonly VRControllersProvider _controllersManager;
        [InjectOptional] private readonly ReplayWatermark _replayWatermark;
        [Inject] private readonly Models.ReplayLaunchData _replayData;

        [UIValue("show-head")] private bool _ShowHead
        {
            get => _replayData.ActualSettings.ShowHead;
            set
            {
                _replayData.ActualToWriteSettings.ShowHead = value;
                _controllersManager.Head.gameObject.SetActive(value);
            }
        }
        [UIValue("show-left-saber")] private bool _ShowLeftSaber
        {
            get => _replayData.ActualSettings.ShowLeftSaber;
            set
            {
                _replayData.ActualToWriteSettings.ShowLeftSaber = value;
                _controllersManager.LeftSaber.gameObject.SetActive(value);
            }
        }
        [UIValue("show-right-saber")] private bool _ShowRightSaber
        {
            get => _replayData.ActualSettings.ShowRightSaber;
            set
            {
                _replayData.ActualToWriteSettings.ShowRightSaber = value;
                _controllersManager.RightSaber.gameObject.SetActive(value);
            }
        }
        [UIValue("show-watermark")] private bool _ShowWatermark
        {
            get => _replayWatermark?.Enabled ?? false;
            set
            {
                if (_replayWatermark != null)
                    _replayWatermark.Enabled = value;
            }
        }
        [UIValue("watermark-can-be-disabled")] private bool _WatermarkCanBeDisabled
        {
            get => _replayWatermark?.CanBeDisabled ?? false;
        }
    }
}
