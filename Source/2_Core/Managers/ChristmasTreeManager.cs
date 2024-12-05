using UnityEngine;
using Zenject;

namespace BeatLeader {
    internal class ChristmasTreeManager : IInitializable, ILateDisposable {
        private ChristmasTree _christmasTree = null!;
        
        public void Initialize() {
            SpawnTree();
            SoloFlowCoordinatorPatch.PresentedEvent += HandleCoordinatorPresented;
            SoloFlowCoordinatorPatch.DismissedEvent += HandleCoordinatorDismissed;
        }

        public void LateDispose() {
            SoloFlowCoordinatorPatch.PresentedEvent -= HandleCoordinatorPresented;
            SoloFlowCoordinatorPatch.DismissedEvent -= HandleCoordinatorDismissed;
        }
        
        private void SpawnTree() {
            var prefab = BundleLoader.ChristmasTree;
            var instance = Object.Instantiate(prefab, null, false);
            instance.transform.position = new Vector3(2.7f, 0f, 4f);
            _christmasTree = instance;
        }

        private void HandleCoordinatorPresented() {
            _christmasTree.Present();
        }
        
        private void HandleCoordinatorDismissed() {
            _christmasTree.Dismiss();
        }
    }
}