using System;
using System.Collections.Generic;
using System.IO;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using Newtonsoft.Json;

namespace BeatLeader.DataManager {
    internal static class HiddenPlayersCache {
        #region Cache file

        private static string CacheFileName => Path.Combine(UnityGame.UserDataPath, "BeatLeader", "HiddenPlayers");

        private static JsonSerializerSettings SerializerSettings => new() {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        static HiddenPlayersCache() {
            try {
                if (!File.Exists(CacheFileName)) return;

                HiddenPlayers = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(CacheFileName), SerializerSettings);
            } catch (Exception e) {
                Plugin.Log.Debug($"HiddenPlayers cache load failed! {e}");
            }
        }

        private static void SaveHiddenPlayersCache() {
            try {
                FileManager.EnsureDirectoryExists(CacheFileName);
                File.WriteAllText(CacheFileName, JsonConvert.SerializeObject(HiddenPlayers, SerializerSettings));
            } catch (Exception e) {
                Plugin.Log.Debug($"HiddenPlayers cache save failed! {e}");
            }
        }

        #endregion

        #region Logic

        public static event Action HiddenPlayersUpdatedEvent;

        private static readonly HashSet<string> HiddenPlayers = new();

        public static void HidePlayer(Player player) {
            HiddenPlayers.Add(player.id);
            HiddenPlayersUpdatedEvent?.Invoke();
            SaveHiddenPlayersCache();
        }

        public static void RevealPlayer(Player player) {
            HiddenPlayers.Remove(player.id);
            HiddenPlayersUpdatedEvent?.Invoke();
            SaveHiddenPlayersCache();
        }

        public static Player HidePlayerIfNeeded(Player player) {
            if (!IsHidden(player)) return player;
            
            return new Player() {
                id = player.id,
                rank = 0,
                name = "~hidden player~",
                country = "not set",
                countryRank = 0,
                experience = 0,
                level = 0,
                prestige = 0,
                pp = 0f,
                role = "",
                clans = Array.Empty<Clan>(),
                socials = Array.Empty<ServiceIntegration>(),
                profileSettings = null
            };
        }

        public static bool IsHidden(Player? player) {
            return player != null && HiddenPlayers.Contains(player.id);
        }

        #endregion
    }
}