using JetBrains.Annotations;
using Reactive;

namespace BeatLeader.UI.Replayer {
    [PublicAPI]
    public interface IBodySettingsViewFactory {
        /// <summary>
        /// Creates a body settings view instance.
        /// </summary>
        /// <returns></returns>
        ILayoutItem CreateBodySettingsView();
    }
}