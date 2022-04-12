using Zenject;
using BeatLeader.UI;
using BeatLeader.Replays;
using BeatLeader.Core.Managers;
using JetBrains.Annotations;

namespace BeatLeader.Installers
{
    [UsedImplicitly]
    public class OnMenuInstaller : Installer<OnMenuInstaller> 
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MonkeyHeadManager>().AsSingle();
            //Container.Bind<ReplayMenuUI>().AsSingle().NonLazy();
            Container.Bind<ReplayRecorder>().AsSingle();
        }
    }
}