using System;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    internal class PluginInteropAttribute : Attribute {
        public PluginInteropAttribute(string pluginId, string? worksUntilVersion = null) {
            this.pluginId = pluginId;
            this.worksUntilVersion = worksUntilVersion;
        }

        public readonly string pluginId;
        public readonly string? worksUntilVersion;
    }
}
