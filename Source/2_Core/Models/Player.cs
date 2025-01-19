﻿using System;
using System.Linq;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    public class User {
        public Player player;
        public Player[] friends;
    }

    public interface IPlayer {
        public string country { get; set; }
        public float pp { get; set; }
        public int rank { get; set; }
        public int countryRank { get; set; }
    }

    public class PlayerContextExtension : IPlayer {
        public int context { get; set; }
        public float pp { get; set; }

        public int rank { get; set; }
        public string country { get; set; }
        public int countryRank { get; set; }
    }

    public class Player : IPlayer {
        public string id;
        public int rank { get; set; }
        public string name;
        public string? avatar;
        public string country { get; set; }
        public int countryRank { get; set; }
        public float pp { get; set; }
        public string role;
        public Clan[] clans;
        public ServiceIntegration[] socials;
        public PlayerContextExtension[]? contextExtensions;
        public ProfileSettings? profileSettings;

        public Player ContextPlayer(int context) {
            var contextPlayer = this.contextExtensions?.FirstOrDefault(ce => ce.context == context);
            if (contextPlayer == null) {
                return this;
            } else {
                return new Player {
                    id = this.id,
                    rank = contextPlayer.rank,
                    name = this.name,
                    avatar = this.avatar,
                    country = this.country,
                    countryRank = contextPlayer.countryRank,
                    pp = contextPlayer.pp,
                    role = this.role,
                    clans = this.clans,
                    socials = this.socials,
                    profileSettings = this.profileSettings
                };
            }
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

    public class ProfileSettings {
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
        public string effectName;
    }

    public class ServiceIntegration {
        public string service;
        public string link;
        public string user;
    }
}