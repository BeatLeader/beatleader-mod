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
            foreach (var item in _lockedComponents)
            {
                if (item.locked && item.component != null && ((item.mode == LockMode.WhenRequired && item.component.isActiveAndEnabled) || item.mode == LockMode.Force))
                {
                    item.component.enabled = false;
                }
            }
        }
        public void Lock(Behaviour component, bool @lock = true, bool enable = false)
        {
            LockData data = GetLockData(component);
            if (data != null)
            {
                data.locked = @lock;
                data.component.enabled = !@lock ? enable : data.component.enabled;
            }    
            else Debug.LogWarning("This component is not locked!");
        }
        public void InstallLock(Behaviour component, LockMode mode)
        {
            if (GetLockData(component) != null)
            {
                Debug.LogWarning("Can not install lock because it is already installed!");
                return;
            }
            _lockedComponents.Add(new LockData(component, mode));
        }
        public void UninstallLock(Behaviour component)
        {
            LockData data = GetLockData(component);
            if (data != null)
                _lockedComponents.Remove(data);
            else Debug.LogWarning("This component is not locked!");
        }
        protected LockData GetLockData(Behaviour component)
        {
            return _lockedComponents.Select(x => x.component).Contains(component) ? 
                _lockedComponents.First(x => x.component == component) : null;
        }
    }
}
