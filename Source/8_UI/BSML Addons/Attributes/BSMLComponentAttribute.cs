using System;

namespace BeatLeader.UI.BSML_Addons {
    [AttributeUsage(AttributeTargets.Class)]
    internal class BSMLComponentAttribute : Attribute {
        /// <summary>
        /// Usable when you want to suppress BSMLComponent attribute defined by a superclass
        /// </summary>
        public bool Suppress { get; set; }
        public string? Name { get; set; }
        public string? Namespace { get; set; }
    }
}