using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader
{
    internal class FloatingConfig
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float GridPosIncrement { get; set; }
        public float GridRotIncrement { get; set; }
        public bool IsPinned { get; set; }
    }
}
