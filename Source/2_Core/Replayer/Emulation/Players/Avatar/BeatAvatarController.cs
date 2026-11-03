using BeatLeader.Utils;
using BeatSaber.BeatAvatarAdapter.AvatarEditor;
using BeatSaber.BeatAvatarSDK;
using UnityEngine;

namespace BeatLeader.Replayer.Emulation {
    public class BeatAvatarController : MonoBehaviour {
        public BeatAvatarPoseController PoseController { get; private set; } = null!;

        private BeatAvatarVisualController _visualController = null!;

        protected virtual void Awake() {
            if (RemoveIfForeignCopy()) return;
            _visualController = GetComponentInChildren<BeatAvatarVisualController>();
            PoseController = GetComponentInChildren<BeatAvatarPoseController>();
            //avatar layer
            SetVisuals(null);
        }

        public virtual void SetVisuals(AvatarData? data, bool animated = false) {
            _visualController.UpdateAvatarVisual(data ?? AvatarUtils.DefaultAvatarData);
        }

        /// <summary>
        /// Other mods (e.g. BeatSaberPlus Multiplayer+) look up the game's avatar with
        /// <c>Resources.FindObjectsOfTypeAll&lt;AvatarTweenController&gt;()</c> and can pick up a BeatLeader avatar instead.
        /// They clone it and strip the <see cref="AvatarTweenController"/>, but this component survives on the copy
        /// and would hide it and throw. Every avatar created by <see cref="BeatAvatarLoader"/> keeps its tween controller,
        /// so a copy without one is not ours: remove this component and leave the copy to its owner.
        /// </summary>
        protected bool RemoveIfForeignCopy() {
            if (GetComponent<AvatarTweenController>()) return false;
            Destroy(this);
            return true;
        }
    }
}