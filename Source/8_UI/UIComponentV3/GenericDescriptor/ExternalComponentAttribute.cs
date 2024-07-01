using System;

namespace BeatLeader.Components {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class ExternalComponentAttribute : Attribute { }
}