using BeatLeader.DataManager;
using BeatLeader.Interop;
using BeatLeader.ViewControllers;
using BeatLeader.Replayer;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnMenuInstaller : Installer<OnMenuInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Debug("OnMenuInstaller");

            BindLeaderboard();
            Container.Bind<ReplayerLauncher>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerMenuLoader>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            if (HeckInterop.IsInstalled) {
                Container.BindInterfacesTo<HeckNavigationFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            } else {
                Container.Bind<IReplayerViewNavigator>().To<ReplayerMenuLoader>().FromResolve();
            }
            Container.BindInterfacesAndSelfTo<ModifiersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            // Container.BindInterfacesAndSelfTo<MonkeyHeadManager>().AsSingle();
        }

        private void BindLeaderboard() {
            Container.BindInterfacesAndSelfTo<LeaderboardView.PreParser>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LeaderboardPanel.PreParser>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanel>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatLeaderCustomLeaderboard>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardHeaderManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardInfoManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<BeatLeaderMenuButtonManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatLeaderFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayLaunchViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}