using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.Components
{
    [RequireComponent(typeof(Graphic))]
    internal class HighlightableView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Color normalColor;
        public Color highlightedColor;

        private Graphic _graphic;

        private void Start()
        {
            _graphic = GetComponent<Graphic>();
            _graphic.color = normalColor;
        }
        public void Init(Color normalColor, Color highlightedColor)
        {
            this.normalColor = normalColor;
            this.highlightedColor = highlightedColor;
        }
        public void OnPointerEnter(PointerEventData data)
        {
            _graphic.color = highlightedColor;
        }
        public void OnPointerExit(PointerEventData data)
        {
            _graphic.color = normalColor;
        }
    }
}
