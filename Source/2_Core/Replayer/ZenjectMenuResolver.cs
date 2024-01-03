using System.Collections.Generic;
using System.Linq;
using IPA.Utilities;
using Zenject;

namespace BeatLeader {
    /// <summary>
    /// The class which helps to acquire menu things from the game scene
    /// </summary>
    internal class ZenjectMenuResolver {
        [Inject] private readonly GameScenesManager _gameScenesManager = null!;

        private DiContainer _menuSceneContainer = null!;
        private bool _isInitialized;

        public T Resolve<T>() {
            Initialize();
            return _menuSceneContainer.Resolve<T>();
        }

        private void Initialize() {
            if (_isInitialized) return;
            var scenes = _gameScenesManager.GetField<List<GameScenesManager.ScenesStackData>, GameScenesManager>("_scenesStack");
            var menuScene = scenes.First(static x => x.sceneNames.Contains("MainMenu"));
            _menuSceneContainer = menuScene.container;
            _isInitialized = true;
        }
    }
}