using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader
{
    public class SoftLocksController : MonoBehaviour //this class allows modders to disable force locking without troubles
    {
        public enum LockMode
        {
            Force,
            WhenRequired
        }

        private List<LockData> _lockedComponents = new List<LockData>();

        private void LateUpdate()
        {
            foreach (var item in _lockedComponents.Where(x => x.locked && x.component != null 
              && ((x.mode == LockMode.WhenRequired && x.component.isActiveAndEnabled) || x.mode == LockMode.Force)))
                item.component.enabled = false;
        }
        public void Lock(Behaviour component, bool @lock = true, bool enable = false)
        {
            if (TryGetLockData(component, out LockData data))
            {
                data.locked = @lock;
                data.component.enabled = !@lock ? enable : data.component.enabled;
            }    
            else Debug.LogWarning("This component is not locked!");
        }
        public void InstallLock(Behaviour component, LockMode mode = LockMode.WhenRequired)
        {
            if (TryGetLockData(component))
            {
                Debug.LogWarning("Can not install lock because it is already installed!");
                return;
            }
            _lockedComponents.Add(new LockData(component, mode));
        }
        public void UninstallLock(Behaviour component)
        {
            if (TryGetLockData(component, out LockData data))
                _lockedComponents.Remove(data);
            else Debug.LogWarning("This component is not locked!");
        }

        private bool TryGetLockData(Behaviour component)
        {
            return TryGetLockData(component, out LockData data);
        }
        private bool TryGetLockData(Behaviour component, out LockData data)
        {
            return (data = _lockedComponents.FirstOrDefault(x => x.component == component)) != null;
        }
    }
}
