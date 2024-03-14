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
    internal abstract class ReactiveComponent {
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

            private void Update() => components.ForEach(static x => x.OnUpdate());
            private void OnDestroy() => components.ForEach(static x => x.PostDestroy());

            private void OnRectTransformDimensionsChange() {
                components.ForEach(
                    static x => {
                        x.UpdateModifier();
                        x.OnRectDimensionsChange();
                    });
            }
        }

        #endregion

        #region UI Props

        public ICollection<ReactiveComponent> Children => _children!;

        public bool Active {
            get => Content.activeInHierarchy;
            set => Content.SetActive(value);
        }

        #endregion

        #region Layout

        public ILayoutModifier Modifier {
            get => _modifier;
            set {
                _modifier.ModifierUpdatedEvent -= UpdateModifier;
                _modifier = value;
                _modifier.ModifierUpdatedEvent += UpdateModifier;
                UpdateModifier();
            }
        }

        private event Action? ModifierUpdatedEvent;

        private ILayoutModifier _modifier = new RectModifier();

        private void UpdateModifier() {
            ContentTransform.localScale = Modifier.Scale;
            if (Modifier.SizeDelta != null) {
                ContentTransform.sizeDelta = Modifier.SizeDelta.Value;
            }
            ContentTransform.anchorMin = Modifier.AnchorMin;
            ContentTransform.anchorMax = Modifier.AnchorMax;
            OnModifierUpdate();
            ModifierUpdatedEvent?.Invoke();
        }

        protected IEnumerable<ReactiveComponent> GetChildrenWithModifiers<T>() where T : ILayoutModifier {
            return Children.Where(static x => x.Modifier is T);
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
            OnChildrenUpdate();
        }
        
        private void ApplyParent(IEnumerable children, bool append) {
            foreach (var child in children) {
                if (child is not ReactiveComponent comp) continue;
                if (append) {
                    AppendChild(comp);
                    comp.ModifierUpdatedEvent += OnChildModifierUpdate;
                } else {
                    TruncateChild(comp);
                    comp.ModifierUpdatedEvent -= OnChildModifierUpdate;
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
            PostDestroy();
        }

        private void PostDestroy() {
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
        protected virtual void OnRectDimensionsChange() { }
        protected virtual void OnChildrenUpdate() { }
        protected virtual void OnModifierUpdate() { }
        protected virtual void OnChildModifierUpdate() { }

        #endregion
    }
}