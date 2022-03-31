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

            var avatarImage = new AvatarImage {
                Texture = handler.texture,
                PlaybackCoroutine = null
            };

            try {
                EnhanceGif(handler.data, ref avatarImage);
            } catch (Exception) {
                // ignored
            }

            Cache[url] = avatarImage;
            onSuccessCallback.Invoke(avatarImage);
        }

        #endregion

        #region EnhanceGif

        private static void EnhanceGif(byte[] data, ref AvatarImage avatarImage) {
            var reader = new BinaryReader(new MemoryStream(data));

            var gifLoader = new GIFLoader();
            var image = gifLoader.Load(reader);

            var tex = new Texture2D(image.screen.width, image.screen.height);

            avatarImage.Texture = tex;
            avatarImage.PlaybackCoroutine = GifPlaybackCoroutine(tex, image);
        }

        private static IEnumerator GifPlaybackCoroutine(Texture2D texture, GIFImage gifImage) {
            if (gifImage.imageData.Count == 0) yield break;

            var colors = texture.GetPixels32();
            while (true) {
                foreach (var frame in gifImage.imageData) {
                    frame.DrawTo(colors, texture.width, texture.height);
                    texture.SetPixels32(colors);
                    texture.Apply();
                    yield return new WaitForSeconds(frame.graphicControl.fdelay);
                }
            }
        }

        #endregion
    }
}