using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.Replays.Models;
using BeatLeader.Replays;
using UnityEngine;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.ViewControllers;
using Zenject;

namespace BeatLeader.UI.ReplayUI
{
    public class ReplayPlaybackMenuController : NotifiableSingleton<ReplayPlaybackMenuController>
    {
        [UIValue("song-time")] int totalTime;
        [UIValue("current-time")] int CurrentTime
        {
            get => currentTime;
            set
            {
                currentTime = value;
                NotifyPropertyChanged(nameof(CurrentTime));
            }
        }
        [UIValue("play-button-text")] string ButtonText
        {
            get => text;
            set
            {
                text = value;
                NotifyPropertyChanged(nameof(ButtonText));
            }
        }
        int currentTime;
        string text = "Pause";

        [UIAction("play-button-clicked")] private void Clicked()
        {
            MovementPatchHelper.player.isPlaying = !MovementPatchHelper.player.isPlaying;
            if (!MovementPatchHelper.player.isPlaying)
            {
                MovementPatchHelper.player.songSyncController.Pause();
                ButtonText = "Play";
            }
            else
            {
                MovementPatchHelper.player.songSyncController.Resume();
                ButtonText = "Pause";
            }
        }
        public void Updated(Frame frame)
        {
            CurrentTime = (int)frame.time;
        }
        public void Init(GameObject handle, int totalSongTime)
        {
            var floating = FloatingScreen.CreateFloatingScreen(new Vector2(50, 20), false, 
                new UnityEngine.Vector3(0, 1, 0), new UnityEngine.Quaternion());
            floating.handle = handle;
            //floating.UpdateHandle();
            totalTime = totalSongTime;
            MovementPatchHelper.player.frameWasUpdated += Updated;
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), 
                "BeatLeader.UI.ReplayUI.BSML.ReplayPlayback.bsml"), floating.gameObject, this);
        }
    }
}
