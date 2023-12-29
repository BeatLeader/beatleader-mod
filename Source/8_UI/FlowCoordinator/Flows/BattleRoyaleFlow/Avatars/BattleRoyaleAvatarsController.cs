using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleAvatarsController : MonoBehaviour {
        #region Injection

        [Inject] private readonly BattleRoyaleAvatar.Pool _battleRoyaleAvatarPool = null!;
        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region Setup

        private void Awake() {
            _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;
            _battleRoyaleHost.HostStateChangedEvent += HandleHostStateChanged;
        }

        private void OnDestroy() {
            _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
            _battleRoyaleHost.HostStateChangedEvent -= HandleHostStateChanged;
        }

        #endregion

        #region Placement

        private readonly Dictionary<IReplayHeaderBase, BattleRoyaleAvatar> _avatars = new();
        private float _anchorAngle = -90f;
        private float _avatarMarginAngle = 30f;
        private float _radiusMultiplier = 3.5f;

        private void RecalculateAvatarPositions() {
            var index = 0;
            foreach (var (_, avatar) in _avatars) {
                var angle = Mathf.Deg2Rad * index * _avatarMarginAngle;
                var x = Mathf.Cos(angle);
                var z = Mathf.Sin(angle);
                var pos = new Vector3(x, 0, z);
                var trans = avatar.transform;
                trans.localPosition = pos * _radiusMultiplier;
                trans.LookAt(Vector3.zero);
                index++;
            }
            var totalLength = (_avatars.Count - 1) * _avatarMarginAngle;
            var rot = _anchorAngle + totalLength / 2;
            transform.localEulerAngles = new(0, rot, 0);
        }

        #endregion

        #region Callbacks

        private void HandleHostStateChanged(bool state) {
            foreach (var (_, avatar) in _avatars) {
                if (state) {
                    avatar.PresentAvatar();
                } else {
                    avatar.HideAvatar();
                }
            }
        }

        private void HandleReplayAdded(IReplayHeaderBase header, object caller) {
            var avatar = _battleRoyaleAvatarPool.Spawn();
            avatar.Init(header);
            avatar.transform.SetParent(transform, false);
            _avatars[header] = avatar;
            RecalculateAvatarPositions();
        }

        private void HandleReplayRemoved(IReplayHeaderBase header, object caller) {
            var avatar = _avatars[header];
            _avatars.Remove(header);
            _battleRoyaleAvatarPool.Despawn(avatar);
            RecalculateAvatarPositions();
        }

        #endregion
    }
}