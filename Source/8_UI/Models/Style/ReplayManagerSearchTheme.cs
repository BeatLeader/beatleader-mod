using System;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader {
    internal class ReplayManagerSearchTheme {
        public SerializableColor SearchHighlightColor {
            get => _searchHighlightColor;
            set {
                _searchHighlightColor = value;
                SearchThemeUpdatedEvent?.Invoke();
            }
        }

        public FontStyle SearchHighlightStyle {
            get => _searchHighlightStyle;
            set {
                _searchHighlightStyle = value;
                SearchThemeUpdatedEvent?.Invoke();
            }
        }

        public event Action? SearchThemeUpdatedEvent;

        private SerializableColor _searchHighlightColor;
        private FontStyle _searchHighlightStyle;
    }
}