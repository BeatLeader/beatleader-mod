using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Emulators;
using BeatLeader.Replays.Tools;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlaybackController : MonoBehaviour
    {
        [Inject] protected readonly SimpleCutScoreEffectSpawner _cutScoreEffectSpawner;
        [Inject] protected readonly FakePlayer _fakePlayer;

        public void Start()
        {

        }
    }
}
