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
using BeatLeader.Replays.Emulators;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Tags;
using BeatLeader.Replays;
using UnityEngine.Events;
using Zenject;

namespace BeatLeader
{
    internal class ReplayMenuUI
    {
        private protected static NoTransitionsButton _replayButton;
        private protected static GameObject _levelWrapContainer;
        private protected static UnityEvent _actionButtonEvent;
        private protected static FakePlayer _player;

        private protected static Replay _replay;
        private protected static IDifficultyBeatmap _beatmapDifficulty;

        private protected static bool _asReplay;
        private static bool _isPatched;

        public static GameObject LevelWrapContainer
        {
            get => _levelWrapContainer = _levelWrapContainer == null ? Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault().gameObject : _levelWrapContainer;
        }
        private static UnityEvent ActionButtonEvent
        {
            get => _actionButtonEvent = _actionButtonEvent == null ? Resources.FindObjectsOfTypeAll<NoTransitionsButton>().First(x => x.name == "ActionButton").onClick : _actionButtonEvent;
        }
        public static FakePlayer player => _player;
        public static Replay replay => _replay;
        public static bool asReplay => _asReplay;
        public static bool isPatched => _isPatched;

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
            _asReplay = true;
            ActionButtonEvent.Invoke();
        }
        private static void SetButtonState(bool state) => _replayButton.interactable = state;
        public static Task CheckIsReplayExists(IDifficultyBeatmap beatmap)
        {
            _asReplay = false;
            SetButtonState(false);
            if (ReplayDataHelper.TryGetReplayBySongInfo(beatmap, out Replay replay, ReplayDataHelper.Result.Completed, ReplayDataHelper.Score.Best))
            {
                Plugin.Log.Notice($"Preloaded replay: {replay.info.songName}/{replay.info.difficulty}/{replay.info.mode} | {replay.info.score}/{replay.info.failTime}");
                SetButtonState(true);
                _replay = replay;
                _beatmapDifficulty = beatmap;
            }
            else
            {
                _replay = null;
            }
            return Task.CompletedTask;
        }
        public static void NotifyReplayEnded() => _asReplay = false;
        public static void SetPlayer(FakePlayer player) => _player = player;
    }
}
