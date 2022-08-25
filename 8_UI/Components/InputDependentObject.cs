using static BeatLeader.Replayer.InputManager;
using BeatLeader.Replayer;
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
        public bool ShouldBeVisible => _inputManager.MatchesCurrentInput(_inputType);

        private InputManager _inputManager;
        private InputType _inputType;

        public void Init(InputManager manager, InputType type = InputType.VR | InputType.FPFC)
        {
            _inputManager = manager;
            InputType = type;
        }
        public void Refresh() => gameObject.SetActive(ShouldBeVisible);
    }
}
