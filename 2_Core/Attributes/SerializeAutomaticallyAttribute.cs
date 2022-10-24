using System;

namespace BeatLeader {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    internal class SerializeAutomaticallyAttribute : Attribute {
        public SerializeAutomaticallyAttribute(string configName = null) {
            this.configName = configName;
        }

        public readonly string? configName;
    }
}
