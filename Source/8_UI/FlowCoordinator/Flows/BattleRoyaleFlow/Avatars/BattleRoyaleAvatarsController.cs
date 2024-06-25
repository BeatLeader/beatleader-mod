using System.Collections;
using System.Collections.Generic;
using BeatLeader.Models;
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
            _battleRoyaleHost.ReplayRefreshRequestedEvent += HandleRefreshRequested;
            _battleRoyaleHost.HostStateChangedEvent += HandleHostStateChanged;
        }

        private void OnDestroy() {
            _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
            _battleRoyaleHost.ReplayRefreshRequestedEvent -= HandleRefreshRequested;
            _battleRoyaleHost.HostStateChangedEvent -= HandleHostStateChanged;
        }

        #endregion

        #region Placement

        //these attributes are useless in the game, but sometimes i need to modify them from RUE,
        //so made them serializable to stop rider from yelling
        [SerializeField] private float anchorAngle = -90f;
        [SerializeField] private float avatarMarginAngle = 30f;
        [SerializeField] private float radiusMultiplier = 3.5f;
        [SerializeField] private float animationTime = 0.4f;
        [SerializeField] private float animationFramerate = 120f;

        private readonly Dictionary<IReplayHeaderBase, BattleRoyaleAvatar> _avatars = new();

        private void RecalculateAvatarPositions(BattleRoyaleAvatar? accentAvatar = null) {
            var index = 0;
            var totalLength = (_avatars.Count - 1) * avatarMarginAngle;
            var adjustmentAngle = anchorAngle + totalLength / 2;
            foreach (var (_, avatar) in _avatars) {
                var deg = index * avatarMarginAngle - adjustmentAngle;
                var rad = Mathf.Deg2Rad * deg;
                var x = Mathf.Cos(rad);
                var z = Mathf.Sin(rad);
                var pos = new Vector3(x, 0, z);
                var trans = avatar.transform;
                pos *= radiusMultiplier;
                if (avatar == accentAvatar) {
                    trans.localPosition = pos;
                    trans.LookAt(Vector3.zero);
                } else {
                    StartCoroutine(AdjustAvatarPositionAnimationCoroutine(trans, pos));
                }
                index++;
            }
        }

        private IEnumerator AdjustAvatarPositionAnimationCoroutine(Transform trans, Vector3 targetPosition) {
            var totalFrames = animationTime * animationFramerate;
            var timePerFrame = animationTime / animationFramerate;
            var startPosition = trans.localPosition;

            for (var i = 0; i < totalFrames; i++) {
                yield return new WaitForSeconds(timePerFrame);
                var t = Mathf.Sin(i / totalFrames * Mathf.PI * 0.5f);
                trans.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                trans.LookAt(Vector3.zero);
            }

            trans.localPosition = targetPosition;
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

        private void HandleReplayAdded(IBattleRoyaleQueuedReplay replay, object caller) {
            var avatar = _battleRoyaleAvatarPool.Spawn(replay);
            avatar.transform.SetParent(transform, false);
            _avatars[replay.ReplayHeader] = avatar;
            RecalculateAvatarPositions(avatar);
        }

        private void HandleReplayRemoved(IBattleRoyaleQueuedReplay replay, object caller) {
            var avatar = _avatars[replay.ReplayHeader];
            _avatars.Remove(replay.ReplayHeader);
            _battleRoyaleAvatarPool.Despawn(avatar);
            RecalculateAvatarPositions();
        }

        private void HandleRefreshRequested() {
            foreach (var avatar in _avatars.Values) {
                avatar.Refresh();
            }
        }

        #endregion
    }
}