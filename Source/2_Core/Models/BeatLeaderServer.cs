using System.Collections.Generic;

namespace BeatLeader.Models {
    internal enum BeatLeaderServer {
        XYZ_DOMAIN,
        NET_DOMAIN,
        COM_DOMAIN
    }

    internal static class BeatLeaderServerUtils {
        public static readonly List<BeatLeaderServer> ServerOptions = new List<BeatLeaderServer>() {
            BeatLeaderServer.COM_DOMAIN,
            BeatLeaderServer.XYZ_DOMAIN,
            BeatLeaderServer.NET_DOMAIN
        };
        
        public static string GetName(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "beatleader.net";
                case BeatLeaderServer.XYZ_DOMAIN: return "beatleader.xyz";
                case BeatLeaderServer.COM_DOMAIN:
                default: return "beatleader.com";
            }
        }

        public static string GetAPIUrl(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "https://api.beatleader.net";
                case BeatLeaderServer.XYZ_DOMAIN: return "https://api.beatleader.xyz";
                case BeatLeaderServer.COM_DOMAIN:
                default: return "https://api.beatleader.com";
            }
        }

        public static string GetWebsiteUrl(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "https://beatleader.net";
                case BeatLeaderServer.XYZ_DOMAIN: return "https://beatleader.xyz";
                case BeatLeaderServer.COM_DOMAIN:
                default: return "https://beatleader.com";
            }
        }
    }
}