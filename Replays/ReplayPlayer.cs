using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Zenject;
using IPA.Utilities;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatLeader.Utils;
using BeatLeader.Replays.Models;
using BeatLeader.Replays.Emulators;
using BeatLeader.UI.ReplayUI;
using ReplayNoteCutInfo = BeatLeader.Replays.Models.NoteCutInfo;
using ReplayVector3 = BeatLeader.Replays.Models.Vector3;
using ReplayQuaternion = BeatLeader.Replays.Models.Quaternion;
using ReplayTransform = BeatLeader.Replays.Models.Transform;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Transform = UnityEngine.Transform;

namespace BeatLeader.Replays
{
    public class ReplayPlayer : MonoBehaviour
    {
        [Inject] public IScoreController controller;
        [Inject] public BeatmapObjectManager beatmapObjectManager;
        [Inject] public AudioTimeSyncController songSyncController;
        [Inject] public Replay replayData;

        public ReplayVRController rightHand;
        public ReplayVRController leftHand;

        private List<(NoteController, NoteEvent)> notesToCut;
        private Frame previousFrame;
        private int totalFramesCount;
        private int currentFrame;
        public bool isPlaying;

        public event Action<Frame> frameWasUpdated;

        public void Start()
        {
            currentFrame = 0;
            totalFramesCount = replayData.frames.Count;
            notesToCut = new List<(NoteController, NoteEvent)>();
            beatmapObjectManager.noteWasSpawnedEvent += AddNoteToCutQueue;
            new ReplayPlaybackMenuController().Init(MovementPatchHelper.menuLeftHand, (int)songSyncController.songLength);
            isPlaying = replayData != null ? true : false;
            foreach (var item in controller.GetEvents())
            {
                Debug.LogWarning(item.Key);
                Debug.Log($"{item.Value.Target}+{item.Value.Method}");
            }
        }
        public void Update()
        {
            if (!isPlaying) return;
            PlayFrame(replayData.GetFrameByTime(songSyncController.songTime));
            currentFrame++;
            //InvokeNotesInQueue();
        }
        public void AddNoteToCutQueue(NoteController controller)
        {
            notesToCut.Add((controller, controller.GetNoteEvent(replayData)));
        }
        private void PlayFrame(Frame frame)
        {
            if (frame == null | frame == previousFrame) return;

            leftHand.SetTransform(frame.leftHand);
            rightHand.SetTransform(frame.rightHand);

            previousFrame = frame;
            frameWasUpdated?.Invoke(frame);
        }
        private void InvokeNotesInQueue()
        {
            foreach (var note in notesToCut)
            {
                if (songSyncController.songTime >= note.Item2.eventTime)
                {
                    try
                    {
                        var subscribers = note.Item1.GetField<LazyCopyHashSet<INoteControllerNoteWasCutEvent>, NoteController>("_noteWasCutEvent");
                        if (subscribers == null) continue;
                        foreach (var item in subscribers.items)
                        {
                            item.HandleNoteControllerNoteWasCut(note.Item1, ReplayNoteCutInfo.Parse(note.Item2.noteCutInfo, note.Item1));
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            notesToCut.Clear();
        }
    }
}
