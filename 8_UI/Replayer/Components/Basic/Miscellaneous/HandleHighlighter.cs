using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.Components {
    [RequireComponent(typeof(Graphic))]
    internal class HandleHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        public Color normalColor;
        public Color highlightedColor;

        private Graphic _graphic;

        private void Awake() {
            _graphic = GetComponent<Graphic>();
            _graphic.color = normalColor;
        }
        public void Setup(Color normalColor, Color highlightedColor) {
            this.normalColor = normalColor;
            this.highlightedColor = highlightedColor;
            _graphic.color = normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _graphic.color = highlightedColor;
        }
        public void OnPointerExit(PointerEventData eventData) {
            _graphic.color = normalColor;
        }
    }
}
