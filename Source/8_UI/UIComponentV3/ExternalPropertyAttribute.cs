using System;

namespace BeatLeader.Components {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class ExternalPropertyAttribute : Attribute {
        public ExternalPropertyAttribute(string? name = null) {
            Name = name;
        }

        public string? Name { get; }
    }
}