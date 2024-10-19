using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BeatLeader.UI.Rendering {
    /// <summary>
    /// A post-process renderer for a camera space canvas.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    internal class PostProcessCanvasRenderer : MonoBehaviour {
        public readonly List<PostProcessModule> modules = new();
        public int lodFactor = 1;

        private RenderTexture _renderTexture = null!;
        private RenderTexture _lodRenderTexture = null!;
        private Camera _camera = null!;

        private void Start() {
            _renderTexture = CreateTexture(1);
            _lodRenderTexture = CreateTexture(lodFactor);
            _camera = GetComponent<Camera>();
        }

        private void Update() {
            StartCoroutine(RenderAfterEverything());
        }

        private IEnumerator RenderAfterEverything() {
            yield return new WaitForEndOfFrame();
            // getting the actual image
            Graphics.Blit(null, _renderTexture);
            // getting the downscaled image
            Graphics.Blit(_renderTexture, _lodRenderTexture, new Vector2(1, -1), new Vector2(0, 1));
            // pre-processing modules
            foreach (var module in modules) {
                module.Process(_lodRenderTexture);
            }
            // rendering the ui
            _camera.Render();
        }

        private static RenderTexture CreateTexture(int factor) {
            return new RenderTexture(
                Screen.width / factor,
                Screen.height / factor,
                32,
                RenderTextureFormat.ARGB32
            );
        }
    }
}