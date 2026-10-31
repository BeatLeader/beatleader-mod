using System.Collections.Generic;
using System.Linq;
using BeatLeader.API;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using UnityEngine;
using Zenject;
using Frame = BeatLeader.Models.Replay.Frame;
using Replay = BeatLeader.Models.Replay.Replay;

namespace BeatLeader {
    internal class DanceDreadGhost {
        private static readonly Vector3 position = new(-2.2f, 0f, 3f);
        private static readonly Color ghostColor = new(0.55f, 0.85f, 1f);

        private IReadOnlyList<Frame>? _frames;
        private int _frameIndex;
        private Transform? _root;
        private Transform? _head;
        private BattleRoyaleVRController? _headController;
        private BattleRoyaleVRController? _leftSaber;
        private BattleRoyaleVRController? _rightSaber;

        public void Load(BeatmapKey beatmapKey) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var hash, out var difficulty, out var mode);
            var player = ProfileManager.Profile?.id ?? "";
            TopDanceScoreRequest.Send(player, hash, difficulty, mode).StateChangedEvent += HandleTopScore;
        }

        public void Update(float stage, float songTime, DiContainer container) {
            if (_frames == null) return;
            if (_root == null) {
                if (stage <= 0f) return;
                Spawn(container);
            }
            var visible = stage > 0.02f;
            _root!.gameObject.SetActive(visible);
            if (!visible) return;
            if (_headController != null) _headController.CoreIntensity = stage;
            SetIntensity(_leftSaber!, stage);
            SetIntensity(_rightSaber!, stage);
            ApplyFrame(_frames, songTime);
        }

        public void Dispose() {
            if (_root != null) Object.Destroy(_root.gameObject);
        }

        private void HandleTopScore(IWebRequest<Paged<Score>> instance, RequestState state, string? failReason) {
            if (state != RequestState.Finished) return;
            var replay = instance.Result?.data?.FirstOrDefault()?.replay;
            if (string.IsNullOrEmpty(replay)) return;
            DownloadReplayRequest.SendRequest(replay!).StateChangedEvent += HandleReplay;
        }

        private void HandleReplay(IWebRequest<Replay> instance, RequestState state, string? failReason) {
            if (state != RequestState.Finished || instance.Result?.frames is not { Count: > 1 } frames) return;
            _frames = frames;
        }

        private void Spawn(DiContainer container) {
            _root = new GameObject("DanceDreadGhost").transform;
            _root.position = position;
            if (BundleLoader.HeadPrefab != null) {
                _head = Object.Instantiate(BundleLoader.HeadPrefab, _root, false).transform;
                _headController = _head.GetComponent<BattleRoyaleVRController>();
                if (_headController != null) _headController.CoreColor = ghostColor;
            }
            _leftSaber = CreateSaber(container);
            _rightSaber = CreateSaber(container);
        }

        private BattleRoyaleVRController CreateSaber(DiContainer container) {
            var saber = Object.Instantiate(BundleLoader.SaberPrefab, _root, false).GetComponent<BattleRoyaleVRController>();
            saber.Init(container);
            saber.CoreColor = ghostColor;
            saber.TrailEnabled = true;
            saber.TrailLength = 0.15f;
            return saber;
        }

        private static void SetIntensity(BattleRoyaleVRController saber, float stage) {
            saber.CoreIntensity = stage;
            saber.TrailOpacity = 0.35f * stage;
        }

        private void ApplyFrame(IReadOnlyList<Frame> frames, float songTime) {
            if (_frameIndex >= frames.Count - 1 || frames[_frameIndex].time > songTime) _frameIndex = 0;
            while (_frameIndex < frames.Count - 2 && frames[_frameIndex + 1].time <= songTime) _frameIndex++;
            var a = frames[_frameIndex];
            var b = frames[_frameIndex + 1];
            var t = b.time > a.time ? Mathf.Clamp01((songTime - a.time) / (b.time - a.time)) : 0f;
            if (_head != null) Pose(_head, a.head, b.head, t);
            Pose(_leftSaber!.transform, a.leftHand, b.leftHand, t);
            Pose(_rightSaber!.transform, a.rightHand, b.rightHand, t);
        }

        private static void Pose(Transform target, Models.Replay.Transform a, Models.Replay.Transform b, float t) {
            target.localPosition = Vector3.Lerp(a.position, b.position, t);
            target.localRotation = Quaternion.Slerp(a.rotation, b.rotation, t);
        }
    }
}
