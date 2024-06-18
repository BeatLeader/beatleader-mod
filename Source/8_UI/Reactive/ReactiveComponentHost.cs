using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal abstract partial class ReactiveComponentBase {
        [RequireComponent(typeof(RectTransform))]
        private class ReactiveHost : MonoBehaviour, ILayoutItem {
            #region LayoutItem

            public ILayoutDriver? LayoutDriver {
                get => _layoutDriver;
                set {
                    if (_layoutDriver == value) return;
                    // do not change! truncation before assignment can cause deadlock!
                    var oldDriver = _layoutDriver;
                    _layoutDriver = value;
                    oldDriver?.TruncateChild(this);
                    _layoutDriver?.AppendChild(this);
                }
            }

            public ILayoutModifier? LayoutModifier {
                get => _modifier;
                set {
                    if (_modifier != null) {
                        ReleaseContextMember(_modifier);
                        _modifier.ModifierUpdatedEvent -= HandleModifierUpdated;
                    }
                    _modifier = value;
                    if (_modifier != null) {
                        _modifier.ModifierUpdatedEvent += HandleModifierUpdated;
                        InsertContextMember(_modifier);
                    }
                    HandleModifierUpdated();
                }
            }

            public float? DesiredHeight => _components.FirstOrDefault()?.DesiredHeight;
            public float? DesiredWidth => _components.FirstOrDefault()?.DesiredWidth;

            public bool WithinLayout {
                get => Enabled || WithinLayoutIfDisabled;
                set {
                    if (WithinLayoutIfDisabled) return;
                    Enabled = value;
                }
            }

            public bool WithinLayoutIfDisabled { get; set; }

            public event Action? ModifierUpdatedEvent;

            private ILayoutDriver? _layoutDriver;
            private ILayoutModifier? _modifier;

            bool IEquatable<ILayoutItem>.Equals(ILayoutItem other) {
                return ReferenceEquals(other, this) || _components.Any(x => x == other);
            }

            public void ApplyTransforms(Action<RectTransform> applicator) {
                _beingRecalculated = true;
                applicator(_rectTransform);
                _beingRecalculated = false;
                _components.ForEach(static x => x.OnLayoutApply());
            }

            #endregion

            #region Layout

            private bool _recalculationScheduled;
            private bool _beingRecalculated;

            public void RefreshLayout() {
                if (_beingRecalculated) return;
                HandleModifierUpdated();
                _components.ForEach(static x => x.OnLayoutRefresh());
            }

            private void HandleModifierUpdated() {
                if (_modifier == null) return;
                _modifier.ReloadLayoutItem(this);
                ModifierUpdatedEvent?.Invoke();
                _components.ForEach(static x => x.OnModifierUpdatedInternal());
            }

            #endregion

            #region Contexts

            private readonly Dictionary<Type, (object context, int members)> _contexts = new();

            public void InsertContextMember(IContextMember member) {
                var type = member.ContextType;
                if (type == null) return;
                object context;
                var members = 1;
                if (_contexts.TryGetValue(type, out var tuple)) {
                    context = tuple.context;
                    members += tuple.members;
                } else {
                    context = member.CreateContext();
                }
                member.ProvideContext(context);
                _contexts[type] = (context, members);
            }

            public void ReleaseContextMember(IContextMember member) {
                var type = member.ContextType;
                if (type == null || !_contexts.TryGetValue(type, out var tuple)) return;
                var members = tuple.members - 1;
                if (members == 0) {
                    _contexts.Remove(type);
                } else {
                    tuple.members = members;
                    _contexts[type] = tuple;
                }
                member.ProvideContext(null);
            }

            #endregion

            #region Components

            public bool Enabled {
                get => gameObject.activeInHierarchy;
                set => gameObject.SetActive(value);
            }

            public bool IsStarted { get; private set; }
            public bool IsDestroyed { get; private set; }

            private readonly List<ReactiveComponentBase> _components = new();

            public void AddComponent(ReactiveComponentBase comp) {
                _components.Add(comp);
                if (IsStarted) comp.OnStart();
            }

            public void RemoveComponent(ReactiveComponentBase comp) {
                _components.Remove(comp);
            }

            #endregion

            #region Events

            private RectTransform _rectTransform = null!;

            private void Awake() {
                _rectTransform = GetComponent<RectTransform>();
            }

            private void Start() {
                _components.ForEach(static x => x.OnStart());
                IsStarted = true;
            }

            private void Update() {
                _components.ForEach(static x => x.OnUpdate());
            }

            private void LateUpdate() {
                if (_recalculationScheduled) {
                    RefreshLayout();
                    _recalculationScheduled = false;
                }
                _components.ForEach(static x => x.OnLateUpdate());
            }

            private void OnDestroy() {
                _components.ForEach(static x => x.DestroyInternal());
                IsDestroyed = true;
            }

            private void OnEnable() {
                RefreshLayout();
                _components.ForEach(static x => x.OnEnable());
            }

            private void OnDisable() {
                RefreshLayout();
                _components.ForEach(static x => x.OnDisable());
            }

            private void OnRectTransformDimensionsChange() {
                // if we look at how the reactive layout system works
                // we'll see that it goes to the root driver and starts recalculation from there.
                // it means that layout controller will be able modify to modify rect properties
                // which is not allowed to be performed on the OnRectTransformDimensionsChange stack.
                // that's why we schedule recalculation to the end of this frame
                _recalculationScheduled = true;
                _components.ForEach(static x => x.OnRectDimensionsChanged());
            }

            #endregion
        }
    }
}