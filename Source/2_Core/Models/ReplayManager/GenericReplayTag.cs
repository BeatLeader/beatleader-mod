using System;
using UnityEngine;

namespace BeatLeader.Models {
    internal class GenericReplayTag : IEditableReplayTag {
        public GenericReplayTag(
            string name,
            Func<string, ReplayTagValidationResult> validateCallback,
            Action<IEditableReplayTag> deleteCallback
        ) {
            _name = name;
            _validateCallback = validateCallback;
            _deleteCallback = deleteCallback;
        }

        public Color Color => _isDeleted ? throw new InvalidOperationException() : _color;
        public string Name => _isDeleted ? throw new InvalidOperationException() : _name;

        public event Action? TagUpdatedEvent;

        private readonly Func<string, ReplayTagValidationResult> _validateCallback;
        private readonly Action<IEditableReplayTag> _deleteCallback;
        private bool _isDeleted;

        private Color _color;
        private string _name;

        public void SetColor(Color color) {
            _color = color;
            TagUpdatedEvent?.Invoke();
        }

        public ReplayTagValidationResult SetName(string name) {
            var result = _validateCallback(name);
            if (result.Ok) {
                _name = name;
                TagUpdatedEvent?.Invoke();
            }
            return result;
        }

        public void Delete() {
            _deleteCallback(this);
            _isDeleted = true;
        }
    }
}