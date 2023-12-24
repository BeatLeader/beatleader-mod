using BeatLeader.DataManager;
using BeatLeader.Interop;
using BeatLeader.ViewControllers;
using BeatLeader.Replayer;
using BeatLeader.UI.Hub;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnMenuInstaller : Installer<OnMenuInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Debug("OnMenuInstaller");

            BindLeaderboard();
            BindHub();
            Container.Bind<ReplayerLauncher>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerMenuLoader>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            if (HeckInterop.IsInstalled) {
                Container.BindInterfacesTo<HeckNavigationFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            } else {
                Container.Bind<IReplayerViewNavigator>().To<ReplayerMenuLoader>().FromResolve();
            }
            Container.BindInterfacesAndSelfTo<ModifiersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }

        private void BindLeaderboard() {
            Container.BindInterfacesAndSelfTo<LeaderboardView.PreParser>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LeaderboardPanel.PreParser>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanel>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatLeaderCustomLeaderboard>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardHeaderManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardInfoManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }

        private void BindHub() {
            //<----------------------------------HUB---------------------------------->
            Container.Bind<BeatLeaderMenuButtonManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatLeaderHubFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatLeaderHubMainViewController>().FromNewComponentAsViewController().AsSingle();
            //<-----------------------------Replay Manager---------------------------->
            Container.Bind<ReplayManagerFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<ReplayManagerViewController>().FromNewComponentAsViewController().AsSingle();
            //<-----------------------------Battle Royale----------------------------->
            Container.Bind<BattleRoyaleFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<BattleRoyaleOpponentsViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}