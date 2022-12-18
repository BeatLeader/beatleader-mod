using System;

namespace BeatLeader {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FirstResourceAttribute : Attribute {
        public FirstResourceAttribute(string? name = null, bool requireActiveInHierarchy = false) {
            this.name = name;
            this.requireActiveInHierarchy = requireActiveInHierarchy;
        }

        public readonly bool requireActiveInHierarchy;
        public readonly string? name = null;
    }
}