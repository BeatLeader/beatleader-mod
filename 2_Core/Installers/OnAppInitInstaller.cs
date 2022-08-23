using BeatLeader.API;
using BeatLeader.DataManager;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnAppInitInstaller : Installer<OnAppInitInstaller> {
        [Inject, UsedImplicitly]
        private IPlatformUserModel _platformUserModel;

        public override void InstallBindings() {
            Plugin.Log.Debug("OnAppInitInstaller");

            switch (_platformUserModel) {
                case OculusPlatformUserModel: 
                    Authentication.SetPlatform(Authentication.AuthPlatform.OculusPC);
                    Container.BindInterfacesAndSelfTo<OculusMigrationManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                    break;
                case SteamPlatformUserModel:
                    Authentication.SetPlatform(Authentication.AuthPlatform.Steam);
                    break;
            }
            
            Container.BindInterfacesAndSelfTo<LeaderboardManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlaylistsManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<ProfileManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoreStatsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ExMachinaManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<VotingManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ModVersionChecker>().AsSingle();
        }
    }
}