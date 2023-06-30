using BeatLeader.Models;
using System.Collections;
using UnityEngine;

namespace BeatLeader.Replayer.Emulation {
    //will be used in battle royale
    internal class LateInitializableVRControllersProvider : IVRControllersProvider {
        public LateInitializableVRControllersProvider(
            IVRControllersProvider earlyProvider,
            VRControllersInstantiator instantiator,
            string? name = null) {
            _actualProvider = earlyProvider;
            _instantiator = instantiator;
            _name = name;
            StartCoroutine();
        }

        public VRController LeftSaber => _actualProvider.LeftSaber;
        public VRController RightSaber => _actualProvider.RightSaber;
        public VRController Head => _actualProvider.Head;

        private readonly string? _name;
        private readonly VRControllersInstantiator _instantiator;
        private IVRControllersProvider _actualProvider;

        private IEnumerator InitializationCoroutine() {
            yield return new WaitForEndOfFrame();
            _actualProvider = _instantiator.CreateInstance(_name);
        }

        private void StartCoroutine() {
            CoroutinesHandler.Instance.StartCoroutine(InitializationCoroutine());
        }
    }
}
