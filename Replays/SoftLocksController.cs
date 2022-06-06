using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Replays
{
    public class SoftLocksController : MonoBehaviour //this class allows modders to disable force locking without troubles
    {
        public enum LockMode
        {
            Force,
            WhenRequired
        }

        public List<LockData> _lockedComponents = new List<LockData>();

        public void LateUpdate()
        {
            foreach (var item in _lockedComponents)
            {
                if (item.locked && item.component != null && ((item.mode == LockMode.WhenRequired && item.component.isActiveAndEnabled) || item.mode == LockMode.Force))
                {
                    item.component.enabled = false;
                }
            }
        }
        public void Lock(bool @lock, Behaviour component)
        {
            LockData data = GetLockData(component);
            if (data != null)
                data.locked = @lock;
            else 
                Debug.LogWarning("This component does not locked!");
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
            else Debug.LogWarning("This component does not locked!");
        }
        protected LockData GetLockData(Behaviour component)
        {
            LockData data = null;
            foreach (var item in _lockedComponents)
            {
                if (item.component == component)
                {
                    data = item;
                    break;
                }
            }
            return data;
        }
    }
}
