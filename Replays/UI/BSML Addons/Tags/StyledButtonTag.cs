using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage;
using UnityEngine.UI;
using UnityEngine;
using HMUI;

namespace BeatLeader.Replays.UI.BSML_Addons.Tags
{
    public class StyledButtonTag : BSMLTag
    {
        public override string[] Aliases => new[] { "styled-button" }; 

        public override GameObject CreateObject(Transform parent)
        {
            GameObject container = new GameObject("BeatLeaderStyledButton");
            ExternalComponents externalComponents = container.AddComponent<ExternalComponents>();

            StyledButton styledButton = container.AddComponent<StyledButton>();
            Image image = container.AddComponent<Image>();
            Button button = container.AddComponent<Button>();
            ContentSizeFitter contentSizeFitter = container.AddComponent<ContentSizeFitter>();
            container.AddComponent<LayoutElement>();

            externalComponents.components.Add(container.AddComponent<StackLayoutGroup>());
            externalComponents.components.Add(styledButton);
            externalComponents.components.Add(image);
            externalComponents.components.Add(button);

            styledButton.ProvideComponents(button, image);
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            image.transform.SetParent(container.transform);
            container.transform.SetParent(parent);

            return container;
        }
    }
}
