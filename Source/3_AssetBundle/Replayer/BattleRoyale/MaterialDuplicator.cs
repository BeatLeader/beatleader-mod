using UnityEngine;

namespace BeatLeader {
    internal class MaterialDuplicator : MonoBehaviour {
        [SerializeField] 
        private MeshRenderer meshRenderer = null!;

        private Material _material = null!;
        
        private void Awake() {
            var sourceMaterial = meshRenderer.material;
            var clonedMaterial = Instantiate(sourceMaterial);
            _material = clonedMaterial;
            meshRenderer.material = clonedMaterial;
        }

        private void OnDestroy() {
            Destroy(_material);
        }
    }
}