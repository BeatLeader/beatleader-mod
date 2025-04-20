using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using B83.Image.GIF;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities.Async;
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

        private static async Task TryDownload(
            string url,
            Action<AvatarImage> onSuccessCallback,
            Action<string> onFailCallback
        ) {
            await Task.Run(async () => {
                var request = await RawDataRequest.Send(url).Join();

                if (request.RequestState != WebRequests.RequestState.Finished) {
                    await UnityMainThreadTaskScheduler.Factory.StartNew(() => {
                        onFailCallback?.Invoke(request.FailReason ?? "");
                    });
                    return;
                }

                var data = request.Result;
                GIFImage? gif = null;
                try {
                    using (var reader = new BinaryReader(new MemoryStream(data))) {
                        gif = new GIFLoader().Load(reader);
                    }
                } catch (Exception e) { }
                
                await UnityMainThreadTaskScheduler.Factory.StartNew(() => {
                    AvatarImage avatarImage;
                    if (gif != null) {
                        var tex = new Texture2D(
                            gif.screen.width,
                            gif.screen.height,
                            TextureFormat.RGBA32,
                            false
                        );

                        avatarImage = AvatarImage.Animated(tex, gif);
                        Cache[url] = avatarImage;
                    } else {
                        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                        var loaded = texture.LoadImage(data);

                        avatarImage = AvatarImage.Static(loaded ? texture : BundleLoader.FileError.texture);
                        if (loaded) Cache[url] = avatarImage;
                    }

                    onSuccessCallback.Invoke(avatarImage);
                });
            });
        }

        #endregion
    }
}