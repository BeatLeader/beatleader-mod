using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using Zenject;
using BeatSaberMarkupLanguage.ViewControllers;

namespace BeatLeader.ViewControllers
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.Replayer2DView.bsml")]
    internal class Replayer2DViewController : BSMLAutomaticViewController
    {
        [Inject] private readonly DiContainer _container;

        [UIValue("main-view")] private MainScreenView _mainScreenView;

        public void OpenLayoutEditor() {
            _mainScreenView?.OpenLayoutEditor();
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _mainScreenView = ReeUIComponentV2WithContainer.InstantiateInContainer<MainScreenView>(_container, null);
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }
    }
}
