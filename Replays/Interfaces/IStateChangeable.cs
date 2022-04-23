using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Replays.Interfaces
{
    public interface IStateChangeable
    {
        void SetEnabled(bool state);
    }
}
