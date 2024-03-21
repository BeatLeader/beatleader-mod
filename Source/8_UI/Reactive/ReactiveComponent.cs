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
    internal abstract class ReactiveComponent : ILayoutItem {
        #region Factory

        protected ReactiveComponent() {
            //do not want to reimplement constructor each inheritance so use this hacky reflection way
            var trace = new StackTrace();
            var mtd = trace.GetFrames()?
                .Select(static x => x.GetMethod())
                .FirstOrDefault(static x => x.Name == "Lazy" && x.DeclaringType == typeof(ReactiveComponent));
            if (mtd == null) ConstructInternal();
        }

        public static T Lazy<T>() where T : ReactiveComponent, new() {
            return new();
        }

        #endregion

        #region Host

        private class ReactiveHost : MonoBehaviour {
            public readonly List<ReactiveComponent> components = new();

            private void Start() {
                components.ForEach(static x => x.OnStart());
            }

            private void Update() {
                components.ForEach(static x => x.OnUpdate());
            }

            private void OnDestroy() {
                components.ForEach(static x => x.DestroyInternal());
            }

            private void OnEnable() {
                components.ForEach(static x => x.OnEnable());
            }

            private void OnDisable() {
                components.ForEach(static x => x.OnDisable());
            }

            private void OnRectTransformDimensionsChange() {
                components.ForEach(static x => x.HandleRectDimensionsChanged());
            }
        }

        private void HandleRectDimensionsChanged() {
            RecalculateLayout();
            OnRectDimensionsChanged();
        }

        #endregion

        #region UI Props

        /// <summary>
        /// Represents the children of the component.
        /// </summary>
        public ICollection<ReactiveComponent> Children => _children;

        /// <summary>
        /// Represents the parent of the component.
        /// </summary>
        public ReactiveComponent? Parent { get; private set; }

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

        #endregion

        #region Layout Item

        ILayoutItem? ILayoutItem.Parent => Parent;
        IEnumerable<ILayoutItem> ILayoutItem.Children => Children;
        RectTransform ILayoutItem.RectTransform => ContentTransform;

        #endregion

        #region Layout Controller

        public ILayoutController? LayoutController => _layoutController;

        private IApplicableLayoutController? _layoutController;

        public void SetLayoutController(IApplicableLayoutController controller) {
            _layoutController = controller;
            _layoutController!.Setup(this);
            RefreshLayoutControllerChildren();
            RecalculateLayout();
        }

        private void RefreshLayoutControllerChildren() {
            _layoutController?.RefreshChildren();
        }

        private void RecalculateLayout() {
            _layoutController?.Recalculate();
        }

        #endregion

        #region Layout Modifier

        public ILayoutModifier LayoutModifier {
            get => _modifier;
            set {
                _modifier.ModifierUpdatedEvent -= HandleModifierUpdated;
                _modifier = value;
                _modifier.ModifierUpdatedEvent += HandleModifierUpdated;
                HandleModifierUpdated();
            }
        }

        private event Action? ModifierUpdatedEvent;

        private ILayoutModifier _modifier = new RectModifier();

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
            RefreshRectModifier();
            OnModifierUpdated();
            ModifierUpdatedEvent?.Invoke();
        }

        private void HandleChildModifierUpdated() {
            RecalculateLayout();
            OnChildModifierUpdated();
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

        #region Children

        private ObservableCollection<ReactiveComponent> _children = new();

        private void HandleChildrenChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (!IsInitialized) return;
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    ApplyParent(e.NewItems, true);
                    break;
                case NotifyCollectionChangedAction.Reset:
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

        private void ApplyParent(IEnumerable children, bool append) {
            foreach (var child in children) {
                if (child is not ReactiveComponent comp) continue;
                if (append) {
                    AppendChild(comp);
                    comp.Parent = this;
                    comp.ModifierUpdatedEvent += HandleChildModifierUpdated;
                } else {
                    TruncateChild(comp);
                    comp.Parent = null;
                    comp.ModifierUpdatedEvent -= HandleChildModifierUpdated;
                }
            }
        }

        protected virtual void AppendChild(ReactiveComponent comp) {
            comp.Use(ContentTransform);
        }

        protected virtual void TruncateChild(ReactiveComponent comp) {
            comp.Use();
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

        /// <summary>
        /// Constructs and reparents the component if needed
        /// </summary>
        public GameObject Use(Transform? parent = null) {
            ValidateExternalInteraction();
            if (!IsInitialized) ConstructInternal();
            ContentTransform.SetParent(parent, false);
            return Content;
        }

        private void ConstructInternal() {
            if (IsInitialized) throw new InvalidOperationException();
            OnInstantiate();

            _content = Construct();
            _content.name = GetType().Name;
            _contentTransform = _content.GetOrAddComponent<RectTransform>();

            _reactiveHost = _content.AddComponent<ReactiveHost>();
            _reactiveHost.components.Add(this);
            IsInitialized = true;

            _children.CollectionChanged += HandleChildrenChanged;
            HandleChildrenChanged(this, new(NotifyCollectionChangedAction.Add, _children));

            OnInitialize();
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
            _children.CollectionChanged -= HandleChildrenChanged;
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
        protected virtual void OnChildrenUpdated() { }
        protected virtual void OnModifierUpdated() { }
        protected virtual void OnChildModifierUpdated() { }

        #endregion
    }
}