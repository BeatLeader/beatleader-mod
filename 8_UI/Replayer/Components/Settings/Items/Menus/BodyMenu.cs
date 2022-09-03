using BeatLeader.Replayer.Movement;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.BodyMenu.bsml")]
    internal class BodyMenu : MenuWithContainer
    {
        [Inject] private readonly VRControllersManager _controllersManager;
        [Inject] private readonly Models.ReplayLaunchData _replayData;

        [UIValue("show-head")] private bool _showHead
        {
            get => _replayData.actualSettings.ShowHead;
            set
            {
                _controllersManager.HeadTransform.gameObject.SetActive(value);
                NotifyPropertyChanged(nameof(_showHead));
                _replayData.actualToWriteSettings.ShowHead = value;
            }
        }
        [UIValue("show-left-saber")] private bool _showLeftSaber
        {
            get => _replayData.actualSettings.ShowLeftSaber;
            set
            {
                _controllersManager.LeftSaber.gameObject.SetActive(value);
                NotifyPropertyChanged(nameof(_showLeftSaber));
                _replayData.actualToWriteSettings.ShowLeftSaber = value;
            }
        }
        [UIValue("show-right-saber")] private bool _showRightSaber
        {
            get => _replayData.actualSettings.ShowRightSaber;
            set
            {
                _controllersManager.RightSaber.gameObject.SetActive(value);
                NotifyPropertyChanged(nameof(_showRightSaber));
                _replayData.actualToWriteSettings.ShowRightSaber = value;
            }
        }
    }
}
