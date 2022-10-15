using System;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    internal class PluginInteropAttribute : Attribute {
        public PluginInteropAttribute(string pluginId) {
            this.pluginId = pluginId;
        }

        public readonly string pluginId;
    }
}
