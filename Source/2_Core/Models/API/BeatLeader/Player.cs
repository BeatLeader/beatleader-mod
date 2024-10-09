using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using Newtonsoft.Json;
using BeatLeader.Themes;
using BeatLeader.Utils;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    public class User {
        public Player player;
        public Player[] friends;
    }

    [Flags]
    public enum LeaderboardContexts {
        None = 0,
        General = 1 << 1,
        NoMods = 1 << 2,
        NoPause = 1 << 3,
        Golf = 1 << 4,
        SCPM = 1 << 5
    }

    public class PlayerContextExtension {
        public LeaderboardContexts context;
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
        string IPlayer.Country => country;
        float IPlayer.PerformancePoints => pp;
        IPlayerProfileSettings? IPlayer.ProfileSettings => profileSettings;

        private static readonly Dictionary<string, AvatarSettings?> avatarSettingsCache = new();
        private static readonly Dictionary<string, SemaphoreSlim?> semaphores = new();

        public async Task<AvatarSettings> GetBeatAvatarAsync(bool bypassCache) {
            var semaphore = semaphores.GetOrAdd(id, new SemaphoreSlim(1, 1))!;
            await semaphore.WaitAsync();
            //fetching if needed
            if (!avatarSettingsCache.TryGetValue(id, out var avatarSettings) || bypassCache) {
                var request = await GetAvatarRequest.Send(id).Join();
                avatarSettings = request.Result;
                avatarSettingsCache[id] = avatarSettings;
            }
            //returning
            semaphore.Release();
            var settings = AvatarSettings.FromAvatarData(AvatarUtils.DefaultAvatarData);
            return avatarSettings ?? settings;
        }

        #endregion

        public static readonly Player GuestPlayer = new() {
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
        public float pp;
        public string role;
        public Clan[] clans;
        public ServiceIntegration[] socials;
        public PlayerContextExtension[]? contextExtensions;
        public ProfileSettings? profileSettings;

        public Player GetContextPlayer(LeaderboardContexts context) {
            var contextPlayer = contextExtensions?.FirstOrDefault(ce => ce.context == context);
            if (contextPlayer == null) return this;
            return new Player {
                id = id,
                rank = contextPlayer.rank,
                name = name,
                avatar = avatar,
                country = country,
                countryRank = contextPlayer.countryRank,
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