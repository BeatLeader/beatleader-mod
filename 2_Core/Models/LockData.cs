using static BeatLeader.SoftLocksController;
using UnityEngine;

namespace BeatLeader.Models
{
    public class LockData
    {
        public readonly Behaviour behaviour;
        public readonly LockMode mode;
        public readonly bool originalState;
        public bool locked;

        public LockData(Behaviour behaviour, LockMode mode, bool originalState)
        {
            this.mode = mode;
            this.behaviour = behaviour;
            locked = true;
            this.originalState = originalState;
        }
    }
}
