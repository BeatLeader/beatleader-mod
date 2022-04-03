using BeatLeader.Replays.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace BeatLeader.Utils
{
    class UploadManager
    {
        private static readonly string _url = "https://beatleader.azurewebsites.net/replay";
        private static readonly int _retry = 3;
        private static readonly HttpClient _client = new();

        public static string authToken;

        public async static Task UploadReplay(Replay replay)
        {
            if (authToken == null)
            {
                Plugin.Log.Debug("No auth token, skip replay upload");
                return; // auth failed, no upload
            }

            MemoryStream stream = new();
            ReplayEncoder.Encode(replay, new BinaryWriter(stream, Encoding.UTF8));

            for (int i = 0; i < _retry; i++)
            {
                Plugin.Log.Debug($"Attempt to upload replay {i + 1}/{_retry}");
                try
                {
                    ByteArrayContent content = new(stream.ToArray());

                    var httpRequestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_url + "?ticket=" + authToken),
                        Content = content
                    };

                    HttpResponseMessage response = _client.SendAsync(httpRequestMessage).Result;

                    var body = response.Content.ReadAsStringAsync().Result;
                    Plugin.Log.Debug($"StatusCode: {response.StatusCode}, ReasonPhrase: '{response.ReasonPhrase}'");
                    if (body != null && !body.StartsWith("{")) { Plugin.Log.Debug($"Response content: {body}"); }

                    if (response.IsSuccessStatusCode)
                    {
                        Plugin.Log.Debug("Upload success");
                        // TODO : notify UI to update leaderboard View
                        return; // if OK - stop retry cycle
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Debug("Exception");
                    Plugin.Log.Debug(e);
                }
            }
            Plugin.Log.Debug("Cannot upload replay");
        }
    }
}
