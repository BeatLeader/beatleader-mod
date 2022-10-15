using BeatLeader.Models;
using System;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class PluginTypeAttribute : Attribute {
        public PluginTypeAttribute(string type) {
            this.type = type;
        }

        public readonly string type;
    }
}
