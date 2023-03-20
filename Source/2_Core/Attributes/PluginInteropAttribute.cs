using System;
using JetBrains.Annotations;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Class), MeansImplicitUse]
    internal class PluginInteropAttribute : Attribute {
        public PluginInteropAttribute(string pluginId, string? worksUntilVersion = null) {
            this.pluginId = pluginId;
            this.worksUntilVersion = worksUntilVersion;
        }

        public readonly string pluginId;
        public readonly string? worksUntilVersion;
    }
}
