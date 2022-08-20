using BeatLeader.Replayer.Movement;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.BodyMenu.bsml")]
    internal class BodyMenu : MenuWithContainer
    {
        [Inject] private readonly VRControllersManager _controllersManager;

        [SerializeAutomatically] private static bool showHead = false;
        [SerializeAutomatically] private static bool showLeftSaber = true;
        [SerializeAutomatically] private static bool showRightSaber = true;

        [UIValue("show-head")] private bool _showHead
        {
            get => _controllersManager.Head.gameObject.activeSelf;
            set
            {
                showHead = _initialized ? value : showHead;
                _controllersManager.Head.gameObject.SetActive(value);
                NotifyPropertyChanged(nameof(_showHead));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }
        [UIValue("show-left-saber")] private bool _showLeftSaber
        {
            get => _controllersManager.LeftSaber.gameObject.activeSelf;
            set
            {
                showLeftSaber = _initialized ? value : showLeftSaber;
                _controllersManager.LeftSaber.gameObject.SetActive(value);
                NotifyPropertyChanged(nameof(_showLeftSaber));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }
        [UIValue("show-right-saber")] private bool _showRightSaber
        {
            get => _controllersManager.RightSaber.gameObject.activeSelf;
            set
            {
                showRightSaber = _initialized ? value : showRightSaber;
                _controllersManager.RightSaber.gameObject.SetActive(value);
                NotifyPropertyChanged(nameof(_showRightSaber));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }

        private bool _initialized;

        protected override void OnAfterParse()
        {
            _showHead = showHead;
            _showLeftSaber = showLeftSaber;
            _showRightSaber = showRightSaber;
            _initialized = true;
        }
    }
}
