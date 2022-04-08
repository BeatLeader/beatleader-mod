using BeatLeader.API;
using BeatLeader.Utils;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnAppInitInstaller : Installer<OnAppInitInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Info("OnAppInitInstaller");

            Container.BindInterfacesAndSelfTo<Authentication>().AsSingle();
            Container.BindInterfacesAndSelfTo<HttpUtils>().AsSingle();
            BindScoreUtil();
        }

        #region BindScoreUtil
        private static ScoreUtil _scoreUtilInstance;
        private void BindScoreUtil() {
            if (_scoreUtilInstance != null) {
                Container.BindInstance(_scoreUtilInstance).AsSingle();
                return;
            }

            Container.Bind<ScoreUtil>().FromNewComponentOnNewGameObject().AsSingle();
            _scoreUtilInstance = Container.TryResolve<ScoreUtil>();
        }
        #endregion
    }
}