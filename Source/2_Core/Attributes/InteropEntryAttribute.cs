using System;
using JetBrains.Annotations;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
    internal class InteropEntryAttribute : Attribute {
        public InteropEntryAttribute(bool ignoreOnNull = false) {
            this.ignoreOnNull = ignoreOnNull;
        }

        public readonly bool ignoreOnNull;
    }
}
