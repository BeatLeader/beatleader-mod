using System;

namespace BeatLeader.Components {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class ExternalPropertyAttribute : Attribute {
        public ExternalPropertyAttribute() { }

        public ExternalPropertyAttribute(string name) {
            Name = name;
        }

        public string? Name { get; }
    }
}