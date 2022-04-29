using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Models;

namespace BeatLeader.Replays
{
    public class ReplayScenesTransitionSetupDataSO : StandardLevelScenesTransitionSetupDataSO
    {
        protected Replay _replay;
        protected ReplayManualInstaller.InitData _initData;

        public virtual void Init(Replay replay, ReplayManualInstaller.InitData initData)
        {
            _replay = replay;
            _initData = initData;
        }
    }
}
