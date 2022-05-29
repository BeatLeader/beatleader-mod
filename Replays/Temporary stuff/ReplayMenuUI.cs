using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMUI;
using TMPro;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.UI;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays.Movement;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Tags;
using BeatLeader.Replays;
using UnityEngine.Events;
using Zenject;

namespace BeatLeader
{
    internal class ReplayMenuUI //боль и страдания, это временное решение для теста
    {
        protected static NoTransitionsButton _replayButton;
        protected static GameObject _levelWrapContainer;

        protected static Replay _replay;
        protected static IDifficultyBeatmap _beatmapDifficulty;
        protected static IPreviewBeatmapLevel _previewBeatmapLevel;

        protected static bool _isPatched;
        protected static bool _inSearch;

        public static ReplayerMenuLauncher launcher;

        public static GameObject LevelWrapContainer
        {
            get => _levelWrapContainer = _levelWrapContainer == null ? Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault().gameObject : _levelWrapContainer;
        }
        public static bool isPatched => _isPatched;

        public static void Refresh(IDifficultyBeatmap difficulty, IPreviewBeatmapLevel preview)
        {
            if (difficulty != null && preview != null)
            {
                CheckIsReplayExists(difficulty);
                _previewBeatmapLevel = preview;
            }
        }
        public static void PatchUI()
        {
            if (_isPatched) return;
            GameObject button = new ButtonTag().CreateObject(LevelWrapContainer.transform);

            button.name = "ReplayButton";
            button.transform.localPosition = new Vector2(18.5f, -14);
            button.transform.Find("Content").GetComponentInChildren<CurvedTextMeshPro>().text = "Replay";

            _replayButton = button.GetComponentInChildren<NoTransitionsButton>();
            _replayButton.onClick.AddListener(StartInReplayMode);

            _isPatched = true;
        }
        public static void StartInReplayMode()
        {
            launcher.StartLevelWithReplay(_beatmapDifficulty, _previewBeatmapLevel, _replay);
        }
        private static void SetButtonState(bool state) => _replayButton.interactable = state;
        public static async void CheckIsReplayExists(IDifficultyBeatmap beatmap)
        {
            if (!_inSearch)
            {
                _inSearch = true;
                SetButtonState(false);
                Debug.LogWarning("Finding...");
                List<Replay> replays = await Task.Run(() => ReplayDataHelper.TryGetReplaysBySongInfoAsync(beatmap));
                Debug.LogWarning("Complete");
                if (replays != null)
                {
                    var replay = replays.GetReplayWithScore(ReplayDataHelper.Score.Best);
                    Plugin.Log.Notice($"Preloaded replay: {replay.info.songName}/{replay.info.difficulty}/{replay.info.mode} | {replay.info.score}/{replay.info.failTime}");
                    SetButtonState(true);
                    _replay = replay;
                    _beatmapDifficulty = beatmap;
                }
                else
                {
                    _replay = null;
                }
                _inSearch = false;
            }
        }
    }
}
