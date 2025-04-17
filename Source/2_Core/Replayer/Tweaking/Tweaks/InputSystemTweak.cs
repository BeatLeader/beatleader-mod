using BeatLeader.Utils;
using Reactive;
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
            _baseEventSystem = EventSystem.current;

            GameObject inputSystemContainer;
            if (InputUtils.IsInFPFC)
            {
                inputSystemContainer = new GameObject("2DEventSystem");
                inputSystemContainer.AddComponent<StandaloneInputModule>();
                InputUtils.EnableCursor(true);
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
            if (_baseEventSystem != null && _baseEventSystem.ToString() != "unknown") {
                EventSystem.current = _baseEventSystem;
            }
            if (InputUtils.IsInFPFC) InputUtils.EnableCursor(false);
        }
    }
}
