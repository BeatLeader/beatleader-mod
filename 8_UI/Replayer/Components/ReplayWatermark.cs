using BeatLeader.DataManager;
using System;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components
{
    internal class ReplayWatermark : MonoBehaviour
    {
        [Inject] private Models.ReplayLaunchData _launchData;

        public bool Enabled
        {
            get => _text.gameObject.activeSelf;
            set
            {
                if (!CanBeDisabled) return;

                _text.gameObject.SetActive(value);
                _launchData.actualToWriteSettings.ShowWatermark = value;
            }
        }
        public bool CanBeDisabled { get; private set; }

        private TextMeshPro _text;

        private void Awake()
        {
            CanBeDisabled = IsWatermarkCanBeDisabled();

            _text = gameObject.AddComponent<TextMeshPro>();
            _text.richText = true;
            _text.fontSize = 1.5f;
            _text.alignment = TextAlignmentOptions.Center;
            transform.position = new Vector3(0, 2.6f, 6);

            var level = _launchData.difficultyBeatmap.level;
            _text.text = GetFormattedText(_launchData.player.name, level.songName, level.songAuthorName);

            Enabled = _launchData.actualSettings.ShowWatermark;
        }
        private string GetFormattedText(string player, string songName, string songAuthor)
        {
            return $"<i><b><color=\"red\">REPLAY</color></b>   {songName} - {songAuthor}   Player: {player}</i>";
        }
        private bool IsWatermarkCanBeDisabled()
        {
            return ProfileManager.IsCurrentPlayer(_launchData.player);
        }
    }
}
