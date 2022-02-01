using System;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader {
    [UsedImplicitly]
    public class MonkeyHeadManager : IInitializable, IDisposable, ITickable {
        #region Constructor

        private readonly GameObject _monkey;

        public MonkeyHeadManager() {
            _monkey = Object.Instantiate(BundleLoader.MonkeyPrefab, Vector3.up, Quaternion.identity);
            _monkey.transform.localScale = Vector3.one * 0.1f;
        }

        #endregion

        public void Initialize() { }

        public void Tick() {
            _monkey.transform.Rotate(0, 0, 1);
        }

        public void Dispose() { }
    }
}