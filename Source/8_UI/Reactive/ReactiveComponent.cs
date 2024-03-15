using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
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

            private void Update() {
                components.ForEach(static x => x.OnUpdate());
            }

            private void OnDestroy() {
                components.ForEach(static x => x.DestroyInternal());
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

        public ICollection<ReactiveComponent> Children => _children!;

        public ReactiveComponent? Parent { get; private set; }

        public Vector2 Scale {
            get => ContentTransform.localScale;
            set => ContentTransform.localScale = value;
        }

        public bool Active {
            get => Content.activeInHierarchy;
            set => Content.SetActive(value);
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

        public Reactive.ILayoutModifier LayoutModifier {
            get => _modifier;
            set {
                _modifier.ModifierUpdatedEvent -= HandleModifierUpdated;
                _modifier = value;
                _modifier.ModifierUpdatedEvent += HandleModifierUpdated;
                HandleModifierUpdated();
            }
        }

        private event Action? ModifierUpdatedEvent;

        private Reactive.ILayoutModifier _modifier = new RectModifier();

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

        protected IEnumerable<ReactiveComponent> GetChildrenWithModifiers<T>() where T : Reactive.ILayoutModifier {
            return Children.Where(static x => x.LayoutModifier is T);
        }

        #endregion

        #region Children

        private ObservableCollection<ReactiveComponent>? _children;

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

        private GameObject? _content;
        private RectTransform? _contentTransform;
        private ReactiveHost? _reactiveHost;

        /// <summary>
        /// Constructs and reparents the component if needed
        /// </summary>
        public void Use(Transform? parent = null) {
            ValidateExternalInteraction();
            if (!IsInitialized) ConstructInternal();
            ContentTransform.SetParent(parent, false);
        }

        /// <summary>
        /// Constructs the component using the specified object as a root. Must be called only once
        /// </summary>
        /// <param name="root">Root object which will be used as a hierarchy base</param>
        /// <exception cref="InvalidOperationException">Throws when object is already constructed</exception>
        public void Apply(RectTransform root) {
            ValidateExternalInteraction();
            if (IsInitialized) throw new InvalidOperationException();
            _content = root.gameObject;
            _contentTransform = root;
            ConstructInternal();
        }

        /// <summary>
        /// Constructs the component using the specified object as a root. Must be called only once
        /// </summary>
        /// <param name="component">Root object which will be used as a hierarchy base</param>
        /// <exception cref="InvalidOperationException">Throws when object is already constructed</exception>
        public void Apply(ReactiveComponent component) {
            _children = component._children;
            _reactiveHost = component.Content.GetComponent<ReactiveHost>();
            Apply(component.ContentTransform);
        }

        private void ConstructInternal() {
            if (IsInitialized) throw new InvalidOperationException();
            OnInstantiate();

            _content ??= new GameObject(GetType().Name);
            _contentTransform ??= _content.AddComponent<RectTransform>();
            _children ??= new();
            Construct(_contentTransform);

            _reactiveHost ??= _content.AddComponent<ReactiveHost>();
            _reactiveHost.components.Add(this);
            IsInitialized = true;

            _children.CollectionChanged += HandleChildrenChanged;
            HandleChildrenChanged(this, new(NotifyCollectionChangedAction.Add, _children));

            OnInitialize();
        }

        private void ValidateExternalInteraction() {
            if (IsDestroyed) throw new InvalidOperationException("Unable to manage the object since it's destroyed");
        }

        protected abstract void Construct(RectTransform rect);

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
            _children!.CollectionChanged -= HandleChildrenChanged;
            _reactiveHost!.components.Remove(this);
            IsDestroyed = true;
            OnDestroy();
        }

        #endregion

        #region Events

        protected virtual void OnInstantiate() { }
        protected virtual void OnInitialize() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnRectDimensionsChanged() { }
        protected virtual void OnChildrenUpdated() { }
        protected virtual void OnModifierUpdated() { }
        protected virtual void OnChildModifierUpdated() { }

        #endregion
    }
}