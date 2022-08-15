using BeatLeader.Replays.Movement;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.BodySetting.bsml")]
    internal class BodySetting : Setting
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
                showHead = value;
                _controllersManager.Head.gameObject.SetActive(value);
            }
        }
        [UIValue("show-left-saber")] private bool _showLeftSaber
        {
            get => _controllersManager.LeftSaber.gameObject.activeSelf;
            set
            {
                showLeftSaber = value;
                _controllersManager.LeftSaber.gameObject.SetActive(value);
            }
        }
        [UIValue("show-right-saber")] private bool _showRightSaber
        {
            get => _controllersManager.RightSaber.gameObject.activeSelf;
            set
            {
                showRightSaber = value;
                _controllersManager.RightSaber.gameObject.SetActive(value);
            }
        }

        public override bool IsSubMenu => true;
        public override int SettingIndex => 0;

        protected override void OnAfterHandling()
        {
            _showHead = showHead;
            _showLeftSaber = showLeftSaber;
            _showRightSaber = showRightSaber;
        }
    }
}
