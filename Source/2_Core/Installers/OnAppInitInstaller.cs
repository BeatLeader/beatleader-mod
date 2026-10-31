using BeatLeader.API;
using BeatLeader.DataManager;
using BeatLeader.Utils;
using JetBrains.Annotations;
using OculusStudios.Platform.Core;
using System;
using System.Runtime.InteropServices;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnAppInitInstaller : Installer<OnAppInitInstaller> {
        [Inject, UsedImplicitly]
        private IVRPlatformHelper _vrPlatformHelper = null!;
        [Inject, UsedImplicitly]
        private IPlatform _platform = null!;

        public override void InstallBindings() {
            Plugin.Log.Debug("OnAppInitInstaller");

            if (_platform != null && _platform.vendor == Vendor.Valve) {
                Authentication.SetPlatform(Authentication.AuthPlatform.Steam);
            } else {
                Authentication.SetPlatform(Authentication.AuthPlatform.OculusPC);
                Container.BindInterfacesAndSelfTo<OculusMigrationManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                OpenXRAcquirer.Init(_vrPlatformHelper.vrPlatformSDK);
            }

            AddCustomModifiers();

            Container.BindInterfacesAndSelfTo<LeaderboardManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlaylistsManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            MapTypesManager.Initialize();
            LeaderboardContextsManager.Initialize();
            LeaderboardContextSwitcher.Initialize();
            PrestigeLevelsManager.Initialize();

            Container.BindInterfacesAndSelfTo<ProfileManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoreStatsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<VotingManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ModVersionChecker>().AsSingle();
        }

        private static void AddCustomModifiers() {
            var modifiers = new[] { SpeedModifiers.BFS, SpeedModifiers.BSF, DanceModifier.DNC, GolfModifier.GOLF };
            foreach (var modifier in modifiers) {
                // ModifiersCore keeps modifiers across soft restarts, while this installer runs again
                if (ModifiersCore.ModifiersManager.HasModifierWithId(modifier.Id)) continue;
                try {
                    ModifiersCore.ModifiersManager.AddModifier(modifier);
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to add the {modifier.Id} modifier: {ex.Message}");
                }
            }
        }
    }
}