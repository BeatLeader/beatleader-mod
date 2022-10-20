using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class RoomOffsetsTweak : GameTweak {
        [FirstResource] private readonly MainSettingsModelSO _mainSettingsModel;
        [Inject] private readonly VRCenterAdjust _centerAdjust;

        public override void Initialize() {
            this.LoadResources();
            _centerAdjust.enabled = false;
            _centerAdjust.transform.localPosition = Vector3.zero;
            _centerAdjust.transform.localRotation = Quaternion.identity;
        }
        public override void Dispose() {
            if (_centerAdjust != null) {
                _centerAdjust.transform.localPosition = _mainSettingsModel.roomCenter;
                _centerAdjust.transform.localEulerAngles = new Vector3(0, _mainSettingsModel.roomRotation, 0);
                _centerAdjust.enabled = true;
            }
        }
    }
}
