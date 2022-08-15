using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Components.Settings
{
    internal class SubMenu : MonoBehaviour
    {
        public string Name => Setting.SubMenuName;
        public Setting Setting { get; private set; }

        public void Init(Setting setting)
        {
            gameObject.GetOrAddComponent<RectTransform>();
            setting.ContentTransform.SetParent(transform, false);
            Setting = setting;
        }
    }
}
