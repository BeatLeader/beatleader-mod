using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Utils
{
    internal class OffsetsApplier : MonoBehaviour
    {
        public Pose Offsets
        {
            get => transform.GetLocalPose();
            set => transform.SetLocalPose(value);
        }

        public void Setup(Transform transform)
        {
            this.transform.SetParent(transform.parent);
            transform.SetParent(this.transform);
        }
    }
}
