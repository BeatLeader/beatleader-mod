using BeatLeader.API;
using JetBrains.Annotations;
using Zenject;
using BeatLeader.Replays;
using BeatLeader.UI;

namespace BeatLeader.Installers
{
    [UsedImplicitly]
    public class OnAppInitInstaller: Installer<OnAppInitInstaller> {
        public override void InstallBindings() 
        {
            Plugin.Log.Info("OnAppInitInstaller");
            Container.BindInterfacesAndSelfTo<Authentication>().AsSingle();
            Container.Bind<ReplaySystemHelper>().AsSingle().NonLazy();
        }
    }
}