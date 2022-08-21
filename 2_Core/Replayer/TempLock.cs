using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Replayer
{
    internal class TempLock : MonoBehaviour
    {
        private bool _originalState;
        private GameObject _go;

        public void Init(GameObject go)
        {
            if (go == null) return;
            _go = go;
            _originalState = go.activeSelf;
            go.SetActive(false);
        }
        private void OnDestroy()
        {
            _go?.SetActive(_originalState);
        }

        public static TempLock Create(GameObject go)
        {
            var container = new GameObject($"{go}Locker");
            var locker = container.AddComponent<TempLock>();
            locker.Init(go);
            return locker;
        }
    }
}
