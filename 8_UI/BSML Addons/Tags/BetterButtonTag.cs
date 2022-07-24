using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage;
using UnityEngine.UI;
using UnityEngine;
using HMUI;

namespace BeatLeader.UI.BSML_Addons.Tags
{
    public class BetterButtonTag : BSMLTag
    {
        public override string[] Aliases => new[] { "better-button", "custom-button" }; 

        public override GameObject CreateObject(Transform parent)
        {
            GameObject container = new GameObject("BeatLeaderBetterButton");

            BetterButton unityButton = container.AddComponent<BetterButton>();
            Image image = container.AddComponent<Image>();
            Button button = container.AddComponent<Button>();
            ContentSizeFitter contentSizeFitter = container.AddComponent<ContentSizeFitter>();
            container.AddComponent<LayoutElement>();

            unityButton.Init(button, image);
            image.material = Utilities.ImageResources.NoGlowMat;
            image.rectTransform.sizeDelta = new Vector2(20f, 20f);
            image.sprite = Utilities.ImageResources.BlankSprite;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            container.transform.SetParent(parent, false);

            return container;
        }
    }
}
