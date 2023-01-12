using BeatLeader.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class CanvasScreen : HMUI.Screen {
        public Canvas Canvas { get; private set; } = null!;
        public CanvasScaler CanvasScaler { get; private set; } = null!;
        public CanvasGroup CanvasGroup { get; private set; } = null!;

        private void Awake() {
            Canvas = gameObject.AddComponent<Canvas>();
            CanvasScaler = gameObject.AddComponent<CanvasScaler>();
            CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }
    }
}
