using Reactive;
using Reactive.Components.Basic;

namespace BeatLeader.UI.Replayer {
    internal class EmptyAvatarSettingsViewFactory : IBodySettingsViewFactory {
        public IReactiveComponent CreateBodySettingsView() {
            return new Label {
                Text = "Seems like Monke doesn't want you to see this category!"
            };
        }
    }
}