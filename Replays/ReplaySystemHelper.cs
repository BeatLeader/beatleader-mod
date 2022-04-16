using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine.Events;   
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader.Replays
{
    public class ReplaySystemHelper 
    {
        private protected static UnityEvent _actionButtonEvent;
        private protected static ReplayPlayer _player;
        private protected static Replay _replay;
        private protected static bool _asReplay;

        private static UnityEvent ActionButtonEvent
        {
            get => _actionButtonEvent = _actionButtonEvent == null ? Resources.FindObjectsOfTypeAll<NoTransitionsButton>().First(x => x.name == "ActionButton").onClick : _actionButtonEvent;
        }
        public static ReplayPlayer player => _player;
        public static Replay replay => _replay;
        public static bool asReplay => _asReplay;

        public static void StartInReplayMode(Replay replay)
        {
            _asReplay = true;
            _replay = replay;
            ActionButtonEvent.Invoke();
        }
    }
}
