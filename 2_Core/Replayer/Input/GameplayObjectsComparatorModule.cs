using BeatLeader.Models;
using BeatLeader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

namespace BeatLeader.Replayer
{
    internal class GameplayObjectsComparatorModule : IComparatorModule<RaycastResult>
    {
        public bool Compare(RaycastResult result)
        {
            var gameplayObject = result.gameObject;
            var note = gameplayObject.GetComponentInParent<NoteController>();
            var obstacle = gameplayObject.GetComponentInParent<ObstacleController>();

            return note == null && obstacle == null;
        }
    }
}
