using BeatLeader.UI.Reactive.Yoga;

namespace BeatLeader.UI.Reactive {
    internal class YogaContext {
        public YogaNode YogaNode {
            get {
                _yogaNode.Touch();
                return _yogaNode;
            }
        }

        private YogaNode _yogaNode;
    }
}