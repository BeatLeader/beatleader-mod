using System;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    internal class PluginInteropAttribute : Attribute {
        public PluginInteropAttribute(string pluginId, string? version = null) {
            this.pluginId = pluginId;
            this.version = version;
        }

        public readonly string pluginId;
        public readonly string? version;
    }
}
