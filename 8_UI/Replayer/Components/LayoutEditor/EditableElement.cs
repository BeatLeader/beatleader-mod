using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components
{
    internal abstract class EditableElement : ReeUIComponentV2WithContainer, IDisposable
    {
        public enum HideMode
        {
            Opacity,
            Hierarchy
        }

        protected virtual RectTransform WrapperRect => ContainerRect;
        protected abstract RectTransform ContainerRect { get; }
        protected virtual HideMode Mode => HideMode.Opacity;
        public virtual string Name => GetType().Name;
        public virtual bool Locked => false;
        public virtual bool Enabled { get; private set; } = true;

        public event Action<string, bool> OnStateChanged;

        private EditableWrapper _editableWrapper;
        private CanvasGroup _containerCanvasGroup;
        private CanvasGroup _wrapperCanvasGroup;
        private bool _tempContainerEnabled;
        private bool _initialized;

        public void Dispose()
        {
            _editableWrapper.Dispose();
            GameObject.Destroy(_containerCanvasGroup);
        }
        public void Rebuild()
        {
            _editableWrapper.Rebuild();
        }
        public void SetEnabled(bool enabled, bool applySettings = true)
        {
            SetupSelfIfNeeded();
            Enabled = applySettings && !enabled ? _tempContainerEnabled : Enabled;
            _editableWrapper.SetEnabled(enabled, Enabled);
            SetContainerEnabled(enabled ? true : Enabled);
            if (enabled) OnToggleValueChanged(Enabled);

            OnStateChanged?.Invoke(Name, Enabled);
        }
        private void SetContainerEnabled(bool enabled = true)
        {
            float alpha = Mode == HideMode.Hierarchy ? 1 : enabled ? 1 : 0;
            if (_containerCanvasGroup != null) _containerCanvasGroup.alpha = alpha;
            _wrapperCanvasGroup.alpha = alpha;
            if (Mode == HideMode.Hierarchy) ContainerRect.gameObject.SetActive(enabled);
        }

        private bool SetupSelfIfNeeded()
        {
            if (_initialized || ContainerRect == null) return false;
            _containerCanvasGroup = Mode == HideMode.Opacity ? ContainerRect.gameObject.GetOrAddComponent<CanvasGroup>() : null;
            _wrapperCanvasGroup = WrapperRect.gameObject.GetOrAddComponent<CanvasGroup>();
            _editableWrapper = new EditableWrapper(WrapperRect, Name, false, Locked, Enabled);
            _editableWrapper.OnToggleStateChanged += OnToggleValueChanged;
            _initialized = true;
            return true;
        }
        private void OnToggleValueChanged(bool state)
        {
            _wrapperCanvasGroup.alpha = state ? 0.7f : 0.4f;
            _tempContainerEnabled = state;
        }
    }
}
