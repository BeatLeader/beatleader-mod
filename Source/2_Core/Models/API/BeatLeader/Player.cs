using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using Newtonsoft.Json;
using BeatLeader.Themes;
using BeatLeader.Utils;
using BeatSaber.BeatAvatarSDK;

namespace BeatLeader.Models {
    public class PlayerContextExtension {
        public int context;
        public float pp;
        public int rank;
        public int countryRank;
        public string country;
    }

    public class Player : IPlayer {
        #region Player Impl

        string IPlayer.Id => id;
        string IPlayer.Name => name;
        string? IPlayer.AvatarUrl => avatar;
        int IPlayer.Rank => rank;
        int IPlayer.CountryRank => countryRank;
        int IPlayer.Level => level;
        int IPlayer.Experience => experience;
        int IPlayer.Prestige => prestige;
        string IPlayer.Country => country;
        float IPlayer.PerformancePoints => pp;
        IPlayerProfileSettings? IPlayer.ProfileSettings => profileSettings;

        private static readonly Dictionary<string, AvatarSettings?> avatarSettingsCache = new();
        private static readonly Dictionary<string, SemaphoreSlim?> semaphores = new();

        private static readonly object locker = new();

        public async Task<AvatarData> GetBeatAvatarAsync(bool bypassCache, CancellationToken token) {
            SemaphoreSlim semaphore;

            lock (locker) {
                if (!semaphores.TryGetValue(id, out semaphore!)) {
                    semaphore = new(1, 1);
                    semaphores[id] = semaphore;
                }
            }

            await semaphore.WaitAsync(token);
            
            AvatarSettings? avatarSettings;
            try {
                if (!avatarSettingsCache.TryGetValue(id, out avatarSettings) || bypassCache) {
                    var request = await GetAvatarRequest.Send(id).Join();
                    avatarSettings = request.Result;
                    avatarSettingsCache[id] = avatarSettings;
                }
            } finally {
                semaphore.Release();
            }

            return avatarSettings?.ToAvatarData() ?? AvatarUtils.DefaultAvatarData;
        }

        #endregion

        public static readonly Player GuestPlayer = new() {
            id = "0",
            name = "Guest",
            avatar = null,
            country = "not set",
            rank = -1,
            pp = -1
        };

        public string id;
        public int rank;
        public string name;
        public string? avatar;
        public string country;
        public int countryRank;
        public int level;
        public int experience;
        public int prestige;
        public float pp;
        public string role;
        public string[] friends;
        public Clan[] clans;
        public ServiceIntegration[] socials;
        public PlayerContextExtension[]? contextExtensions;
        public ProfileSettings? profileSettings;

        public Player ContextPlayer(int context) {
            var contextPlayer = contextExtensions?.FirstOrDefault(ce => ce.context == context);
            if (contextPlayer == null) return this;
            return new Player {
                id = id,
                rank = contextPlayer.rank,
                name = name,
                avatar = avatar,
                country = country,
                countryRank = contextPlayer.countryRank,
                level = level,
                experience = experience,
                prestige = prestige,
                pp = contextPlayer.pp,
                role = role,
                clans = clans,
                socials = socials,
                profileSettings = profileSettings
            };
        }
    }

    public class Clan {
        public int id;
        public string tag;
        public string color;
        public string name;
        public string avatar;
        public int rank;
        public int captureLeaderboardsCount;
        public float rankedPoolPercentCaptured;
    }

    public class ProfileSettings : IPlayerProfileSettings {
        #region PlayerProfileSettings Impl

        string IPlayerProfileSettings.UserMessage => message;
        int IPlayerProfileSettings.EffectHue => hue;
        float IPlayerProfileSettings.EffectSaturation => saturation;

        #endregion

        public ThemeType ThemeType { get; private set; }
        public ThemeTier ThemeTier { get; private set; }

        [JsonProperty("effectName")]
        private string EffectName {
            set => RefreshTheme(value);
        }

        [JsonProperty("hue")]
        private int? Hue {
            set => hue = value ?? 0;
        }

        [JsonProperty("saturation")]
        private float? Saturation {
            set => saturation = value ?? 0;
        }

        [JsonIgnore]
        public int hue;

        [JsonIgnore]
        public float saturation;

        public string message;

        private void RefreshTheme(string effectName) {
            ThemesUtils.ParseEffectName(effectName, out var themeType, out var themeTier);
            ThemeType = themeType;
            ThemeTier = themeTier;
        }
    }

    public class ServiceIntegration {
        public string service;
        public string link;
        public string user;
    }
}