using BeatLeader.Replayer.Movement;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
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
            get => _replayData.ActualSettings.ShowHead;
            set
            {
                _replayData.ActualToWriteSettings.ShowHead = value;
                _controllersManager.ShowNode(XRNode.Head, value);
            }
        }
        [UIValue("show-left-saber")] private bool _ShowLeftSaber
        {
            get => _replayData.ActualSettings.ShowLeftSaber;
            set
            {
                _replayData.ActualToWriteSettings.ShowLeftSaber = value;
                _controllersManager.ShowNode(XRNode.LeftHand, value);
            }
        }
        [UIValue("show-right-saber")] private bool _ShowRightSaber
        {
            get => _replayData.ActualSettings.ShowRightSaber;
            set
            {
                _replayData.ActualToWriteSettings.ShowRightSaber = value;
                _controllersManager.ShowNode(XRNode.RightHand, value);
            }
        }
        [UIValue("show-watermark")] private bool _ShowWatermark
        {
            get => _replayWatermark.Enabled;
            set => _replayWatermark.Enabled = value;
        }

        [UIObject("watermark-toggle")] private GameObject _watermarkToggle;

        protected override void OnAfterParse()
        {
            _watermarkToggle.SetActive(_replayWatermark.CanBeDisabled);
        }
    }
}
