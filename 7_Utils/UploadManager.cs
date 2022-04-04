using BeatLeader.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using BeatLeader.Manager;
using Newtonsoft.Json;

namespace BeatLeader.Utils
{
    class UploadManager
    {
        private static readonly string _url = BLConstants.REPLAY_UPLOAD_URL;
        private static readonly int _retry = 3;
        private static readonly HttpClient _client = new();

        public async static Task UploadReplay(Replay replay)
        {
            string authToken = BLContext.steamAuthToken;
            if (authToken == null)
            {
                Plugin.Log.Debug("No auth token, skip replay upload");
                return; // auth failed, no upload
            }

            LeaderboardEvents.NotifyUploadStarted();

            MemoryStream stream = new();
            ReplayEncoder.Encode(replay, new BinaryWriter(stream, Encoding.UTF8));

            for (int i = 1; i <= _retry; i++)
            {
                Plugin.Log.Debug($"Attempt to upload replay {i}/{_retry}");
                try
                {
                    ByteArrayContent content = new(stream.ToArray());

                    var httpRequestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_url + "?ticket=" + authToken),
                        Content = content
                    };

                    HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);

                    var body = await response.Content.ReadAsStringAsync();
                    Plugin.Log.Debug($"StatusCode: {response.StatusCode}, ReasonPhrase: '{response.ReasonPhrase}'");
                    if (body != null && body.Length > 0) {
                        if (!(body.StartsWith("{") || body.StartsWith("[") || body.StartsWith("<"))) {
                            Plugin.Log.Debug($"Response content: {body}");
                        }
                    }

                    if (response.IsSuccessStatusCode) {
                        Plugin.Log.Debug(body);
                        var options = new JsonSerializerSettings() {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore
                        };
                        Score score = JsonConvert.DeserializeObject<Score>(body, options);
                        // Plugin.Log.Debug(score.player.name); update profile from score.player ?
                        Plugin.Log.Debug("Upload success");

                        LeaderboardEvents.NotifyUploadSuccess();

                        return; // if OK - stop retry cycle
                    } else {
                        LeaderboardEvents.NotifyUploadFailed(i == _retry, i);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Debug("Exception");
                    Plugin.Log.Debug(e);
                    LeaderboardEvents.NotifyUploadFailed(i == _retry, i);
                }
            }
            Plugin.Log.Debug("Cannot upload replay");
        }
    }
}
