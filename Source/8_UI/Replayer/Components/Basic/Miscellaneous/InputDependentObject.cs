using UnityEngine;
using static BeatLeader.Utils.InputUtils;

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
        public bool ShouldBeVisible => MatchesCurrentInput(_inputType);

        private InputType _inputType;

        public void Init(InputType type = InputType.VR | InputType.FPFC)
        {
            InputType = type;
        }
        public void Refresh() => gameObject.SetActive(ShouldBeVisible);
    }
}
