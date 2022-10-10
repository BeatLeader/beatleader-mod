using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using Zenject;

namespace BeatLeader.Components
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.MainScreenSpaceView.bsml")]
    internal class MainScreenView : ReeUIComponentV2WithContainer
    {
        [Inject] private readonly IReplayPauseController _pauseController;
        [Inject] private readonly IReplayExitController _exitController;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayLaunchData _launchData;

        [UIValue("song-info")] private HorizontalBeatmapLevelPreview _songInfo;
        [UIValue("player-info")] private HorizontalMiniProfile _playerInfo;
        [UIValue("toolbar")] private Toolbar _toolbar;
        [UIValue("layout-editor")] private LayoutEditor _layoutEditor;

        protected override void OnInstantiate()
        {
            _playerInfo = InstantiateOnSceneRoot<HorizontalMiniProfile>();
            _songInfo = InstantiateOnSceneRoot<HorizontalBeatmapLevelPreview>();
            _layoutEditor = InstantiateInContainer<LayoutEditor>(Container, transform);
            _toolbar = InstantiateInContainer<Toolbar>(Container, transform);

            _playerInfo.SetPlayer(_launchData.Player);
            _songInfo.SetBeatmapLevel(_launchData.DifficultyBeatmap.level);
            _toolbar.Setup(_launchData.Replay, _pauseController, _exitController, _beatmapTimeController);  
        }
        protected override void OnInitialize()
        {
            _layoutEditor.TryAddObject(_playerInfo);
            _layoutEditor.TryAddObject(_songInfo);
            _layoutEditor.TryAddObject(_toolbar);
        }
    }
}
