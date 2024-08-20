using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class RoomOffsetsTweak : GameTweak {
        [FirstResource] private readonly MainSystemInit _mainSystemInit = null!;
        [Inject] private readonly VRCenterAdjust _centerAdjust;

        public override void Initialize() {
            this.LoadResources();
            _centerAdjust.enabled = false;
            _centerAdjust.transform.localPosition = Vector3.zero;
            _centerAdjust.transform.localRotation = Quaternion.identity;
        }
        public override void Dispose() {
            if (_centerAdjust == null) return;
            var settingsManager = _mainSystemInit._settingsManager;
            _centerAdjust.transform.localPosition = settingsManager.settings.room.center;
            _centerAdjust.transform.localEulerAngles = new Vector3(0, settingsManager.settings.room.rotation, 0);
            _centerAdjust.enabled = true;
        }
    }
}
