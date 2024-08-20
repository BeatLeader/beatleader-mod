using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Replayer {
    internal class ReplayerExtraObjectsProvider : MonoBehaviour {
        [FirstResource]
        private readonly MainSystemInit _mainSystemInit = null!;

        [FirstResource("VRGameCore", requireActiveInHierarchy: true)]
        private readonly Transform _origin = null!;

        public Transform ReplayerCore => transform;
        public Transform ReplayerCenterAdjust { get; private set; } = null!;
        public Transform VRGameCore => _origin;

        private Vector3 _posOffset;
        private Quaternion _rotOffset;

        private void Awake() {
            this.LoadResources();
            ReplayerCore.SetParent(VRGameCore, false);
            name = "ReplayerCore";

            ReplayerCenterAdjust = new GameObject("CenterAdjust").transform;
            ReplayerCenterAdjust.SetParent(ReplayerCore, false);

            var settingsModel = _mainSystemInit._settingsManager.settings;
            _posOffset = settingsModel.room.center;
            _rotOffset = Quaternion.Euler(0, settingsModel.room.rotation, 0);
        }

        private void Start() {
            ApplyOffsets();
        }

        private void OnEnable() {
            ApplyOffsets();
        }

        private void ApplyOffsets() {
            if (InputUtils.IsInFPFC) return;
            ReplayerCenterAdjust.localPosition = _posOffset;
            ReplayerCenterAdjust.localRotation = _rotOffset;
        }
    }
}
