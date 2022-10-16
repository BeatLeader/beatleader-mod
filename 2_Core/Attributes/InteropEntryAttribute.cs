using System;

namespace BeatLeader.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    internal class InteropEntryAttribute : Attribute {
        public InteropEntryAttribute(bool ignoreOnNull = false) {
            this.ignoreOnNull = ignoreOnNull;
        }

        public readonly bool ignoreOnNull;
    }
}
