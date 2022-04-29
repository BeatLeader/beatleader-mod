using BeatLeader.Replays;
using BeatLeader.DataManager;
using BeatLeader.Utils;
using BeatLeader.ViewControllers;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnMenuInstaller : Installer<OnMenuInstaller> 
    {
        public override void InstallBindings() 
        {
            BindLeaderboard();
            var launcher = new GameObject("ReplayzLauncher").AddComponent<ReplayMenuLauncher>();
            Container.Bind<ReplayMenuLauncher>().FromInstance(launcher).AsSingle();
            launcher.InjectAllFields(Container);
            ReplayMenuUI.laucher = launcher;
            //Container.BindInterfacesAndSelfTo<MonkeyHeadManager>().AsSingle();
        }

        private void BindLeaderboard() {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanel>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatLeaderCustomLeaderboard>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardHeaderManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<LeaderboardManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ProfileManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<HttpUtils>().AsSingle();
        }
    }
}