using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using Zenject;

namespace BeatLeader.Replays.MapEmitating
{
    public class SimpleNoteCutComparator : MonoBehaviour, INoteControllerNoteWasCutEvent
    {
        public class Pool : MonoMemoryPool<SimpleNoteCutComparator>
        {
            protected override void Reinitialize(SimpleNoteCutComparator item)
            {
                Destroy(item._noteCutter);
                item._noteCutEvent = null;
                item._noteController = null;
                item._availableForCut = false;
                item._initialized = false;
                item._isFinished = false;
            }
        }

        [Inject] public AudioTimeSyncController timeSyncController;

        protected NoteEvent _noteCutEvent;
        protected SimpleNoteCutter _noteCutter;
        protected NoteController _noteController;
        protected bool _availableForCut;
        protected bool _initialized;
        protected bool _isFinished;

        public NoteEvent noteCutEvent => _noteCutEvent;
        public NoteController noteController => _noteController;
        public bool availableForCut => _availableForCut;
        public bool isFinished => _isFinished;
        public float cutTime => _noteCutEvent.eventTime;
        public int noteID => _noteCutEvent.noteID;

        public void Update()
        {
            if (_initialized && availableForCut && timeSyncController != null && timeSyncController.songTime >= cutTime)
            {
                if (_noteCutEvent != null && _noteCutEvent.noteCutInfo != null)
                {
                    _noteCutter.Cut(ReplayNoteCutInfo.Parse(_noteCutEvent.noteCutInfo, _noteController));
                }
                HandleNoteControllerNoteWasCut();
            }
        }
        public void SetUnavailableForCut()
        {
            _availableForCut = false;
        }
        public void Init(NoteController noteController, NoteEvent noteCutEvent)
        {
            _noteController = noteController;
            _noteCutEvent = noteCutEvent;
            _noteCutter = _noteController.gameObject.AddComponent<SimpleNoteCutter>();
            _noteController.noteWasCutEvent.Add(this);
            _availableForCut = true;
            _initialized = true;
        }
        public void HandleNoteControllerNoteWasCut()
        {
            _noteController.noteWasCutEvent.Remove(this);
            Destroy(_noteCutter);
            _isFinished = true;
        }
        public void HandleNoteControllerNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            HandleNoteControllerNoteWasCut();
        }
    }
}
