using System;

namespace BeatLeader
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class FirstResourceAttribute : Attribute
    {
        public FirstResourceAttribute() { }
        public FirstResourceAttribute(string name)
        {
            this.name = name;
        }

        public readonly string? name = null;
    }
}