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
using BeatLeader.Utils;
using BeatLeader.Replays.Models;
using BeatLeader.Replays.Emulators;
using BeatLeader.UI.ReplayUI;
using static BeatLeader.Replays.Models.Replay;
using ReplayNoteCutInfo = BeatLeader.Replays.Models.Replay.NoteCutInfo;
using ReplayVector3 = BeatLeader.Replays.Models.Replay.Vector3;
using ReplayQuaternion = BeatLeader.Replays.Models.Replay.Quaternion;
using ReplayTransform = BeatLeader.Replays.Models.Replay.Transform;
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

        public ReplayVRController rightHand;
        public ReplayVRController leftHand;
        public Replay replayData;

        private List<(NoteController, NoteEvent)> notesToCut = new List<(NoteController, NoteEvent)>();
        private Frame previousFrame;
        private protected bool _isPlaying;
        public bool isPlaying => _isPlaying;

        public event Action<Frame> frameWasUpdated;

        public void Init(Replay replay, ReplayVRController leftHand, ReplayVRController rightHand)
        {
            this.replayData = replay;
            this.leftHand = leftHand;
            this.rightHand = rightHand;
            _isPlaying = true;
        }
        public void Start()
        {
            beatmapObjectManager.noteWasSpawnedEvent += AddNoteToCutQueue;
            //new ReplayPlaybackMenuController().Init(MovementPatchHelper.menuLeftHand, (int)songSyncController.songLength);
            //_isPlaying = replayData != null ? true : false;
        }
        public void Update()
        {
            if (!isPlaying) return;
            //PlayFrame(replayData.GetFrameByTime(songSyncController.songTime));
            InvokeNotesInQueue();
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
            List<int> notesToRemove = new List<int>();
            for (int i = 0; i < notesToCut.Count; i++)
            {
                var note = notesToCut[i];
                Debug.Log(note.Item2.eventTime);
                if (songSyncController.songTime >= note.Item2.eventTime && songSyncController.songTime <= note.Item2.eventTime + 0.15f)
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
                    notesToRemove.Add(i);
                }
            }
            //foreach (var index in notesToRemove)
            //{
            //    notesToCut.RemoveAt(index);
            //}
            int indexOffset = 0;
            notesToRemove.Sort();
            for (int i = 0; i < notesToRemove.Count; i++)
            {
                notesToCut.Remove(notesToCut[i - indexOffset]);
                indexOffset++;
            }
        }
    }
}
