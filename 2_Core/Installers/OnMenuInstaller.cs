using BeatLeader.DataManager;
using BeatLeader.ViewControllers;
using BeatLeader.Replays;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Installers
{
    [UsedImplicitly]
    public class OnMenuInstaller : Installer<OnMenuInstaller>
    {
        public override void InstallBindings()
        {
            Plugin.Log.Debug("OnMenuInstaller");

            BindLeaderboard();
            Container.BindInterfacesAndSelfTo<ModifiersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerMenuLauncher>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            ReplayMenuUI.launcher = Container.Resolve<ReplayerMenuLauncher>();
            // Container.BindInterfacesAndSelfTo<MonkeyHeadManager>().AsSingle();
        }

        private void BindLeaderboard()
        {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanel>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatLeaderCustomLeaderboard>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardHeaderManager>().AsSingle();
        }
    }
}