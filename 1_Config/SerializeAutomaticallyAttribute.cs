using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
