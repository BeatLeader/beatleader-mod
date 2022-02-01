using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers;

[UsedImplicitly]
public class OnMenuInstaller : Installer<OnMenuInstaller> {
    public override void InstallBindings() {
        Plugin.Log.Info("OnMenuInstaller");

        Container.BindInterfacesAndSelfTo<MonkeyHeadManager>().AsSingle();
    }
}