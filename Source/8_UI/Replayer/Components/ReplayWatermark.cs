using BeatLeader.DataManager;
using BeatLeader.Models;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components {
    internal class ReplayWatermark : MonoBehaviour, IReplayWatermark {
        [Inject] private readonly ReplayLaunchData _launchData;

        public bool Enabled {
            get => _text.gameObject.activeSelf;
            set {
                if (!CanBeDisabled) return;
                _text.gameObject.SetActive(value);
                _launchData.Settings.ShowWatermark = value;
            }
        }
        public bool CanBeDisabled { get; private set; }

        private TextMeshPro _text;

        private void Awake() {
            _text = gameObject.AddComponent<TextMeshPro>();
            _text.richText = true;
            _text.fontSize = 1.5f;
            _text.alignment = TextAlignmentOptions.Center;
            transform.position = new(0, 2.6f, 6);
            RefreshText();
            RefreshVisibility();
        }

        public void RefreshVisibility() {
            Enabled = _launchData.Settings.ShowWatermark;
        }

        public void RefreshText() {
            var text = $"<i><b><color=\"red\">REPLAY</color></b>   ";
            var level = _launchData.BeatmapLevel;
            text += $"{level.songName} - {level.songAuthorName}   ";
            if (_launchData.IsBattleRoyale) {
                CanBeDisabled = true;
                text += "<color=\"yellow\">BATTLE ROYALE</color>";
            } else {
                var player = _launchData.MainReplay.ReplayData.Player;
                CanBeDisabled = player != null && ProfileManager.IsCurrentPlayer(player.id);
                text += $"Player:  {player?.name ?? "Unknown"}";
            }
            text += "</i>";
            _text.text = text;
        }
    }
}
