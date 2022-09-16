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
            Custom,
            Opacity,
            Hierarchy
        }

        protected virtual RectTransform WrapperRect => ContainerRect;
        protected abstract RectTransform ContainerRect { get; }
        protected virtual HideMode Mode => HideMode.Opacity;
        protected virtual Action<bool> CustomVisibilityController { get; set; }
        public virtual string Name => GetType().Name;
        public virtual bool Locked => false;
        public virtual bool Enabled { get; private set; } = true;

        public event Action<string, bool> OnStateChanged;

        protected CanvasGroup ContainerCanvasGroup { get; private set; }
        protected CanvasGroup WrapperCanvasGroup { get; private set; }

        private EditableWrapper _editableWrapper;
        private bool _tempContainerEnabled;
        private bool _initialized;

        public void Dispose()
        {
            _editableWrapper.Dispose();
            Destroy(ContainerCanvasGroup);
        }
        public void SetElementEnabled(bool enabled)
        {
            Enabled = enabled;
            SetupSelfIfNeeded();
            SetContainerEnabled(enabled);
        }
        public void SetEditableEnabled(bool enabled, bool applySettings = true)
        {
            Enabled = applySettings && !enabled ? _tempContainerEnabled : Enabled;
            SetContainerEnabled(enabled ? true : Enabled);
            ContainerRect.ForceUpdateRectTransforms();
            SetupSelfIfNeeded();
            _editableWrapper.SetEditableEnabled(enabled, Enabled);
            if (enabled) NotifyToggleValueChanged(Enabled);

            OnStateChanged?.Invoke(Name, Enabled);
        }
        private void SetContainerEnabled(bool enabled = true)
        {
            float alpha = Mode == HideMode.Opacity ? (enabled ? 1 : 0) : 1;
            if (ContainerCanvasGroup != null) ContainerCanvasGroup.alpha = alpha;
            if (WrapperCanvasGroup != null) WrapperCanvasGroup.alpha = enabled ? 1 : 0;
            if (Mode == HideMode.Hierarchy) ContainerRect.gameObject.SetActive(enabled);
            else if (Mode == HideMode.Custom) CustomVisibilityController?.Invoke(enabled);
        }

        private bool SetupSelfIfNeeded()
        {
            if (_initialized || ContainerRect == null) return false;
            ContainerCanvasGroup = ContainerRect.gameObject.GetOrAddComponent<CanvasGroup>();
            WrapperCanvasGroup = WrapperRect.gameObject.GetOrAddComponent<CanvasGroup>();
            _editableWrapper = new EditableWrapper(WrapperRect, Name, false, Locked, Enabled);
            _editableWrapper.OnToggleStateChanged += NotifyToggleValueChanged;
            _initialized = true;
            return true;
        }
        private void NotifyToggleValueChanged(bool state)
        {
            WrapperCanvasGroup.alpha = state ? 0.7f : 0.4f;
            _tempContainerEnabled = state;
        }
    }
}
