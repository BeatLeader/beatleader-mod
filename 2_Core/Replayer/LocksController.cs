using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader
{
    public class LocksController : MonoBehaviour
    {
        public enum LockMode
        {
            Force,
            WhenRequired,
            DoNotLock
        }

        private Dictionary<Behaviour, LockData> _lockedComponents = new();

        private void LateUpdate()
        {
            foreach (var item in _lockedComponents)
            {
                var behaviour = item.Key;
                var lockData = item.Value;

                if (lockData.mode == LockMode.DoNotLock || !lockData.locked) continue;

                behaviour.enabled = (lockData.mode == LockMode.Force) ||
                    (lockData.mode == LockMode.WhenRequired && behaviour.enabled) ? false : behaviour.enabled;
            }
        }
        private void OnDestroy()
        {
            foreach (var item in _lockedComponents)
            {
                var behaviour = item.Key;
                var lockData = item.Value;

                if (behaviour == null) continue;
                if (lockData.returnBackOnDestroy)
                    Plugin.Log.Notice($"[Locker] Setting {behaviour} back to {lockData.returnBackValue}");
                behaviour.enabled = lockData.returnBackOnDestroy ? lockData.returnBackValue : behaviour.enabled;
            }
        }
        public void InstallLock(Behaviour behaviour, LockData data)
        {
            if (!_lockedComponents.TryAdd(behaviour, data))
                Plugin.Log.Warn("[Locker] Lock are already installed");
        }
        public void InstallLock(Behaviour behaviour)
        {
            if (!_lockedComponents.TryAdd(behaviour, LockData.defaultData))
                Plugin.Log.Warn("[Locker] Lock are already installed!");
        }
        public void UninstallLock(Behaviour behaviour)
        {
            if (_lockedComponents.TryGetValue(behaviour, out var data))
                _lockedComponents.Remove(behaviour);
            else Plugin.Log.Warn("[Locker] This component is not locked!");
        }
    }
}
