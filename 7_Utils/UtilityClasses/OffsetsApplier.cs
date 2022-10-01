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

        private Transform _transform;

        public void Setup(Transform objectTransform)
        {
            _transform = objectTransform;
            var thisTransform = transform;
            
            thisTransform.SetParent(objectTransform.parent, false);
            objectTransform.SetParent(thisTransform);
        }
        public void Dispose()
        {
            _transform?.SetParent(transform.parent, false);
        }
    }
}
