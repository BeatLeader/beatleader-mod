using BeatLeader.Utils;
using UnityEngine.EventSystems;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace BeatLeader.Replayer.Tweaking
{
    internal class InputSystemTweak : GameTweak
    {
        [Inject] private readonly VRInputModule _inputModule;
        [Inject] private readonly DiContainer _container;

        private EventSystem _baseEventSystem;
        private EventSystem _customEventSystem;

        public override void Initialize()
        {
            _baseEventSystem = _inputModule.GetComponent<EventSystem>();

            GameObject inputSystemContainer;
            if (InputManager.IsInFPFC)
            {
                inputSystemContainer = new GameObject("2DEventSystem");
                inputSystemContainer.AddComponent<StandaloneInputModule>();
                InputManager.EnableCursor(true);
            }
            else
            {
                inputSystemContainer = GameObject.Instantiate(_inputModule.gameObject);
                _container.Inject(inputSystemContainer.GetComponent<VRInputModule>());
            }

            _customEventSystem = inputSystemContainer.GetOrAddComponent<EventSystem>();
            EventSystem.current = _customEventSystem;
        }
        public override void Dispose()
        {
            _customEventSystem?.gameObject.TryDestroy();
            EventSystem.current = _baseEventSystem;
            if (InputManager.IsInFPFC) InputManager.EnableCursor(false);
        }
    }
}
