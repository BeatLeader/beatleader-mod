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
using BeatLeader.Replays.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Tags;
using BeatLeader.Replays;
using UnityEngine.Events;
using Zenject;

namespace BeatLeader.UI
{
    internal class ReplayMenuUI
    {
        private protected static MenuTransitionsHelper _menuTransitionsHelper => Zenjector.menuContainer.Resolve<MenuTransitionsHelper>();
        private protected static ReplaySystemHelper _systemHelper => Zenjector.appContainer.Resolve<ReplaySystemHelper>();

        private protected static GameObject _levelWrapContainer;
        private protected static NoTransitionsButton _replayButton;
        private protected static IDifficultyBeatmap _beatmapDifficulty;
        private static Replay _replayData;
        private static bool _isPatched;

        public static GameObject LevelWrapContainer
        {
            get => _levelWrapContainer = _levelWrapContainer == null ? Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault().gameObject : _levelWrapContainer;
        }
        public static bool isStartedAsReplay
        {
            get;
            private set;
        }
        public static bool isPatched => _isPatched;
        public static Replay replayData => _replayData; 

        public static void PatchUI()
        {
            if (_isPatched) return;
            Debug.LogWarning("Patching");
            GameObject button = new ButtonTag().CreateObject(LevelWrapContainer.transform);
            button.name = "ReplayButton";
            button.transform.localPosition = new Vector2(18.5f, -14);
            _replayButton = button.GetComponentInChildren<NoTransitionsButton>();
            _replayButton.onClick.AddListener(StartButtonClicked);
            button.transform.Find("Content").GetComponentInChildren<CurvedTextMeshPro>().text = "Replay";
            _isPatched = true;
        }
        private static void StartButtonClicked()
        {
            var go = LevelWrapContainer.GetComponent<StandardLevelDetailView>();
            var events = go.GetEvents();
            Debug.Log(events.Count);
            foreach (var item in events)
            {
                Debug.LogWarning(item.Key);
                foreach (var item2 in item.Value.GetInvocationList())
                {
                    Debug.Log(item2.Target + " " + item2.Method);
                }
            }
            Debug.LogWarning(ReplayJsonSerializer.Serialize(replayData));
        }
        private static void SetButtonState(bool state) => _replayButton.interactable = state;
        public static void CheckIsReplayExists(IDifficultyBeatmap beatmap)
        {
            if (ReplayDataHelper.TryGetReplayBySongInfo(beatmap, out Replay replay, ReplayDataHelper.Result.Completed, ReplayDataHelper.Score.Best))
            {
                Plugin.Log.Notice($"Preloaded replay: {replay.info.songName}/{replay.info.difficulty}/{replay.info.mode} | {replay.info.score}/{replay.info.failTime}");
                SetButtonState(true);
                _replayData = replay;
                _beatmapDifficulty = beatmap;
            }
            else
            {
                SetButtonState(false);
                _replayData = null;
            }
        }
        public static void NotifyReplayEnded() => isStartedAsReplay = false;
    }
}
