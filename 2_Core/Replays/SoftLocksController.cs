using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader
{
    public class SoftLocksController : MonoBehaviour
    {
        public enum LockMode
        {
            Force,
            WhenRequired
        }

        private List<LockData> _lockedComponents = new List<LockData>();

        private void LateUpdate()
        {
            _lockedComponents.Where(x => x.locked && x.behaviour != null
              && ((x.mode == LockMode.WhenRequired && x.behaviour.isActiveAndEnabled) || x.mode == LockMode.Force))
                .ToList().ForEach(x => x.behaviour.enabled = false);
        }
        public void Lock(Behaviour behaviour, bool @lock = true, bool enable = false)
        {
            if (TryGetLockData(behaviour, out LockData data))
            {
                data.locked = @lock;
                data.behaviour.enabled = !@lock ? enable : data.behaviour.enabled;
            }
            else Debug.LogWarning("This component is not locked!");
        }
        public void InstallLock(Behaviour behaviour, LockMode mode = LockMode.WhenRequired)
        {
            if (TryGetLockData(behaviour))
            {
                Debug.LogWarning("Can not install lock because it is already installed!");
                return;
            }
            _lockedComponents.Add(new LockData(behaviour, mode));
        }
        public void UninstallLock(Behaviour behaviour)
        {
            if (TryGetLockData(behaviour, out LockData data))
                _lockedComponents.Remove(data);
            else Debug.LogWarning("This component is not locked!");
        }

        private bool TryGetLockData(Behaviour behaviour)
        {
            return TryGetLockData(behaviour, out LockData data);
        }
        private bool TryGetLockData(Behaviour behaviour, out LockData data)
        {
            return (data = _lockedComponents.FirstOrDefault(x => x.behaviour == behaviour)) != null;
        }
    }
}
