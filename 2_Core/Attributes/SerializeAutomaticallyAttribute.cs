using System;

namespace BeatLeader
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeAutomaticallyAttribute : Attribute
    {
        public SerializeAutomaticallyAttribute(string configName)
        {
            this.configName = configName;
        }
        public SerializeAutomaticallyAttribute() 
        {
            configName = string.Empty;
        }

        public readonly string configName;
    }
}
