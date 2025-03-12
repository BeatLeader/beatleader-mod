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

        private void OnEnable() {
            foreach (var (_, avatar) in _avatars) {
                avatar.PresentAvatar(false);
            }
        }

        #endregion

        #region Placement

        private const float AnchorAngle = -90f;
        private const float AvatarMarginAngle = 30f;
        private const float RadiusMultiplier = 3.5f;
        private const float AnimationTime = 0.4f;
        private const float AnimationFramerate = 120f;
        
        private static readonly int maxAvatarsCount = Mathf.FloorToInt(360 / AvatarMarginAngle) - 1;

        private readonly Dictionary<IReplayHeaderBase, BattleRoyaleAvatar> _avatars = new();

        private void RecalculateAvatarPositions(BattleRoyaleAvatar? accentAvatar = null) {
            var index = 0;
            var totalLength = Mathf.Clamp(_avatars.Count - 1, 0, maxAvatarsCount) * AvatarMarginAngle;
            var adjustmentAngle = AnchorAngle + totalLength / 2;
            
            foreach (var (_, avatar) in _avatars) {
                var deg = index * AvatarMarginAngle - adjustmentAngle;
                var rad = Mathf.Deg2Rad * deg;
                
                var pos = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
                var trans = avatar.transform;
                pos *= RadiusMultiplier;
                
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
            var totalFrames = AnimationTime * AnimationFramerate;
            var timePerFrame = AnimationTime / AnimationFramerate;
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

        private void HandleReplayAdded(IBattleRoyaleReplay replay, object caller) {
            if (_battleRoyaleAvatarPool.NumActive == maxAvatarsCount) {
                return;
            }
            
            var avatar = _battleRoyaleAvatarPool.Spawn(replay);
            avatar.transform.SetParent(transform, false);
            _avatars[replay.ReplayHeader] = avatar;
            
            RecalculateAvatarPositions(avatar);
        }

        private void HandleReplayRemoved(IBattleRoyaleReplay replay, object caller) {
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