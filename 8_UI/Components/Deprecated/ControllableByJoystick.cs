using BeatLeader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Components
{
    public class ControllableByJoystick : MonoBehaviour
    {
        public GrabbableHandle GrabbableHandle
        {
            get => _grabbableHandle;
            set
            {
                if (_grabbableHandle != null)
                {
                    _grabbableHandle.OnHandleGrabbed -= NotifyHandleGrabbed;
                    _grabbableHandle.OnHandleUngrabbed -= NotifyHandleUngrabbed;
                }

                _grabbableHandle = value;
                _grabbableHandle.OnHandleGrabbed += NotifyHandleGrabbed;
                _grabbableHandle.OnHandleUngrabbed += NotifyHandleUngrabbed;
            }
        }
        public Transform view;

        private GrabbableHandle _grabbableHandle;
        private VRController _controller;
        private Vector3 _grabPos;

        private void Update()
        {
            if (_controller == null || view == null) return;

            float joyVal = _controller.verticalAxisValue * Time.deltaTime * -1;
            float num = _grabPos.magnitude > 0.25f ? joyVal : Mathf.Clamp(joyVal, float.MinValue, 0f);

            view.position = new Vector3(view.position.x, view.position.y, num);
        }
        private void NotifyHandleGrabbed(VRController controller)
        {
            _controller = controller;
            _grabPos = controller.transform.InverseTransformPoint(view.position);
        }
        private void NotifyHandleUngrabbed(VRController controller)
        {
            _controller = null;
        }
    }
}
