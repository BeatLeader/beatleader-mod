using BeatLeader.UI.Rendering;
using UnityEngine;

namespace BeatLeader.UI.Replayer.Desktop {
    internal class ReplayerDesktopUIRenderer : MonoBehaviour {
        public Camera RenderCamera { get; private set; } = null!;

        private void Awake() {
            RenderCamera = gameObject.AddComponent<Camera>();
            RenderCamera.cullingMask = 1 << 5;
            RenderCamera.clearFlags = CameraClearFlags.Depth;
            RenderCamera.nearClipPlane = 0.1f;
            RenderCamera.farClipPlane = 1f;
            RenderCamera.enabled = false;
            var postProcessRenderer = gameObject.AddComponent<BlurPostProcessCanvasRenderer>();
            postProcessRenderer.lodFactor = 2;
        }
    }
}