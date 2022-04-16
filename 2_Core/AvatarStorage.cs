using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using B83.Image.GIF;
using BeatLeader.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader {
    public static class AvatarStorage {
        #region Cache

        private static readonly Dictionary<string, AvatarImage> Cache = new Dictionary<string, AvatarImage>();

        public static void ClearCache() {
            Cache.Clear();
        }

        #endregion

        #region GetPlayerAvatarCoroutine

        public static IEnumerator GetPlayerAvatarCoroutine(
            string url,
            bool forceUpdate,
            Action<AvatarImage> onSuccessCallback,
            Action<string> onFailCallback
        ) {
            if (!forceUpdate && Cache.ContainsKey(url)) {
                onSuccessCallback.Invoke(Cache[url]);
                yield break;
            }

            yield return TryDownload(url, onSuccessCallback, onFailCallback);
        }

        #endregion

        #region TryDownload

        private static IEnumerator TryDownload(
            string url,
            Action<AvatarImage> onSuccessCallback,
            Action<string> onFailCallback
        ) {
            var handler = new DownloadHandlerTexture();
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            request.downloadHandler = handler;
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError) {
                onFailCallback?.Invoke(request.error);
                yield break;
            }

            AvatarImage avatarImage;

            try {
                avatarImage = TryLoadGif(handler.data);
                Cache[url] = avatarImage;
            } catch (Exception e) {
                var isUsableTexture = (handler.texture != null) && (handler.texture.width != 8);
                avatarImage = AvatarImage.Static(isUsableTexture ? handler.texture : BundleLoader.FileError.texture);
                if (isUsableTexture) Cache[url] = avatarImage;
            }

            onSuccessCallback.Invoke(avatarImage);
        }

        #endregion

        #region TryLoadGif

        private static AvatarImage TryLoadGif(byte[] data) {
            var reader = new BinaryReader(new MemoryStream(data));
            var image = new GIFLoader().Load(reader);

            var tex = new Texture2D(
                image.screen.width,
                image.screen.height,
                TextureFormat.RGBA32,
                false
            );

            return AvatarImage.Animated(tex, image);
        }

        #endregion
    }
}