using BeatLeader.DataManager;
using BeatLeader.ViewControllers;
using BeatLeader.Replayer;
using BeatLeader.Replayer.Emulation;
using BeatLeader.UI.Hub;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnMenuInstaller : Installer<OnMenuInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Debug("OnMenuInstaller");

            BindLeaderboard();
            BindHub();
            Container.BindInterfacesTo<ReplayManager>().FromInstance(ReplayManager.Instance).AsSingle();
            Container.Bind<ReplayerLauncher>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerMenuLoader>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
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
            Container.Bind<BeatLeaderMiniScreenSystem>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<ReplaysLoader>().AsSingle();
            //<-----------------------------Replay Manager---------------------------->
            Container.Bind<ReplayManagerFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<ReplayManagerViewController>().FromNewComponentAsViewController().AsSingle();
            //<-----------------------------Battle Royale----------------------------->
            Container.Bind<AvatarLoader>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BattleRoyaleAvatarsController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindMemoryPool<BattleRoyaleAvatar, BattleRoyaleAvatar.Pool>().FromNewComponentOnNewPrefab(new GameObject("BattleRoyaleAvatar"));
            Container.BindInterfacesAndSelfTo<BattleRoyaleFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<BattleRoyaleOpponentsViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BattleRoyaleReplaySelectionViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BattleRoyaleBattleSetupViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}