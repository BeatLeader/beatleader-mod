using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BeatLeader.Models {
    internal enum BeatLeaderServer {
        COM_DOMAIN,
        NET_DOMAIN,
    }

    internal static class BeatLeaderServerUtils {
        public static readonly List<BeatLeaderServer> ServerOptions = new List<BeatLeaderServer>() {
            BeatLeaderServer.COM_DOMAIN,
            BeatLeaderServer.NET_DOMAIN
        };
        
        public static string GetName(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "beatleader.net";
                case BeatLeaderServer.COM_DOMAIN:
                default: return "beatleader.com";
            }
        }

        public static string GetAPIUrl(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "https://api.beatleader.net";
                case BeatLeaderServer.COM_DOMAIN:
                default: return "https://api.beatleader.com";
            }
        }

        public static string GetWebsiteUrl(this BeatLeaderServer server) {
            switch (server) {
                case BeatLeaderServer.NET_DOMAIN: return "https://beatleader.net";
                case BeatLeaderServer.COM_DOMAIN:
                default: return "https://beatleader.com";
            }
        }

        #region ReplaceDomain

        private static readonly Regex DomainRegex = new(
            @"(?<=^https?://(?:[^/?#]*\.)?)beatleader\.(?:xyz|com|net)(?=[/:?#]|$)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Replaces any beatleader domain (.xyz/.com/.net) in the url host with the domain of the specified server.
        /// Urls pointing to other hosts are returned unchanged.
        /// </summary>
        public static string ReplaceDomain(this BeatLeaderServer server, string url) {
            if (string.IsNullOrEmpty(url)) return url;
            return DomainRegex.Replace(url, server.GetName(), 1);
        }

        public static Uri ReplaceDomain(this BeatLeaderServer server, Uri uri) {
            if (!uri.IsAbsoluteUri) return uri;
            var original = uri.OriginalString;
            var replaced = server.ReplaceDomain(original);
            return original == replaced ? uri : new Uri(replaced);
        }

        public static string ReplaceDomain(string url) {
            return PluginConfig.MainServer.ReplaceDomain(url);
        }

        public static Uri ReplaceDomain(Uri uri) {
            return PluginConfig.MainServer.ReplaceDomain(uri);
        }

        #endregion
    }
}