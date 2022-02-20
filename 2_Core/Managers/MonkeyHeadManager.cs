using System;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader {
    [UsedImplicitly]
    public class MonkeyHeadManager : IInitializable, IDisposable, ITickable {
        #region Constructor

        public MonkeyHeadManager() {
            Object.Instantiate(BundleLoader.MonkeyPrefab, new Vector3(1.0f, 1.0f, -0.4f), Quaternion.identity);
        }

        #endregion

        public void Initialize() { }

        public void Tick() { }

        public void Dispose() { }
    }
}