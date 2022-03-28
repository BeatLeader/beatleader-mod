using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.ScoreProvider;
using BeatLeader.Utils;
using BeatLeader.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnMenuInstaller : Installer<OnMenuInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Info("OnMenuInstaller");

            BindLeaderboard();
            Container.BindInterfacesAndSelfTo<MonkeyHeadManager>().AsSingle();
        }

        private void BindLeaderboard() {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanel>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatLeaderCustomLeaderboard>().AsSingle();

            Container.BindInterfacesAndSelfTo<LeaderboardManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<GlobalScoreProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<CountryScoreProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<AroundMeScoreProvider>().AsSingle();

            Container.BindInterfacesAndSelfTo<HttpUtils>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileManager>().AsSingle();
        }
    }
}