using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Models
{
    internal interface ILateInitializable
    {
        void LateInitialize();
    }
}
