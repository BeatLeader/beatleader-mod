using System;
using BeatLeader.UI.Reactive.Yoga;

namespace BeatLeader.UI.Reactive {
    internal class YogaContext : IDisposable {
        public YogaNode YogaNode {
            get {
                _yogaNode.Touch();
                return _yogaNode;
            }
        }

        private YogaNode _yogaNode;
        
        public void Dispose() {
            _yogaNode.Free();
        }
    }
}