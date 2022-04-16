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
using BeatLeader.Replays.Emulators;
using BeatLeader.Models;
using BeatLeader.Replays;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using ReplayVector3 = BeatLeader.Models.Vector3;
using ReplayQuaternion = BeatLeader.Models.Quaternion;
using ReplayTransform = BeatLeader.Models.Transform;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Transform = UnityEngine.Transform;

namespace BeatLeader.Replays
{
    public class ReplayPlayer : MonoBehaviour
    {
        [Inject] private protected IScoreController controller;
        [Inject] private protected BeatmapObjectManager beatmapObjectManager;
        [Inject] private protected AudioTimeSyncController songSyncController;

        public GameObject fakeHead;
        public FakeVRController rightHand;
        public FakeVRController leftHand;
        public Replay replayData;
        public bool lerpEnabled;

        private List<SimpleNoteCutComparator> comparators;
        private Frame previousFrame;
        private protected bool _isPlaying;
        public bool isPlaying => _isPlaying;

        public event Action<Frame> frameWasUpdated;

        public void Start()
        {
            replayData = ReplayMenuUI.replay;
            comparators = new List<SimpleNoteCutComparator>();
            PatchOriginalSabers();
            beatmapObjectManager.noteWasSpawnedEvent += AddNoteComparator;
            _isPlaying = replayData != null ? true : false;
        }
        public void Update()
        {
            if (!isPlaying) return;
            PlayFrame(replayData.GetFrameByTime(songSyncController.songTime));
        }
        private void PlayFrame(Frame frame)
        {
            if (frame == null | frame == previousFrame) return;

            leftHand.SetTransform(frame.leftHand);
            rightHand.SetTransform(frame.rightHand);

            previousFrame = frame;
            frameWasUpdated?.Invoke(frame);
        }
        private void PatchOriginalSabers()
        {
            var left = Resources.FindObjectsOfTypeAll<VRController>().First(x => x.name == "LeftHand");
            var right = Resources.FindObjectsOfTypeAll<VRController>().First(x => x.name == "RightHand");

            var leftGO = left.gameObject;
            var rightGO = right.gameObject;

            leftGO.gameObject.SetActive(false);
            rightGO.gameObject.SetActive(false);

            Destroy(left);
            Destroy(right);

            leftHand = leftGO.AddComponent<FakeVRController>();
            rightHand = rightGO.AddComponent<FakeVRController>();

            leftHand.node = UnityEngine.XR.XRNode.LeftHand;
            rightHand.node = UnityEngine.XR.XRNode.RightHand;

            leftGO.gameObject.SetActive(true);
            rightGO.gameObject.SetActive(true);
        }
        private void AddNoteComparator(NoteController controller)
        {
            NoteEvent noteCutEvent = controller.GetNoteEvent(replayData);
            if (noteCutEvent != null && noteCutEvent.eventType != NoteEventType.miss)
            {
                var comparator = controller.gameObject.AddComponent<SimpleNoteCutComparator>();
                comparator.timeSyncController = songSyncController;
                comparator.noteCutEvent = noteCutEvent;
                comparators.Add(comparator);
            }
        }
    }
}
