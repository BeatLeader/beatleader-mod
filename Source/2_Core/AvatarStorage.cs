using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

            var data = handler.data;
            
            var task = Task.Run(() => {
                using var reader = new BinaryReader(new MemoryStream(data));
                return new GIFLoader().Load(reader);
            });
            
            yield return new WaitUntil(() => task.Status is TaskStatus.Faulted or TaskStatus.RanToCompletion);
            
            AvatarImage avatarImage;

            if (task.Status is TaskStatus.RanToCompletion && task.Result != null) {
                var gifImage = task.Result;

                var tex = new Texture2D(
                    gifImage.screen.width,
                    gifImage.screen.height,
                    TextureFormat.RGBA32,
                    false
                );

                avatarImage = AvatarImage.Animated(tex, gifImage);
                Cache[url] = avatarImage;
            } else {
                var isUsableTexture = handler.texture != null && handler.texture.width != 8;
                avatarImage = AvatarImage.Static(isUsableTexture ? handler.texture : BundleLoader.FileError.texture);
                if (isUsableTexture) Cache[url] = avatarImage;
            }

            onSuccessCallback.Invoke(avatarImage);
        }

        #endregion
    }
}