using BeatLeader.Components;
using BeatLeader.Installers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.UI.Reactive.Components {
    [RequireComponent(typeof(PointerEventsHandler))]
    internal class VRScrollAdapter : MonoBehaviour {
        private PointerEventsHandler _pointerEventsHandler = null!;
        private IVRPlatformHelper _platformHelper = null!;
        
        private void Awake() {
            _pointerEventsHandler = GetComponent<PointerEventsHandler>();
            _platformHelper = OnMenuInstaller.Container.Resolve<IVRPlatformHelper>();
            _platformHelper.joystickWasNotCenteredThisFrameEvent += HandleJoystickWasNotCentered;
        }

        private void OnDestroy() {
            _platformHelper.joystickWasNotCenteredThisFrameEvent -= HandleJoystickWasNotCentered;
        }

        private void HandleJoystickWasNotCentered(Vector2 delta) {
            var pointerEventData = new PointerEventData(EventSystem.current) {
                scrollDelta = delta
            };
            _pointerEventsHandler.OnScroll(pointerEventData);
        }
    }
}