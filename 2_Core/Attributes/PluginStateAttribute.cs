using System;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class PluginStateAttribute : Attribute {
    }
}
