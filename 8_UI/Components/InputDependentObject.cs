using static BeatLeader.Replays.Managers.InputManager;
using BeatLeader.Replays.Managers;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class InputDependentObject : MonoBehaviour
    {
        public InputType InputType
        {
            get => _inputType;
            set
            {
                _inputType = value;
                Refresh();
            }
        }

        private InputManager _inputManager;
        private InputType _inputType;

        public void Init(InputManager manager, InputType type = InputType.VR | InputType.FPFC)
        {
            _inputManager = manager;
            InputType = type;
        }
        public void Refresh() => gameObject.SetActive(ShouldBeVisible());
        public bool ShouldBeVisible()
        {
            return _inputManager.MatchesCurrentInput(_inputType);
        }
    }
}
