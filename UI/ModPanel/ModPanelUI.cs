using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;

namespace BeatLeader {
    public class ModPanelUI : NotifiableSingleton<ModPanelUI> {
        #region Template button

        [UIValue("template-button-text")] [UsedImplicitly]
        private string _templateButtonText = "Hello world!";

        #endregion
    }
}