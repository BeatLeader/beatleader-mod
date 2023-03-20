using System;
using JetBrains.Annotations;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property), MeansImplicitUse]
    internal class PluginStateAttribute : Attribute {
    }
}
