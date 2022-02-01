using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers; 

[UsedImplicitly]
public class OnAppInitInstaller: Installer<OnAppInitInstaller> {
    public override void InstallBindings() {
        Plugin.Log.Info("OnAppInitInstaller");
    }
}