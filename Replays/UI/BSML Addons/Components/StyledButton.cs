using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage;
using UnityEngine.UI;
using UnityEngine;
using HMUI;

namespace BeatLeader.Replays.UI.BSML_Addons.Components
{
    public class StyledButton : MonoBehaviour
    {
        public Button Button { get; protected set; }
        public Sprite NormalStateImage
        {
            set
            {
                ((Image)Button.targetGraphic).sprite = value;
                transform.localScale = Vector3.one;
            }
        }
        public Sprite PressedStateImage
        {
            set
            {
                SpriteState state = Button.spriteState;
                state.pressedSprite = value;
                Button.spriteState = state;
                transform.localScale = Vector3.one;
            }
        }
        public Sprite HighlightedStateImage
        {
            set
            {
                SpriteState state = Button.spriteState;
                state.highlightedSprite = value;
                Button.spriteState = state;
                transform.localScale = Vector3.one;
            }
        }
        public bool PreserveAspectRate
        {
            set
            {
                ((Image)Button.targetGraphic).preserveAspect = value;
            }
        }

        protected Vector2 _scale;

        public void ProvideComponents(Button button, Image targetGraphic)
        { 
            _scale = Vector2.one;
            Button = button;
            Button.targetGraphic = targetGraphic;
            Button.transition = Selectable.Transition.SpriteSwap;
            Button.navigation = new Navigation() { mode = Navigation.Mode.None };
        }
    }
}
