using System;
using System.Collections;
using B83.Image.GIF;
using UnityEngine;

namespace BeatLeader.Models {
    public class AvatarImage {
        #region Constructors

        public readonly bool IsAnimated;
        private readonly Texture2D _originalTexture;
        private readonly GIFImage _gifImage;

        private AvatarImage(
            Texture2D originalTexture,
            bool isAnimated,
            GIFImage gifImage
        ) {
            IsAnimated = isAnimated;
            _originalTexture = originalTexture;
            _gifImage = gifImage;
        }

        public static AvatarImage Static(Texture2D texture) {
            return new AvatarImage(texture, false, null);
        }

        public static AvatarImage Animated(Texture2D texture, GIFImage gifImage) {
            return new AvatarImage(texture, true, gifImage);
        }

        #endregion

        #region PlaybackCoroutine

        public IEnumerator PlaybackCoroutine(RenderTexture targetTexture) {
            if (!IsAnimated) {
                Graphics.Blit(_originalTexture, targetTexture);
                yield break;
            }

            if (_gifImage.imageData.Count == 0) yield break;

            var colors = _originalTexture.GetPixels32();
            for (var i = 0; i < colors.Length; i++) {
                colors[i] = new Color32(0, 0, 0, 0);
            }

            while (true) {
                foreach (var frame in _gifImage.imageData) {
                    try {
                        frame.DrawTo(colors, _originalTexture.width, _originalTexture.height);
                    } catch (Exception) {
                        Plugin.Log.Error("Broken GIF!");
                        _originalTexture.SetPixels32(colors);
                        _originalTexture.Apply();
                        Graphics.Blit(_originalTexture, targetTexture);
                        yield break;
                    }
                    
                    _originalTexture.SetPixels32(colors);
                    _originalTexture.Apply();
                    Graphics.Blit(_originalTexture, targetTexture);
                    yield return new WaitForSeconds(frame.graphicControl.fdelay);
                    frame.Dispose(colors, _originalTexture.width, _originalTexture.height);
                }
            }
        }

        #endregion
    }
}