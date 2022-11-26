using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class CanvasScreen : HMUI.Screen {
        public Canvas Canvas { get; private set; }
        public CanvasScaler CanvasScaler { get; private set; }

        private void Awake() {
            Canvas = gameObject.AddComponent<Canvas>();
            CanvasScaler = gameObject.AddComponent<CanvasScaler>();
        }
    }
}
