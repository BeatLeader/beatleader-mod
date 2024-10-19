using BeatLeader.Models;
using BeatLeader.UI.Rendering;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Replayer.Desktop {
    internal class ReplayerDesktopUIRenderer : MonoBehaviour {
        public Camera RenderCamera { get; private set; } = null!;

        private PostProcessCanvasRenderer _postProcessRenderer = null!;
        
        private void Awake() {
            RenderCamera = gameObject.AddComponent<Camera>();
            RenderCamera.cullingMask = 1 << 5;
            RenderCamera.clearFlags = CameraClearFlags.Depth;
            RenderCamera.nearClipPlane = 0.1f;
            RenderCamera.farClipPlane = 1f;
            RenderCamera.enabled = false;
            _postProcessRenderer = gameObject.AddComponent<PostProcessCanvasRenderer>();
            _postProcessRenderer.lodFactor = 5;
        }

        public void ReloadEffects(GameObject uiRoot) {
            _postProcessRenderer.modules.Clear();
            var modules = uiRoot.GetComponentsInChildren<PostProcessModule>(true);
            _postProcessRenderer.modules.AddRange(modules);
        }
    }
}