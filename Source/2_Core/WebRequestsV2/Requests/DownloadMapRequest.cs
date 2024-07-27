using System.Net.Http;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API { 
    public class DownloadMapRequest : PersistentWebRequestBaseWithResult<DownloadMapRequest, byte[], RawWebRequestResponseParser> {
        public static IWebRequest<byte[]> SendRequest(string mapHash) {
            return SendRet(BeatSaverUtils.CreateDownloadMapUrl(mapHash), HttpMethod.Get);
        }
        
        public static IWebRequest<byte[]> SendRequestWithUrl(string downloadUrl) {
            return SendRet(downloadUrl, HttpMethod.Get);
        }
    }
}