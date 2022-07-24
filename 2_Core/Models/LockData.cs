using static BeatLeader.SoftLocksController;
using UnityEngine;

namespace BeatLeader.Models
{
    public class LockData
    {
        public readonly Behaviour component;
        public readonly LockMode mode;
        public bool locked;

        public LockData(Behaviour component, LockMode mode)
        {
            this.mode = mode;
            this.component = component;
            locked = true;
        }
    }
}
