using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
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
                _layoutController = value;
                RefreshLayoutControllerChildren();
                RecalculateLayout();
            }
        }

        private ILayoutController? _layoutController;

        private void RefreshLayoutControllerChildren() {
            _layoutController?.ReloadChildren(_children);
        }

        private void RecalculateLayout() {
            _layoutController?.ReloadDimensions(ContentTransform.rect);
            _layoutController?.Recalculate();
        }

        #endregion

        #region Children

        /// <summary>
        /// Represents the children of the component.
        /// </summary>
        protected ICollection<ILayoutItem> Children => _children;

        IEnumerable<ILayoutItem> ILayoutDriver.Children => Children;

        private readonly ObservableCollection<ILayoutItem> _children = new();

        private void ApplyParent(IEnumerable children, bool append) {
            foreach (var child in children) {
                if (child is not ILayoutItem comp) continue;
                if (append) {
                    AppendChild(comp);
                    comp.LayoutDriver = this;
                    comp.ModifierUpdatedEvent += HandleChildModifierUpdated;
                } else {
                    TruncateChild(comp);
                    comp.LayoutDriver = null;
                    comp.ModifierUpdatedEvent -= HandleChildModifierUpdated;
                }
            }
        }

        void ILayoutDriver.AppendChild(ILayoutItem item) => AppendChild(item);
        void ILayoutDriver.TruncateChild(ILayoutItem item) => TruncateChild(item);

        protected virtual void AppendChild(ILayoutItem item) {
            if (item is ReactiveComponentBase comp) {
                AppendReactiveChild(comp);
            } else {
                item.RectTransform.SetParent(ContentTransform, false);
            }
        }

        protected virtual void TruncateChild(ILayoutItem item) {
            if (item is ReactiveComponentBase comp) {
                TruncateReactiveChild(comp);
            } else {
                item.RectTransform.SetParent(null, false);
            }
        }

        protected virtual void AppendReactiveChild(ReactiveComponentBase comp) {
            comp.Use(ContentTransform);
        }

        protected virtual void TruncateReactiveChild(ReactiveComponentBase comp) {
            comp.Use();
        }

        #endregion

        #region Callbacks

        private void HandleChildModifierUpdated() {
            RecalculateLayout();
        }

        private void HandleChildrenChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (!IsInitialized) return;
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    ApplyParent(e.NewItems, true);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ApplyParent(e.OldItems, false);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ApplyParent(e.OldItems, false);
                    ApplyParent(e.NewItems, true);
                    break;
            }
            OnChildrenUpdated();
            RefreshLayoutControllerChildren();
        }

        #endregion

        #region Overrides

        protected sealed override void DestroyInternal() {
            base.DestroyInternal();
            _children.CollectionChanged -= HandleChildrenChanged;
        }

        protected sealed override void ConstructInternal() {
            base.ConstructInternal();
            _children.CollectionChanged += HandleChildrenChanged;
            HandleChildrenChanged(this, new(NotifyCollectionChangedAction.Add, _children));
        }

        protected sealed override void OnRectDimensionsChangedInternal() {
            RecalculateLayout();
            base.OnRectDimensionsChangedInternal();
        }

        #endregion

        #region Events

        protected virtual void OnChildrenUpdated() { }

        #endregion
    }

    internal abstract class ReactiveComponent : ReactiveComponentBase {
        #region Overrides

        protected sealed override void DestroyInternal() {
            base.DestroyInternal();
        }

        protected sealed override void ConstructInternal() {
            base.ConstructInternal();
        }

        protected sealed override void OnRectDimensionsChangedInternal() {
            base.OnRectDimensionsChangedInternal();
        }

        #endregion
    }

    internal abstract class ReactiveComponentBase : ILayoutItem {
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

        #region Host

        private class ReactiveHost : MonoBehaviour {
            public readonly List<ReactiveComponentBase> components = new();

            public bool IsStarted { get; private set; }
            public bool IsDestroyed { get; private set; }

            private void Start() {
                components.ForEach(static x => x.OnStart());
                IsStarted = true;
            }

            private void Update() {
                components.ForEach(static x => x.OnUpdate());
            }

            private void OnDestroy() {
                components.ForEach(static x => x.DestroyInternal());
                IsDestroyed = true;
            }

            private void OnEnable() {
                components.ForEach(static x => x.OnEnable());
            }

            private void OnDisable() {
                components.ForEach(static x => x.OnDisable());
            }

            private void OnRectTransformDimensionsChange() {
                components.ForEach(static x => x.OnRectDimensionsChangedInternal());
            }
        }

        protected virtual void OnRectDimensionsChangedInternal() {
            OnRectDimensionsChanged();
        }

        #endregion

        #region UI Props

        /// <summary>
        /// Represents the parent of the component.
        /// </summary>
        public ILayoutDriver? LayoutDriver {
            get => _parent;
            set {
                _parent?.TruncateChild(this);
                _parent = value;
                _parent?.AppendChild(this);
            }
        }

        /// <summary>
        /// Gets or sets the local scale of the transform.
        /// </summary>
        public Vector2 Scale {
            get => ContentTransform.localScale;
            set => ContentTransform.localScale = value;
        }

        /// <summary>
        /// Gets or sets state of the transform.
        /// </summary>
        public bool Active {
            get => Content.activeInHierarchy;
            set => Content.SetActive(value);
        }

        /// <summary>
        /// Gets or sets name of the content game object.
        /// </summary>
        public string Name {
            get => Content.name;
            set => Content.name = value;
        }

        private ILayoutDriver? _parent;

        #endregion

        #region Layout Modifier

        RectTransform ILayoutItem.RectTransform => ContentTransform;
        float? ILayoutItem.DesiredHeight => DesiredHeight;
        float? ILayoutItem.DesiredWidth => DesiredWidth;

        public ILayoutModifier LayoutModifier {
            get => _modifier;
            set {
                _modifier.ModifierUpdatedEvent -= HandleModifierUpdated;
                _modifier = value;
                _modifier.ModifierUpdatedEvent += HandleModifierUpdated;
                HandleModifierUpdated();
            }
        }

        protected virtual float? DesiredHeight => null;
        protected virtual float? DesiredWidth => null;

        public event Action? ModifierUpdatedEvent;

        private ILayoutModifier _modifier = new RectModifier();
        
        protected void RefreshLayout() {
            HandleModifierUpdated();
        } 
        
        private void RefreshRectModifier() {
            if (LayoutModifier is not RectModifier rectModifier) return;
            ContentTransform.anchorMin = rectModifier.AnchorMin;
            ContentTransform.anchorMax = rectModifier.AnchorMax;
            if (rectModifier.SizeDelta != null) {
                ContentTransform.sizeDelta = rectModifier.SizeDelta.Value;
            }
        }

        private void HandleModifierUpdated() {
            ContentTransform.pivot = LayoutModifier.Pivot;
            _modifier.ReloadLayoutItem(this);
            RefreshRectModifier();
            OnModifierUpdated();
            ModifierUpdatedEvent?.Invoke();
        }

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

        private Canvas? _canvas;
        private GameObject? _content;
        private RectTransform? _contentTransform;
        private ReactiveHost? _reactiveHost;
        private bool _needToStartManually;

        /// <summary>
        /// Constructs and reparents the component if needed
        /// </summary>
        public GameObject Use(Transform? parent = null) {
            ValidateExternalInteraction();
            if (!IsInitialized) ConstructAndInit();
            ContentTransform.SetParent(parent, false);
            return Content;
        }

        private void ConstructAndInit() {
            ConstructInternal();
            OnInitialize();
            if (_needToStartManually) OnStart();
        }

        protected virtual void ConstructInternal() {
            if (IsInitialized) throw new InvalidOperationException();
            OnInstantiate();

            _content = Construct();
            _content.name = GetType().Name;
            _contentTransform = _content.GetOrAddComponent<RectTransform>();

            var host = _content.GetComponent<ReactiveHost>();
            if (host != null && !host.IsDestroyed) {
                if (host.IsStarted) {
                    _needToStartManually = true;
                }
            } else {
                if (host != null) Object.Destroy(host);
                host = _content.AddComponent<ReactiveHost>();
            }

            _reactiveHost = host;
            _reactiveHost.components.Add(this);
            IsInitialized = true;
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

        protected virtual void DestroyInternal() {
            _reactiveHost!.components.Remove(this);
            IsDestroyed = true;
            OnDestroy();
        }

        #endregion

        #region Events

        protected virtual void OnInstantiate() { }
        protected virtual void OnInitialize() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnStart() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnRectDimensionsChanged() { }
        protected virtual void OnModifierUpdated() { }

        #endregion
    }
}