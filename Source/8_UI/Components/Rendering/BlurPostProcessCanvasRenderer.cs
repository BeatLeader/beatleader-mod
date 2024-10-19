using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace BeatLeader.UI.Rendering {
    /// <summary>
    /// A post-process renderer for a camera space canvas.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    internal class BlurPostProcessCanvasRenderer : MonoBehaviour {
        public int lodFactor = 1;

        private static readonly int blurredTexPropertyId = Shader.PropertyToID("_BlurredTex");
        private static readonly int blurTempRTPropertyId = Shader.PropertyToID("_BlurTempRT");

        private RenderTexture? _blurredTexture;
        private CommandBuffer? _commandBuffer;
        private Camera _camera = null!;

        private void Start() {
            _camera = GetComponent<Camera>();
            _blurredTexture = CreateTexture(lodFactor);
            _commandBuffer = BuildCommandBuffer(_blurredTexture, BundleLoader.Materials.blurMaterial);
            _camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
            StartCoroutine(RenderAfterEverything());
        }

        private void OnDestroy() {
            _commandBuffer?.Release();
            _blurredTexture?.Release();
        }

        private IEnumerator RenderAfterEverything() {
            while (enabled) {
                yield return new WaitForEndOfFrame();
                _camera.Render();
            }
        }

        private static CommandBuffer BuildCommandBuffer(RenderTexture blurredTexture, Material blurBlitMaterial) {
            //not a part of a CommandBuffer but whatever, has to be called once somewhere :D
            Shader.SetGlobalTexture(blurredTexPropertyId, blurredTexture);

            var commandBuffer = new CommandBuffer { name = "BeatLeader.OverlayCanvas" };
            //create temporary RT
            commandBuffer.GetTemporaryRT(blurTempRTPropertyId, blurredTexture.width, blurredTexture.height, 0, FilterMode.Bilinear, GraphicsFormat.R32G32B32A32_SFloat);
            //downscale to temporary RT
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, blurTempRTPropertyId);
            //apply blur
            commandBuffer.Blit(blurTempRTPropertyId, blurredTexture, blurBlitMaterial);
            //release temporary RT
            commandBuffer.ReleaseTemporaryRT(blurTempRTPropertyId);
            return commandBuffer;
        }

        private static RenderTexture CreateTexture(int factor) {
            var renderTexture = new RenderTexture(
                Mathf.Max(Screen.width / factor, 1),
                Mathf.Max(Screen.height / factor, 1),
                0,
                RenderTextureFormat.ARGB32
            );
            renderTexture.Create();
            return renderTexture;
        }
    }
}