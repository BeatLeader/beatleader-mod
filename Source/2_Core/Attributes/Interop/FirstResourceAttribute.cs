using System;
using JetBrains.Annotations;

namespace BeatLeader {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property), MeansImplicitUse]
    internal class FirstResourceAttribute : Attribute {
        public bool RequireActiveInHierarchy { get; set; }
        public string? Name { get; set; }
        public string? ParentName { get; set; }
    }
}