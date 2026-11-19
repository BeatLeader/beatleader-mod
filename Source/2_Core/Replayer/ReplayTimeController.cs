using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayTimeController : BeatmapTimeController, IReplayTimeController {
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;

        public float ReplayEndTime {
            get {
                if (_replayEndTime is -1) {
                    var replays = _launchData.Replays;
                    var maxReplayFinishTime = replays.Select(x => x.ReplayData.FinishTime).Max();
                    _replayEndTime = maxReplayFinishTime;
                }
                return _replayEndTime;
            }
        }

        public event Action? SongReachedReplayEndEvent;

        private bool _songReachedReplayEnd;
        private float _replayEndTime = -1;

        public override void Rewind(float time, bool resumeAfterRewind = true) {
            time = Mathf.Clamp(time, SongStartTime, ReplayEndTime);
            base.Rewind(time, resumeAfterRewind);
            _songReachedReplayEnd = false;
        }

        protected override bool ShouldRespawnNote(NoteData noteData) {
            var replay = _playersManager.PrimaryPlayer.Replay;
            NoteEvent? match = null;
            var minTimeDifference = 0.2f;
            foreach (var noteEvent in replay.NoteEvents) {
                var timeDifference = Mathf.Abs(noteEvent.spawnTime - noteData.time);
                if (timeDifference > minTimeDifference || !replay.NoteComparator.Compare(noteEvent, noteData)) continue;
                minTimeDifference = timeDifference;
                match = noteEvent;
            }
            return match is not { } cut || cut.eventType is NoteEvent.NoteEventType.Miss || cut.CutTime >= SongTime;
        }

        protected override bool TryGetHeadPose(float time, out Pose pose) {
            var frames = _playersManager.PrimaryPlayer.Replay.PlayerMovementFrames;
            int low = 0, high = frames.Count;
            while (low < high) {
                var middle = (low + high) / 2;
                if (frames[middle].time < time) low = middle + 1;
                else high = middle;
            }
            if (low == 0 || low == frames.Count) {
                pose = default;
                return false;
            }
            var previous = frames[low - 1];
            var next = frames[low];
            pose = previous.headPose.Lerp(next.headPose, (time - previous.time) / (next.time - previous.time));
            return true;
        }

        private void LateUpdate() {
            if (_songReachedReplayEnd || SongTime < ReplayEndTime) return;
            SongReachedReplayEndEvent?.Invoke();
            _songReachedReplayEnd = true;
        }
    }
}