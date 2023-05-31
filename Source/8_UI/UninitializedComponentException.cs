using System;

namespace BeatLeader {
    public class UninitializedComponentException : Exception {
        public UninitializedComponentException() : base("The component was not initialized") {}
    }
}