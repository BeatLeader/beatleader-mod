using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BeatLeader.Components.Settings
{
    internal class LayoutEditorSetting : ReeUIComponentV2WithContainer
    {
        [InjectOptional] private readonly LayoutEditor _layoutEditor;

        public event Action OnEnteredEditMode;

        protected override void OnInitialize()
        {
           Content.gameObject.AddComponent<InputDependentObject>().Init(InputManager.InputType.FPFC);
        }

        [UIAction("button-clicked")] private void OpenEditor()
        {
            _layoutEditor?.SetEditModeEnabled(true);
            OnEnteredEditMode?.Invoke();
        }
    }
}
