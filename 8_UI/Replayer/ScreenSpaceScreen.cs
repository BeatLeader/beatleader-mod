using BeatLeader.Utils;
using HMUI;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ScreenSpaceScreen : HMUI.Screen {
        public int SortingOrder {
            get => _canvas.sortingOrder; 
            set => _canvas.sortingOrder = value; 
        }
        public CanvasScaler CanvasScaler { get; private set; }

        private Canvas _canvas;

        private void Awake() {
            _canvas = gameObject.AddComponent<Canvas>();
            CanvasScaler = gameObject.AddComponent<CanvasScaler>();

            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1;
            _canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.TexCoord2;

            CanvasScaler.referenceResolution = new(350, 300);
            CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            CanvasScaler.dynamicPixelsPerUnit = 3.44f;
            CanvasScaler.referencePixelsPerUnit = 10;
        }
        public override void SetRootViewController(ViewController newRootViewController, ViewController.AnimationType animationType) {
            newRootViewController.gameObject.GetOrAddComponent<GraphicRaycaster>();
            base.SetRootViewController(newRootViewController, animationType);
        }
    }
}
