using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Tools;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlaybackController : MonoBehaviour
    {
        [Inject] protected readonly SimpleCutScoreEffectSpawner _cutScoreEffectSpawner;
        [Inject] protected readonly SimpleNoteComparatorsSpawner _noteComparatorsHandler;
        [Inject] protected readonly MenuSabersManager _menuSabersManager;
        [Inject] protected readonly Replayer _replayer;
 
        public void Start()
        {
            _menuSabersManager.ShowMenuControllers();
        }
    }
}
