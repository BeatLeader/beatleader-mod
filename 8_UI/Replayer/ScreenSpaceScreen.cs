using BeatLeader.Utils;
using HMUI;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components
{
    internal class ScreenSpaceScreen : HMUI.Screen
    {
        private Canvas _canvas;
        private CanvasScaler _canvasScaler;

        private void Awake()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvasScaler = gameObject.AddComponent<CanvasScaler>();

            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1;
            _canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.TexCoord2;

            _canvasScaler.referenceResolution = new(350, 300);
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.dynamicPixelsPerUnit = 3.44f;
            _canvasScaler.referencePixelsPerUnit = 10;
        }
        public override void SetRootViewController(ViewController newRootViewController, ViewController.AnimationType animationType)
        {
            newRootViewController.gameObject.GetOrAddComponent<GraphicRaycaster>();
            base.SetRootViewController(newRootViewController, animationType);
        }
    }
}
