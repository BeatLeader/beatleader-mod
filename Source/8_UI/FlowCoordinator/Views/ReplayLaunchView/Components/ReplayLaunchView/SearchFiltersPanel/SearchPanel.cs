using System;
using System.Linq;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using BGLib.Polyglot;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class SearchPanel : ReeUIComponentV2 {
        #region Prefab

        private static GameObject InputFieldViewPrefab => _inputFieldViewPrefab ?
            _inputFieldViewPrefab! : _inputFieldViewPrefab = Resources
                .FindObjectsOfTypeAll<InputFieldView>().First().gameObject;

        private static GameObject? _inputFieldViewPrefab;

        #endregion

        #region Events

        public event Action<string>? TextChangedEvent;

        #endregion
        
        #region UI Components

        [UIComponent("container")]
        private readonly Transform _container = null!;

        private TMP_Text? _placeholder;
        
        #endregion
        
        #region Init

        protected override void OnInitialize() {
            var searchField = Instantiate(InputFieldViewPrefab, 
                _container, false).GetComponent<InputFieldView>();
            var placeholderTransform = searchField.transform.Find("PlaceholderText");
            placeholderTransform.GetComponent<LocalizedTextMeshProUGUI>().TryDestroy();
            _placeholder = placeholderTransform.GetComponent<TMP_Text>();
            searchField.transform.SetSiblingIndex(0);
            searchField.onValueChanged.AddListener(HandleInputFieldValueChanged);
        }

        #endregion

        #region Placeholder

        public string Placeholder {
            get => _placeholder?.text ?? throw new UninitializedComponentException();
            set => (_placeholder! ?? throw new UninitializedComponentException()).text = value;
        }

        #endregion

        #region Callbacks

        private void HandleInputFieldValueChanged(InputFieldView field) {
            TextChangedEvent?.Invoke(field.text);
        }
        
        #endregion
    }
}