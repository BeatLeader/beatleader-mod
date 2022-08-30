using static BeatLeader.LocksController;
using UnityEngine;

namespace BeatLeader.Models
{
    public class LockData
    {
        public static readonly LockData defaultData = new(LockMode.WhenRequired, true);

        public readonly LockMode mode;
        public readonly bool returnBackOnDestroy;
        public readonly bool returnBackValue;
        public bool locked;

        public LockData(LockMode mode, bool locked = true, bool returnBackOnDestroy = false, bool returnBackValue = true)
        {
            this.mode = mode;
            this.locked = locked;
            this.returnBackOnDestroy = returnBackOnDestroy;
            this.returnBackValue = returnBackValue;
        }
    }
}
