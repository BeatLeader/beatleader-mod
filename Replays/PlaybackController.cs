using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Interfaces;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Tools;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlaybackController : MonoBehaviour
    {
        [Inject] protected readonly MenuSabersManager _menuSabersManager;
        [Inject] protected readonly Replayer _replayer;

        protected List<IStateChangeable> _elementsToPause;

        public void Start()
        {
            _menuSabersManager.ShowMenuControllers();
            _elementsToPause = new List<IStateChangeable>();
            _elementsToPause.Add(_replayer);
            //Pause();
        }
        public void Pause()
        {
            foreach (var item in _elementsToPause)
            {
                item.SetEnabled(false); 
            }
        }
    }
}
