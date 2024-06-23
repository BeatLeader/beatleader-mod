using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using BeatLeader.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BeatLeader.UI.Reactive {
    internal abstract class DrivingReactiveComponent : DrivingReactiveComponentBase {
        /// <summary>
        /// Represents the children of the component.
        /// </summary>
        public new ICollection<ILayoutItem> Children => base.Children;
    }

    internal abstract class DrivingReactiveComponentBase : ReactiveComponentBase, ILayoutDriver {
        #region Layout Controller

        public ILayoutController? LayoutController {
            get => _layoutController;
            set {
                if (_layoutController != null) {
                    ReleaseContextMember(_layoutController);
                    _layoutController.LayoutControllerUpdatedEvent -= RecalculateLayoutTree;
                }
                _layoutController = value;
                if (_layoutController != null) {
                    InsertContextMember(_layoutController);
                    _layoutController.LayoutControllerUpdatedEvent += RecalculateLayoutTree;
                }
                RecalculateLayoutWithChildren();
            }
        }

        private ILayoutController? _layoutController;
        private bool _beingRecalculated;

        private void RecalculateLayoutWithChildren() {
            _layoutController?.ReloadChildren(_children);
            RecalculateLayoutTree();
        }

        private void RecalculateLayoutInternal(bool root) {
            if (_layoutController == null || Children.Count == 0) return;
            _layoutController.ReloadDimensions(ContentTransform.rect);
            _layoutController.Recalculate(root);
            if (root) {
                _layoutController.ApplySelf(this);
            }
            _layoutController.ApplyChildren();
        }

        public void RecalculateLayoutTree() {
            _beingRecalculated = true;
            //items without modifiers are not supposed to be controlled
            if (LayoutModifier != null && LayoutDriver?.LayoutController != null) {
                LayoutDriver!.RecalculateLayoutTree();
                _beingRecalculated = false;
                return;
            }
            RecalculateLayoutInternal(true);
            _beingRecalculated = false;
        }

        public void RecalculateLayout() {
            RecalculateLayoutInternal(false);
        }

        #endregion

        #region Children

        /// <summary>
        /// Represents the children of the component.
        /// </summary>
        protected ICollection<ILayoutItem> Children => _children;

        IEnumerable<ILayoutItem> ILayoutDriver.Children => Children;

        private ObservableCollectionAdapter<ILayoutItem> _children = null!;

        void ILayoutDriver.AppendChild(ILayoutItem item) {
            if (ContainsChild(item)) return;
            _children.collection.Add(item);
            AppendChildInternal(item);
        }

        void ILayoutDriver.TruncateChild(ILayoutItem item) {
            if (!ContainsChild(item)) return;
            _children.collection.Remove(item);
            TruncateChildInternal(item);
        }

        private bool ContainsChild(ILayoutItem item) {
            return _children.Any(x => x.Equals(item));
        }

        private void AppendChildInternal(ILayoutItem item) {
            AppendChild(item);
            item.LayoutDriver = this;
            item.ModifierUpdatedEvent += HandleChildModifierUpdated;
            RecalculateLayoutWithChildren();
            OnChildrenUpdated();
        }

        private void TruncateChildInternal(ILayoutItem item) {
            TruncateChild(item);
            item.LayoutDriver = null;
            item.ModifierUpdatedEvent -= HandleChildModifierUpdated;
            RecalculateLayoutWithChildren();
            OnChildrenUpdated();
        }

        private void TruncateChildrenInternal(IEnumerable<ILayoutItem> items) {
            foreach (var item in items) {
                TruncateChildInternal(item);
            }
        }

        private void HandleChildModifierUpdated(ILayoutItem item) {
            if (_beingRecalculated) return;
            //
            if (item.LayoutModifier == null) {
                RecalculateLayoutWithChildren();
            } else {
                RecalculateLayoutTree();
            }
        }

        protected override void OnLayoutRefresh() {
            if (_beingRecalculated) return;
            RecalculateLayoutTree();
        }

        protected override void OnLayoutApply() {
            if (LayoutDriver == null) return;
            RecalculateLayout();
        }

        #endregion

        #region Handle Children

        protected virtual Transform ChildrenContainer => ContentTransform;

        protected virtual void AppendChild(ILayoutItem item) {
            if (item is ReactiveComponentBase comp) {
                AppendReactiveChild(comp);
            } else {
                item.ApplyTransforms(x => x.SetParent(ChildrenContainer, false));
            }
        }

        protected virtual void TruncateChild(ILayoutItem item) {
            if (item is ReactiveComponentBase comp) {
                TruncateReactiveChild(comp);
            } else {
                item.ApplyTransforms(x => x.SetParent(null, false));
            }
        }

        protected virtual void AppendReactiveChild(ReactiveComponentBase comp) {
            comp.Use(ChildrenContainer);
        }

        protected virtual void TruncateReactiveChild(ReactiveComponentBase comp) {
            comp.Use();
        }

        #endregion

        #region Construct

        private class LayoutItemComparer : IEqualityComparer<ILayoutItem> {
            public bool Equals(ILayoutItem x, ILayoutItem y) => x.Equals(y);
            public int GetHashCode(ILayoutItem obj) => obj.GetHashCode();
        }

        private static readonly LayoutItemComparer layoutItemComparer = new();

        protected sealed override void ConstructInternal() {
            base.ConstructInternal();
            _children = new(
                new HashSet<ILayoutItem>(layoutItemComparer),
                AppendChildInternal,
                TruncateChildInternal,
                TruncateChildrenInternal
            );
        }

        #endregion

        #region Overrides

        protected sealed override float? DesiredHeight => base.DesiredHeight;
        protected sealed override float? DesiredWidth => base.DesiredWidth;

        protected sealed override void OnModifierUpdatedInternal() {
            if (LayoutDriver == null) RecalculateLayoutInternal(true);
        }

        #endregion

        #region Events

        protected virtual void OnChildrenUpdated() { }

        #endregion
    }

    internal abstract class ReactiveComponent : ReactiveComponentBase {
        #region Overrides

        protected sealed override void ConstructInternal() {
            base.ConstructInternal();
        }

        protected sealed override void OnModifierUpdatedInternal() {
            base.OnModifierUpdatedInternal();
        }

        #endregion
    }

    internal abstract partial class ReactiveComponentBase : ILayoutItem, IObservableHost, IReactiveComponent {
        #region Factory

        protected ReactiveComponentBase() {
            //do not want to reimplement constructor each inheritance so use this hacky reflection way
            var trace = new StackTrace();
            var mtd = trace.GetFrames()?
                .Select(static x => x.GetMethod())
                .FirstOrDefault(static x => x.Name == "Lazy" && x.DeclaringType == typeof(ReactiveComponent));
            if (mtd == null) ConstructAndInit();
        }

        public static T Lazy<T>() where T : ReactiveComponent, new() {
            return new();
        }

        #endregion

        #region UI Props

        /// <summary>
        /// Gets or sets state of the transform.
        /// </summary>
        public bool Enabled {
            get => Host.Enabled;
            set => Host.Enabled = value;
        }

        /// <summary>
        /// Gets or sets name of the content game object.
        /// </summary>
        public string Name {
            get => Content.name;
            set => Content.name = value;
        }

        #endregion

        #region Observable

        private ObservableHost _observableHost = null!;

        public void AddCallback<T>(string propertyName, Action<T> callback) {
            _observableHost.AddCallback(propertyName, callback);
        }

        public void RemoveCallback<T>(string propertyName, Action<T> callback) {
            _observableHost.RemoveCallback(propertyName, callback);
        }

        public void NotifyPropertyChanged([CallerMemberName] string? name = null) {
            _observableHost.NotifyPropertyChanged(name);
        }

        #endregion

        #region Context

        protected void InsertContextMember(IContextMember member) => Host.InsertContextMember(member);

        protected void ReleaseContextMember(IContextMember member) => Host.ReleaseContextMember(member);

        #endregion

        #region Layout Item

        public ILayoutDriver? LayoutDriver {
            get => Host.LayoutDriver;
            set => Host.LayoutDriver = value;
        }

        public ILayoutModifier? LayoutModifier {
            get => Host.LayoutModifier;
            set => Host.LayoutModifier = value;
        }

        bool ILayoutItem.WithinLayout {
            get => Enabled || WithinLayoutIfDisabled;
            set {
                if (WithinLayoutIfDisabled) return;
                Enabled = value;
            }
        }

        float? ILayoutItem.DesiredHeight => Host.DesiredHeight;
        float? ILayoutItem.DesiredWidth => Host.DesiredWidth;

        public bool WithinLayoutIfDisabled {
            get => _reactiveHost!.WithinLayoutIfDisabled;
            set => _reactiveHost!.WithinLayoutIfDisabled = value;
        }

        protected virtual float? DesiredHeight => null;
        protected virtual float? DesiredWidth => null;

        public event Action<ILayoutItem>? ModifierUpdatedEvent {
            add => Host.ModifierUpdatedEvent += value;
            remove => Host.ModifierUpdatedEvent -= value;
        }

        bool IEquatable<ILayoutItem>.Equals(ILayoutItem other) => ((ILayoutItem)Host).Equals(other);

        void ILayoutItem.ApplyTransforms(Action<RectTransform> applicator) => Host.ApplyTransforms(applicator);

        protected void RefreshLayout() => Host.RefreshLayout();

        protected virtual void OnModifierUpdatedInternal() { }

        #endregion

        #region Validation

        /// <summary>
        /// Validates is component initialized or not
        /// </summary>
        /// <exception cref="UninitializedComponentException">Thrown if component is not initialized</exception>
        protected void ValidateAndThrow() {
            if (!IsInitialized || !Validate()) throw new UninitializedComponentException();
        }

        /// <summary>
        /// Called with <see cref="ValidateAndThrow" />. Used for initialization checks
        /// </summary>
        /// <returns>True if validation has completed, False if not</returns>
        protected virtual bool Validate() => true;

        #endregion

        #region Coroutines

        /// <summary>
        /// Starts a coroutine on the <see cref="ReactiveHost"/> instance.
        /// </summary>
        /// <param name="coroutine">The coroutine to start.</param>
        protected void StartCoroutine(IEnumerator coroutine) {
            _reactiveHost!.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Stops a coroutine on the <see cref="ReactiveHost"/> instance.
        /// </summary>
        /// <param name="coroutine">The coroutine to stop.</param>
        protected void StopCoroutine(IEnumerator coroutine) {
            _reactiveHost!.StopCoroutine(coroutine);
        }

        /// <summary>
        /// Stops all coroutines on the <see cref="ReactiveHost"/> instance.
        /// </summary>
        protected void StopAllCoroutines() {
            _reactiveHost!.StopAllCoroutines();
        }

        #endregion

        #region Construct

        public RectTransform ContentTransform => _contentTransform ?? throw new UninitializedComponentException();
        public GameObject Content => _content ?? throw new UninitializedComponentException();

        public bool IsInitialized { get; private set; }
        public bool IsDestroyed { get; private set; }

        protected Canvas? Canvas {
            get {
                if (!_canvas) {
                    _canvas = Content.GetComponentInParent<Canvas>();
                }
                return _canvas;
            }
        }

        private ReactiveHost Host => _reactiveHost ?? throw new UninitializedComponentException();

        private Canvas? _canvas;
        private GameObject? _content;
        private RectTransform? _contentTransform;
        private ReactiveHost? _reactiveHost;

        /// <summary>
        /// Constructs and reparents the component if needed
        /// </summary>
        public GameObject Use(Transform? parent = null) {
            ValidateExternalInteraction();
            if (!IsInitialized) ConstructAndInit();
            ContentTransform.SetParent(parent, false);
            if (parent == null) LayoutDriver = null;
            return Content;
        }

        private void ConstructAndInit() {
            _observableHost = new(this);
            ConstructInternal();
            OnInitialize();
        }

        protected virtual void ConstructInternal() {
            if (IsInitialized) throw new InvalidOperationException();
            OnInstantiate();

            _content = Construct();
            _content.name = GetType().Name;
            _contentTransform = _content.GetOrAddComponent<RectTransform>();

            _reactiveHost = _content.GetOrAddComponent<ReactiveHost>();
            IsInitialized = true;
            _reactiveHost.AddComponent(this);
        }

        private void ValidateExternalInteraction() {
            if (IsDestroyed) throw new InvalidOperationException("Unable to manage the object since it's destroyed");
        }

        protected virtual void Construct(RectTransform rect) { }

        protected virtual GameObject Construct() {
            var go = new GameObject();
            var rect = go.AddComponent<RectTransform>();
            Construct(rect);
            return go;
        }

        #endregion

        #region Destroy

        /// <summary>
        /// Destroys the component
        /// </summary>
        public void Destroy() {
            Object.Destroy(Content);
            DestroyInternal();
        }

        private void DestroyInternal() {
            IsDestroyed = true;
            OnDestroy();
        }

        #endregion

        #region Events

        protected virtual void OnInstantiate() { }
        protected virtual void OnInitialize() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnStart() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnRectDimensionsChanged() { }
        protected virtual void OnLayoutRefresh() { }
        protected virtual void OnLayoutApply() { }

        #endregion
    }
}