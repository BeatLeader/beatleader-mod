using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Interop
{
    internal class DetectorSource
    {
        public DetectorSource()
        { }

        private string _name;
        private bool _isPlaying;
        private Transform _replayHeadTransform;
        private Transform _offset;

        public string name => _name;
        public bool isPlaying => _isPlaying;
        public Transform replayHeadTransform => _replayHeadTransform;
        public Transform offset => _offset;
    }
}
