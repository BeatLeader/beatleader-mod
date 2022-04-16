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

            handler.texture.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(
                handler.texture,
                Rect.MinMaxRect(0, 0, handler.texture.width, handler.texture.height),
                Vector2.one / 2
            );
            Cache[country] = sprite;
            onSuccessCallback.Invoke(sprite);
        }

        private static string GetCountryFlagLink(string country) {
            return $"https://cdn.beatleader.xyz/flags/{country.ToLower()}.png";
        }

        #endregion
    }
}