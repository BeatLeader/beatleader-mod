using BeatLeader.Utils;
using System.Linq;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace BeatLeader.Replayer.Tweaking
{
    internal class GarbageDisablerTweak : GameTweak
    {
        [Inject] private readonly MainCamera _mainCamera;

        [FirstResource] private SaberBurnMarkArea _burnMarkArea;
        [FirstResource] private VRLaserPointer _pointer;

        public override void Initialize()
        {
            this.LoadResources();

            _pointer.gameObject.SetActive(!InputManager.IsInFPFC);
            _burnMarkArea.gameObject.SetActive(!InputManager.IsInFPFC);
            _mainCamera.gameObject.SetActive(false);
        }
        public override void Dispose()
        {
            if (_pointer != null)
                _pointer.gameObject.SetActive(true);

            if (_burnMarkArea != null)
                _burnMarkArea.gameObject.SetActive(true);

            if (_mainCamera != null)
                _mainCamera.gameObject.SetActive(true);
        }
    }
}
