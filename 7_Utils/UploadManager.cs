using BeatLeader.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;

namespace BeatLeader.Utils
{
    class UploadManager
    {
        private static readonly string _url = "https://beatleader.azurewebsites.net/replay";
        private static readonly int _retry = 3;
        private static readonly HttpClient _client = new();

        public static string authToken;

        public async static void UploadReplay(Replay replay)
        {
            if (authToken == null) return; // auth failed, no upload

            MemoryStream stream = new();
            ReplayEncoder.Encode(replay, new BinaryWriter(stream, Encoding.UTF8));

            for (int i = 0; i < _retry; i++)
            {
                try
                {
                    ByteArrayContent content = new(stream.ToArray());
                    //HttpResponseMessage response = await _client.PostAsync(_url, content);

                    var httpRequestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_url + "?ticket=" + authToken),
                        //Headers = {
                            //{ HttpRequestHeader.Authorization.ToString(), authToken }
                        //},
                        Content = content
                    };

                    HttpResponseMessage response = _client.SendAsync(httpRequestMessage).Result;

                    Plugin.Log.Debug(response.ToString());
                    if (response.Content != null) { Plugin.Log.Debug(await response.Content.ReadAsStringAsync()); }

                    if (response.IsSuccessStatusCode)
                    {
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
