using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.UI.BSML_Addons.Components {
    public class BetterButton : MonoBehaviour {
        public Button Button { get; protected set; }
        public Image TargetGraphic { get; protected set; }

        public void Init(Button button, Image targetGraphic) {
            Button = button;
            TargetGraphic = targetGraphic;
            Button.targetGraphic = targetGraphic;
            Button.navigation = new Navigation() {
                mode = Navigation.Mode.None
            };
        }
    }
}
