using System;

namespace BeatLeader.Models {
    public class VirtualPlayerSabersConfig {
        public bool Primary { get; private set; }
        public float Alpha { get; private set; }

        public event Action? ConfigUpdatedEvent;

        public void SetPrimary(bool primary) {
            Primary = primary;
            NotifyConfigUpdated();
        }

        public void SetAlpha(float alpha) {
            Alpha = alpha;
            NotifyConfigUpdated();
        }

        private void NotifyConfigUpdated() {
            ConfigUpdatedEvent?.Invoke();
        }
    }
}