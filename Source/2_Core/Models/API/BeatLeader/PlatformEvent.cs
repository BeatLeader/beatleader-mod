using System;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models {
    [PublicAPI]
    public class PlatformEvent {
        public required string id;
        public required string image;
        public required string name;
        public required long endDate;
        public required bool downloadable;
        public required int playerCount;
        public required int playlistId;
        public required int eventType;
        
        public string? description;
        public string? mainColor;
        public string? secondaryColor;

        private Color? _mainColor;
        private bool _mainColorSet;
        
        private Color? _secondaryColor;
        private bool _secondaryColorSet;

        public bool IsHappening() {
            return DateTime.UtcNow.ToUnixTime() < endDate;
        }

        public TimeSpan ExpiresIn() {
            return endDate.AsUnixTime() - DateTime.UtcNow;
        }

        public Color? MainColor() {
            if (!_mainColorSet) {
                if (ColorUtility.TryParseHtmlString(mainColor, out var color)) {
                    _mainColor = color;
                }

                _mainColorSet = true;
            }

            return _mainColor;
        }
        
        public Color? SecondaryColor() {
            if (!_secondaryColorSet) {
                if (ColorUtility.TryParseHtmlString(secondaryColor, out var color)) {
                    _secondaryColor = color;
                }

                _secondaryColorSet = true;
            }

            return _secondaryColor;
        }
    }
}