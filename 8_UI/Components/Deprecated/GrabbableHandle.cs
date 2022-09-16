using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRUIControls;

namespace BeatLeader.Components
{
    public class GrabbableHandle : MonoBehaviour
    {
        public event Action<VRController> OnHandleGrabbed;
        public event Action<VRController> OnHandleUngrabbed;

        public Transform handle;

        private VRPointer _pointer = Resources.FindObjectsOfTypeAll<VRPointer>().FirstOrDefault();
        private VRController _grabbingController;

        private void Update()
        {
            if (handle == null) return;

            var controller = _pointer.vrController;
            if (controller.triggerValue > 0.9f)
            {
                if (_grabbingController != null || !Physics.Raycast(controller.position, 
                    controller.forward, out var hit, 50f) || hit.transform != handle) return;

                _grabbingController = _pointer.vrController;
                OnHandleGrabbed?.Invoke(_grabbingController);
            }
            else
            {
                if (_grabbingController != null) OnHandleUngrabbed?.Invoke(_grabbingController);
                _grabbingController = null;
            }
        }
    }
}
