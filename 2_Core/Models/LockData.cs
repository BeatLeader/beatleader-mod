using static BeatLeader.SoftLocksController;
using UnityEngine;

namespace BeatLeader.Models
{
    public class LockData
    {
        public readonly Behaviour behaviour;
        public readonly LockMode mode;
        public bool locked;

        public LockData(Behaviour behaviour, LockMode mode)
        {
            this.mode = mode;
            this.behaviour = behaviour;
            locked = true;
        }
    }
}
