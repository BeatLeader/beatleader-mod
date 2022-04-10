using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BeatLeader.Replays.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Tags;
using BeatLeader.Replays;

namespace BeatLeader.UI
{
    internal class ReplayMenuUI
    {
        private protected static GameObject levelWrapContainer;
        private static NoTransitionsButton replayButton;
        public static Replay replayData;
        private static bool isPatched;

        public static GameObject LevelWrapContainer
        {
            get => levelWrapContainer == null ? Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault().gameObject : levelWrapContainer;
        }
        public static bool isStartedAsReplay
        {
            get;
            private set;
        }

        public static void PatchUI()
        {
            if (isPatched) return;
            var actionButton = Resources.FindObjectsOfTypeAll<NoTransitionsButton>().FirstOrDefault(x => x.gameObject.name == "ActionButton");
            GameObject button = new ButtonTag().CreateObject(LevelWrapContainer.transform);
            button.name = "ReplayButton";
            button.transform.localPosition = new Vector2(18.5f, -14);
            replayButton = button.GetComponentInChildren<NoTransitionsButton>();
            replayButton.onClick.AddListener(() =>
            {
                isStartedAsReplay = true;
                actionButton.GetComponent<NoTransitionsButton>().onClick.Invoke();
            });
            CurvedTextMeshPro text = button.transform.Find("Content").GetComponentInChildren<CurvedTextMeshPro>();
            text.text = "Replay";
            isPatched = true;
        }
        private static void SetButtonState(bool state) => replayButton.interactable = state;
        public static void CheckIsReplayExists(IDifficultyBeatmap beatmap)
        {
            if (ReplayDataHelper.TryGetReplayBySongInfo(beatmap, out Replay replay, ReplayDataHelper.Result.Completed, ReplayDataHelper.Score.Best))
            {
                Plugin.Log.Notice($"Preloaded replay: {replay.info.songName}/{replay.info.difficulty}/{replay.info.mode} | {replay.info.score}/{replay.info.failTime}");
                SetButtonState(true);
                replayData = replay;
            }
            else
            {
                SetButtonState(false);
                replayData = null;
            }
        }
        public static void NotifyReplayEnded() => isStartedAsReplay = false;
    }
}
