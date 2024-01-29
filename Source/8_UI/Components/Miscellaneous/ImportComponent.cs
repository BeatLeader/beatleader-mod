using System;
using BeatLeader.UI.BSML_Addons;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [BSMLComponent(Name = "Import")]
    [RequireComponent(typeof(RectTransform))]
    internal class ImportComponent : ReeUIComponentV3<ImportComponent> {
        [ExternalProperty("From"), UsedImplicitly]
        private ReeUIComponentV3Base? Host { get; set; }

        private Transform _parent = null!;
        private bool _isInitialized;
        
        protected override void OnPropertySet() {
            if (_isInitialized) return;
            _isInitialized = true;
            if (Host is null) {
                throw new InvalidOperationException("\"From\" property is required for the \"Import\" component");
            }
            var hostTransform = Host.ContentTransform;
            hostTransform.SetParent(_parent, false);
            MoveChildren(hostTransform);
        }

        private void MoveChildren(Transform parent) {
            foreach (Transform child in transform) {
                child.SetParent(parent, false);
            }
        }

        protected override void OnChildrenChange() {
            if (Host?.ContentTransform is not { } trans) return;
            MoveChildren(trans);
        }

        protected override GameObject Construct(Transform parent) {
            _parent = parent;
            return gameObject;
        }
    }
}