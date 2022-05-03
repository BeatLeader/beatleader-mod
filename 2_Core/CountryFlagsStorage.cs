using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader {
    public static class CountryFlagsStorage {
        #region Cache

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static void ClearCache() {
            Cache.Clear();
        }

        #endregion

        #region GetCountryFlagCoroutine

        public static IEnumerator GetCountryFlagCoroutine(
            string country,
            bool forceUpdate,
            Action<Sprite> onSuccessCallback,
            Action<string> onFailCallback
        ) {
            if (!forceUpdate && Cache.ContainsKey(country)) {
                onSuccessCallback.Invoke(Cache[country]);
                yield break;
            }

            yield return TryDownload(country, onSuccessCallback, onFailCallback);
        }

        #endregion

        #region TryDownload

        private static IEnumerator TryDownload(
            string country,
            Action<Sprite> onSuccessCallback,
            Action<string> onFailCallback
        ) {
            var handler = new DownloadHandlerTexture();
            var request = new UnityWebRequest(GetCountryFlagLink(country), UnityWebRequest.kHttpVerbGET);
            request.downloadHandler = handler;
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError) {
                onFailCallback?.Invoke(request.error);
                yield break;
            }

            var texture = ResampleTexture(handler.texture);
            var sprite = Sprite.Create(
                texture,
                Rect.MinMaxRect(0, 0, texture.width, texture.height),
                Vector2.one / 2
            );
            Cache[country] = sprite;
            onSuccessCallback.Invoke(sprite);
        }

        private static string GetCountryFlagLink(string country) {
            return $"https://cdn.beatleader.xyz/flags/{country.ToLower()}.png";
        }

        #endregion

        #region ResampleTexture
        
        private const int Width = 32;

        private static Texture2D ResampleTexture(Texture2D source) {
            var aspectRatio = (float) source.height / source.width;
            var height = (int) (Width * aspectRatio);
            
            var bufferTexture = new RenderTexture(Width, height, 0, RenderTextureFormat.Default);
            bufferTexture.Create();
            
            Graphics.Blit(source, bufferTexture);
            
            GL.Flush();
            var texture2D = new Texture2D(Width, height, TextureFormat.RGB24, false) {
                wrapMode = TextureWrapMode.Clamp
            };
            RenderTexture.active = bufferTexture;
            texture2D.ReadPixels(new Rect(0.0f, 0.0f, Width, height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
            GL.Flush();
            
            return texture2D;
        }

        #endregion
    }
}