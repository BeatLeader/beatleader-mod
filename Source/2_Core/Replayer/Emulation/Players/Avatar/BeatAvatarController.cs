using BeatLeader.Utils;
using BeatSaber.BeatAvatarSDK;
using UnityEngine;

namespace BeatLeader.Replayer.Emulation {
    public class BeatAvatarController : MonoBehaviour {
        public BeatAvatarPoseController PoseController { get; private set; } = null!;

        private BeatAvatarVisualController _visualController = null!;

        protected virtual void Awake() {
            _visualController = GetComponentInChildren<BeatAvatarVisualController>();
            PoseController = GetComponentInChildren<BeatAvatarPoseController>();
            //avatar layer
            SetVisuals(null);
            gameObject.SetActive(false);
        }

        public virtual void SetVisuals(AvatarData? data, bool animated = false) {
            _visualController.UpdateAvatarVisual(data ?? AvatarUtils.DefaultAvatarData);
        }
    }
}