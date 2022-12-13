using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class HorizontalSeparator : ReeUIComponentV2 {
        public float SeparatorHeight {
            get => _separatorElement.preferredHeight;
            set => _separatorElement.preferredHeight = value;
        }

        [UIComponent("separator-group")]
        private readonly LayoutElement _separatorElement;
    }
}
