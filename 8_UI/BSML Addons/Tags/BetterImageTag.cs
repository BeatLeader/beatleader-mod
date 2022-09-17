using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage;
using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.UI.BSML_Addons.Tags
{
    public class BetterImageTag : BSMLTag
    {
        public override string[] Aliases => new[] { "better-image", "better-img", "custom-image", "custom-img" };

        public override GameObject CreateObject(Transform parent)
        {
            GameObject gameObject = new GameObject("BeatLeaderBetterImage");

            Image image = gameObject.AddComponent<Image>();
            image.material = Utilities.ImageResources.NoGlowMat;
            image.rectTransform.sizeDelta = new Vector2(20f, 20f);
            image.sprite = Utilities.ImageResources.BlankSprite;
            ContentSizeFitter fitter = gameObject.AddComponent<ContentSizeFitter>();
            //fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            //fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            gameObject.AddComponent<LayoutElement>();
            gameObject.AddComponent<BetterImage>().Init(image);

            image.rectTransform.SetParent(parent, false);
            return gameObject;
        }
    }
}
