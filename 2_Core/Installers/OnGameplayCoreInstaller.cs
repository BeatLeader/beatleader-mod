using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers
{
    [UsedImplicitly]
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Info("OnGameplayCoreInstaller");

            Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
        }
    }
}