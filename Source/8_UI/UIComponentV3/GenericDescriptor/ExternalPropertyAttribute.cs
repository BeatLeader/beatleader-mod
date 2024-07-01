using System;

namespace BeatLeader.Components {
    public enum PropertyExportMode {
        Inherit,
        Direct
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
    internal class ExternalPropertyAttribute : Attribute {
        public ExternalPropertyAttribute() {
            ExportMode = PropertyExportMode.Direct;
        }
        
        public ExternalPropertyAttribute(string name) : this() {
            Name = name;
        }

        public ExternalPropertyAttribute(string? prefix, params string[] inheritProperties) {
            ExportMode = PropertyExportMode.Inherit;
            Prefix = prefix;
            PropertiesToInherit = inheritProperties.Length is 0 ? null : inheritProperties;
        }

        public string? Name { get; }
        public string? Prefix { get; }
        public string[]? PropertiesToInherit { get; }
        public PropertyExportMode ExportMode { get; }
    }
}