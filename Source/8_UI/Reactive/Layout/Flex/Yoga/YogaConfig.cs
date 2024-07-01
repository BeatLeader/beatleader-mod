using System;

namespace BeatLeader.UI.Reactive.Yoga {
    internal struct YogaConfig {
        public static YogaConfig Default { get; } = new() {
            _configPtr = YogaNative.YGConfigGetDefault()
        };
        
        private IntPtr _configPtr;

        public void SetPointScaleFactor(float factor) {
            YogaNative.YGConfigSetPointScaleFactor(_configPtr, factor);
        }
    }
}