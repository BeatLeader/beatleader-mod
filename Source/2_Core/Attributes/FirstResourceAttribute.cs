using System;
using JetBrains.Annotations;

namespace BeatLeader {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property), MeansImplicitUse]
    public class FirstResourceAttribute : Attribute {
        public FirstResourceAttribute(string? name = null, bool requireActiveInHierarchy = false) {
            this.name = name;
            this.requireActiveInHierarchy = requireActiveInHierarchy;
        }

        public readonly bool requireActiveInHierarchy;
        public readonly string? name = null;
    }
}