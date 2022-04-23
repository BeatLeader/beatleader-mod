using BeatLeader.DataManager;
using BeatLeader.ViewControllers;
using JetBrains.Annotations;
using Zenject;
using SiraUtil.Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnMenuInstaller : Installer<OnMenuInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Debug("OnMenuInstaller");

            BindLeaderboard();
            Container.BindInterfacesAndSelfTo<ModifiersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            // Container.BindInterfacesAndSelfTo<MonkeyHeadManager>().AsSingle();
        }

        private void BindLeaderboard() {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanel>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatLeaderCustomLeaderboard>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardHeaderManager>().AsSingle();
        }
    }
}