using BeatLeader.Models;
using Reactive;
using Zenject;

namespace BeatLeader.UI.Replayer {
    internal class BasicAvatarSettingsViewFactory : IBodySettingsViewFactory {
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public IReactiveComponent CreateBodySettingsView() {
            var view = new BasicAvatarSettingsView();

            view.Setup(
                _launchData.Settings.BodySettings,
                _launchData.IsBattleRoyale
            );

            return view;
        }
    }
}