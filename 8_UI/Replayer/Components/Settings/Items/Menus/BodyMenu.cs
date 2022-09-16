using BeatLeader.Replayer.Movement;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.XR;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.BodyMenu.bsml")]
    internal class BodyMenu : MenuWithContainer
    {
        [Inject] private readonly VRControllersManager _controllersManager;
        [Inject] private readonly Models.ReplayLaunchData _replayData;

        [UIValue("show-head")] private bool showHead
        {
            get => _replayData.actualSettings.ShowHead;
            set
            {
                _replayData.actualToWriteSettings.ShowHead = value;
                _controllersManager.ShowNode(XRNode.Head, value);
            }
        }
        [UIValue("show-left-saber")] private bool showLeftSaber
        {
            get => _replayData.actualSettings.ShowLeftSaber;
            set
            {
                _replayData.actualToWriteSettings.ShowLeftSaber = value;
                _controllersManager.ShowNode(XRNode.LeftHand, value);
            }
        }
        [UIValue("show-right-saber")] private bool showRightSaber
        {
            get => _replayData.actualSettings.ShowRightSaber;
            set
            {
                _replayData.actualToWriteSettings.ShowRightSaber = value;
                _controllersManager.ShowNode(XRNode.RightHand, value);
            }
        }
    }
}
