using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BeatLeader.Utils
{
    internal class HttpUtils
    {
        private readonly HttpClient _client = new();

        internal async Task<T> getData<T>(string url, CancellationToken token, T defaultValue)
        {
            var uri = new Uri(url);
            Plugin.Log.Debug($"Request url = {uri}");

            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri, token);

                var body = response.Content.ReadAsStringAsync().Result;

                Plugin.Log.Debug($"StatusCode: {response.StatusCode}, ReasonPhrase: '{response.ReasonPhrase}'");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerSettings()
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    return JsonConvert.DeserializeObject<T>(body, options);
                }
                else
                {
                    if (body != null)
                    {
                        if (!(body.StartsWith("{") || body.StartsWith("[")))
                        {
                            Plugin.Log.Debug($"Response content: {body}");
                        }
                    }
                }
            }
            catch (TaskCanceledException ce)
            {
                Plugin.Log.Debug("Request was cancelled");
            }
            catch (SocketException se)
            {
                Plugin.Log.Error("Socket exception");
            }
            catch (Exception e)
            {
                Plugin.Log.Error("Exception");
                Plugin.Log.Error(e);
            }

            return defaultValue;
        }
    }
}
