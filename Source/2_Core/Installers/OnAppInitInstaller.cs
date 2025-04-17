﻿using BeatLeader.API;
using BeatLeader.DataManager;
using BeatLeader.SteamVR;
using BeatLeader.Utils;
using JetBrains.Annotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnAppInitInstaller : Installer<OnAppInitInstaller> {
        [Inject, UsedImplicitly]
        private IVRPlatformHelper _vrPlatformHelper;

        public override void InstallBindings() {
            Plugin.Log.Debug("OnAppInitInstaller");
            
            var steamPlatformUserModel = Assembly.GetAssembly(typeof(IPlatformUserModel))
                .GetTypes()
                .FirstOrDefault(t => t.FullName.Contains("SteamPlatformUserModel"));
            if (steamPlatformUserModel != null) {
                Authentication.SetPlatform(Authentication.AuthPlatform.Steam);
            } else {
                Authentication.SetPlatform(Authentication.AuthPlatform.OculusPC);
                Container.BindInterfacesAndSelfTo<OculusMigrationManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                OpenXRAcquirer.Init(_vrPlatformHelper.vrPlatformSDK);
            }

            Container.BindInterfacesAndSelfTo<LeaderboardContextsManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LeaderboardManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlaylistsManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ProfileManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoreStatsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<VotingManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ModVersionChecker>().AsSingle();
        }
    }
}