using BeatLeader.DataManager;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnAppInitInstaller : Installer<OnAppInitInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Debug("OnAppInitInstaller");
            
            Container.BindInterfacesAndSelfTo<LeaderboardManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ProfileManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ScoreStatsManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}