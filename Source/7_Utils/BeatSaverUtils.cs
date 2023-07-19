using System.Threading.Tasks;
using BeatLeader.Models.BeatSaver;
using Newtonsoft.Json;
using static BeatLeader.BeatSaverAPI.BeatSaverConstants;

namespace BeatLeader.Utils {
    internal static class BeatSaverUtils {
        public static async Task<MapDetail?> GetMapByHashAsync(string hash) {
            return await WebUtils.SendAsync(BEATSAVER_API_URL + MAPS_HASH_ENDPOINT + hash) is { IsSuccessStatusCode: true } res ?
                JsonConvert.DeserializeObject<MapDetail>(await res.Content.ReadAsStringAsync()) : null;
        }

        public static string CreateDownloadMapUrl(string mapHash) {
            return $"{BEATSAVER_CDN_URL}{mapHash.ToLower()}.zip";
        }
        
        public static string CreateMapPageUrl(string bsr) {
            return $"{BEATSAVER_WEBSITE_URL}{MAPS_ENDPOINT}{bsr}";
        }

        public static string FormatBeatmapFolderName(string? id, string? songName, string? authorName, string? hash) {
            return $"{id} ({songName} - {authorName}) [{hash}]";
        }
    }
}