using System;

namespace BeatLeader.UI.BSML_Addons {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class ExternalPropertyAttribute : Attribute {
        public ExternalPropertyAttribute(string? name = null) {
            Name = name;
        }

        public string? Name { get; }
    }
}