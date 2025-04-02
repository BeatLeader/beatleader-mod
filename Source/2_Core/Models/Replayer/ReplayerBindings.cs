using BeatLeader.Models.AbstractReplay;
using BeatLeader.UI.Replayer;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    /// <summary>
    /// A data class with replayer binding overrides. 
    /// </summary>
    [PublicAPI]
    public class ReplayerBindings {
        /// <summary>
        /// A comparator that is used to compare the actual note data with replay events.
        /// When null is specified, replayer will use the default comparator meant for BeatLeader replays.
        /// </summary>
        public IReplayNoteComparator? ReplayComparator { get; set; }
        
        /// <summary>
        /// A body spawner. When null is specified, the base-game avatar will be used. 
        /// </summary>
        public IVirtualPlayerBodySpawner? BodySpawner { get; set; }
        
        /// <summary>
        /// A factory for the body settings view. Null removes the settings view at all. Default leaves it as is.
        /// </summary>
        public Optional<IBodySettingsViewFactory> BodySettingsFactory { get; set; }
    }
}