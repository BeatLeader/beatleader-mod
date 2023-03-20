using System;
using JetBrains.Annotations;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property), MeansImplicitUse]
    internal class PluginTypeAttribute : Attribute {
        public PluginTypeAttribute(string type) {
            this.type = type;
        }

        public readonly string type;
    }
}
